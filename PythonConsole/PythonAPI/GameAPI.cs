using ColossalFramework;
using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PythonConsole
{
    public class GameAPI
    {
        public static object GetPropFromId(object msg)
        {
            return NetLogic.PrepareProp((ushort)((int)msg));
        }

        public static object GetTreeFromId(object msg)
        {
            return NetLogic.PrepareTree((uint)((long)msg));
        }
        public static object GetNodeFromId(object msg)
        {
            return NetLogic.PrepareNode((ushort)((int)msg));
        }

        public static object GetSegmentFromId(object msg)
        {
            return NetLogic.PrepareSegment((ushort)((int)msg));
        }
        public static object CreateProp(object msg)
        {
            var data = (CreatePropMessage)msg;
			ushort id;
            PropInfo info = PrefabCollection<PropInfo>.FindLoaded(data.Type);
            Util.Assert(info, "Prefab '" + data.Type + "' not found");
            Vector3 vect = data.Position.ToUnity();
            Vector3 pos = new Vector3(vect.x, data.Position.is_height_defined ? vect.y : NetUtil.TerrainHeight(vect), vect.z);
            if (Singleton<PropManager>.instance.CreateProp(out id, ref Singleton<SimulationManager>.instance.m_randomizer, info, pos, (float)data.Angle, true))
			{
                return NetLogic.PrepareProp(id);
			}
            throw new Exception("Internal error - failed to create prop");
        }

        public static object CreateTree(object msg)
        {
            var data = (CreateTreeMessage)msg;
            
            TreeInfo info = PrefabCollection<TreeInfo>.FindLoaded(data.prefab_name);
            Util.Assert(info, "Prefab '" + data.prefab_name + "' not found");
            Vector3 vect = data.Position.ToUnity();
            Vector3 pos = new Vector3(vect.x, data.Position.is_height_defined ? vect.y : NetUtil.TerrainHeight(vect), vect.z);
            uint id = ManagersUtil.CreateTree(pos, info, true);
            return NetLogic.PrepareTree(id);
        }

        public static object CreateNode(object msg)
        {
            var data = (CreateNodeMessage)msg;
            NetInfo info = PrefabCollection<NetInfo>.FindLoaded(data.Type);
            Util.Assert(info, "Prefab '" + data.Type + "' not found");
            Vector3 vect = data.Position.ToUnity();
            Vector3 pos = new Vector3(vect.x, data.Position.is_height_defined ? vect.y : NetUtil.TerrainHeight(vect), vect.z);
            ushort id = NetUtil.CreateNode(info, pos);
            return NetLogic.PrepareNode(id);
        }

        public static object CreateSegment(object msg)
        {
            return NetLogic.CreateSegment((CreateSegmentMessage)msg);
        }

        public static object CreateSegments(object msg)
        {
            return NetLogic.CreateSegments((CreateSegmentMessage)msg);
        }

        public static object ExistsPrefab(object msg)
        {
            string name = (string)msg;
            bool ret = false;
            if(PrefabCollection<NetInfo>.FindLoaded(name)
                || PrefabCollection<TreeInfo>.FindLoaded(name)
                || PrefabCollection<PropInfo>.FindLoaded(name)
                || PrefabCollection<BuildingInfo>.FindLoaded(name))
            {
                ret = true;
            }
            return ret;
        }
    }
}
