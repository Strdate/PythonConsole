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
        public static object GetObjectFromId(object msg)
        {
            GetObjectMessage data = (GetObjectMessage)msg;
            return GetObjectFromIdInternal(data.id, data.idString, data.type);
            
        }

        private static object GetObjectFromIdInternal(uint id, string idString, string type)
        {
            object ret;
            switch (type) {
                case "node":
                    ret = NetLogic.PrepareNode((ushort)(id));
                    break;
                case "segment":
                    ret = NetLogic.PrepareSegment((ushort)(id));
                    break;
                case "building":
                    ret = ManagersLogic.PrepareBuilding((ushort)(id));
                    break;
                case "prop":
                    ret = ManagersLogic.PrepareProp((ushort)(id));
                    break;
                case "tree":
                    ret = ManagersLogic.PrepareTree((uint)(id));
                    break;
                case "net prefab":
                    ret = NetLogic.PrepareNetInfo(idString);
                    break;
                default:
                    throw new Exception($"Unknown type '{type}'");
            }
            return ret;
        }

        public static object GetObjectsStartingFromIndex(object msg)
        {
            GetObjectsFromIndexMessage data = (GetObjectsFromIndexMessage)msg;
            switch (data.type) {
                case "node":
                    return NetLogic.PrepareNodesStartingFromIndex((ushort)data.index);
                case "segment":
                    return NetLogic.PrepareSegmentsStartingFromIndex((ushort)data.index);
                case "building":
                    return ManagersLogic.PrepareBuildingsStartingFromIndex((ushort)data.index);
                case "prop":
                    return ManagersLogic.PreparePropsStartingFromIndex((ushort)data.index);
                case "tree":
                    return ManagersLogic.PrepareTreesStartingFromIndex((uint)data.index);
                default:
                    throw new Exception($"Unknown type '{data.type}'");
            }
        }

        public static object CreateProp(object msg)
        {
            var data = (CreatePropMessage)msg;
            PropInfo info = PrefabCollection<PropInfo>.FindLoaded(data.Type);
            Util.Assert(info, "Prefab '" + data.Type + "' not found");
            return ManagersLogic.PrepareProp( ManagersUtil.CreateProp(data.Position.ToUnityTerrain(), (float)data.Angle, info, true) );
        }

        public static object GetSegmentsForNodeId(object msg)
        {
            return NetUtil.GetSegmentsFromNode((ushort)(uint)msg).Select((seg) => NetLogic.PrepareSegment(seg)).ToList();
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
        
        public static object MoveObject(object msg)
        {
            return ManagersLogic.Move((MoveMessage)msg);
        }

        public static object DeleteObject(object msg)
        {
            DeleteObjectMessage data = (DeleteObjectMessage)msg;
            switch(data.type) {
                case "node":
                    NetUtil.ReleaseNode((ushort)data.id);
                    break;
                case "segment":
                    NetUtil.ReleaseSegment((ushort)data.id, !data.keep_nodes);
                    break;
                case "building":
                    ManagersUtil.ReleaseBuilding((ushort)data.id);
                    break;
                case "prop":
                    ManagersUtil.ReleaseProp((ushort)data.id);
                    break;
                case "tree":
                    ManagersUtil.ReleaseTree(data.id);
                    break;
                default:
                    throw new Exception("Unknown type '" + data.type + "'");
            }
            return GetObjectFromIdInternal(data.id, null, data.type);
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

        public static object GetWaterLevel(object msg)
        {
            return NetUtil.GetTerrainIncludeWater(((Vector)msg).ToUnity());
        }

        public static object RenderVector(object msg)
        {
            return PythonConsole.Instance.RenderManager.AddObj(new RenderableVector((RenderVectorMessage)msg));
        }

        public static object RenderCircle(object msg)
        {
            return PythonConsole.Instance.RenderManager.AddObj(new RenderableCircle((RenderCircleMessage)msg));
        }

        public static object RemoveRenderedObject(object msg)
        {
            PythonConsole.Instance.RenderManager.RemoveObj((int)msg);
            return null;
        }

        public static object GetNaturalResourceCells(object msg)
        {
            var cells = Singleton<NaturalResourceManager>.instance.m_naturalResources;
            var mapped = new NaturalResourceCellBase[cells.Length];
            for (int i = 0; i < cells.Length; i++) {
                mapped[i] = ManagersLogic.ConvertResourceCell(i);
                /*mapped[i].ore = cells[i].m_ore;
                mapped[i].oil = cells[i].m_oil;
                mapped[i].forest = cells[i].m_forest;
                mapped[i].fertility = cells[i].m_fertility;
                mapped[i].pollution = cells[i].m_pollution;
                mapped[i].water = cells[i].m_water;*/
            }
            return mapped;
        }

        public static object GetNaturalResourceCellSingle(object msg)
        {
            return ManagersLogic.ConvertResourceCell((int)msg);
        }

        public static object SetNaturalResource(object msg)
        {
            ManagersLogic.SetNaturalResource((SetNaturalResourceMessage)msg);
            return null;
        }
    }
}
