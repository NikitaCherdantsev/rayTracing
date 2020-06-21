using vector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;


namespace rayTracing {

    class Program {

        private static Vector Reflection(Vector L, Vector N) { return L - N * 2f * (L * N); }

        private static Vector Cast_ray(Vector orig, Vector direct, List<Sphere> spheres, List<Light> lights, int depth = 0)
        {

            Vector point = new Vector();
            Vector N = new Vector();
            Material material = new Material();

            if (depth > 4 || !SceneIntersection(orig, direct, spheres, ref point, ref N, ref material))
            {

                // Цвет заднего фона.
                return new Vector(.37f, .72f, .21f);
            }

            Vector reflection_dir = Reflection(direct, N).VecNormalize();
            Vector refraction_dir = Refraction(direct, N, material.refIndex).VecNormalize();

            //Смещение исходной точки, чтобы избежать окклюзии самим объектом.
            Vector reflection_orig = reflection_dir * N < 0 ? point - N * 1e-3f : point + N * 1e-3f;
            Vector refraction_orig = refraction_dir * N < 0 ? point - N * 1e-3f : point + N * 1e-3f;
            Vector reflection_color = Cast_ray(reflection_orig, reflection_dir, spheres, lights, depth + 1);
            Vector refraction_color = Cast_ray(refraction_orig, refraction_dir, spheres, lights, depth + 1);

            float diffuse_light_intensity = 0, specula_light_intensity = 0;

            foreach (var light in lights)
            {
                Vector light_direct = (light.position - point).VecNormalize();
                float light_distance = (light.position - point).VecLong();

                //Проверка, лежит ли точка в тени света.
                Vector shadow_orig = light_direct * N < 0 ? point - N * 1e-3f : point + N * 1e-3f;
                Vector shadow_pt = new Vector();
                Vector shadow_N = new Vector();
                Material tmpMaterial = new Material();

                if (SceneIntersection(shadow_orig, light_direct, spheres, ref shadow_pt, ref shadow_N, ref tmpMaterial) && (shadow_pt - shadow_orig).VecLong() < light_distance)
                    continue;

                diffuse_light_intensity += light.intensity * Math.Max(0, light_direct * N);
                specula_light_intensity += (float)Math.Pow(Math.Max(0, -Reflection(-light_direct, N) * direct), material.specExp) * light.intensity;
            }

            return material.color * diffuse_light_intensity * material.albedo[0]
                + new Vector(1, 1, 1) * specula_light_intensity * material.albedo[1]
                + reflection_color * material.albedo[2]
                + refraction_color * material.albedo[3];
        }

        private static bool SceneIntersection(Vector orig, Vector direct, List<Sphere> spheres, ref Vector hit, ref Vector N, ref Material material) {

            float spheres_dist = float.MaxValue;

            foreach (var sphere in spheres) {

                float disti = 0f;

                if (sphere.RayIntersection(orig, direct, ref disti) && disti < spheres_dist) {

                    spheres_dist = disti;
                    hit = orig + direct * disti;
                    N = (hit - sphere.center_pos).VecNormalize();
                    material = sphere.material;
                }
            }

            float checkerboard_dist = float.MaxValue;
            if (Math.Abs(direct.y) > 1e-3) {

                //Плоскость шахматной доски имеет уравнение y = -4.
                float d = -(orig.y + 4) / direct.y;
                Vector pt = orig + direct * d;

                if (d > 0 && Math.Abs(pt.x) < 10 && pt.z < -10 && pt.z > -30 && d < spheres_dist) {

                    checkerboard_dist = d;
                    hit = pt;
                    N = new Vector(0, 1, 0);
                    Vector c1 = new Vector(.15f, .15f, .15f);
                    Vector c2 = new Vector(.4f, .4f, .4f);
                    material.color = (((int)(.9 * hit.x + 1000) + (int)(.9 * hit.z)) & 1) == 1 ? c1 : c2;
                }
            }

            return Math.Min(spheres_dist, checkerboard_dist) < 1000;
        }

