using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public class NetNodeMessage : InstanceMessage
    {
        public ushort id { get; set; }

        public string prefab_name { get; set; }

        public ushort building_id { get; set; }

        public int seg_count { get; set; }

        public float terrain_offset { get; set; }

        public Vector position { get; set; }
    }
}
