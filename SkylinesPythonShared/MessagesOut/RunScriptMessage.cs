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

        public InstanceData[] clipboard { get; set; }

        public string[] searchPaths { get; set; }
    }
}
