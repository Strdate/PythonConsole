using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public class MoveMessage
    {
        public uint id { get; set; }

        public string type { get; set; }

        public Vector position { get; set; }

        public float angle { get; set; }

        public bool is_angle_defined { get; set; }
    }
}
