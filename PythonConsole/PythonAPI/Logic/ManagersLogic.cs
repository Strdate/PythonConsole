using PythonConsole.MoveIt;
using SkylinesPythonShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PythonConsole
{
    public static class ManagersLogic
    {
        public static PropMessage PrepareProp(ushort id)
        {
            if (!ManagersUtil.ExistsProp(id)) {
                return null;
            }
            PropInstance prop = ManagersUtil.Prop(id);
            return new PropMessage() {
                id = id,
                position = prop.Position.FromUnity(),
                prefab_name = prop.Info.name,
                angle = prop.Angle
            };
        }

        public static TreeMessage PrepareTree(uint id)
        {
            if (!ManagersUtil.ExistsTree(id)) {
                return null;
            }
            TreeInstance tree = ManagersUtil.Tree(id);
            return new TreeMessage() {
                id = id,
                position = tree.Position.FromUnity(),
                prefab_name = tree.Info.name
            };
        }

        public static BuildingMessage PrepareBuilding(ushort id)
        {
            if (!ManagersUtil.ExistsBuilding(id)) {
                return null;
            }
            Building building = ManagersUtil.BuildingS(id);
            return new BuildingMessage() {
                id = id,
                position = building.m_position.FromUnity(),
                prefab_name = building.Info.name,
                angle = building.m_angle
            };
        }

        public static InstanceMessage Move(MoveMessage msg)
        {
            switch (msg.type) {
                case "node":
                    MoveableNode mnode = new MoveableNode(new InstanceID() {
                        NetNode = (ushort)msg.id
                    });
                    mnode.MoveCall(msg.position != null ? msg.position.ToUnity() : mnode.position);
                    return NetLogic.PrepareNode((ushort)msg.id);
                case "segment":
                    MoveableSegment seg = new MoveableSegment(new InstanceID() {
                        NetSegment = (ushort)msg.id
                    });
                    seg.MoveCall(msg.position != null ? msg.position.ToUnity() : seg.position);
                    return NetLogic.PrepareSegment((ushort)msg.id);
                case "building":
                    MoveableBuilding b = new MoveableBuilding(new InstanceID() {
                        Building = (ushort)msg.id
                    });
                    b.MoveCall(msg.position != null ? msg.position.ToUnity() : b.position, msg.is_angle_defined ? msg.angle : b.angle);
                    return ManagersLogic.PrepareBuilding((ushort)msg.id);
                case "prop":
                    MoveableProp p = new MoveableProp(new InstanceID() {
                        Prop = (ushort)msg.id
                    });
                    p.MoveCall(msg.position != null ? msg.position.ToUnity() : p.position, msg.is_angle_defined ? msg.angle : p.angle);
                    return ManagersLogic.PrepareProp((ushort)msg.id);
                case "tree":
                    MoveableTree t = new MoveableTree(new InstanceID() {
                        Tree = msg.id
                    });
                    t.MoveCall(msg.position != null ? msg.position.ToUnity() : t.position);
                    return ManagersLogic.PrepareTree(msg.id);
                default:
                    throw new Exception($"Cannot move {msg.type}");
            }
        }
    }
}
