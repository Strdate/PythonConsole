﻿using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public class PropMessage : InstanceMessage
    {
        public ushort id { get; set; }

        public string prefab_name { get; set; }

        public Vector position { get; set; }

        public float angle { get; set; }
    }
}
