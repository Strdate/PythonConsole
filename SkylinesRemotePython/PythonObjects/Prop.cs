using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    public class Prop : ObjectAPI
    {
        public override string type => "prop";

        public string prefab_name { get; private set; }

        public Vector pos => position;

        public Vector position { get; private set; }

        public double angle { get; private set; }

        public override void refresh()
        {
            AssignData(api.client.RemoteCall<PropMessage>(Contracts.GetPropFromId, id));
        }

        internal void AssignData(PropMessage msg)
        {
            if (msg == null) {
                is_deleted = true;
                return;
            }
            id = msg.id;
            prefab_name = msg.prefab_name;
            position = msg.position;
            angle = msg.angle;
        }
        internal Prop(PropMessage obj, GameAPI api) : base(api)
        {
            AssignData(obj);
        }
    }
}
