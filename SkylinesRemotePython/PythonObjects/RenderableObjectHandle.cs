using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    public class RenderableObjectHandle : ApiRefObject
    {
        public int id { get; private set; }

        public void delete()
        {
            api.client.RemoteCall<Object>(Contracts.RemoveRenderedObject, id);
        }

        internal RenderableObjectHandle(int id, GameAPI api) : base(api)
        {
            this.id = id;
        }
    }
}
