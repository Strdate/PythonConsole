using ColossalFramework;
using ColossalFramework.Math;
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

            ref NetNode startNode = ref NetUtil.Node(data.start_node_id);
            ref NetNode endNode = ref NetUtil.Node(data.end_node_id);

            GetSegmentVectors(data, ref startNode, ref endNode, out Vector3 startDir, out Vector3 endDir);

            return PrepareSegment( NetUtil.CreateSegment(data.start_node_id, data.end_node_id, startDir, endDir, info) );
        }

        public static NetSegmentListMessage CreateSegments(CreateSegmentMessage data)
        {
            NetInfo info = PrefabCollection<NetInfo>.FindLoaded(data.prefab_name);
            Util.Assert(info, "Prefab '" + data.prefab_name + "' not found");

            ref NetNode startNode = ref NetUtil.Node(data.start_node_id);
            ref NetNode endNode = ref NetUtil.Node(data.end_node_id);

            GetSegmentVectors(data, ref startNode, ref endNode, out Vector3 firstStartDir, out Vector3 lastEndDir);

            NetSegment.CalculateMiddlePoints(startNode.m_position, firstStartDir, endNode.m_position, lastEndDir, false, false, out Vector3 b, out Vector3 c);
            Bezier3 bezier = new Bezier3(startNode.m_position, b, c, endNode.m_position);
            Vector3 halfWay = bezier.Position(0.5f);
            float length = (new Vector2(halfWay.x, halfWay.z) - new Vector2(startNode.m_position.x, startNode.m_position.z)).magnitude
                + (new Vector2(halfWay.x, halfWay.z) - new Vector2(endNode.m_position.x, endNode.m_position.z)).magnitude;
            int numOfSegments = Mathf.Min(1000, Mathf.FloorToInt(length / 100f) + 1);

            List<NetSegmentMessage> segments = new List<NetSegmentMessage>();

            ushort prevNodeId = 0;
            Vector3 prevVector = firstStartDir;
            for (int i = 0; i < numOfSegments; i++) {
                float t1 = (float)(i+1) / (float)numOfSegments;
                //float height = Mathf.Lerp(startNode.m_position.y, endNode.m_position.y, t);
                
                //Vector3 startDir = VectorUtils.NormalizeXY(bezier.Tangent(t));

                ushort endNodeId;
                ushort startNodeId;
                if(i == 0) {
                    startNodeId = data.start_node_id;
                } else {
                    startNodeId = prevNodeId;
                }
                Vector3 startDir = prevVector;

                Vector3 endPos = bezier.Position(t1);
                Vector3 endDir;
                if (i + 1 < numOfSegments) {
                    endNodeId = NetUtil.CreateNode(info, endPos);
                    endDir = -VectorUtils.NormalizeXZ(VectorUtils.NormalizeXY(bezier.Tangent(t1)));
                } else {
                    endNodeId = data.end_node_id;
                    endDir = lastEndDir;
                }

                segments.Add( PrepareSegment( NetUtil.CreateSegment(startNodeId, endNodeId, startDir, endDir, info) ));

                prevNodeId = endNodeId;
                prevVector = -endDir;
            }

            return new NetSegmentListMessage() {
                list = segments
            };
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

        private static void GetSegmentVectors(CreateSegmentMessage data, ref NetNode startNode, ref NetNode endNode, out Vector3 startDir, out Vector3 endDir)
        {
            if (data.middle_pos == null && (data.start_dir == null || data.end_dir == null)) {
                startDir = (endNode.m_position - startNode.m_position).normalized;
                endDir = (startNode.m_position - endNode.m_position).normalized;
            } else if (data.middle_pos != null) {
                VectUtil.DirectionVectorsFromMiddlePos(startNode.m_position, endNode.m_position, data.middle_pos.ToUnity(), out startDir, out endDir);
            } else if (data.start_dir != null && data.end_dir != null) {
                startDir = data.start_dir.ToUnity().normalized;
                endDir = data.end_dir.ToUnity().normalized;
            } else {
                throw new Exception("Cannot calculate segment from provided vectors");
            }
        }

    }
}
