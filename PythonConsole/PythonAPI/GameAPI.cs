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
            return PrepareProp((ushort)((int)msg));
        }
        public static object GetNodeFromId(object msg)
        {
            return PrepareNode((ushort)((int)msg));
        }
        public static object CreateProp(object msg)
        {
            var data = (CreatePropMessage)msg;
			ushort id;
            PropInfo info = PrefabCollection<PropInfo>.FindLoaded(data.Type);
            Assert(info, "Prefab '" + data.Type + "' not found");
            Vector3 vect = RemoteFuncManager.ConvertVector(data.Position);
            Vector3 pos = new Vector3(vect.x, data.Position.is_height_defined ? vect.y : NetUtil.TerrainHeight(vect), vect.z);
            if (Singleton<PropManager>.instance.CreateProp(out id, ref Singleton<SimulationManager>.instance.m_randomizer, info, pos, (float)data.Angle, true))
			{
                return PrepareProp(id);
			}
            throw new Exception("Internal error - failed to create prop");
        }

        public static object CreateNode(object msg)
        {
            var data = (CreateNodeMessage)msg;
            NetInfo info = PrefabCollection<NetInfo>.FindLoaded(data.Type);
            Assert(info, "Prefab '" + data.Type + "' not found");
            Vector3 vect = RemoteFuncManager.ConvertVector(data.Position);
            Vector3 pos = new Vector3(vect.x, data.Position.is_height_defined ? vect.y : NetUtil.TerrainHeight(vect), vect.z);
            ushort id = NetUtil.CreateNode(info, pos);
            return PrepareNode(id);
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

        private static SkylinesPythonShared.NetNodeMessage PrepareNode(ushort id)
        {
            ref NetNode node = ref NetUtil.Node(id);
            return new NetNodeMessage()
            {
                id = id,
                position = RemoteFuncManager.ConvertVectorBack(node.m_position),
                prefab_name = node.Info.name
            };
        }

        private static PropMessage PrepareProp(ushort id)
        {
            PropInstance prop = Singleton<PropManager>.instance.m_props.m_buffer[id];
            return new PropMessage()
            {
                id = id,
                position = RemoteFuncManager.ConvertVectorBack(prop.Position),
                prefab_name = prop.Info.name,
                angle = prop.Angle
            };
        }

        private static void Assert(object obj, string ex)
        {
            if(obj == null)
            {
                throw new Exception(ex);
            }
        }
    }
}
