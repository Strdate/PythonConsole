using SkylinesPythonShared;
using SkylinesRemotePython.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesRemotePython
{
    public class NetLogic
    {
        public static List<Segment> PrepareSegmentList(object obj, GameAPI api)
        {
            List<NetSegmentMessage> list = (List<NetSegmentMessage>)obj;
            return list.Select(e => new Segment(e, api)).ToList();
        }
    }
}
