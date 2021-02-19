using Microsoft.Scripting.Hosting;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesRemotePython.API
{
    public class PathBuilder
    {
        private object last_position;

        public Node last_node { get; private set; }

        public NetOptions options { get; set; }

        private GameAPI api;

        internal static PathBuilder BeginPath(GameAPI api, object obj, object options = null)
        {
            return new PathBuilder(api, obj, options);
        }

        private PathBuilder(GameAPI api, object startNode, object options = null)
        {
            if (!(startNode is Node) && !(startNode is Vector) && !(startNode is Point)) {
                throw new Exception("Segment startNode must be NetNode, Vector or Point - not " + startNode.GetType().Name);
            }
            if(options == null && startNode is Node) {
                options = ((Node)startNode).prefab_name;
            } else if (options == null) {
                throw new Exception("You must provide prefab name or NetOptions as the second param, if that connot be inferred from first param");
            }
            NetOptions parsedOptions = NetOptionsUtil.Ensure(options);
            this.api = api;
            last_position = startNode;
            this.options = parsedOptions;
        }

        public PathBuilder path_to(object endNode)
        {
            PathToImpl(last_position, endNode, options, null, null, null);
            return this;
        }

        public PathBuilder path_to(object endNode, Vector middle_pos)
        {
            PathToImpl(last_position, endNode, options, null, null, middle_pos);
            return this;
        }

        public PathBuilder path_to(object endNode, Vector start_dir, Vector end_dir)
        {
            PathToImpl(last_position, endNode, options, start_dir, end_dir, null);
            return this;
        }

        private void PathToImpl(object last_position, object endNode, object options, Vector start_dir, Vector end_dir, Vector middle_pos)
        {
            last_node = api._netLogic.CreateSegmentsImpl(last_position, endNode, options, start_dir, end_dir, middle_pos).Last().end_node;
            this.last_position = last_node;
        }
    }
}
