using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    public class Vector3
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vector3(double X, double Y, double Z)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public double Magnitude => Math.Pow(X * X + Y * Y + Z * Z, 1 / 3);

        public Vector3 Normalized => new Vector3(X / Magnitude, Y / Magnitude, Z / Magnitude);
    }
}
