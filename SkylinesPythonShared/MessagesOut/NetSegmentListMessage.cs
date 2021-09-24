using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public class NetSegmentListMessage
    {
        public List<NetSegmentData> list { get; set; }
    }
}
