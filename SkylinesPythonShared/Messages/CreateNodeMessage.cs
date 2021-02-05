using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    [Serializable]
    public class CreateNodeMessage
    {
        public Vector3 Position { get; set; }
    }
}
