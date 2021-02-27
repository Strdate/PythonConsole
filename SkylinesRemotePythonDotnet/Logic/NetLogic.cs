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

        internal Segment CreateSegmentImpl(IPositionable startNode, IPositionable endNode, object type, Vector start_dir, Vector end_dir, IPositionable middle_pos)
        {
            NetOptions options = NetOptionsUtil.Ensure(type);
            CreateSegmentMessage msg = new CreateSegmentMessage() {
                start_node_id = startNode is Node ? (ushort)((Node)startNode).id : (ushort)0,
                end_node_id = endNode is Node ? (ushort)((Node)endNode).id : (ushort)0,
                start_postition = startNode is Node ? null : startNode.position,
                end_postition = endNode is Node ? null : endNode.position,
                net_options = options,
                start_dir = start_dir,
                end_dir = end_dir,
                control_point = middle_pos?.position
            };
            return new Segment(api.client.RemoteCall<NetSegmentMessage>(Contracts.CreateSegment, msg), api);
        }

        internal IList<Segment> CreateSegmentsImpl(IPositionable startNode, IPositionable endNode, object type, Vector start_dir, Vector end_dir, IPositionable middle_pos)
        {
            NetOptions options = NetOptionsUtil.Ensure(type);
            CreateSegmentMessage msg = new CreateSegmentMessage() {
                start_node_id = startNode is Node ? (ushort)((Node)startNode).id : (ushort)0,
                end_node_id = endNode is Node ? (ushort)((Node)endNode).id : (ushort)0,
                start_postition = startNode is Node ? null : startNode.position,
                end_postition = endNode is Node ? null : endNode.position,
                net_options = options,
                start_dir = start_dir,
                end_dir = end_dir,
                control_point = middle_pos?.position,
                auto_split = true
            };
            return NetLogic.PrepareSegmentList(api.client.RemoteCall<NetSegmentListMessage>(Contracts.CreateSegments, msg).list, api);
        }
    }
}
