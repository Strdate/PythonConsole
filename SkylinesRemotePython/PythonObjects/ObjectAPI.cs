using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API {
    public abstract class ObjectAPI
    {
        protected GameAPI api;

        public uint id { get; protected set; }

        public bool deleted { get; protected set; }

        public abstract void refresh();

        public virtual string type => "";

        internal ObjectAPI(GameAPI api)
        {
            this.api = api;
        }

        public virtual bool delete()
        {
            if(deleted) {
                return true;
            }
            api.client.RemoteCall<bool>(Contracts.DeleteObject, new DeleteObjectMessage() {
                id = id,
                type = type
            });
            refresh();
            return deleted;
        }

        public override bool Equals(object obj)
        {
            ObjectAPI other = (ObjectAPI)obj;
            return this.type == other.type &&
                   id == other.id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(id, type);
        }

        public static bool operator ==(ObjectAPI lhs, ObjectAPI rhs)
        {
            if (System.Object.ReferenceEquals(lhs, null)) {
                if (System.Object.ReferenceEquals(rhs, null)) {
                    return true;
                }

                return false;
            }
            return lhs.id == rhs.id && lhs.type == rhs.type;
        }

        public static bool operator !=(ObjectAPI lhs, ObjectAPI rhs)
        {
            return !(lhs == rhs);
        }
    }
}
