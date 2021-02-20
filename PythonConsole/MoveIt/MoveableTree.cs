using UnityEngine;
using System;
using System.Collections.Generic;
using ColossalFramework.Math;


namespace MoveIt
{
    public class TreeState : InstanceState
    {
        public bool single;
    }

    public class MoveableTree : Instance
    {
        public override HashSet<ushort> segmentList
        {
            get
            {
                return new HashSet<ushort>();
            }
        }

        public MoveableTree(InstanceID instanceID) : base(instanceID)
        {
            //if (((TreeInstance.Flags)TreeManager.instance.m_trees.m_buffer[instanceID.Tree].m_flags & TreeInstance.Flags.Created) == TreeInstance.Flags.None)
            //{
            //    throw new Exception($"Tree #{instanceID.Tree} not found!");
            //}
            Info = new Info_Prefab(TreeManager.instance.m_trees.m_buffer[instanceID.Tree].Info);
        }

        public override InstanceState SaveToState(bool integrate = true)
        {
            TreeState state = new TreeState
            {
                instance = this,
                isCustomContent = Info.Prefab.m_isCustomContent
            };

            uint tree = id.Tree;
            state.Info = Info;

            state.position = TreeManager.instance.m_trees.m_buffer[tree].Position;
            state.terrainHeight = TerrainManager.instance.SampleOriginalRawHeightSmooth(state.position);

            state.single = TreeManager.instance.m_trees.m_buffer[tree].Single;

            //state.SaveIntegrations(integrate);

            return state;
        }

        public override void LoadFromState(InstanceState state)
        {
            if (!(state is InstanceState treeState)) return;

            uint tree = id.Tree;
            TreeManager.instance.MoveTree(tree, treeState.position);
            TreeManager.instance.UpdateTreeRenderer(tree, true);
        }

        public override Vector3 position
        {
            get
            {
                if (id.IsEmpty) return Vector3.zero;
                return TreeManager.instance.m_trees.m_buffer[id.Tree].Position;
            }
            set
            {
                if (id.IsEmpty) TreeManager.instance.m_trees.m_buffer[id.Tree].Position = Vector3.zero;
                else TreeManager.instance.m_trees.m_buffer[id.Tree].Position = value;
            }
        }

        public override float angle
        {
            get { return 0f; }
            set { }
        }

        public override bool isValid
        {
            get
            {
                if (id.IsEmpty) return false;
                return TreeManager.instance.m_trees.m_buffer[id.Tree].m_flags != 0;
            }
        }

        public void MoveCall(Vector3 newPosition)
        {
            Bounds originalBounds = GetBounds(false);
            Move(newPosition, 0);
            Bounds fullbounds = GetBounds(false);
            MoveItTool.UpdateArea(originalBounds, true);
            MoveItTool.UpdateArea(fullbounds, true);
        }

        public override void Transform(InstanceState state, ref Matrix4x4 matrix4x, float deltaHeight, float deltaAngle, Vector3 center, bool followTerrain)
        {
            Vector3 newPosition = matrix4x.MultiplyPoint(state.position - center);
            newPosition.y = state.position.y + deltaHeight;

            if (followTerrain)
            {
                newPosition.y = newPosition.y + TerrainManager.instance.SampleOriginalRawHeightSmooth(newPosition) - state.terrainHeight;
            }

            Move(newPosition, 0);
        }

        public override void Move(Vector3 location, float angle)
        {
            if (!isValid) return;

            uint tree = id.Tree;
            TreeManager.instance.MoveTree(tree, location);
            TreeManager.instance.UpdateTreeRenderer(tree, true);
        }

        public override void SetHeight(float height)
        {
            Vector3 newPosition = position;
            newPosition.y = height;

            uint tree = id.Tree;
            TreeManager.instance.MoveTree(tree, newPosition);
            TreeManager.instance.UpdateTreeRenderer(tree, true);
        }

        public override Instance Clone(InstanceState instanceState, ref Matrix4x4 matrix4x, float deltaHeight, float deltaAngle, Vector3 center, bool followTerrain, Dictionary<ushort, ushort> clonedNodes, Action action)
        {
            TreeState state = instanceState as TreeState;

            Vector3 newPosition = matrix4x.MultiplyPoint(state.position - center);
            newPosition.y = state.position.y + deltaHeight;

            if (followTerrain)
            {
                newPosition.y = newPosition.y + TerrainManager.instance.SampleOriginalRawHeightSmooth(newPosition) - state.terrainHeight;
            }

            Instance cloneInstance = null;

            TreeInstance[] buffer = TreeManager.instance.m_trees.m_buffer;

            if (TreeManager.instance.CreateTree(out uint clone, ref SimulationManager.instance.m_randomizer,
                state.Info.Prefab as TreeInfo, newPosition, state.single))
            {
                InstanceID cloneID = default;
                cloneID.Tree = clone;
                cloneInstance = new MoveableTree(cloneID);
            }

            return cloneInstance;
        }

