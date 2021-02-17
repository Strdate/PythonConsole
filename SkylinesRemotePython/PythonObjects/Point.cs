using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    public class Point
    {
        public Vector pos => position;

        public string type => "point";

        public Vector position { get; private set; }

        public Point(Vector vector)
        {
            position = vector;
        }

    }
}
