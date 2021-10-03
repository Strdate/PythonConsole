using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public class GetObjectsFromIndexMessage
    {
        public uint index { get; set; }

        public string type { get; set; }
    }
}
