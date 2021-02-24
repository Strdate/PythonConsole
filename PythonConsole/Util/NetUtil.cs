using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace PythonConsole
{
    /* By Strad, 2019 */

    /* This is a little library which makes net stuff (working with segments/nodes) easier. Feel free to reuse it */

    public static class NetUtil
    {
        public const int DEFAULT_ELEVATTION_STEP = 3;

        public static bool ReleaseSegment(ushort id, bool tryReleaseNodes = false, bool suppressWarnings = false)
        {
            if (id > 0 && (NetManager.instance.m_segments.m_buffer[id].m_flags & NetSegment.Flags.Created) != NetSegment.Flags.None)
            {
                try
                {
                    ushort node1 = Segment(id).m_startNode;
                    ushort node2 = Segment(id).m_endNode;
                    NetManager.instance.ReleaseSegment(id, true);
                    if (tryReleaseNodes)
                    {
                        ReleaseNode(node1, true);
                        ReleaseNode(node2, true);
                    }
                    return true;
                }
                catch (Exception e)
                {
                    if (!suppressWarnings) Debug.LogWarning("Exception while releasing segment " + id + ": " + e);
                    return false;
                }
            }
            else
            {
                if (!suppressWarnings) Debug.LogWarning("Failed to release NetSegment " + id + ": Segment does not exist");
                return false;
            }
        }

        public static bool ReleaseNode(ushort id, bool suppressWarnings = false)
        {
            if ((Node(id).m_flags & NetNode.Flags.Created) == NetNode.Flags.None)
            {
                if (!suppressWarnings) Debug.LogWarning("Failed to release NetNode " + id + ": Not Created");
                return false;
            }

            if (Node(id).CountSegments() > 0)
            {
                if (!suppressWarnings) Debug.LogWarning("Failed to release NetNode " + id + ": Has segments");
                return false;
            }

            if (id > 0 && (NetManager.instance.m_nodes.m_buffer[id].m_flags & NetNode.Flags.Created) != NetNode.Flags.None)
            {
                NetManager.instance.ReleaseNode(id);
                return true;
            }
            else
            {
                if (!suppressWarnings) Debug.LogWarning("Failed to release NetNode " + id);
                return false;
            }
        }

        public static byte GetElevation(Vector3 position, NetAI net_ai)
        {
            if (!net_ai.IsUnderground() && !net_ai.IsOverground())
            {
                return 0; // on ground.
            }

            net_ai.GetElevationLimits(out int min, out int max);
            if (min == max)
            {
                return 0; // From NetTool.GetElevation()
            }

            float elevation = position.y - TerrainHeight(position);

#if DEBUG
            // tolerated error = +-1
            if (!(min * 12 - 1 <= elevation && elevation <= max * 12 + 1))
                Debug.LogWarning($"Elevation out of range expected {min * 12 - 1} <= {elevation} <={max * 12 + 1}");
#endif

            elevation = Mathf.Clamp(elevation, min * 12, max * 12); // 12 is from NetTool.GetElevation()
            elevation = Mathf.Abs(elevation);
            return (byte)Mathf.Clamp(elevation, 1, 255); // underground/overground road should not have 0 elevation.
        }

        public static float GetTerrainOffset(Vector3 position)
        {
            return position.y - TerrainHeight(position);
        }

        public static float GetTerrainIncludeWater(Vector3 position)
        {
            return Math.Max(Singleton<TerrainManager>.instance.WaterLevel(new Vector2(position.x, position.z)),TerrainHeight(position));
        }

        public static float TerrainHeight(Vector3 position)
        {
            return Singleton<TerrainManager>.instance.SampleDetailHeightSmooth(position);
        }

        public static ushort CreateNode(NetInfo info, Vector3 position)
        {
            var randomizer = Singleton<SimulationManager>.instance.m_randomizer;
            bool result = NetManager.instance.CreateNode(out ushort nodeId, ref randomizer, info, position,
                Singleton<SimulationManager>.instance.m_currentBuildIndex);

            if (!result)
                throw new Exception("Failed to create NetNode at " + position.ToString());

            NetManager.instance.m_nodes.m_buffer[nodeId].m_elevation = GetElevation(position, info.m_netAI);

            Singleton<SimulationManager>.instance.m_currentBuildIndex++;

            return nodeId;
        }

        public static ushort CreateSegment(ushort startNodeId, ushort endNodeId, Vector3 startDirection, Vector3 endDirection, NetInfo netInfo, bool invert = false, bool switchStartAndEnd = false, bool dispatchPlacementEffects = false)
        {
            var randomizer = Singleton<SimulationManager>.instance.m_randomizer;

            NetNode startNode = Node(startNodeId);
            NetNode endNode = Node(endNodeId);

            if ((startNode.m_flags & NetNode.Flags.Created) == NetNode.Flags.None || (endNode.m_flags & NetNode.Flags.Created) == NetNode.Flags.None)
                throw new Exception("Failed to create NetSegment: Invalid node(s)");

            var result = NetManager.instance.CreateSegment(out ushort newSegmentId, ref randomizer, netInfo, switchStartAndEnd ? endNodeId : startNodeId,
                 switchStartAndEnd ? startNodeId : endNodeId,
                 (switchStartAndEnd ? endDirection : startDirection), (switchStartAndEnd ? startDirection : endDirection), Singleton<SimulationManager>.instance.m_currentBuildIndex,
                         Singleton<SimulationManager>.instance.m_currentBuildIndex, invert);

            if (!result)
                throw new Exception("Failed to create NetSegment");

            Singleton<SimulationManager>.instance.m_currentBuildIndex++;

            if (dispatchPlacementEffects)
            {
                bool smoothStart = (startNode.m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
                bool smoothEnd = (endNode.m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
                NetSegment.CalculateMiddlePoints(startNode.m_position, startDirection, endNode.m_position, endDirection, smoothStart, smoothEnd, out Vector3 b, out Vector3 c);
                NetTool.DispatchPlacementEffect(startNode.m_position, b, c, endNode.m_position, netInfo.m_halfWidth, false);
            }

            return newSegmentId;
        }

        public static bool ExistsSegment(ushort segmentId)
        {
            return ExistsSegment(Manager.m_segments.m_buffer[segmentId]);
        }

        public static bool ExistsSegment(NetSegment segment)
        {
            return (segment.m_flags & NetSegment.Flags.Created) != NetSegment.Flags.None;
        }

        public static bool ExistsNode(ushort nodeId)
        {
            return ExistsNode(Manager.m_nodes.m_buffer[nodeId]);
        }

        public static bool ExistsNode(NetNode node)
        {
            return (node.m_flags & NetNode.Flags.Created) != NetNode.Flags.None;
        }

        public static ushort NetinfoToIndex(NetInfo netInfo)
        {
            return (ushort)Mathf.Clamp(netInfo.m_prefabDataIndex, 0, 65535);
        }

        public static NetInfo NetinfoFromIndex(ushort index)
        {
            return PrefabCollection<NetInfo>.GetPrefab(index);
        }

        /* From Elektrix */
        public static bool DoSegmentsIntersect(ushort segment1, ushort segment2, out float t1, out float t2)
        {
            // First segment data
            NetSegment s1 = Segment(segment1);
            Bezier3 bezier = default(Bezier3);

            // Second segment data
            NetSegment s2 = Segment(segment2);
            Bezier3 secondBezier = default(Bezier3);

            // Turn the segment data into a Bezier2 for easier calculations supported by the game
            bezier.a = Node(s1.m_startNode).m_position;
            bezier.d = Node(s1.m_endNode).m_position;

            bool smoothStart = (Singleton<NetManager>.instance.m_nodes.m_buffer[s1.m_startNode].m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
            bool smoothEnd = (Singleton<NetManager>.instance.m_nodes.m_buffer[s1.m_endNode].m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;

            NetSegment.CalculateMiddlePoints(bezier.a, s1.m_startDirection, bezier.d, s1.m_endDirection, smoothStart, smoothEnd, out bezier.b, out bezier.c);

            Bezier2 xz = Bezier2.XZ(bezier);

            // Second segment:
            secondBezier.a = Node(s2.m_startNode).m_position;
            secondBezier.d = Node(s2.m_endNode).m_position;

            smoothStart = (Singleton<NetManager>.instance.m_nodes.m_buffer[s2.m_startNode].m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;
            smoothEnd = (Singleton<NetManager>.instance.m_nodes.m_buffer[s2.m_endNode].m_flags & NetNode.Flags.Middle) != NetNode.Flags.None;

            NetSegment.CalculateMiddlePoints(secondBezier.a, s2.m_startDirection, secondBezier.d, s2.m_endDirection, smoothStart, smoothEnd, out secondBezier.b, out secondBezier.c);

            Bezier2 xz2 = Bezier2.XZ(secondBezier);

            return xz.Intersect(xz2, out t1, out t2, 5);
        }

        /// <summary>
        /// Returns first non-zero segment found
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static ushort GetNonzeroSegment(ushort nodeId, int index)
        {
            return GetNonzeroSegment(Node(nodeId), index);
        }

        public static List<ushort> GetSegmentsFromNode(ushort nodeId)
        {
            ref NetNode node = ref Node(nodeId);
            List<ushort> list = new List<ushort>();
            for (int i = 0; i < 8; i++) {
                ushort segment = node.GetSegment(i);
                if (segment != 0) {
                    list.Add(segment);
                }
            }
            return list;
        }

        public static ushort GetNonzeroSegment(NetNode node, int index)
        {
            for (int i = 0; i < 8; i++)
            {
                if (node.GetSegment(i) != 0)
                {
                    if (index == 0)
                    {
                        return node.GetSegment(i);
                    }
                    index--;
                }
            }
            return 0;
        }

        public static bool IsOneWay(NetInfo netInfo)
        {
            return netInfo.m_hasForwardVehicleLanes ^ netInfo.m_hasBackwardVehicleLanes;
        }

        public static IEnumerable<int> SegmentsFromMask(ulong[] segmentMask)
        {
            int num = segmentMask.Length;
            for (int i = 0; i < num; i++)
            {
                ulong num3 = segmentMask[i];
                if (num3 != 0UL)
                {
                    for (int j = 0; j < 64; j++)
                    {
                        if ((num3 & 1UL << j) != 0UL)
                        {
                            int num4 = i << 6 | j;
                            if (num4 != 0)
                            {
                                yield return num4;
                            }
                        }
                    }
                }
            }
        }

        public static UnityEngine.Vector3 ToUnityTerrain(this SkylinesPythonShared.API.Vector vect)
        {
            Vector3 unity = vect.ToUnity();
            return new UnityEngine.Vector3(unity.x, vect.is_height_defined ? unity.y : NetUtil.TerrainHeight(unity), unity.z);
        }

        /* As always */

        public static NetManager Manager
        {
            get { return Singleton<NetManager>.instance; }
        }
        public static ref NetNode Node(ushort id)
        {
            return ref Manager.m_nodes.m_buffer[id];
        }
        public static ref NetSegment Segment(ushort id)
        {
            return ref Manager.m_segments.m_buffer[id];
        }

        // Fastlist extension
        public static bool Contains<T>(this FastList<T> list, T item)
        {
            foreach (T item2 in list)
            {
                if (item2.Equals(item))
                    return true;
            }
            return false;
        }
    }
}
