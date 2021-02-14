using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API {
    public class ObjectAPI
    {
        protected GameAPI api;

        public uint id { get; protected set; }

        public virtual string type => "";

        internal ObjectAPI(GameAPI api)
        {
            this.api = api;
        }

        public virtual bool delete(bool param = false)
        {
            return api.client.RemoteCall<bool>(Contracts.DeleteObject, new DeleteObjectMessage() {
                id = id,
                type = type
            });
        }
    }
}
