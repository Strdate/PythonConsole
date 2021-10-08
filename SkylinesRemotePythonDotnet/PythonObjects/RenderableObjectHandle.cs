using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API
{
    [Doc("Handle for deleting shapes rendered on the map")]
    public class RenderableObjectHandle : ISimpleToString
    {
        [Doc("Handle ID")]
        public int id { get; private set; }

        [Doc("Deletes the shape")]
        public void delete()
        {
            ClientHandler.Instance.SynchronousCall<object>(Contracts.RemoveRenderedObject, id);
        }

        internal RenderableObjectHandle(int id)
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
