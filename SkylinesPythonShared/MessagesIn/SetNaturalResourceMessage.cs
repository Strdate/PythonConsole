using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public class SetNaturalResourceMessage
    {
        public int cell_id { get; set; }

        public byte value { get; set; }

        public string type { get; set; }
    }
}
