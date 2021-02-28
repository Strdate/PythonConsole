using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared.API
{
    [Serializable]
    [Doc("3D Vector class")]
    public class Vector : InstanceMessage, IPositionable, ISimpleToString
    {
        [Doc("X cooord")]
        public double x { get; private set; }

        [Doc("Y cooord (height)")]
        public double y { get; private set; }

        [Doc("Z cooord")]
        public double z { get; private set; }

        [Doc("Is height (Y coord) defined")]
        public bool is_height_defined { get; private set; }

        [Doc("Returns itself")]
        public Vector position => this;

        private Vector()
        {
            
        }

        [Doc("Creates new vector given all 3 coords")]
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

        [Doc("Creates new vector given the X and Z coord")]
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

        [Doc("Returns angle in XZ plane. If 'other' vector is null, returns angle with the X axis")]
        public double flat_angle(Vector other = null)
        {
            if(other == null) {
                other = Vector.vector_xz(1, 0);
            }

            double sin = x * other.z - other.x * z;
            double cos = x * other.x + z * other.z;
            double angle = pi - Math.Atan2(sin, cos);
            return angle > 0 ? angle : angle + 2*pi;
        }

        [Doc("Rotates vector in XZ plane")]
        public Vector flat_rotate(double angle, Vector pivot = null)
        {
            pivot = pivot ?? zero;
            Vector difference = this - pivot;

            double sin = Math.Sin(angle);
            double cos = Math.Cos(angle);

            Vector newPoint = new Vector((float)(difference.x * cos - difference.z * sin + pivot.x), pivot.y, (float)(difference.x * sin + difference.z * cos + pivot.z));

            return newPoint;
        }

        [Doc("Returns new vector with changed y value")]
        public Vector increase_y(double value)
        {
            return new Vector(x, y + value, z);
        }

        [Doc("Vector length")]
        public double magnitude => Math.Sqrt(x * x + y * y + z * z);

        [Doc("Returns new vectors with length 1")]
        public Vector normalized => new Vector(x / magnitude, y / magnitude, z / magnitude);

        [Doc("Returns new vector with undefined Y coord")]
        public Vector flat => new Vector() {
            x = this.x,
            z = this.z,
            is_height_defined = false
        };

        public override string ToString()
        {
            return "{x: " + x.ToString("F3") + ", y: " + (is_height_defined ? y.ToString("F3") : "undefined") + ", z: " + z.ToString("F3") + "}";
        }

        public string SimpleToString()
        {
            return ToString();
        }

        [Doc("Returns zero vector")]
        public static Vector zero => new Vector(0, 0, 0);

        [Doc("PI constant")]
        public static double pi => 3.141592653;

        [Doc("Linearlly interpolates new postion between a and b")]
        public static Vector Lerp(Vector a, Vector b, double t)
        {
            t = Clamp01(t);
            return new Vector(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        }

        private static double Clamp01(double x)
        {
            return x > 1 ? 1 : (x < 0 ? 0 : x);
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
