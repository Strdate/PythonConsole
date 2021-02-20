using UnityEngine;
using System;
using System.Collections.Generic;
using ColossalFramework.Math;


namespace MoveIt
{
    public class PropState : InstanceState
    {
        public bool single;
        public bool fixedHeight;
    }

    public class MoveableProp : Instance
    {
        public override HashSet<ushort> segmentList
        {
            get
            {
                return new HashSet<ushort>();
            }
        }

        public MoveableProp(InstanceID instanceID) : base(instanceID)
        {
            Info = new Info_Prefab(PropManager.instance.m_props.m_buffer[instanceID.Prop].Info);
        }

        public override InstanceState SaveToState(bool integrate = true)
        {
            PropState state = new PropState
            {
                instance = this,
                isCustomContent = Info.Prefab.m_isCustomContent
            };

            ushort prop = id.Prop;
            state.Info = Info;

            state.position = PropManager.instance.m_props.m_buffer[prop].Position;
            state.angle = PropManager.instance.m_props.m_buffer[prop].Angle;
            state.terrainHeight = TerrainManager.instance.SampleOriginalRawHeightSmooth(state.position);

            state.single = PropManager.instance.m_props.m_buffer[prop].Single;
            state.fixedHeight = PropManager.instance.m_props.m_buffer[prop].FixedHeight;

            //state.SaveIntegrations(integrate);

            return state;
        }

        public override void LoadFromState(InstanceState state)
        {
            if (!(state is PropState propState)) return;

            ushort prop = id.Prop;
            PropManager.instance.m_props.m_buffer[prop].Angle = propState.angle;
            PropManager.instance.m_props.m_buffer[prop].FixedHeight = propState.fixedHeight;
            PropManager.instance.MoveProp(prop, propState.position);
            PropManager.instance.UpdatePropRenderer(prop, true);
        }

        public override Vector3 position
        {
            get
            {
                if (id.IsEmpty) return Vector3.zero;
                return PropManager.instance.m_props.m_buffer[id.Prop].Position;
            }
            set
            {
                if (id.IsEmpty) PropManager.instance.m_props.m_buffer[id.Prop].Position = Vector3.zero;
                else PropManager.instance.m_props.m_buffer[id.Prop].Position = value;
            }
        }

        public override float angle
        {
            get
            {
                if (id.IsEmpty) return 0f;
                return PropManager.instance.m_props.m_buffer[id.Prop].Angle;
            }
            set
            {
                if (id.IsEmpty) return;
                PropManager.instance.m_props.m_buffer[id.Prop].Angle = (value + Mathf.PI * 2) % (Mathf.PI * 2);
            }
        }

        public override bool isValid
        {
            get
            {
                if (id.IsEmpty) return false;
                return PropManager.instance.m_props.m_buffer[id.Prop].m_flags != 0;
            }
        }

        public void MoveCall(Vector3 newPosition, float angle)
        {
            Bounds originalBounds = GetBounds(false);
            Move(newPosition, angle);
            Bounds fullbounds = GetBounds(false);
            MoveItTool.UpdateArea(originalBounds, true);
            MoveItTool.UpdateArea(fullbounds, true);
        }

        public override void Transform(InstanceState state, ref Matrix4x4 matrix4x, float deltaHeight, float deltaAngle, Vector3 center, bool followTerrain)
        {
            Vector3 newPosition = matrix4x.MultiplyPoint(state.position - center);
            newPosition.y = state.position.y + deltaHeight;

            /*if (!PropManager.instance.m_props.m_buffer[id.Prop].FixedHeight && deltaHeight != 0 && (MoveItLoader.loadMode == ICities.LoadMode.LoadAsset || MoveItLoader.loadMode == ICities.LoadMode.NewAsset))
            {
                PropManager.instance.m_props.m_buffer[id.Prop].FixedHeight = true;
            }*/

            if (followTerrain)
            {
                newPosition.y = newPosition.y + TerrainManager.instance.SampleOriginalRawHeightSmooth(newPosition) - state.terrainHeight;
            }

            Move(newPosition, state.angle + deltaAngle);
        }

        public override void Move(Vector3 location, float angle)
        {
            if (!isValid) return;

            ushort prop = id.Prop;
            PropManager.instance.m_props.m_buffer[prop].Angle = angle;
            PropManager.instance.MoveProp(prop, location);
            PropManager.instance.UpdatePropRenderer(prop, true);
        }

        public override void SetHeight(float height)
        {
            Vector3 newPosition = position;
            newPosition.y = height;

            ushort prop = id.Prop;
            PropManager.instance.MoveProp(prop, newPosition);
            PropManager.instance.UpdatePropRenderer(prop, true);
        }

        public override Instance Clone(InstanceState instanceState, ref Matrix4x4 matrix4x, float deltaHeight, float deltaAngle, Vector3 center, bool followTerrain, Dictionary<ushort, ushort> clonedNodes, Action action)
        {
            PropState state = instanceState as PropState;

            Vector3 newPosition = matrix4x.MultiplyPoint(state.position - center);
            newPosition.y = state.position.y + deltaHeight;

            if (followTerrain)
            {
                newPosition.y = newPosition.y + TerrainManager.instance.SampleOriginalRawHeightSmooth(newPosition) - state.terrainHeight;
            }

            Instance cloneInstance = null;

            PropInstance[] buffer = PropManager.instance.m_props.m_buffer;

            if (PropManager.instance.CreateProp(out ushort clone, ref SimulationManager.instance.m_randomizer,
                state.Info.Prefab as PropInfo, newPosition, state.angle + deltaAngle, state.single))
            {
                InstanceID cloneID = default;
                cloneID.Prop = clone;
                buffer[clone].FixedHeight = state.fixedHeight;
                cloneInstance = new MoveableProp(cloneID);
            }

            return cloneInstance;
        }

