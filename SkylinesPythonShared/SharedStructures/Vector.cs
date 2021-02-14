using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared.API
{
    [Serializable]
    public class Vector
    {
        public double x { get; private set; }
        public double y { get; private set; }
        public double z { get; private set; }

        public bool is_height_defined { get; private set; }

        private Vector()
        {

        }

        public Vector(double X, double Y, double Z)
        {
            this.x = X;
            this.y = Y;
            this.z = Z;
            this.is_height_defined = true;
        }

        private Vector(double X, double Y, double Z, bool is_height_defined)
        {
            this.x = X;
            this.y = Y;
            this.z = Z;
            this.is_height_defined = is_height_defined;
        }

        public static Vector vector_xz(double X, double Z)
        {
            return new Vector()
            {
                x = X,
                z = Z,
                is_height_defined = false
            };
        }

        public override bool Equals(object obj)
        {
            return obj is Vector vector &&
                   x == vector.x &&
                   y == vector.y &&
                   z == vector.z &&
                   is_height_defined == vector.is_height_defined;
        }

        public override int GetHashCode()
        {
            int hashCode = -393058493;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            hashCode = hashCode * -1521134295 + z.GetHashCode();
            hashCode = hashCode * -1521134295 + is_height_defined.GetHashCode();
            return hashCode;
        }

        public double plain_angle => Math.Atan(x / z);
        public double magnitude => Math.Sqrt(x * x + y * y + z * z);

        public Vector normalized => new Vector(x / magnitude, y / magnitude, z / magnitude);

        public Vector flat => new Vector() {
            x = this.x,
            z = this.z,
            is_height_defined = false
        };

        public override string ToString()
        {
            return "[" + x.ToString("N3") + ", " + (is_height_defined ? y.ToString("N3") : "undefined") + ", " + z.ToString("N3") + "]";
        }
        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.x + b.x, a.y + b.y, a.z + b.z, a.is_height_defined && b.is_height_defined);
        }

        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(a.x - b.x, a.y - b.y, a.z - b.z, a.is_height_defined && b.is_height_defined);
        }

        public static Vector operator -(Vector a)
        {
            return new Vector(-a.x, -a.y, -a.z, a.is_height_defined);
        }

        public static Vector operator *(Vector a, double d)
        {
            return new Vector(a.x * d, a.y * d, a.z * d);
        }

        public static Vector operator *(double d, Vector a)
        {
            return new Vector(a.x * d, a.y * d, a.z * d);
        }

        public static Vector operator /(Vector a, float d)
        {
            return new Vector(a.x / d, a.y / d, a.z / d);
        }

    }
}
