using vector;

namespace rayTracing {
    public class Light {

        public Vector position;
        public float intensity;

        public Light(Vector position, float intensity) {

            this.position = position;
            this.intensity = intensity;
        }
    }
}
