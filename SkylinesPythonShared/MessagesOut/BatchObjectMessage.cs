using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public class BatchObjectMessage
    {
        public object[] array { get; set; }

        public uint lastVisitedIndex { get; set; }

        public bool endOfStream { get; set; }
    }
}
