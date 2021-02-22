using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    [Doc("Point on map")]
    public class Point : IPositionable
    {
        public Vector pos => position;

        [Doc("Object type")]
        public string type => "point";

        public Vector position { get; private set; }

        [Doc("Creates new point from the vector")]
        public Point(Vector vector)
        {
            position = vector;
        }

        public override string ToString()
        {
            return PythonHelp.RuntimeToString(this);
        }
    }
}
