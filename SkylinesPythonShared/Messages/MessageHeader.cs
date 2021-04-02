using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public class MessageHeader
    {
        public string version;
        public string messageType;
        public bool ignoreReturnValue;
        public object payload;
    }
}
