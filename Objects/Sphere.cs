using System;
using vector;

namespace rayTracing {

    public class Sphere {

        public Vector center_pos;
        public float radius;
        public Material material;

        public Sphere(Vector center, float radius, Material material) {

            this.center_pos = center;
            this.radius = radius;
            this.material = material;
        }

        public bool RayIntersection(Vector orig, Vector dir, ref float t0) {

            var L = center_pos - orig;
            var tca = L * dir;
            var d2 = L * L - tca * tca;

            if (d2 > radius * radius) return false;

            var thc = (float) Math.Sqrt(radius * radius - d2);
            t0 = tca - thc;
            var t1 = tca + thc;

            if (t0 < 0) t0 = t1;

            if (t0 < 0) return false;

            return true;
        }
    }
}
