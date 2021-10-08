using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public abstract class InstanceData : InstanceDataBase<uint>
    {
        public Vector position { get; set; }

        public string prefab_name { get; set; }

        public bool exists { get; set; }
    }
}
