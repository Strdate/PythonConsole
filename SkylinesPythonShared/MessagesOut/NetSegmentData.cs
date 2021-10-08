using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public class NetSegmentData : InstanceData
    {
        public ushort start_node_id { get; set; }

        public ushort end_node_id { get; set; }

        public Vector start_dir { get; set; }

        public Vector end_dir { get; set; }

        public Bezier bezier { get; set; }

        public float length { get; set; }

        public bool is_straight { get; set; }
    }
}
