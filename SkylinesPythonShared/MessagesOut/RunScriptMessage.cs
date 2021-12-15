using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public class RunScriptMessage
    {
        public string script { get; set; }

        public List<object> clipboard { get; set; }

        public List<string> searchPaths { get; set; }
    }
}
