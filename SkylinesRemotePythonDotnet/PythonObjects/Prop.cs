using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    [Doc("Free standing prop object")]
    public class Prop : CitiesObject<PropData, Prop>
    {
        public override string type => "prop";

        private protected override ObjectInstanceStorage<PropData, Prop> GetStorage()
        {
            return ObjectStorage.Instance.Props;
        }

        [Doc("Prop rotation in rad")]
        public double angle {
            get => _.angle;
            set => MoveImpl(null, (float?)value);
        }

        [Doc("Move to new position")]
        public void move(IPositionable pos, double? angle = null) => MoveImpl(pos.position, (float?)angle);

        public override void refresh()
        {
            ObjectStorage.Instance.Props.RefreshInstance(id);
        }

        public Prop()
        {
            if (!CitiesObjectController.AllowInstantiation) {
                throw new Exception("Instantiation is not allowed!");
            }
        }
    }
}
