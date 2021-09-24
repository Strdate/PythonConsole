using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    [Doc("Free standing tree structure")]
    public class Tree : CitiesObject<TreeData>
    {
        public override string type => "tree";

        [Doc("Moves node to new position")]
        public void move(IPositionable pos) => MoveImpl(pos.position, null);

        public override void refresh()
        {
            ObjectStorage.Instance.Trees.RefreshInstance(this);
        }

        public Tree()
        {
            if (!CitiesObjectController.AllowInstantiation) {
                throw new Exception("Instantiation is not allowed!");
            }
        }
    }
}