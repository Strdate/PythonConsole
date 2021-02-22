using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    [Doc("Free standing prop object")]
    public class Prop : CitiesObject
    {
        public override string type => "prop";

        [Doc("Prop type (eg. 'Large Fountain')")]
        public string prefab_name { get; private set; }

        [Doc("Prop rotation in rad")]
        public double angle {
            get => _angle;
            set => MoveImpl(null, (float?)value);
        }

        [Doc("Move to new position")]
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
    }
}
