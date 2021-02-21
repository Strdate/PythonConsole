using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    public class Point : IPositionable
    {
        public Vector pos => position;

        public string type => "point";

        public Vector position { get; private set; }

        public Point(Vector vector)
        {
            position = vector;
        }

        public override string ToString()
        {
            return "{" + "\n" +
                "type: " + type + "\n" +
                "position: " + pos + "\n" +
                "}";
        }

    }
}
