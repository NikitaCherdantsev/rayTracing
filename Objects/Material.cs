using vector;

namespace rayTracing {

    public class Material {

        public float refIndex;        
        public Vector color;
        public float specExp;
        public float[] albedo;

        public Material(float refIndex, Vector diffColor, float specExp, float[] albedo) {

            this.refIndex = refIndex;            
            this.color = diffColor;
            this.specExp = specExp;
            this.albedo = albedo;
        }

        public Material() {

            refIndex = 1;
            color = new Vector();
            specExp = 0;
            albedo = new[] {1f, 0f, 0f, 0f};          
        }
    }
}
