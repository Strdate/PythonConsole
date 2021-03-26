using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public struct NaturalResourceCellBase
    {
        public byte ore { get; set; }

        public byte oil { get; set; }

        public byte forest { get; set; }

        public byte fertility { get; set; }

        public byte pollution { get; set; }

        public byte water { get; set; }
    }
}
