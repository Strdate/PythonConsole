using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public class RenderVectorMessage
    {
        public Vector vector { get; set; }

        public Vector origin { get; set; }

        public string color { get; set; }

        public float length { get; set; }

        public float size { get; set; }
    }
}
