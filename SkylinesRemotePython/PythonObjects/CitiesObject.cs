using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API {
    public abstract class CitiesObject : ApiRefObject
    {
        public uint id { get; protected set; }

        public bool deleted { get; protected set; }

        public abstract void refresh();

        public virtual string type => "";

        internal CitiesObject(GameAPI api) : base(api)
        {

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
            CitiesObject other = (CitiesObject)obj;
            return this.type == other.type &&
                   id == other.id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(id, type);
        }

        public static bool operator ==(CitiesObject lhs, CitiesObject rhs)
        {
            if (System.Object.ReferenceEquals(lhs, null)) {
                if (System.Object.ReferenceEquals(rhs, null)) {
                    return true;
                }

                return false;
            }
            return lhs.id == rhs.id && lhs.type == rhs.type;
        }

        public static bool operator !=(CitiesObject lhs, CitiesObject rhs)
        {
            return !(lhs == rhs);
        }
    }
}
