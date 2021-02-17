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

        public int elevation { get; set; }

        public Vector position { get; set; }
    }
}
