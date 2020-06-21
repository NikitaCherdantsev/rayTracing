using System;

namespace vector {

    public class Vector {

        public float x;
        public float y;
        public float z;

        public Vector(float x, float y, float z) {

            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector() {}

        public float this[int i] { get { return i <= 0 ? x : (i == 1 ? y : z); } }

        public float VecLong() { return (float) Math.Sqrt(x * x + y * y + z * z); }

        public Vector VecNormalize() { return this / VecLong(); }

        public float[] ToArray() { return new[] {x, y, z}; }

        public static Vector operator +(Vector self, Vector other) { return new Vector(self.x + other.x, self.y + other.y, self.z + other.z); }

        public static Vector operator -(Vector self, Vector other) { return new Vector(self.x - other.x, self.y - other.y, self.z - other.z); }

        public static float operator *(Vector self, Vector other) { return self.x * other.x + self.y * other.y + self.z * other.z; }

        public static Vector operator *(Vector self, float value) { return new Vector(self.x * value, self.y * value, self.z * value); }

        public static Vector operator /(Vector self, float value) { return self * (1f / value); }

        public static Vector operator -(Vector self) { return self * -1f; }
    }
}
