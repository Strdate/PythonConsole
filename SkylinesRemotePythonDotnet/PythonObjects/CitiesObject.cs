using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API {
    public abstract class CitiesObject : ApiRefObject, IPositionable, ISimpleToString
    {
        [Doc("Game ID")]
        public uint id { get; protected set; }

        [Doc("Returns if object exists")]
        public bool deleted { get; protected set; }

        protected Vector _position;

        [Doc("Object position. Can be assigned to to move the object")]
        public virtual Vector position { 
            get => _position;
            set => MoveImpl(value, null);//throw new Exception($"Position of {type} cannot be changed");
        }

        [ToStringIgnore]
        [Doc("Object position. Can be assigned to to move the object")]
        public Vector pos {
            get => position;
            set => position = value;
        }
        
        internal double _angle;

        [Doc("Reloads object properties from game")]
        public abstract void refresh();

        internal abstract void AssignData(InstanceMessage msg);

        [Doc("Object type (node, buidling, prop etc.)")]
        public virtual string type => "";

        internal CitiesObject(GameAPI api) : base(api)
        {

        }

        protected void MoveImpl(Vector position, float? angle)
        {
            AssignData(api.client.RemoteCall<InstanceMessage>(Contracts.MoveObject, new MoveMessage() {
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
            refresh();
            return deleted;
        }

        public override bool Equals(object obj)
        {
            CitiesObject other = (CitiesObject)obj;
            return this.type == other.type &&
                   id == other.id;
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
}
