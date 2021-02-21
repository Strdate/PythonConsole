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

        public double angle {
            get => _angle;
            set => MoveImpl(null, (float?)value);
        }

        public void move(IPositionable pos, double? angle = null) => MoveImpl(pos.position, (float?)angle);

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

        public override string ToString()
        {
            return "{" + "\n" +
                "type: " + type + "\n" +
                "id: " + id + "\n" +
                "position: " + pos + "\n" +
                "angle: " + angle.ToString("F3") + "\n" +
                "prefab_name: " + prefab_name + "\n" +
                "}";
        }
    }
}
