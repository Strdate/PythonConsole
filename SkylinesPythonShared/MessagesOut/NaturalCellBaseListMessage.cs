using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public class NaturalCellBaseListMessage
    {
        public List<NaturalResourceCellBase> cells { get; set; }
    }
}