        public override Instance Clone(InstanceState instanceState, Dictionary<ushort, ushort> clonedNodes)
        {
            TreeState state = instanceState as TreeState;

            Instance cloneInstance = null;

            if (TreeManager.instance.CreateTree(out uint clone, ref SimulationManager.instance.m_randomizer,
                state.Info.Prefab as TreeInfo, state.position, state.single))
            {
                InstanceID cloneID = default;
                cloneID.Tree = clone;
                cloneInstance = new MoveableTree(cloneID);
            }

            return cloneInstance;
        }

        public override void Delete()
        {
            if (isValid) TreeManager.instance.ReleaseTree(id.Tree);
        }

        public override Bounds GetBounds(bool ignoreSegments = true)
        {
            TreeInstance[] buffer = TreeManager.instance.m_trees.m_buffer;
            uint tree = id.Tree;
            TreeInfo info = buffer[tree].Info;

            Randomizer randomizer = new Randomizer(tree);
            float scale = info.m_minScale + (float)randomizer.Int32(10000u) * (info.m_maxScale - info.m_minScale) * 0.0001f;
            float radius = Mathf.Max(info.m_generatedInfo.m_size.x, info.m_generatedInfo.m_size.z) * scale;

            return new Bounds(buffer[tree].Position, new Vector3(radius, 0, radius));
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo, Color toolColor, Color despawnColor)
        {
            if (!isValid) return;
            //if (MoveItTool.m_isLowSensitivity) return;

            uint tree = id.Tree;
            TreeManager treeManager = TreeManager.instance;
            TreeInfo treeInfo = treeManager.m_trees.m_buffer[tree].Info;
            Vector3 position = treeManager.m_trees.m_buffer[tree].Position;
            Randomizer randomizer = new Randomizer(tree);
            float scale = treeInfo.m_minScale + (float)randomizer.Int32(10000u) * (treeInfo.m_maxScale - treeInfo.m_minScale) * 0.0001f;
            float alpha = 1f;
            TreeTool.CheckOverlayAlpha(treeInfo, scale, ref alpha);
            toolColor.a *= alpha;
            TreeTool.RenderOverlay(cameraInfo, treeInfo, position, scale, toolColor);
        }

        public override void RenderCloneOverlay(InstanceState instanceState, ref Matrix4x4 matrix4x, Vector3 deltaPosition, float deltaAngle, Vector3 center, bool followTerrain, RenderManager.CameraInfo cameraInfo, Color toolColor)
        {
            //if (MoveItTool.m_isLowSensitivity) return;

            TreeState state = instanceState as TreeState;

            TreeInfo info = state.Info.Prefab as TreeInfo;

            Randomizer randomizer = new Randomizer(state.instance.id.Tree);
            float scale = info.m_minScale + (float)randomizer.Int32(10000u) * (info.m_maxScale - info.m_minScale) * 0.0001f;
            //float brightness = info.m_minBrightness + (float)randomizer.Int32(10000u) * (info.m_maxBrightness - info.m_minBrightness) * 0.0001f;

            Vector3 newPosition = matrix4x.MultiplyPoint(state.position - center);
            newPosition.y = state.position.y + deltaPosition.y;

            if (followTerrain)
            {
                newPosition.y = newPosition.y - state.terrainHeight + TerrainManager.instance.SampleOriginalRawHeightSmooth(newPosition);
            }

            TreeTool.RenderOverlay(cameraInfo, info, newPosition, scale, toolColor);
        }

        public override void RenderCloneGeometry(InstanceState instanceState, ref Matrix4x4 matrix4x, Vector3 deltaPosition, float deltaAngle, Vector3 center, bool followTerrain, RenderManager.CameraInfo cameraInfo, Color toolColor)
        {
            TreeState state = instanceState as TreeState;

            TreeInfo info = state.Info.Prefab as TreeInfo;

            Randomizer randomizer = new Randomizer(state.instance.id.Tree);
            float scale = info.m_minScale + (float)randomizer.Int32(10000u) * (info.m_maxScale - info.m_minScale) * 0.0001f;
            float brightness = info.m_minBrightness + (float)randomizer.Int32(10000u) * (info.m_maxBrightness - info.m_minBrightness) * 0.0001f;

            Vector3 newPosition = matrix4x.MultiplyPoint(state.position - center);
            newPosition.y = state.position.y + deltaPosition.y;

            if (followTerrain)
            {
                newPosition.y = newPosition.y - state.terrainHeight + TerrainManager.instance.SampleOriginalRawHeightSmooth(newPosition);
            }

            TreeInstance.RenderInstance(cameraInfo, info, newPosition, scale, brightness, RenderManager.DefaultColorLocation);
        }
    }
}
