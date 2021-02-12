using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared.API
{
    [Serializable]
    public class NetOptions
    {
        public string prefab_name { get; private set; }

        public string elevation_mode { get; private set; }

        public bool invert { get; private set; }

        public NetOptions(string prefab_name, string elevation_mode = "default", bool invert = false)
        {
            this.prefab_name = prefab_name;
            this.elevation_mode = elevation_mode;
            this.invert = invert;
        }
    }
}
