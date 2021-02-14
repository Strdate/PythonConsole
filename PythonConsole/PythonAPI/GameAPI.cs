using ColossalFramework;
using SkylinesPythonShared;
using SkylinesPythonShared.API;
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
            return ManagersLogic.PrepareProp((ushort)((int)msg));
        }

        public static object GetTreeFromId(object msg)
        {
            return ManagersLogic.PrepareTree((uint)((long)msg));
        }

        public static object GetBuildingFromId(object msg)
        {
            return ManagersLogic.PrepareBuilding((ushort)((int)msg));
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
            PropInfo info = PrefabCollection<PropInfo>.FindLoaded(data.Type);
            Util.Assert(info, "Prefab '" + data.Type + "' not found");
            return ManagersLogic.PrepareProp( ManagersUtil.CreateProp(data.Position.ToUnityTerrain(), (float)data.Angle, info, true) );
        }

        public static object CreateTree(object msg)
        {
            var data = (CreateTreeMessage)msg;
            
            TreeInfo info = PrefabCollection<TreeInfo>.FindLoaded(data.prefab_name);
            Util.Assert(info, "Prefab '" + data.prefab_name + "' not found");
            uint id = ManagersUtil.CreateTree(data.Position.ToUnityTerrain(), info, true);
            return ManagersLogic.PrepareTree(id);
        }

        public static object CreateBuilding(object msg)
        {
            var data = (CreateBuildingMessage)msg;
            BuildingInfo info = PrefabCollection<BuildingInfo>.FindLoaded(data.Type);
            Util.Assert(info, "Prefab '" + data.Type + "' not found");
            return ManagersLogic.PrepareBuilding(ManagersUtil.CreateBuilding(data.Position.ToUnityTerrain(), (float)data.Angle, info));
        }

        public static object CreateNode(object msg)
        {
            var data = (CreateNodeMessage)msg;
            NetInfo info = PrefabCollection<NetInfo>.FindLoaded(data.Type);
            Util.Assert(info, "Prefab '" + data.Type + "' not found");
            ushort id = NetUtil.CreateNode(info, data.Position.ToUnityTerrain());
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

        public static object DeleteObject(object msg)
        {
            DeleteObjectMessage data = (DeleteObjectMessage)msg;
            bool ret;
            switch(data.type) {
                case "node":
                    ret = NetUtil.ReleaseNode((ushort)data.id);
                    break;
                case "segment":
                    ret = NetUtil.ReleaseSegment((ushort)data.id, data.param);
                    break;
                case "building":
                    ret = ManagersUtil.ReleaseBuilding((ushort)data.id);
                    break;
                case "prop":
                    ret = ManagersUtil.ReleaseProp((ushort)data.id);
                    break;
                case "tree":
                    ret = ManagersUtil.ReleaseTree(data.id);
                    break;
                default:
                    throw new Exception("Unknown type '" + data.type + "'");
            }
            return ret;
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

        public static object GetTerrainHeight(object msg)
        {
            return NetUtil.TerrainHeight(((Vector)msg).ToUnity());
        }
    }
}
