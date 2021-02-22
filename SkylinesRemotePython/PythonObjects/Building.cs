using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    [Doc("Structure for building objects (eg. 'Water Tower')")]
    public class Building : CitiesObject
    {
        public override string type => "building";

        [Doc("Prefab name - eg. 'Elementary School'")]
        public string prefab_name { get; private set; }

        [Doc("Building roation in rad")]
        public double angle {
            get => _angle;
            set => MoveImpl(null, (float?)value);
        }

        [Doc("Move to new position")]
        public void move(IPositionable pos, double? angle = null) => MoveImpl(pos.position, (float?)angle);

        public override void refresh()
        {
            AssignData(api.client.RemoteCall<BuildingMessage>(Contracts.GetBuildingFromId, id));
        }

        internal override void AssignData(InstanceMessage data)
        {
            BuildingMessage msg = data as BuildingMessage;
            if (msg == null) {
                deleted = true;
                return;
            }
            id = msg.id;
            prefab_name = msg.prefab_name;
            _position = msg.position;
            _angle = msg.angle;
        }

        internal Building(BuildingMessage obj, GameAPI api) : base(api)
        {
            AssignData(obj);
        }
    }
}