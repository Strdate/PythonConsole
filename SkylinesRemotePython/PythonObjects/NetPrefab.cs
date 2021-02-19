using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    public class NetPrefab : ApiRefObject
    {
        public string name { get; private set; }

        public float width { get; private set; } 

        public bool is_overground { get; private set; }

        public bool is_underground { get; private set; }

        protected void AssignData(NetPrefabMessage msg)
        {
            name = msg.name;
            width = msg.width;
            is_overground = msg.is_overground;
            is_underground = msg.is_underground;
        }

        protected NetPrefab(NetPrefabMessage obj, GameAPI api) : base(api)
        {
            AssignData(obj);
        }

        internal static NetPrefab GetNetPrefab(string name, GameAPI api)
        {
            return new NetPrefab(api.client.RemoteCall<NetPrefabMessage>(Contracts.GetNetPrefabFromName, name), api);
        }
    }
}
