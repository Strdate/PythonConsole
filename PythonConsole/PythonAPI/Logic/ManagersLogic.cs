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
            if (!ManagersUtil.ExistsProp(id)) {
                return null;
            }
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
            if (!ManagersUtil.ExistsTree(id)) {
                return null;
            }
            TreeInstance tree = ManagersUtil.Tree(id);
            return new TreeMessage() {
                id = id,
                position = tree.Position.FromUnity(),
                prefab_name = tree.Info.name
            };
        }

        public static BuildingMessage PrepareBuilding(ushort id)
        {
            if (!ManagersUtil.ExistsBuilding(id)) {
                return null;
            }
            Building building = ManagersUtil.BuildingS(id);
            return new BuildingMessage() {
                id = id,
                position = building.m_position.FromUnity(),
                prefab_name = building.Info.name
            };
        }
    }
}
