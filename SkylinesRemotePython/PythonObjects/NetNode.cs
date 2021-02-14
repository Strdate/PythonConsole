using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    public class NetNode : ObjectAPI
    {
        public override string type => "node";

        public string prefab_name { get; private set; }

        public Vector position { get; private set; }

        public void refresh()
        {
            AssignData(api.client.RemoteCall<NetNodeMessage>(Contracts.GetNodeFromId, id));
        }

        internal void AssignData(NetNodeMessage msg)
        {
            id = msg.id;
            prefab_name = msg.prefab_name;
            position = msg.position;
        }

        internal NetNode(NetNodeMessage obj, GameAPI api) : base(api)
        {
            AssignData(obj);
        }
    }
}
