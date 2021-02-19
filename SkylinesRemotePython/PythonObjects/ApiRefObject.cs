using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    public class ApiRefObject
    {
        protected GameAPI api;

        protected ApiRefObject(GameAPI api)
        {
            this.api = api;
        }
    }
}
