using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    [Doc("Free standing tree structure")]
    public class Tree : CitiesObject<TreeData, Tree>
    {
        public override string type => "tree";

        private protected override CitiesObjectStorage<TreeData, Tree, uint> GetStorage()
        {
            return ObjectStorage.Instance.Trees;
        }

        [Doc("Moves node to new position")]
        public void move(IPositionable pos) => MoveImpl(pos.position, null);

        public override void refresh()
        {
            ObjectStorage.Instance.Trees.RefreshInstance(id);
        }

        public Tree()
        {
            if (!CitiesObjectController.AllowInstantiation) {
                throw new Exception("Instantiation is not allowed!");
            }
        }
    }
}