        private static Vector Refraction(Vector I, Vector N, float eta_t, float eta_i = 1f)
        {

            float cosi = -Math.Max(-1f, Math.Min(1, I * N));

            if (cosi < 0) return Refraction(I, -N, eta_i, eta_t);

            float eta = eta_i / eta_t;
            float k = 1 - eta * eta * (1 - cosi * cosi);

            return k < 0 ? new Vector(1, 0, 0) : I * eta + N * (eta * cosi - (float)Math.Sqrt(k));
        }


        private static void Rendering(List<Sphere> spheres, List<Light> lights, int width, int height) {

            float fov = (float)(Math.PI / 3f);
            Vector[] buffer = new Vector[width * height];

            for (var j = 0; j < height; j++) {

                for (var i = 0; i < width; i++) {

                    float dir_x = i + .5f - width / 2f;
                    float dir_y = -(j + .5f) + height / 2f;
                    float dir_z = -height / (2f * (float)Math.Tan(fov / 2f));

                    buffer[i + j * width] = Cast_ray(new Vector(0, 0, 0), new Vector(dir_x, dir_y, dir_z).VecNormalize(), spheres, lights);
                }
            }

            // Сохранение картинки в формате png.
            const int pixelSize = 4;
            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData bmpData = null;

            try {

                bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

                for (var y = 0; y < height; ++y)
                    unsafe {

                        byte* target_row = (byte*)bmpData.Scan0 + y * bmpData.Stride;

                        for (var x = 0; x < width; ++x) {

                            int i = y * width + x;
                            Vector c = buffer[i];
                            float max = Math.Max(c[0], Math.Max(c[1], c[2]));

                            if (max > 1) c /= max;

                            target_row[x * pixelSize + 0] = (byte)(255 * Math.Max(0f, Math.Min(1f, c[2])));
                            target_row[x * pixelSize + 1] = (byte)(255 * Math.Max(0f, Math.Min(1f, c[1])));
                            target_row[x * pixelSize + 2] = (byte)(255 * Math.Max(0f, Math.Min(1f, c[0])));
                            target_row[x * pixelSize + 3] = 255;
                        }
                    }
            }

            finally {

                if (bmpData != null) bmp.UnlockBits(bmpData);
            }

            bmp.Save("image5.png", ImageFormat.Png);
        }

        static void Main() {

            // Материалы.
            Material red_plastic = new Material(1, new Vector(.255f, .0f, .0f), 50, new[] { .6f, .3f, .1f, .0f });
            Material glass = new Material(1.5f, new Vector(.6f, .7f, .8f), 125, new[] { .0f, .5f, .1f, .8f });
            Material blue_matte_rubber = new Material(1, new Vector(.0f, .0f, .255f), 10, new[] { .9f, .1f, .0f, .0f });
            Material mirror = new Material(1, new Vector(1, 1, 1), 1425, new[] { .0f, 10f, .8f, .0f });

            // Сферы.            
            List<Sphere> spheres = new List<Sphere> {

                new Sphere(new Vector(-3, 0, -16), 2, red_plastic),
                new Sphere(new Vector(-1, -1.5f, -12), 1, glass),
                new Sphere(new Vector(1.5f, -0.5f, -18), 2, blue_matte_rubber),
                new Sphere(new Vector(7, 5, -18), 3, mirror)
            };

            // Огоньки на сферах.
            List<Light> lights = new List<Light> {

                //Левый
                new Light(new Vector(0, 20, 20), 1.5f),
                //Верхний
                new Light(new Vector(55, 70, -45), 1.8f),
                //Правый
                new Light(new Vector(40, 20, 30), 1.7f)
            };

            int width = 640;
            int height = 480;

            Rendering(spheres, lights, width, height);
        }
    }
}
