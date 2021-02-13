using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API {
    public class ObjectAPI
    {
        protected GameAPI api;

        internal ObjectAPI(GameAPI api)
        {
            this.api = api;
        }
    }
}
