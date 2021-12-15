using ColossalFramework;
using ColossalFramework.Math;
using PythonConsole.MoveIt;
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
        public static NetSegmentData CreateSegment(CreateSegmentMessage data)
        {
            ParseNetOptions(data.net_options, out NetInfo info, out bool invert, out TerrainMode mode);

            ushort startNodeId = data.start_node_id;
            ushort endNodeId = data.end_node_id;
            ref NetNode startNode = ref EnsureNode(ref startNodeId, data.start_postition, info, mode);
            ref NetNode endNode = ref EnsureNode(ref endNodeId, data.end_postition, info, mode);

            GetSegmentVectors(data, ref startNode, ref endNode, out Vector3 startDir, out Vector3 endDir);

            return PrepareSegment( NetUtil.CreateSegment(startNodeId, endNodeId, startDir, endDir, info, invert) );
        }

        public static NetSegmentListMessage CreateSegments(CreateSegmentMessage data)
        {
            ParseNetOptions(data.net_options, out NetInfo info, out bool invert, out TerrainMode mode);

            ushort startNodeId = data.start_node_id;
            ushort endNodeId = data.end_node_id;
            ref NetNode startNode = ref EnsureNode(ref startNodeId, data.start_postition, info, mode);
            ref NetNode endNode = ref EnsureNode(ref endNodeId, data.end_postition, info, mode);

            GetSegmentVectors(data, ref startNode, ref endNode, out Vector3 firstStartDir, out Vector3 lastEndDir);

            NetSegment.CalculateMiddlePoints(startNode.m_position, firstStartDir, endNode.m_position, lastEndDir, false, false, out Vector3 b, out Vector3 c);
            Bezier3 bezier = new Bezier3(startNode.m_position, b, c, endNode.m_position);
            Vector3 halfWay = bezier.Position(0.5f);
            float length = (new Vector2(halfWay.x, halfWay.z) - new Vector2(startNode.m_position.x, startNode.m_position.z)).magnitude
                + (new Vector2(halfWay.x, halfWay.z) - new Vector2(endNode.m_position.x, endNode.m_position.z)).magnitude;
            int numOfSegments = Mathf.Min(1000, Mathf.FloorToInt(length / (float)data.net_options.node_spacing) + 1);

            List<NetSegmentData> segments = new List<NetSegmentData>();
            bool straight = NetSegment.IsStraight(startNode.m_position, firstStartDir, endNode.m_position, lastEndDir);

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
                Vector3 startDir = straight ? firstStartDir : prevVector;

                // Maybe startElevation.m_elevation can be used instead, but it is not reliable
                float startElevation = NetUtil.GetTerrainOffset(startNode.m_position);
                float endElevation = NetUtil.GetTerrainOffset(endNode.m_position);

                Vector3 endPos = straight ? LerpPosition(startNode.m_position, endNode.m_position, t1, info) : bezier.Position(t1);
                Vector3 endDir;
                if (i + 1 < numOfSegments) {
                    if (mode == TerrainMode.Follow) {
                        endPos.y = NetUtil.GetTerrainIncludeWater(endPos);
                    }
                    if (mode == TerrainMode.Lerp) {
                        endPos.y = NetUtil.GetTerrainIncludeWater(endPos) + Mathf.Lerp(startElevation, endElevation, t1);
                    }
                    curEndNodeId = NetUtil.CreateNode(info, endPos);
                    endDir = straight ? lastEndDir : -VectorUtils.NormalizeXZ(VectorUtils.NormalizeXY(bezier.Tangent(t1)));
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

        private static ref NetNode EnsureNode(ref ushort id, Vector position, NetInfo info, TerrainMode terrainMode)
        {
            if(id == 0) {
                if(!position.is_height_defined && terrainMode == TerrainMode.Lerp) {
                    throw new Exception("'Lerp' follow terrain option is not supported if any of the nodes has undefined elevation");
                }
                Vector3 vect = position.ToUnity();
                Vector3 pos = new Vector3(vect.x, position.is_height_defined && terrainMode != TerrainMode.Follow ? vect.y : NetUtil.TerrainHeight(vect), vect.z);
                id = NetUtil.CreateNode(info, pos);
            }
            return ref NetUtil.Node(id);
        }

        public static SkylinesPythonShared.NetNodeData PrepareNode(ushort id)
        {
            if (!NetUtil.ExistsNode(id)) {
                return new NetNodeData();
            }
            ref NetNode node = ref NetUtil.Node(id);
            return new NetNodeData() {
                id = id,
                position = node.m_position.FromUnity(),
                prefab_name = node.Info.name,
                terrain_offset = node.m_position.y - NetUtil.GetTerrainIncludeWater(node.m_position),
                building_id = node.m_building,
                seg_count = node.CountSegments(),
                exists = true
            };
        }

        public static SkylinesPythonShared.NetSegmentData PrepareSegment(ushort id)
        {
            if (!NetUtil.ExistsSegment(id)) {
                return new NetSegmentData();
            }
            ref NetSegment segment = ref NetUtil.Segment(id);
            ref NetNode startNode = ref NetUtil.Node(segment.m_startNode);
            ref NetNode endNode = ref NetUtil.Node(segment.m_endNode);

            bool smoothStart = (startNode.m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
            bool smoothEnd = (endNode.m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;

            NetSegment.CalculateMiddlePoints(startNode.m_position, segment.m_startDirection, endNode.m_position, segment.m_endDirection, smoothStart, smoothEnd, out Vector3 b, out Vector3 c);
            Bezier bezier = new Bezier3(startNode.m_position, b, c, endNode.m_position).FromUnity();
            return new NetSegmentData() {
                id = id,
                prefab_name = segment.Info.name,
                start_node_id = segment.m_startNode,
                end_node_id = segment.m_endNode,
                start_dir = segment.m_startDirection.FromUnity(),
                end_dir = segment.m_endDirection.FromUnity(),
                length = segment.m_averageLength,
                position = segment.m_middlePosition.FromUnity(),
                bezier = bezier,
                is_straight = segment.IsStraight(),
                exists = true
            };
        }

        public static BatchObjectMessage PrepareNodesStartingFromIndex(ushort id)
        {
            var buffer = NetUtil.Manager.m_nodes.m_buffer;
            var resultArray = new List<object>(500);
            int resultArrayIndex = 0;
            bool endOfStream = true;
            ushort i;
            for (i = id; i < buffer.Length; i++) {
                if (NetUtil.ExistsNode(i)) {
                    resultArray.Add(PrepareNode(i));
                    resultArrayIndex++;
                    if (resultArrayIndex == 500) {
                        endOfStream = false;
                        break;
                    }
                }
                if (i == ushort.MaxValue) {
                    break;
                }
            }
            return new BatchObjectMessage() {
                array = resultArray,
                endOfStream = endOfStream,
                lastVisitedIndex = i
            };
        }

        public static BatchObjectMessage PrepareSegmentsStartingFromIndex(ushort id)
        {
            var buffer = NetUtil.Manager.m_segments.m_buffer;
            var resultArray = new List<object>(500);
            int resultArrayIndex = 0;
            bool endOfStream = true;
            ushort i;
            for (i = id; i < buffer.Length; i++) {
                if (NetUtil.ExistsSegment(i)) {
                    resultArray.Add(PrepareSegment(i));
                    resultArrayIndex++;
                    if (resultArrayIndex == 500) {
                        endOfStream = false;
                        break;
                    }
                }
                if (i == ushort.MaxValue) {
                    break;
                }
            }
            return new BatchObjectMessage() {
                array = resultArray,
                endOfStream = endOfStream,
                lastVisitedIndex = i
            };
        }

        public static NetPrefabData PrepareNetInfo(string name)
        {
            NetInfo info = PrefabCollection<NetInfo>.FindLoaded(name);
            return new NetPrefabData() {
                id = info.name,
                width = info.m_halfWidth * 2,
                is_overground = info.m_netAI.IsOverground(),
                is_underground = info.m_netAI.IsUnderground(),
                fw_vehicle_lane_count = info.m_forwardVehicleLaneCount,
                bw_vehicle_lane_count = info.m_backwardVehicleLaneCount
            };
        }

        private static void ParseNetOptions(NetOptions options, out NetInfo info, out bool invert, out TerrainMode mode)
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
                    throw new Exception("'" + elevationMode + "' is not valid elevation mode. Allowed values: default, ground, elevated, bridge, tunnel, slope");
            }

            switch(options.follow_terrain.ToLower()) {
                case "true":
                    mode = TerrainMode.Follow;
                    break;
                case "false":
                    mode = TerrainMode.DontFollow;
                    break;
                case "auto_offset":
                    mode = TerrainMode.Lerp;
                    break;
                default:
                    throw new Exception("'" + options.follow_terrain + "' is not valid follow terrain option. Allowed values: true, false, auto_offset");
            }

            invert = options.invert;
        }

        private static void GetSegmentVectors(CreateSegmentMessage data, ref NetNode startNode, ref NetNode endNode, out Vector3 startDir, out Vector3 endDir)
        {
            if (data.control_point == null && (data.start_dir == null || data.end_dir == null)) {
                startDir = (endNode.m_position - startNode.m_position).normalized;
                endDir = (startNode.m_position - endNode.m_position).normalized;
            } else if (data.control_point != null) {
                VectUtil.DirectionVectorsFromMiddlePos(startNode.m_position, endNode.m_position, data.control_point.ToUnity(), out startDir, out endDir);
            } else if (data.start_dir != null && data.end_dir != null) {
                startDir = data.start_dir.ToUnity();
                endDir = data.end_dir.ToUnity();
            } else {
                throw new Exception("Cannot calculate segment from provided vectors");
            }
        }

        private static Vector3 LerpPosition(Vector3 refPos1, Vector3 refPos2, float t, NetInfo info)
        {
            float snap = info.m_netAI.GetLengthSnap();
            if (snap != 0f) {
                Vector2 vector = new Vector2(refPos2.x - refPos1.x, refPos2.z - refPos1.z);
                float magnitude = vector.magnitude;
                if (magnitude != 0f) {
                    t = Mathf.Round(t * magnitude / snap + 0.01f) * (snap / magnitude);
                }
            }
            return Vector3.Lerp(refPos1, refPos2, t);
        }

        private enum TerrainMode
        {
            Follow,
            DontFollow,
            Lerp
        }
    }
}
