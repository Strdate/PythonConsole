using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API {
    public abstract class CitiesObject : ApiRefObject, IPositionable
    {
        public uint id { get; protected set; }

        public bool deleted { get; protected set; }

        protected Vector _position;
        public virtual Vector position { 
            get => _position;
            set => throw new Exception($"Position of {type} cannot be changed");
        }

        public Vector pos {
            get => position;
            set => position = value;
        }
        
        internal double _angle;

        public abstract void refresh();

        internal abstract void AssignData(InstanceMessage msg);

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
                angle = angle
            }));
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
