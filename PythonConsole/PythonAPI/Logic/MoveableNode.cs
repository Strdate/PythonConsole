using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PythonConsole
{
    public static class Moveable
    {
        private static void CalculateSegmentDirections(ref NetSegment segment, ushort segmentID)
        {
            if (segment.m_flags != NetSegment.Flags.None) {
                segment.m_startDirection.y = 0;
                segment.m_endDirection.y = 0;

                segment.m_startDirection.Normalize();
                segment.m_endDirection.Normalize();

                segment.m_startDirection = segment.FindDirection(segmentID, segment.m_startNode);
                segment.m_endDirection = segment.FindDirection(segmentID, segment.m_endNode);
            }
        }

        private static void UpdateSegmentBlocks(ushort segment, ref NetSegment data)
        {
            /*NetSegment[] segmentBuffer = NetManager.instance.m_segments.m_buffer;
            if (segmentBuffer[segment].m_flags != NetSegment.Flags.None) {
                ReleaseSegmentBlock(segment, ref segmentBuffer[segment].m_blockStartLeft);
                ReleaseSegmentBlock(segment, ref segmentBuffer[segment].m_blockStartRight);
                ReleaseSegmentBlock(segment, ref segmentBuffer[segment].m_blockEndLeft);
                ReleaseSegmentBlock(segment, ref segmentBuffer[segment].m_blockEndRight);
            }

            segmentBuffer[segment].Info.m_netAI.CreateSegment(segment, ref segmentBuffer[segment]);*/
        }

        private static void ReleaseSegmentBlock(ushort segment, ref ushort segmentBlock)
        {
            if (segmentBlock != 0) {
                ZoneManager.instance.ReleaseBlock(segmentBlock);
                segmentBlock = 0;
            }
        }

        public static void MoveSegment(ushort segmentId, Vector3 location, float angle)
        {
            //if (!isValid) return;

            //TransformPosition = location;

            //if (!isVirtual()) {
            ushort segment = segmentId;//id.NetSegment;
                ushort startNode = NetUtil.Segment(segment).m_startNode;
                ushort endNode = NetUtil.Segment(segment).m_endNode;

                NetUtil.Segment(segment).m_startDirection = location - NetUtil.Node(startNode).m_position;
                NetUtil.Segment(segment).m_endDirection = location - NetUtil.Node(endNode).m_position;

                CalculateSegmentDirections(ref NetUtil.Segment(segment), segment);

            NetUtil.Manager.UpdateSegmentRenderer(segment, true);
                UpdateSegmentBlocks(segment, ref NetUtil.Segment(segment));

            NetUtil.Manager.UpdateNode(startNode);
            NetUtil.Manager.UpdateNode(endNode);
            //}
        }
        public static void MoveNode(ushort nodeId, Vector3 location, float angle)
        {
            /*if (!isValid) return;

            TransformAngle = angle;
            TransformPosition = location;

            if (!isVirtual()) {*/
                ushort node = nodeId;//id.NetNode;
                Vector3 oldPosition = NetUtil.Node(node).m_position;

                NetUtil.Manager.MoveNode(node, location);

                for (int i = 0; i < 8; i++) {
                    ushort segment = NetUtil.Node(node).GetSegment(i);
                    if (segment != 0 /*&& !Action.IsSegmentSelected(segment)*/) {
                        ushort startNode = NetUtil.Segment(segment).m_startNode;
                        ushort endNode = NetUtil.Segment(segment).m_endNode;

                        Vector3 oldVector;
                        if (node == endNode) {
                            oldVector = oldPosition - NetUtil.Node(startNode).m_position;
                        } else {
                            oldVector = NetUtil.Node(endNode).m_position - oldPosition;
                        }
                        oldVector.Normalize();

                        Vector3 startDirection = new Vector3(NetUtil.Segment(segment).m_startDirection.x, 0, NetUtil.Segment(segment).m_startDirection.z);
                        Vector3 endDirection = new Vector3(NetUtil.Segment(segment).m_endDirection.x, 0, NetUtil.Segment(segment).m_endDirection.z);

                        Quaternion startRotation = Quaternion.FromToRotation(oldVector, startDirection.normalized);
                        Quaternion endRotation = Quaternion.FromToRotation(-oldVector, endDirection.normalized);

                        Vector3 newVector = NetUtil.Node(endNode).m_position - NetUtil.Node(startNode).m_position;
                        newVector.Normalize();

                        NetUtil.Segment(segment).m_startDirection = startRotation * newVector;
                        NetUtil.Segment(segment).m_endDirection = endRotation * -newVector;

                        CalculateSegmentDirections(ref NetUtil.Segment(segment), segment);

                        NetUtil.Manager.UpdateSegmentRenderer(segment, true);
                        UpdateSegmentBlocks(segment, ref NetUtil.Segment(segment));

                        if (node != startNode) {
                            NetUtil.Manager.UpdateNode(startNode);
                        } else {
                            NetUtil.Manager.UpdateNode(endNode);
                        }
                    }
                }

                NetUtil.Manager.UpdateNode(node);
            //}
        }
    }
}
