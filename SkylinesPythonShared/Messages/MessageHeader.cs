using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public class MessageHeader
    {
        public string version { get; set; }
        public string messageType { get; set; }
        public string requestId { get; set; }
        public object payload { get; set; }
    }
}