        public override Instance Clone(InstanceState instanceState, Dictionary<ushort, ushort> clonedNodes)
        {
            PropState state = instanceState as PropState;

            Instance cloneInstance = null;

            if (PropManager.instance.CreateProp(out ushort clone, ref SimulationManager.instance.m_randomizer,
                state.Info.Prefab as PropInfo, state.position, state.angle, state.single))
            {
                InstanceID cloneID = default;
                cloneID.Prop = clone;
                cloneInstance = new MoveableProp(cloneID);
            }

            return cloneInstance;
        }

        public override void Delete()
        {
            if (isValid) PropManager.instance.ReleaseProp(id.Prop);
        }

        public override Bounds GetBounds(bool ignoreSegments = true)
        {
            ushort prop = id.Prop;
            PropInfo info = PropManager.instance.m_props.m_buffer[prop].Info;

            Randomizer randomizer = new Randomizer(prop);
            float scale = info.m_minScale + (float)randomizer.Int32(10000u) * (info.m_maxScale - info.m_minScale) * 0.0001f;
            float radius = Mathf.Max(info.m_generatedInfo.m_size.x, info.m_generatedInfo.m_size.z) * scale;

            return new Bounds(PropManager.instance.m_props.m_buffer[prop].Position, new Vector3(radius, 0, radius));
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo, Color toolColor, Color despawnColor)
        {
            if (!isValid) return;
            //if (MoveItTool.m_isLowSensitivity) return;

            ushort prop = id.Prop;
            PropManager propManager = PropManager.instance;
            PropInfo propInfo = propManager.m_props.m_buffer[prop].Info;
            Vector3 position = propManager.m_props.m_buffer[prop].Position;
            float angle = propManager.m_props.m_buffer[prop].Angle;
            Randomizer randomizer = new Randomizer((int)prop);
            float scale = propInfo.m_minScale + (float)randomizer.Int32(10000u) * (propInfo.m_maxScale - propInfo.m_minScale) * 0.0001f;
            float alpha = 1f;
            PropTool.CheckOverlayAlpha(propInfo, scale, ref alpha);
            toolColor.a *= alpha;
            PropTool.RenderOverlay(cameraInfo, propInfo, position, scale, angle, toolColor);
        }

        public override void RenderCloneOverlay(InstanceState instanceState, ref Matrix4x4 matrix4x, Vector3 deltaPosition, float deltaAngle, Vector3 center, bool followTerrain, RenderManager.CameraInfo cameraInfo, Color toolColor)
        {
            //if (MoveItTool.m_isLowSensitivity) return;

            PropState state = instanceState as PropState;

            PropInfo info = state.Info.Prefab as PropInfo;
            Randomizer randomizer = new Randomizer(state.instance.id.Prop);
            float scale = info.m_minScale + (float)randomizer.Int32(10000u) * (info.m_maxScale - info.m_minScale) * 0.0001f;

            Vector3 newPosition = matrix4x.MultiplyPoint(state.position - center);
            newPosition.y = state.position.y + deltaPosition.y;

            if (followTerrain)
            {
                newPosition.y = newPosition.y - state.terrainHeight + TerrainManager.instance.SampleOriginalRawHeightSmooth(newPosition);
            }

            float newAngle = state.angle + deltaAngle;

            PropTool.RenderOverlay(cameraInfo, info, newPosition, scale, newAngle, toolColor);
        }

        public override void RenderCloneGeometry(InstanceState instanceState, ref Matrix4x4 matrix4x, Vector3 deltaPosition, float deltaAngle, Vector3 center, bool followTerrain, RenderManager.CameraInfo cameraInfo, Color toolColor)
        {
            RenderCloneGeometryImplementation(instanceState, ref matrix4x, deltaPosition, deltaAngle, center, followTerrain, cameraInfo);
        }

        public static void RenderCloneGeometryImplementation(InstanceState instanceState, ref Matrix4x4 matrix4x, Vector3 deltaPosition, float deltaAngle, Vector3 center, bool followTerrain, RenderManager.CameraInfo cameraInfo)
        {
            InstanceID id = instanceState.instance.id;

            PropInfo info = instanceState.Info.Prefab as PropInfo;
            Randomizer randomizer = new Randomizer(id.Prop);
            float scale = info.m_minScale + (float)randomizer.Int32(10000u) * (info.m_maxScale - info.m_minScale) * 0.0001f;

            Vector3 newPosition = matrix4x.MultiplyPoint(instanceState.position - center);
            newPosition.y = instanceState.position.y + deltaPosition.y;

            if (followTerrain)
            {
                newPosition.y = newPosition.y - instanceState.terrainHeight + TerrainManager.instance.SampleOriginalRawHeightSmooth(newPosition);
            }

            float newAngle = instanceState.angle + deltaAngle;
            
            if (info.m_requireHeightMap)
            {
                TerrainManager.instance.GetHeightMapping(newPosition, out Texture heightMap, out Vector4 heightMapping, out Vector4 surfaceMapping);
                PropInstance.RenderInstance(cameraInfo, info, id, newPosition, scale, newAngle, info.GetColor(ref randomizer), RenderManager.DefaultColorLocation, true, heightMap, heightMapping, surfaceMapping);
            }
            else
            {
                PropInstance.RenderInstance(cameraInfo, info, id, newPosition, scale, newAngle, info.GetColor(ref randomizer), RenderManager.DefaultColorLocation, true);
            }
        }
    }
}
