using ColossalFramework;
using ColossalFramework.Math;
using SkylinesPythonShared;
using SkylinesPythonShared.API;
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
            ParseNetOptions(data.net_options, out NetInfo info, out bool invert);

            ushort startNodeId = data.start_node_id;
            ushort endNodeId = data.end_node_id;
            ref NetNode startNode = ref EnsureNode(ref startNodeId, data.start_postition, info);
            ref NetNode endNode = ref EnsureNode(ref endNodeId, data.end_postition, info);

            GetSegmentVectors(data, ref startNode, ref endNode, out Vector3 startDir, out Vector3 endDir);

            return PrepareSegment( NetUtil.CreateSegment(startNodeId, endNodeId, startDir, endDir, info, invert) );
        }

        public static NetSegmentListMessage CreateSegments(CreateSegmentMessage data)
        {
            ParseNetOptions(data.net_options, out NetInfo info, out bool invert);

            ushort startNodeId = data.start_node_id;
            ushort endNodeId = data.end_node_id;
            ref NetNode startNode = ref EnsureNode(ref startNodeId, data.start_postition, info);
            ref NetNode endNode = ref EnsureNode(ref endNodeId, data.end_postition, info);

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

                ushort curEndNodeId;
                ushort curStartNodeId;
                if(i == 0) {
                    curStartNodeId = startNodeId;
                } else {
                    curStartNodeId = prevNodeId;
                }
                Vector3 startDir = prevVector;

                Vector3 endPos = bezier.Position(t1);
                Vector3 endDir;
                if (i + 1 < numOfSegments) {
                    curEndNodeId = NetUtil.CreateNode(info, endPos);
                    endDir = -VectorUtils.NormalizeXZ(VectorUtils.NormalizeXY(bezier.Tangent(t1)));
                } else {
                    curEndNodeId = endNodeId;
                    endDir = lastEndDir;
                }

                segments.Add( PrepareSegment( NetUtil.CreateSegment(curStartNodeId, curEndNodeId, startDir, endDir, info, invert) ));

                prevNodeId = curEndNodeId;
                prevVector = -endDir;
            }

            return new NetSegmentListMessage() {
                list = segments
            };
        }

        private static ref NetNode EnsureNode(ref ushort id, Vector position, NetInfo info)
        {
            if(id == 0) {
                Vector3 vect = position.ToUnity();
                Vector3 pos = new Vector3(vect.x, position.is_height_defined ? vect.y : NetUtil.TerrainHeight(vect), vect.z);
                id = NetUtil.CreateNode(info, pos);
            }
            return ref NetUtil.Node(id);
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
                end_node_id = segment.m_endNode,
                length = segment.m_averageLength
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

        public static TreeMessage PrepareTree(uint id)
        {
            TreeInstance tree = ManagersUtil.Tree(id);
            return new TreeMessage() {
                id = id,
                position = tree.Position.FromUnity(),
                prefab_name = tree.Info.name
            };
        }

        private static void ParseNetOptions(NetOptions options, out NetInfo info, out bool invert)
        {
            info = PrefabCollection<NetInfo>.FindLoaded(options.prefab_name);
            Util.Assert(info, "Prefab '" + options.prefab_name + "' not found");

            RoadAI roadAI = info.m_netAI as RoadAI;
            string elevationMode = options.elevation_mode.ToLower();
            switch (elevationMode) {
                case "default":
                    break;
                case "ground":
                    info = roadAI?.m_info ?? info;
                    break;
                case "elevated":
                    info = roadAI?.m_elevatedInfo ?? info;
                    break;
                case "bridge":
                    info = roadAI?.m_bridgeInfo ?? info;
                    break;
                case "tunnel":
                    info = roadAI?.m_tunnelInfo ?? info;
                    break;
                case "slope":
                    info = roadAI?.m_tunnelInfo ?? info;
                    break;
                default:
                    throw new Exception("'" + elevationMode + "' is not valid elevation mode");
            }

            invert = options.invert;
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
