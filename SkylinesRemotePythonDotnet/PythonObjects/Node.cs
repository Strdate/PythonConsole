using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesRemotePython.API
{
    [Doc("Node structure - junction of Segments (roads/power lines etc.)")]
    public class Node : CitiesObject<NetNodeData>
    {
        public override string type => "node";

        [Doc("Node asset object (eg. 'Basic Road')")]
        public NetPrefab prefab => NetPrefab.GetNetPrefab(prefab_name, api);

        [Doc("Elevation over terrain or water level")]
        public double terrain_offset => _.terrain_offset;

        [Doc("ID of building (usually pillar)")]
        public int building_id => _.building_id;

        [Doc("Node building (usually pillar)")]
        public Building building => building_id == 0 ? null : new Building(api.client.RemoteCall<BuildingData>(Contracts.GetBuildingFromId, (uint)building_id), api);

        [Doc("Count of adjacent segments")]
        public int seg_count => _.seg_count;

        [Doc("Move to new position")]
        public void move(IPositionable pos) => MoveImpl(pos.position, null);

        private CachedObj<List<Segment>> _cachedSegments;

        [Doc("List of adjacent segments")]
        public List<Segment> segments => _cachedSegments.Get;

        public override void refresh()
        {
            ObjectStorage.Instance.Nodes.RefreshInstance(this);
        }

        internal override void AssignData(InstanceData data, string initializationErrorMsg = null)
        {
            base.AssignData(data, initializationErrorMsg);
            _cachedSegments = new CachedObj<List<Segment>>(() => api.client.RemoteCall<List<NetSegmentData>>(Contracts.GetSegmentsForNodeId, id).Select((obj) => new Segment(obj, api)).ToList());
        }

        public Node()
        {
            if(!CitiesObjectController.AllowInstantiation) {
                throw new Exception("Instantiation is not allowed!");
            }
        }
    }
}
