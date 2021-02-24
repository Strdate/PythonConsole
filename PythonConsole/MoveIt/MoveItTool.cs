using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PythonConsole.MoveIt
{
    public class MoveItTool
    {
        private static MoveItTool _instance;
        public static MoveItTool instance {
            get {
                if(_instance == null) {
                    _instance = new MoveItTool();
                }
                return _instance;
            }
        }

        public int areaUpdateCountdown = -1;
        public HashSet<Bounds> areasToUpdate = new HashSet<Bounds>();

        public int segmentUpdateCountdown = -1;
        public HashSet<ushort> segmentsToUpdate = new HashSet<ushort>();

        public static void UpdateArea(Bounds bounds, bool full = false)
        {
            try {
                //if (true(TypeMask & (TypeMasks.Building | TypeMasks.Network)) != TypeMasks.None) {
                    if (full) {
                        TerrainModify.UpdateArea(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z, true, true, false);
                    }

                    bounds.Expand(32f);
                    MoveItTool.instance.areasToUpdate.Add(bounds);
                    MoveItTool.instance.areaUpdateCountdown = 60;

                    if (full) {
                        Singleton<BuildingManager>.instance.ZonesUpdated(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z);
                        Singleton<PropManager>.instance.UpdateProps(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z);
                        Singleton<TreeManager>.instance.UpdateTrees(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z);
                        bounds.Expand(64f);
                        Singleton<ElectricityManager>.instance.UpdateGrid(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z);
                        Singleton<WaterManager>.instance.UpdateGrid(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z);
                    }
                /*} else {
                    bounds.Expand(32f);
                    MoveItTool.instance.areasToUpdate.Add(bounds);
                    MoveItTool.instance.areaUpdateCountdown = 60;
                    //Singleton<PropManager>.instance.UpdateProps(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z);
                    //Singleton<TreeManager>.instance.UpdateTrees(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z);
                }*/
            }
            catch (IndexOutOfRangeException e) {
                Debug.LogError($"EXCEPTION\n{bounds}\n{e}");
            }
        }

        public void SimulationStep()
        {
            if (segmentUpdateCountdown == 0) {
                UpdateSegments();
            }

            if (segmentUpdateCountdown >= 0) {
                segmentUpdateCountdown--;
            }

            if (areaUpdateCountdown == 0) {
                UpdateAreas();
            }

            if (areaUpdateCountdown >= 0) {
                areaUpdateCountdown--;
            }
        }

        public void UpdateAreas()
        {
            //foreach (Bounds b in areasToUpdate)
            //{
            //    AddDebugBox(b, new Color32(255, 31, 31, 31));
            //}
            HashSet<Bounds> merged = MergeBounds(areasToUpdate);
            //foreach (Bounds b in merged)
            //{
            //    b.Expand(4f);
            //    AddDebugBox(b, new Color32(31, 31, 255, 31));
            //}

            foreach (Bounds bounds in merged) {
                try {
                    bounds.Expand(64f);
                    Singleton<VehicleManager>.instance.UpdateParkedVehicles(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z);
                    TerrainModify.UpdateArea(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z, true, true, false);
                    UpdateRender(bounds);
                    bounds.Expand(512f);
                    Singleton<ElectricityManager>.instance.UpdateGrid(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z);
                    Singleton<WaterManager>.instance.UpdateGrid(bounds.min.x, bounds.min.z, bounds.max.x, bounds.max.z);
                }
                catch (IndexOutOfRangeException) {
                    //Log.Error($"Failed to update bounds {bounds}");
                }
            }

            areasToUpdate.Clear();
        }

        public void UpdateSegments()
        {
            foreach (ushort segment in segmentsToUpdate) {
                NetSegment[] segmentBuffer = NetManager.instance.m_segments.m_buffer;
                if (segmentBuffer[segment].m_flags != NetSegment.Flags.None) {
                    ReleaseSegmentBlock(segment, ref segmentBuffer[segment].m_blockStartLeft);
                    ReleaseSegmentBlock(segment, ref segmentBuffer[segment].m_blockStartRight);
                    ReleaseSegmentBlock(segment, ref segmentBuffer[segment].m_blockEndLeft);
                    ReleaseSegmentBlock(segment, ref segmentBuffer[segment].m_blockEndRight);
                }

                segmentBuffer[segment].Info.m_netAI.CreateSegment(segment, ref segmentBuffer[segment]);
            }
            segmentsToUpdate.Clear();
        }

        protected static void ReleaseSegmentBlock(ushort segment, ref ushort segmentBlock)
        {
            if (segmentBlock != 0) {
                ZoneManager.instance.ReleaseBlock(segmentBlock);
                segmentBlock = 0;
            }
        }

        private static void UpdateRender(Bounds bounds)
        {
            int num1 = Mathf.Clamp((int)(bounds.min.x / 64f + 135f), 0, 269);
            int num2 = Mathf.Clamp((int)(bounds.min.z / 64f + 135f), 0, 269);
            int x0 = num1 * 45 / 270 - 1;
            int z0 = num2 * 45 / 270 - 1;

            num1 = Mathf.Clamp((int)(bounds.max.x / 64f + 135f), 0, 269);
            num2 = Mathf.Clamp((int)(bounds.max.z / 64f + 135f), 0, 269);
            int x1 = num1 * 45 / 270 + 1;
            int z1 = num2 * 45 / 270 + 1;

            RenderManager renderManager = Singleton<RenderManager>.instance;
            RenderGroup[] renderGroups = renderManager.m_groups;

            for (int i = z0; i < z1; i++) {
                for (int j = x0; j < x1; j++) {
                    int n = Mathf.Clamp(i * 45 + j, 0, renderGroups.Length - 1);

                    if (n < 0) {
                        continue;
                    } else if (n >= renderGroups.Length) {
                        break;
                    }

                    if (renderGroups[n] != null) {
                        renderGroups[n].SetAllLayersDirty();
                        renderManager.m_updatedGroups1[n >> 6] |= 1uL << n;
                        renderManager.m_groupsUpdated1 = true;
                    }
                }
            }
        }

        internal static HashSet<Bounds> MergeBounds(HashSet<Bounds> outerList)
        {
            HashSet<Bounds> innerList = new HashSet<Bounds>();
            HashSet<Bounds> newList = new HashSet<Bounds>();

            int c = 0;

            do {
                foreach (Bounds outer in outerList) {
                    //Color32 color = GetRandomDebugColor();
                    //AddDebugBox(outer, color);

                    bool merged = false;

                    float outerVolume = outer.size.x * outer.size.y * outer.size.z;
                    foreach (Bounds inner in innerList) {
                        float separateVolume = (inner.size.x * inner.size.y * inner.size.z) + outerVolume;

                        Bounds encapsulated = inner;
                        encapsulated.Encapsulate(outer);
                        float encapsulateVolume = encapsulated.size.x * encapsulated.size.y * encapsulated.size.z;

                        if (!merged && encapsulateVolume < separateVolume) {
                            newList.Add(encapsulated);
                            merged = true;
                        } else {
                            newList.Add(inner);
                        }
                    }
                    if (!merged) {
                        newList.Add(outer);
                    }

                    innerList = new HashSet<Bounds>(newList);
                    newList.Clear();
                }

                if (outerList.Count <= innerList.Count) {
                    break;
                }
                outerList = new HashSet<Bounds>(innerList);
                innerList.Clear();

                if (c > 1000) {
                    //Log.Error($"Looped bounds-merge a thousand times");
                    break;
                }

                c++;
            }
            while (true);

            //foreach (Bounds b in innerList)
            //{
            //    b.Expand(4f);
            //    AddDebugBox(b, new Color32(255, 0, 0, 200));
            //}
            //Log.Debug($"\nStart:{originalList.Count}\nInner:{innerList.Count}");
            return innerList;
        }
    }
}
