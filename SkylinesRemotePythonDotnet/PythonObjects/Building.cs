using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    [Doc("Structure for building objects (eg. 'Water Tower')")]
    public class Building : CitiesObject<BuildingData,Building>
    {
        public override string type => "building";

        private protected override CitiesObjectStorage<BuildingData, Building, uint> GetStorage()
        {
            return ObjectStorage.Instance.Buildings;
        }

        [Doc("Building roation in rad")]
        public double angle {
            get => _.angle;
            set => MoveImpl(null, (float?)value);
        }

        [Doc("Move to new position")]
        public void move(IPositionable pos, double? angle = null) => MoveImpl(pos.position, (float?)angle);

        public override void refresh()
        {
            ObjectStorage.Instance.Buildings.RefreshInstance(id);
        }

        public Building()
        {
            if (!CitiesObjectController.AllowInstantiation) {
                throw new Exception("Instantiation is not allowed!");
            }
        }
    }
}