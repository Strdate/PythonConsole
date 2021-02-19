using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public class NetPrefabMessage
    {
        public string name { get; set; }

        public float width { get; set; }

        public bool is_overground { get; set; }

        public bool is_underground { get; set; }
    }
}
