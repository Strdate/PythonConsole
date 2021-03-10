using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public class CreateSegmentMessage
    {
        public NetOptions net_options { get; set; }

        public Vector start_postition { get; set; }

        public Vector end_postition { get; set; }

        public ushort start_node_id { get; set; }

        public ushort end_node_id { get; set; }

        public Vector start_dir { get; set; }

        public Vector end_dir { get; set; }

        public Vector control_point { get; set; }

        public bool auto_split { get; set; }
    }
}
