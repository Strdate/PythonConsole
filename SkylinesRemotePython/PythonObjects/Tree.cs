using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    public class Tree : ObjectAPI
    {
        public override string type => "tree";

        public string prefab_name { get; private set; }

        public Vector pos => position;

        public Vector position { get; private set; }

        public override void refresh()
        {
            AssignData(api.client.RemoteCall<TreeMessage>(Contracts.GetTreeFromId, id));
        }

        internal void AssignData(TreeMessage msg)
        {
            if (msg == null) {
                deleted = true;
                return;
            }
            id = msg.id;
            prefab_name = msg.prefab_name;
            position = msg.position;
        }
        internal Tree(TreeMessage obj, GameAPI api) : base(api)
        {
            AssignData(obj);
        }
    }
}