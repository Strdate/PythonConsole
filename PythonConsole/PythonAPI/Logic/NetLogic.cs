using ColossalFramework;
using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PythonConsole
{
    public static class NetLogic
    {
        public static NetSegmentMessage CreateSegment(CreateSegmentMessage data)
        {
            NetInfo info = PrefabCollection<NetInfo>.FindLoaded(data.prefab_name);
            Util.Assert(info, "Prefab '" + data.prefab_name + "' not found");

            Vector3 startDir;
            Vector3 endDir;
            ref NetNode startNode = ref NetUtil.Node(data.start_node_id);
            ref NetNode endNode = ref NetUtil.Node(data.end_node_id);
            if (data.middle_pos == null && (data.start_dir == null || data.end_dir == null)) {
                startDir = (endNode.m_position - startNode.m_position).normalized;
                endDir = (startNode.m_position - endNode.m_position).normalized;
            } else if (data.middle_pos != null) {
                VectUtil.DirectionVectorsFromMiddlePos(startNode.m_position, endNode.m_position, data.middle_pos.ToUnity(), out startDir, out endDir);
            } else if (data.start_dir != null && data.end_dir != null) {
                startDir = data.start_dir.ToUnity();
                endDir = data.end_dir.ToUnity();
            } else {
                throw new Exception("Cannot create segment - not enough vectors provided");
            }

            return PrepareSegment( NetUtil.CreateSegment(data.start_node_id, data.end_node_id, startDir, endDir, info) );
        }

        public static SkylinesPythonShared.NetNodeMessage PrepareNode(ushort id)
        {
            ref NetNode node = ref NetUtil.Node(id);
            return new NetNodeMessage() {
                id = id,
                position = node.m_position.FromUnity(),
                prefab_name = node.Info.name
            };
        }

        public static SkylinesPythonShared.NetSegmentMessage PrepareSegment(ushort id)
        {
            ref NetSegment segment = ref NetUtil.Segment(id);
            return new NetSegmentMessage() {
                id = id,
                prefab_name = segment.Info.name,
                start_node_id = segment.m_startNode,
                end_node_id = segment.m_endNode
            };
        }

        public static PropMessage PrepareProp(ushort id)
        {
            PropInstance prop = Singleton<PropManager>.instance.m_props.m_buffer[id];
            return new PropMessage() {
                id = id,
                position = prop.Position.FromUnity(),
                prefab_name = prop.Info.name,
                angle = prop.Angle
            };
        }

    }
}
