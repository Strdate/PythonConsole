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
        public static uint CreateTree(Vector3 position, TreeInfo treeInfo, bool single)
        {
            var randomizer = Singleton<SimulationManager>.instance.m_randomizer;
            bool result = TreeManager.CreateTree(out uint id, ref randomizer,
                treeInfo, position, single);

            if (!result)
                throw new Exception("Failed to create TreeInstance at " + position.ToString());

            return id;
        }

        public static ref TreeInstance Tree(uint id)
        {
            return ref TreeManager.m_trees.m_buffer[id];
        }

        public static TreeManager TreeManager => TreeManager.instance;
    }
}
