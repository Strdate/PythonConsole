using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkylinesRemotePython.API {
    public abstract class CitiesObject<T,U> : CitiesObjectBase<T,U,uint>, IPositionable, ISimpleToString 
        where T : InstanceData
        where U : CitiesObject<T,U>, new()
    {
        [Doc("Game ID")]
        public uint id => _.id;

        [Doc("Returns if object exists")]
        public bool exists => _.exists;

        public bool deleted => !_.exists;


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

        protected void MoveImpl(Vector position, float? angle)
        {
            // Save before wiping from cache
            uint _id = id;
            string _type = type;
            GetStorage().WipeFromCache(id);
            ClientHandler.Instance.RemoteCall(Contracts.MoveObject, new MoveMessage() {
                id = _id,
                type = _type,
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
        public virtual void delete()
        {
            if(!exists) {
                return;
            }
            GetStorage().Delete(id);
        }

        public override bool Equals(object obj)
        {
            CitiesObject<T, U> other = (CitiesObject<T, U>)obj;
            return this.type == other.type &&
                   id == other.id;
        }

        public static bool operator ==(CitiesObject<T, U> lhs, CitiesObject<T, U> rhs)
        {
            if (System.Object.ReferenceEquals(lhs, null)) {
                if (System.Object.ReferenceEquals(rhs, null)) {
                    return true;
                }

                return false;
            }
            if (System.Object.ReferenceEquals(rhs, null)) {
                return false;
            }
            return lhs.id == rhs.id && lhs.type == rhs.type;
        }

        public static bool operator !=(CitiesObject<T, U> lhs, CitiesObject<T, U> rhs)
        {
            return !(lhs == rhs);
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
