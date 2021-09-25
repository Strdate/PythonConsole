using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API {
    public abstract class CitiesObject<T,U> : IPositionable, ISimpleToString 
        where T : InstanceData
        where U : CitiesObject<T,U>, new()
    {
        private uint _internalId { get; set; }

        public string initialization_error_msg { get; private set; }

        protected T _ {
            get {
                if(IsInitialized()) {
                    return GetStorage().GetData(_internalId);
                }
                while(true) {
                    ClientHandler.Instance.WaitForNextMessage();
                    if(IsInitialized()) {
                        return GetStorage().GetData(_internalId);
                    }
                }
            }
        }

        private bool IsInitialized()
        {
            if (_internalId > 0) {
                return true;
            }
            if (initialization_error_msg != null) {
                throw new Exception(initialization_error_msg);
            }
            return false;
        }

        private protected abstract ObjectInstanceStorage<T,U> GetStorage();

        [Doc("Game ID")]
        public uint id => _.id;

        [Doc("Object type (node, buidling, prop etc.)")]
        public abstract string type { get; }

        [Doc("Returns if object exists")]
        public bool deleted => _.deleted;


        [Doc("Object position. Can be assigned to to move the object")]
        public virtual Vector position { 
            get => _.position;
            set => MoveImpl(value, null);//throw new Exception($"Position of {type} cannot be changed");
        }

        [ToStringIgnore]
        [Doc("Object position. Can be assigned to to move the object")]
        public Vector pos {
            get => position;
            set => position = value;
        }

        [Doc("Node asset name (eg. 'Basic Road', 'Elementary School')")]
        public string prefab_name => _.prefab_name;

        [Doc("Reloads object properties from game")]
        public abstract void refresh();

        internal virtual void AssignData(InstanceData msg, string initializationErrorMsg = null)
        {
            if(initializationErrorMsg != null) {
                initialization_error_msg = initializationErrorMsg;
                return;
            }
            _internalId = ((T)msg).id;
        }

        protected void MoveImpl(Vector position, float? angle)
        {
            ClientHandler.Instance.RemoteCall(Contracts.MoveObject, new MoveMessage() {
                id = id,
                type = type,
                position = position,
                angle = angle.HasValue ? angle.Value : 0f,
                is_angle_defined = angle.HasValue
            },
            (ret, error) => {
                GetStorage().SaveData((T)ret);
                return null;
            });
        }

        [Doc("Delete object")]
        public virtual bool delete()
        {
            if(deleted) {
                return true;
            }
            api.client.RemoteCall<bool>(Contracts.DeleteObject, new DeleteObjectMessage() {
                id = id,
                type = type
            });
            if(!api.client.AsynchronousMode) {
                refresh();
            }
            return deleted;
        }

        public override bool Equals(object obj)
        {
            CitiesObject<T,U> other = (CitiesObject<T,U>)obj;
            return this.type == other.type &&
                   id == other.id;
        }

        public static bool operator ==(CitiesObject<T,U> lhs, CitiesObject<T,U> rhs)
        {
            if (System.Object.ReferenceEquals(lhs, null)) {
                if (System.Object.ReferenceEquals(rhs, null)) {
                    return true;
                }

                return false;
            }
            return lhs.id == rhs.id && lhs.type == rhs.type;
        }

        public static bool operator !=(CitiesObject<T,U> lhs, CitiesObject<T,U> rhs)
        {
            return !(lhs == rhs);
        }

        public override string ToString()
        {
            return PythonHelp.RuntimeToString(this);
        }

        public string SimpleToString()
        {
            return "id " + id;
        }

        public override int GetHashCode()
        {
            int hashCode = -1056084179;
            hashCode = hashCode * -1521134295 + id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(type);
            return hashCode;
        }
    }

    internal class CitiesObjectController
    {
        // hack - types with new() constraint cannot have internal constructor which would prevent calling them from Python code
        [ThreadStatic]
        internal static bool AllowInstantiation;
    }
}
