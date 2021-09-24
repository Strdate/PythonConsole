using SkylinesPythonShared;
using System;
using UnityEngine;

namespace PythonConsole
{
    public class ObjectInstance : IEquatable<ObjectInstance>
    {
        public uint Id { get; private set; }

        public Type ObjectType { get; private set; }

        private SelectedPoint _point;

        public bool Exists {
            get {
                if(ObjectType == Type.Point) {
                    return true;
                } else if (ObjectType == Type.Node) {
                    return NetUtil.ExistsNode((ushort)Id);
                } else if (ObjectType == Type.Segment) {
                    return NetUtil.ExistsSegment((ushort)Id);
                } else if (ObjectType == Type.Building) {
                    return ManagersUtil.ExistsBuilding((ushort)Id);
                } else if (ObjectType == Type.Prop) {
                    return ManagersUtil.ExistsProp((ushort)Id);
                } else {
                    return ManagersUtil.ExistsTree(Id);
                }
            }
        }
        public Vector3 Position {
            get {
                if (ObjectType == Type.Point) {
                    return _point.position;
                } else if (ObjectType == Type.Node) {
                    return NetUtil.Node((ushort)Id).m_position;
                } else if (ObjectType == Type.Segment) {
                    return NetUtil.Segment((ushort)Id).m_middlePosition;
                } else if (ObjectType == Type.Building) {
                    return ManagersUtil.BuildingS((ushort)Id).m_position;
                } else if (ObjectType == Type.Prop) {
                    return ManagersUtil.Prop((ushort)Id).Position;
                } else {
                    return ManagersUtil.Tree(Id).Position;
                } 
            }
        }

        public static ObjectInstance FromInstance(InstanceID ins)
        {
            if (ins.NetNode != 0) {
                return new ObjectInstance(ins.NetNode, Type.Node);
            } else if (ins.NetSegment != 0) {
                return new ObjectInstance(ins.NetSegment, Type.Segment);
            } else if (ins.Building != 0) {
                return new ObjectInstance(ins.Building, Type.Building);
            } else if (ins.Prop != 0) {
                return new ObjectInstance(ins.Prop, Type.Prop);
            } else if (ins.Tree != 0) {
                return new ObjectInstance(ins.Tree, Type.Tree);
            }
            return null;
        }

        public InstanceData ToMessage()
        {
            if (ObjectType == Type.Point) {
                return _point.position.FromUnity();
            } else if (ObjectType == Type.Node) {
                return NetLogic.PrepareNode((ushort)Id);
            } else if (ObjectType == Type.Segment) {
                return NetLogic.PrepareSegment((ushort)Id);
            } else if (ObjectType == Type.Building) {
                return ManagersLogic.PrepareBuilding((ushort)Id);
            } else if (ObjectType == Type.Prop) {
                return ManagersLogic.PrepareProp((ushort)Id);
            } else {
                return ManagersLogic.PrepareTree(Id);
            }
        }

        public override bool Equals(object obj)
        {
            return obj is ObjectInstance instance &&
                   Id == instance.Id
                   && ObjectType == instance.ObjectType
                   && Id != 0;
        }

        public override int GetHashCode()
        {
            int hashCode = -2147301958;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + ObjectType.GetHashCode();
            return hashCode;
        }

        public bool Equals(ObjectInstance other)
        {
            if (System.Object.ReferenceEquals(other, null)) {
                return false;
            }

            if (System.Object.ReferenceEquals(this, other)) {
                return true;
            }

            if (this.GetType() != other.GetType()) {
                return false;
            }

            return (Id == other.Id) && (ObjectType == other.ObjectType) && Id != 0;
        }

        public static bool operator ==(ObjectInstance lhs, ObjectInstance rhs)
        {
            if (System.Object.ReferenceEquals(lhs, null)) {
                if (System.Object.ReferenceEquals(rhs, null)) {
                    return true;
                }

                return false;
            }
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ObjectInstance lhs, ObjectInstance rhs)
        {
            return !(lhs == rhs);
        }

        public ObjectInstance(uint id, Type type)
        {
            Id = id;
            ObjectType = type;
        }

        public ObjectInstance(SelectedPoint point)
        {
            _point = point;
            ObjectType = Type.Point;
        }

        public enum Type
        {
            Node,
            Segment,
            Prop,
            Tree,
            Building,
            Point
        }
    }
}
