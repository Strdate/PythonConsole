using ColossalFramework;
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

        public static NaturalResourceCellBase ConvertResourceCell(int id)
        {
            var cells = Singleton<NaturalResourceManager>.instance.m_naturalResources;
            NaturalResourceCellBase _base = default;
            _base.ore = cells[id].m_ore;
            _base.oil = cells[id].m_oil;
            _base.forest = cells[id].m_forest;
            _base.fertility = cells[id].m_fertility;
            _base.pollution = cells[id].m_pollution;
            _base.water = cells[id].m_water;
            return _base;
        }

        public static void SetNaturalResource(SetNaturalResourceMessage data)
        {
            ref var cell = ref Singleton<NaturalResourceManager>.instance.m_naturalResources[data.cell_id];
            switch(data.type) {
                case "ore":
                    cell.m_ore = data.value;
                    cell.m_modified |= 1;
                    break;
                case "oil":
                    cell.m_oil = data.value;
                    cell.m_modified |= 1;
                    break;
                case "fertility":
                    cell.m_fertility = data.value;
                    cell.m_modified |= 1;
                    break;
                case "pollution":
                    cell.m_pollution = data.value;
                    cell.m_modified |= 2;
                    break;
            }
        }
    }
}
