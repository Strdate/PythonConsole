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
        public static void GetPropFromId(object msg, TcpClient client)
        {
            client.SendMessage(PrepareProp((ushort)((int)msg)), "s_ret_Prop");
        }
        public static void GetNodeFromId(object msg, TcpClient client)
        {
            client.SendMessage(PrepareNode((ushort)((int)msg)), "s_ret_NetNode");
        }
        public static void CreateProp(object msg, TcpClient client)
        {
            var data = (CreatePropMessage)msg;
			ushort id;
            PropInfo info = PrefabCollection<PropInfo>.FindLoaded(data.Type);
            Assert(info, "Prefab '" + data.Type + "' not found");
            Vector3 vect = HandleCall.ConvertVector(data.Position);
            Vector3 pos = new Vector3(vect.x, data.Position.is_height_defined ? vect.y : NetUtil.TerrainHeight(vect), vect.z);
            if (Singleton<PropManager>.instance.CreateProp(out id, ref Singleton<SimulationManager>.instance.m_randomizer, info, pos, (float)data.Angle, true))
			{
                client.SendMessage(PrepareProp(id), "s_ret_Prop");
                return;
			}
            throw new Exception("Internal error - failed to create prop");
        }

        public static void CreateNode(object msg, TcpClient client)
        {
            var data = (CreateNodeMessage)msg;
            NetInfo info = PrefabCollection<NetInfo>.FindLoaded(data.Type);
            Assert(info, "Prefab '" + data.Type + "' not found");
            Vector3 vect = HandleCall.ConvertVector(data.Position);
            Vector3 pos = new Vector3(vect.x, data.Position.is_height_defined ? vect.y : NetUtil.TerrainHeight(vect), vect.z);
            ushort id = NetUtil.CreateNode(info, pos);
            client.SendMessage(PrepareNode(id), "s_ret_NetNode");
        }

        public static void ExistsPrefab(object msg, TcpClient client)
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
            client.SendMessage(ret, "s_ret_bool");
        }

        private static SkylinesPythonShared.NetNodeMessage PrepareNode(ushort id)
        {
            ref NetNode node = ref NetUtil.Node(id);
            return new NetNodeMessage()
            {
                id = id,
                position = HandleCall.ConvertVectorBack(node.m_position),
                prefab_name = node.Info.name
            };
        }

        private static PropMessage PrepareProp(ushort id)
        {
            PropInstance prop = Singleton<PropManager>.instance.m_props.m_buffer[id];
            return new PropMessage()
            {
                id = id,
                position = HandleCall.ConvertVectorBack(prop.Position),
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
