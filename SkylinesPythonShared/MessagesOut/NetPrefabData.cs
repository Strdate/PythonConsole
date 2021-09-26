using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public class NetPrefabData : InstanceDataBase<string>
    {
        public float width { get; set; }

        public bool is_overground { get; set; }

        public bool is_underground { get; set; }

        public int fw_vehicle_lane_count { get; set; }

        public int bw_vehicle_lane_count { get; set; }
    }
}
