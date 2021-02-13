using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PythonConsole
{
    public static class ManagersUtil
    {
        // ?? subbuildings
        public static ushort CreateBuilding(Vector3 position, float angle, BuildingInfo buildingInfo)
        {
            var randomizer = Singleton<SimulationManager>.instance.m_randomizer;
            bool result = BuildingManager.CreateBuilding(out ushort id, ref randomizer,
                buildingInfo, position, angle,
                0, SimulationManager.instance.m_currentBuildIndex);

            if (!result)
                throw new Exception("Failed to create Building at " + position.ToString());

            Singleton<SimulationManager>.instance.m_currentBuildIndex++;

            return id;
        }

        public static ushort CreateProp(Vector3 position, float angle, PropInfo propInfo, bool single)
        {
            var randomizer = Singleton<SimulationManager>.instance.m_randomizer;
            bool result = PropManager.CreateProp(out ushort id, ref randomizer,
                propInfo, position, angle, single);

            if (!result)
                throw new Exception("Failed to create PropInstance at " + position.ToString());

            return id;
        }

        public static uint CreateTree(Vector3 position, TreeInfo treeInfo, bool single)
        {
            var randomizer = Singleton<SimulationManager>.instance.m_randomizer;
            bool result = TreeManager.CreateTree(out uint id, ref randomizer,
                treeInfo, position, single);

            if (!result)
                throw new Exception("Failed to create PropInstance at " + position.ToString());

            return id;
        }


        // ?? subbuildings
        public static bool ReleaseBuilding(ushort building)
        {
            ref Building data = ref BuildingS(building);
            if (data.m_flags != Building.Flags.None && (data.m_flags & Building.Flags.Deleted) == Building.Flags.None) {
                BuildingManager.ReleaseBuilding(building);
                return true;
            } else {
                return false;
            }
        }

        public static bool ReleaseProp(ushort prop)
        {
            ref PropInstance data = ref Prop(prop);
            if (data.m_flags != 0) {
                PropManager.ReleaseProp(prop);
                return true;
            } else {
                return false;
            }
        }

        public static bool ReleaseTree(uint tree)
        {
            ref TreeInstance data = ref Tree(tree);
            if (data.m_flags != 0) {
                TreeManager.ReleaseTree(tree);
                return true;
            } else {
                return false;
            }
        }

        public static bool ExistsBuilding(ushort id)
        {
            ref Building data = ref BuildingS(id);
            return id != 0 && data.m_flags != Building.Flags.None && (data.m_flags & Building.Flags.Deleted) == Building.Flags.None;
        }

        public static ref Building BuildingS(ushort id)
        {
            return ref BuildingManager.m_buildings.m_buffer[id];
        }

        public static ref PropInstance Prop(ushort id)
        {
            return ref PropManager.m_props.m_buffer[id];
        }

        public static ref TreeInstance Tree(uint id)
        {
            return ref TreeManager.m_trees.m_buffer[id];
        }

        public static BuildingManager BuildingManager => BuildingManager.instance;

        public static PropManager PropManager => PropManager.instance;

        public static TreeManager TreeManager => TreeManager.instance;
    }
}
