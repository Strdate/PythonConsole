using SkylinesPythonShared;
using SkylinesPythonShared.API;
using SkylinesRemotePython.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesRemotePython
{
    public class NetLogic
    {
        private GameAPI api;

        public NetLogic(GameAPI api)
        {
            this.api = api;
        }

        public static List<Segment> PrepareSegmentList(object obj, GameAPI api)
        {
            List<NetSegmentMessage> list = (List<NetSegmentMessage>)obj;
            return list.Select(e => new Segment(e, api)).ToList();
        }

        internal Segment CreateSegmentImpl(object startNode, object endNode, object type, Vector start_dir, Vector end_dir, Vector middle_pos)
        {
            if (!(startNode is NetNode) && !(startNode is Vector) && !(startNode is Point)) {
                throw new Exception("Segment startNode must be NetNode, Vector or Point - not " + startNode.GetType().Name);
            }
            if (!(endNode is NetNode) && !(endNode is Vector) && !(endNode is Point)) {
                throw new Exception("Segment endNode must be NetNode, Vector or Point - not " + endNode.GetType().Name);
            }
            if (!(type is string) && !(type is NetOptions)) {
                throw new Exception("Segment type must be prefab name or NetOptions object, not " + type.GetType().Name);
            }
            NetOptions options = type as NetOptions ?? new NetOptions((string)type);
            CreateSegmentMessage msg = new CreateSegmentMessage() {
                start_node_id = startNode is NetNode ? (ushort)((NetNode)startNode).id : (ushort)0,
                end_node_id = endNode is NetNode ? (ushort)((NetNode)endNode).id : (ushort)0,
                start_postition = startNode is Vector ? (Vector)startNode : (startNode is Point ? ((Point)startNode).position : null),
                end_postition = endNode is Vector ? (Vector)endNode : (endNode is Point ? ((Point)endNode).position : null),
                net_options = options,
                start_dir = start_dir,
                end_dir = end_dir,
                control_point = middle_pos
            };
            return new Segment(api.client.RemoteCall<NetSegmentMessage>(Contracts.CreateSegment, msg), api);
        }

        internal IList<Segment> CreateSegmentsImpl(object startNode, object endNode, object type, Vector start_dir, Vector end_dir, Vector middle_pos)
        {
            if (!(startNode is NetNode) && !(startNode is Vector) && !(startNode is Point)) {
                throw new Exception("Segment startNode must be NetNode, Vector or Point - not " + startNode.GetType().Name);
            }
            if (!(endNode is NetNode) && !(endNode is Vector) && !(endNode is Point)) {
                throw new Exception("Segment endNode must be NetNode, Vector or Point - not " + endNode.GetType().Name);
            }
            if (!(type is string) && !(type is NetOptions)) {
                throw new Exception("Segment type must be prefab name or NetOptions object, not " + type.GetType().Name);
            }
            NetOptions options = type as NetOptions ?? new NetOptions((string)type);
            CreateSegmentMessage msg = new CreateSegmentMessage() {
                start_node_id = startNode is NetNode ? (ushort)((NetNode)startNode).id : (ushort)0,
                end_node_id = endNode is NetNode ? (ushort)((NetNode)endNode).id : (ushort)0,
                start_postition = startNode is Vector ? (Vector)startNode : null,
                end_postition = endNode is Vector ? (Vector)endNode : null,
                net_options = options,
                start_dir = start_dir,
                end_dir = end_dir,
                control_point = middle_pos,
                auto_split = true
            };
            return NetLogic.PrepareSegmentList(api.client.RemoteCall<NetSegmentListMessage>(Contracts.CreateSegments, msg).list, api);
        }
    }
}
