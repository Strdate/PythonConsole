using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PythonConsole
{
    public static class ManagersLogic
    {
        public static PropMessage PrepareProp(ushort id)
        {
            PropInstance prop = ManagersUtil.Prop(id);
            return new PropMessage() {
                id = id,
                position = prop.Position.FromUnity(),
                prefab_name = prop.Info.name,
                angle = prop.Angle
            };
        }

        public static TreeMessage PrepareTree(uint id)
        {
            TreeInstance tree = ManagersUtil.Tree(id);
            return new TreeMessage() {
                id = id,
                position = tree.Position.FromUnity(),
                prefab_name = tree.Info.name
            };
        }

        public static BuildingMessage PrepareBuilding(ushort id)
        {
            Building building = ManagersUtil.BuildingS(id);
            return new BuildingMessage() {
                id = id,
                position = building.m_position.FromUnity(),
                prefab_name = building.Info.name
            };
        }
    }
}
