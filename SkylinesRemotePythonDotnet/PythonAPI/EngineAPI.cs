using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkylinesRemotePython.API
{
    [Doc("Contains a set of functions to control the python engine")]
    public class EngineAPI
    {
        internal ClientHandler client;

        internal EngineAPI(ClientHandler client)
        {
            this.client = client;
        }

        [Doc("If enabled, performance of some methods will be significantly improved. These methods include: " +
            "create_prop, create_tree, and other create_* methods, draw_circle, draw_vector and possibly more. In return, these methods will always return either null or default object " +
            "marked as deleted. Exceptions will be ignored and warning will be printed in the output. Usage case is eg. planting trees in bulk")]
        public bool async_mode {
            get => client.AsynchronousMode;
            set => client.AsynchronousMode = value;
        }
    }
}
