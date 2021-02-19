using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    public class Building : CitiesObject
    {
        public override string type => "building";

        public string prefab_name { get; private set; }

        public double angle => _angle;

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
