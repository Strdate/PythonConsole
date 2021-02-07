using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    public class NetNode
    {
        public ushort id { get; private set; }

        public string prefab_name { get; private set; }

        public Vector position { get; private set; }

        internal NetNode(object obj)
        {
            NetNodeMessage msg = (NetNodeMessage)obj;
            id = msg.id;
            prefab_name = msg.prefab_name;
            position = msg.position;
        }
    }
}
