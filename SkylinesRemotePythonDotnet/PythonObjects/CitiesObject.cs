using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API {
    public abstract class CitiesObject<T> : IPositionable, ISimpleToString where T : InstanceData
    {
        private T _instanceData;

        internal long initHandle;

        public string initialization_error_msg { get; private set; }

        protected T _ {
            get {
                if (_instanceData != null) {
                    return _instanceData;
                }
                if (initialization_error_msg != null) {
                    throw new Exception(initialization_error_msg);
                }
                ClientHandler.Instance.WaitOnHandle(initHandle);
                return _instanceData;
            }
        }

        [Doc("Game ID")]
        public uint id => _.id;

        [Doc("Object type (node, buidling, prop etc.)")]
        public virtual string type => "";

        [Doc("Returns if object exists")]
        public bool deleted { get; protected set; }


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
            if (msg == null) {
                deleted = true;
            }
            _instanceData = (T)msg;
        }

        protected void MoveImpl(Vector position, float? angle)
        {
            AssignData(api.client.RemoteCall<InstanceData>(Contracts.MoveObject, new MoveMessage() {
                id = id,
                type = type,
                position = position,
                angle = angle.HasValue ? angle.Value : 0f,
                is_angle_defined = angle.HasValue
            }));
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
            CitiesObject<T> other = (CitiesObject<T>)obj;
            return this.type == other.type &&
                   id == other.id;
        }

        public static bool operator ==(CitiesObject<T> lhs, CitiesObject<T> rhs)
        {
            if (System.Object.ReferenceEquals(lhs, null)) {
                if (System.Object.ReferenceEquals(rhs, null)) {
                    return true;
                }

                return false;
            }
            return lhs.id == rhs.id && lhs.type == rhs.type;
        }

        public static bool operator !=(CitiesObject<T> lhs, CitiesObject<T> rhs)
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
