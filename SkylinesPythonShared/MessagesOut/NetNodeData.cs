using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public class NetNodeData : InstanceData
    {
        public ushort building_id { get; set; }

        public int seg_count { get; set; }

        public float terrain_offset { get; set; }

        // hack - not needed during transfer
        public object _cachedSegments { get; set; }
    }
}
