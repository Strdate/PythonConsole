using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API {
    public abstract class ObjectAPI
    {
        protected GameAPI api;

        public uint id { get; protected set; }

        public bool is_deleted { get; protected set; }

        public abstract void refresh();

        public virtual string type => "";

        internal ObjectAPI(GameAPI api)
        {
            this.api = api;
        }

        public virtual bool delete()
        {
            if(is_deleted) {
                return true;
            }
            api.client.RemoteCall<bool>(Contracts.DeleteObject, new DeleteObjectMessage() {
                id = id,
                type = type
            });
            refresh();
            return is_deleted;
        }
    }
}
