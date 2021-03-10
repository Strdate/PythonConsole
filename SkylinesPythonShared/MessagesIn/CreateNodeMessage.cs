using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public class CreateNodeMessage
    {
        public Vector Position { get; set; }

        public string Type { get; set; }
    }
}
