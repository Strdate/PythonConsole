using SkylinesPythonShared;
using SkylinesRemotePython.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkylinesRemotePython
{
    public class CachedObjects
    {
        private ClientHandler _client;

        [ThreadStatic]
        public static CachedObjects Instance;
        public CachedObjects(ClientHandler client)
        {
            _client = client;
            Instance = this;

            NaturalResources = new NaturalResourcesManager(client);
        }

        public NaturalResourcesManager NaturalResources;
    }
}
