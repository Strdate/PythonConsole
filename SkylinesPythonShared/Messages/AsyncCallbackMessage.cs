using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared.Messages
{
    [Serializable]
    public class AsyncCallbackMessage
    {
        public long callbackObjectKey;
        public object payload;
    }
}
