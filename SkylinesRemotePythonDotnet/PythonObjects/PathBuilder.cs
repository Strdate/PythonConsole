using Microsoft.Scripting.Hosting;
using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesRemotePython.API
{
    [Doc("Abstract structure for building a row of linked segments")]
    public class PathBuilder
    {
        private IPositionable last_position;

        [Doc("Current end of the road")]
        public Node last_node { get; private set; }

        [Doc("Start of the road")]
        public Node first_node { get; private set; }

        [Doc("Returns list of the last batch of segments built with path_to function")]
        public IList<Segment> last_segments { get; private set; }

        [Doc("Returns list of all segments built with this PathBuilder")]
        public IList<Segment> segments { get; private set; } = new List<Segment>();

        [Doc("Road type, elevation etc.")]
        public NetOptions options { get; set; }

        private GameAPI api;

        internal static PathBuilder BeginPath(GameAPI api, IPositionable obj, object options = null)
        {
            return new PathBuilder(api, obj, options);
        }

        private PathBuilder(GameAPI api, IPositionable startNode, object options = null)
        {
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

        [Doc("Creates multiple straight segments to the given position")]
        public PathBuilder path_to(IPositionable endNode)
        {
            PathToImpl(last_position, endNode, options, null, null, null);
            return this;
        }

        [Doc("Creates multiple segments to the given position")]
        public PathBuilder path_to(IPositionable endNode, IPositionable middle_pos)
        {
            PathToImpl(last_position, endNode, options, null, null, middle_pos.position);
            return this;
        }

        [Doc("Creates multiple segments to the given position")]
        public PathBuilder path_to(IPositionable endNode, Vector start_dir, Vector end_dir)
        {
            PathToImpl(last_position, endNode, options, start_dir, end_dir, null);
            return this;
        }

        private void PathToImpl(IPositionable last_position, IPositionable endNode, object options, Vector start_dir, Vector end_dir, Vector middle_pos)
        {
            last_segments = api._netLogic.CreateSegmentsImpl(last_position, endNode, options, start_dir, end_dir, middle_pos);
            segments = segments.Concat(last_segments).ToList();
            first_node = first_node ?? last_segments.First().start_node;
            last_node = last_segments.Last().end_node;
            this.last_position = last_node;
        }

        public override string ToString()
        {
            return PythonHelp.RuntimeToString(this);
        }
    }
}
