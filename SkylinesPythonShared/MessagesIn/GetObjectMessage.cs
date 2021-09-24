using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public class GetObjectMessage
    {
        public uint id { get; set; }

        public string type { get; set; }
    }
}
