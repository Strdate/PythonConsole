using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public class NetSegmentMessage
    {
        public ushort id { get; set; }

        public string prefab_name { get; set; }

        public ushort start_node_id { get; set; }

        public ushort end_node_id { get; set; }

        public float length { get; set; }
    }
}
