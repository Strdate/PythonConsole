using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PythonConsole
{
    internal class SelectionTool : DefaultTool
    {
        private ushort hoveredSegment;
        private ushort hoveredBuilding;

        private bool isShiftPressed;

        public static SelectionTool Instance;

        public List<ObjectInstance> Clipboard = new List<ObjectInstance>();

        public override NetNode.Flags GetNodeIgnoreFlags() => NetNode.Flags.None;

        public override NetSegment.Flags GetSegmentIgnoreFlags(out bool nameOnly)
        {
            nameOnly = false;
            return NetSegment.Flags.None;
        }

        public override Building.Flags GetBuildingIgnoreFlags() => Building.Flags.None;

        public override TreeInstance.Flags GetTreeIgnoreFlags() => TreeInstance.Flags.None;

        public override PropInstance.Flags GetPropIgnoreFlags() => PropInstance.Flags.None;

        public override Vehicle.Flags GetVehicleIgnoreFlags() => 0;

        public override VehicleParked.Flags GetParkedVehicleIgnoreFlags() => VehicleParked.Flags.None;

        public override CitizenInstance.Flags GetCitizenIgnoreFlags() => CitizenInstance.Flags.None;

        public override TransportLine.Flags GetTransportIgnoreFlags() => TransportLine.Flags.None;

        public override District.Flags GetDistrictIgnoreFlags() => District.Flags.None;

        public override DistrictPark.Flags GetParkIgnoreFlags() => DistrictPark.Flags.None;

        public override DisasterData.Flags GetDisasterIgnoreFlags() => DisasterData.Flags.None;

        public SelectionTool()
        {
            Instance = this;
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            base.RenderOverlay(cameraInfo);

            if (isShiftPressed && !m_toolController.IsInsideUI) {
                RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.red, m_accuratePosition, 3f, m_accuratePosition.y - 1f, m_accuratePosition.y + 1f, true, true);
            }

            for (int i = 0; i < Clipboard.Count; i++) {
                if (Clipboard[i].ObjectType == ObjectInstance.Type.Point) {
                    Vector3 pos = Clipboard[i].Position;
                    RenderManager.instance.OverlayEffect.DrawCircle(cameraInfo, Color.black, pos, 3f, pos.y - 1f, pos.y + 1f, true, true);
                }
            }

            if (m_hoverInstance.NetNode == 0) {
                return;
            }

            RenderManager.instance.OverlayEffect.DrawCircle(
                cameraInfo,
                GetToolColor(false, m_selectErrors != ToolErrors.None),
                NetManager.instance.m_nodes.m_buffer[m_hoverInstance.NetNode].m_position,
                NetManager.instance.m_nodes.m_buffer[m_hoverInstance.NetNode].m_bounds.size.magnitude,
                NetManager.instance.m_nodes.m_buffer[m_hoverInstance.NetNode].m_position.y - 1f,
                NetManager.instance.m_nodes.m_buffer[m_hoverInstance.NetNode].m_position.y + 1f,
                true,
                true);
        }

        protected override void OnToolUpdate()
        {
            base.OnToolUpdate();
            isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        public override void SimulationStep()
        {
            base.SimulationStep();
            if (m_hoverInstance.CitizenInstance > 0 || m_hoverInstance.Vehicle > 0 ||
                m_hoverInstance.ParkedVehicle > 0 ||
                m_hoverInstance.District > 0 || m_hoverInstance.Park > 0 || m_hoverInstance.TransportLine > 0 ||
                m_hoverInstance.Prop > 0 || m_hoverInstance.Tree > 0) {
                this.hoveredSegment = m_hoverInstance.NetSegment;
                hoveredBuilding = m_hoverInstance.Building;
                return;
            }

            if (!RayCastSegmentAndNode(out var hoveredSegment, out var hoveredNode)) {
                return;
            }

            var segments = new Dictionary<ushort, SegmentAndNode>();

            if (hoveredSegment != 0) {
                var segment = NetManager.instance.m_segments.m_buffer[hoveredSegment];
                var startNode = NetManager.instance.m_nodes.m_buffer[segment.m_startNode];
                var endNode = NetManager.instance.m_nodes.m_buffer[segment.m_endNode];
                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (startNode.CountSegments() > 0) {
                    var bounds = startNode.m_bounds;
                    if (hoveredNode != 0) {
                        bounds.extents /= 2f;
                    }

                    if (bounds.IntersectRay(mouseRay)) {
                        hoveredNode = segment.m_startNode;
                    }
                }

                if (hoveredSegment != 0 && endNode.CountSegments() > 0) {
                    var bounds = endNode.m_bounds;
                    if (hoveredNode != 0) {
                        bounds.extents /= 2f;
                    }

                    if (bounds.IntersectRay(mouseRay)) {
                        hoveredNode = segment.m_endNode;
                    }
                }

                if (hoveredSegment != 0 && !segments.ContainsKey(hoveredSegment)) {
                    segments.Clear();
                    SetSegments(hoveredSegment, segments);
                }
            }

            hoveredBuilding = m_hoverInstance.Building;

            if (hoveredNode > 0) {
                m_hoverInstance.NetNode = hoveredNode;
            } else if (hoveredSegment > 0) {
                m_hoverInstance.NetSegment = hoveredSegment;
            }

            this.hoveredSegment = hoveredSegment > 0 ? hoveredSegment : m_hoverInstance.NetSegment;
        }

        protected override void OnToolGUI(Event e)
        {
            DrawLabel();
            if (m_toolController.IsInsideUI || e.type != EventType.MouseDown) {
                base.OnToolGUI(e);
                return;
            }

            if(e.button == 0) {
                if(e.shift) {
                    ToggleInstanceSelection(new ObjectInstance(new SelectedPoint(m_accuratePosition)), e);
                } else if(!m_hoverInstance.IsEmpty) {
                    ObjectInstance inst = ObjectInstance.FromInstance(m_hoverInstance);
                    if(inst != null) {
                        ToggleInstanceSelection(inst, e);
                    }
                }
            } else if (e.button == 1 && !m_hoverInstance.IsEmpty) {
                if (m_hoverInstance.NetNode > 0) {
                    ToggleInstanceSelection(new ObjectInstance(hoveredSegment, ObjectInstance.Type.Segment), e);
                }
            }
        }

        private void ToggleInstanceSelection(ObjectInstance inst, Event e)
        {
            if(e.control) {
                if (!Clipboard.Remove(inst)) {
                    Clipboard.Add(inst);
                }
            } else {
                Clipboard.Clear();
                Clipboard.Add(inst);
            }
            
        }

        protected override bool CheckNode(ushort node, ref ToolErrors errors) => true;

        protected override bool CheckSegment(ushort segment, ref ToolErrors errors) => true;

        protected override bool CheckBuilding(ushort building, ref ToolErrors errors) => true;

        protected override bool CheckProp(ushort prop, ref ToolErrors errors) => true;

        protected override bool CheckTree(uint tree, ref ToolErrors errors) => true;

        protected override bool CheckVehicle(ushort vehicle, ref ToolErrors errors) => true;

        protected override bool CheckParkedVehicle(ushort parkedVehicle, ref ToolErrors errors) => true;

        protected override bool CheckCitizen(ushort citizenInstance, ref ToolErrors errors) => true;

        protected override bool CheckDisaster(ushort disaster, ref ToolErrors errors) => true;

        private static void SetSegments(ushort segmentId, IDictionary<ushort, SegmentAndNode> segments)
        {
            var segment = NetManager.instance.m_segments.m_buffer[segmentId];
            var seg = new SegmentAndNode() {
                SegmentId = segmentId,
                TargetNode = segment.m_endNode,
            };

            segments[segmentId] = seg;

            var infoIndex = segment.m_infoIndex;
            var node = NetManager.instance.m_nodes.m_buffer[segment.m_startNode];
            if (node.CountSegments() == 2) {
                SetSegments(node.m_segment0 == segmentId ? node.m_segment1 : node.m_segment0, infoIndex, ref seg, segments);
            }

            node = NetManager.instance.m_nodes.m_buffer[segment.m_endNode];
            if (node.CountSegments() == 2) {
                SetSegments(node.m_segment0 == segmentId ? node.m_segment1 : node.m_segment0, infoIndex, ref seg, segments);
            }
        }

        private static void SetSegments(ushort segmentId, ushort infoIndex, ref SegmentAndNode previousSeg, IDictionary<ushort, SegmentAndNode> segments)
        {
            var segment = NetManager.instance.m_segments.m_buffer[segmentId];

            if (segment.m_infoIndex != infoIndex || segments.ContainsKey(segmentId)) {
                return;
            }

            var seg = default(SegmentAndNode);
            seg.SegmentId = segmentId;

            var previousSegment = NetManager.instance.m_segments.m_buffer[previousSeg.SegmentId];
            ushort nextNode;
            if (segment.m_startNode == previousSegment.m_endNode ||
                segment.m_startNode == previousSegment.m_startNode) {
                nextNode = segment.m_endNode;
                seg.TargetNode = segment.m_startNode == previousSeg.TargetNode
                    ? segment.m_endNode
                    : segment.m_startNode;
            } else {
                nextNode = segment.m_startNode;
                seg.TargetNode = segment.m_endNode == previousSeg.TargetNode
                    ? segment.m_startNode
                    : segment.m_endNode;
            }

            segments[segmentId] = seg;

            var node = NetManager.instance.m_nodes.m_buffer[nextNode];
            if (node.CountSegments() == 2) {
                SetSegments(node.m_segment0 == segmentId ? node.m_segment1 : node.m_segment0, infoIndex, ref seg, segments);
            }
        }

        private static bool RayCastSegmentAndNode(out ushort netSegment, out ushort netNode)
        {
            if (RayCastSegmentAndNode(out var output)) {
                netSegment = output.m_netSegment;
                netNode = output.m_netNode;
                return true;
            }

            netSegment = 0;
            netNode = 0;
            return false;
        }

        private static bool RayCastSegmentAndNode(out RaycastOutput output)
        {
            var input = new RaycastInput(Camera.main.ScreenPointToRay(Input.mousePosition), Camera.main.farClipPlane) {
                m_netService = { m_itemLayers = ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels },
                m_ignoreSegmentFlags = NetSegment.Flags.None,
                m_ignoreNodeFlags = NetNode.Flags.None,
                m_ignoreTerrain = true,
            };

            return RayCast(input, out output);
        }

        private void DrawLabel()
        {
            var hoverInstance1 = m_hoverInstance;
            var text = (string)null;
            var varInfo = "";

            var inst = ObjectInstance.FromInstance(hoverInstance1);
            int index = Clipboard.IndexOf(inst);

            if (index > -1 && !isShiftPressed) {
                varInfo = Clipboard.Count == 1 ? "Var: cb\n" : "Var: cba[" + index + "]\n";
            }

            string position = "";
            if(inst != null) {
                position = FormatVector(inst.Position) + "\n";
            }

            if(isShiftPressed) {
                text = FormatVector(m_accuratePosition);
            } else if (hoverInstance1.NetNode != 0) {
                text = $"Node ID: {hoverInstance1.NetNode}\nSegment ID: {hoveredSegment}\n{position}Asset: {hoverInstance1.GetNetworkAssetName()}";
            } else if (hoverInstance1.NetSegment != 0) {
                text = $"Segment ID: {hoverInstance1.NetSegment}\nBuilding ID: {hoveredBuilding}\n{position}Asset: {hoverInstance1.GetNetworkAssetName()}";
            } else if (hoverInstance1.Building != 0) {
                text = $"Building ID: {hoverInstance1.Building}\n{position}Asset: {hoverInstance1.GetBuildingAssetName()}";
            } else if (hoverInstance1.Vehicle != 0) {
                text = $"Vehicle ID: {hoverInstance1.Vehicle}\n{position}Asset: {hoverInstance1.GetVehicleAssetName()}";
            } else if (hoverInstance1.ParkedVehicle != 0) {
                text = $"Parked Vehicle ID: {hoverInstance1.ParkedVehicle}\nAsset: {hoverInstance1.GetVehicleAssetName()}";
            } else if (hoverInstance1.CitizenInstance != 0) {
                text = $"Citizen instance ID: {hoverInstance1.CitizenInstance}\nCitizen ID: {hoverInstance1.GetCitizenId()}\nAsset: {hoverInstance1.GetCitizenAssetName()}";
            } else if (hoverInstance1.Prop != 0) {
                text = $"Prop ID: {hoverInstance1.Prop}\n{position}Asset: {hoverInstance1.GetPropAssetName()}";
            } else if (hoverInstance1.Tree != 0) {
                text = $"Tree ID: {hoverInstance1.Tree}\n{position}Asset: {hoverInstance1.GetTreeAssetName()}";
            } else if (hoverInstance1.Park != 0) {
                text = $"Park ID: {hoverInstance1.Park}\nName: {hoverInstance1.GetParkName()}";
            } else if (hoverInstance1.District != 0) {
                text = $"District ID: {hoverInstance1.District}\nName: {hoverInstance1.GetDistrictName()}";
            } else if (hoverInstance1.TransportLine != 0) {
                text = $"Transport Line ID: {hoverInstance1.TransportLine}\nName: {hoverInstance1.GetLineName()}";
            }

            if (text != null) {
                text = varInfo + text;

                var screenPoint = Input.mousePosition;
                screenPoint.y = Screen.height - screenPoint.y;
                var color = GUI.color;
                GUI.color = Color.white;
                DeveloperUI.LabelOutline(new Rect(screenPoint.x, screenPoint.y, 500f, 500f), "\n" + text, Color.black, Color.cyan, GUI.skin.label, 2f);
                GUI.color = color;
            }
        }

        private static string FormatVector(Vector3 vect)
        {
            return $"[{vect.x.ToString("N2")}, {vect.y.ToString("N2")}, {vect.z.ToString("N2")}]";
        } 

        public void DrawVarLabels()
        {
            if(!enabled) {
                return;
            }

            for (int i = 0; i < Clipboard.Count; i++) {
                if ((Clipboard[i] != ObjectInstance.FromInstance(m_hoverInstance) || isShiftPressed) && Clipboard[i].Exists) {
                    Vector3 pos = Clipboard[i].Position;

                    string label = Clipboard.Count == 1 ? "Var: cb" : "Var: cba[" + i + "]";

                    if (WorldToScreenPoint(pos, out Vector3 screenPos)) {
                        DeveloperUI.LabelOutline(new Rect(screenPos.x, screenPos.y, 500f, 500f), "\n" + label, Color.black, Color.cyan, GUI.skin.label, 2f);
                    }
                }
            }
        }

        private struct SegmentAndNode
        {
            public ushort SegmentId;
            public ushort TargetNode;
        }

        internal static bool WorldToScreenPoint(Vector3 worldPos, out Vector3 screenPos)
        {
            screenPos = Camera.main.WorldToScreenPoint(worldPos);
            screenPos.y = (float)Screen.height - screenPos.y;
            return screenPos.z >= 0f;
        }
    }
}