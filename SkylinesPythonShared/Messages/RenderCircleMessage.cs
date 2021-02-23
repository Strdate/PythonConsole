using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public class RenderCircleMessage
    {
        public Vector position { get; set; }

        public float radius { get; set; }

        public string color { get; set; }        
    }
}
