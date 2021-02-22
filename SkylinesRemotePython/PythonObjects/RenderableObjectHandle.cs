using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    [Doc("Handle for deleting shapes rendered on the map")]
    public class RenderableObjectHandle : ApiRefObject, ISimpleToString
    {
        [Doc("Handle ID")]
        public int id { get; private set; }

        [Doc("Deletes the shape")]
        public void delete()
        {
            api.client.RemoteCall<Object>(Contracts.RemoveRenderedObject, id);
        }

        internal RenderableObjectHandle(int id, GameAPI api) : base(api)
        {
            this.id = id;
        }

        public override string ToString()
        {
            return PythonHelp.RuntimeToString(this);
        }

        public string SimpleToString()
        {
            return "id " + id;
        }
    }
}
