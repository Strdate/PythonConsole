using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    public class Prop : CitiesObject
    {
        public override string type => "prop";

        public string prefab_name { get; private set; }

        public double angle => _angle;

        public override void refresh()
        {
            AssignData(api.client.RemoteCall<PropMessage>(Contracts.GetPropFromId, id));
        }

        internal override void AssignData(InstanceMessage data)
        {
            PropMessage msg = data as PropMessage;
            if (msg == null) {
                deleted = true;
                return;
            }
            id = msg.id;
            prefab_name = msg.prefab_name;
            _position = msg.position;
            _angle = msg.angle;
        }
        internal Prop(PropMessage obj, GameAPI api) : base(api)
        {
            AssignData(obj);
        }
    }
}
