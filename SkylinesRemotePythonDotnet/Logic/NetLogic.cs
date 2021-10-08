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

            Segment shell = ObjectStorage.Instance.Segments.CreateShell();
            ClientHandler.Instance.RemoteCall(Contracts.CreateSegment, msg, (ret, error) => {
                if (error != null) {
                    shell.AssignData(null, error);
                    return null;
                }
                NetSegmentData data = (NetSegmentData)ret;
                ObjectStorage.Instance.Segments.AddDataToDictionary(data);
                shell.AssignData(data);
                return null;
            });
            return shell;
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

            PythonList<Segment> shell = new PythonList<Segment>();
            var handle = ClientHandler.Instance.RemoteCall(Contracts.CreateSegments, msg, (ret, error) => {
                if(error != null) {
                    shell.AssignData(null, error);
                    return null;
                }
                NetSegmentListMessage raw = ret as NetSegmentListMessage;
                shell.AssignData(raw.list.Select((item) => ObjectStorage.Instance.Segments.SaveData(item)).ToList());
                return null;
            });
            shell.CacheFunc = () => { ClientHandler.Instance.WaitOnHandle(handle); };
            return shell;
        }
    }
}
