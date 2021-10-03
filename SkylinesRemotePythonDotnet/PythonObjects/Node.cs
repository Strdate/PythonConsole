using SkylinesPythonShared;
using SkylinesPythonShared.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesRemotePython.API
{
    [Doc("Node structure - junction of Segments (roads/power lines etc.)")]
    public class Node : CitiesObject<NetNodeData,Node>
    {
        public override string type => "node";

        private protected override CitiesObjectStorage<NetNodeData,Node, uint> GetStorage()
        {
            return ObjectStorage.Instance.Nodes;
        }

        [Doc("Node asset object (eg. 'Basic Road')")]
        public NetPrefab prefab => ObjectStorage.Instance.NetPrefabs.GetById(prefab_name);

        [Doc("Elevation over terrain or water level")]
        public double terrain_offset => _.terrain_offset;

        [Doc("ID of building (usually pillar)")]
        public int building_id => _.building_id;

        [Doc("Node building (usually pillar)")]
        public Building building => building_id == 0 ? null : ObjectStorage.Instance.Buildings.GetById((uint)building_id);

        [Doc("Count of adjacent segments")]
        public int seg_count => _.seg_count;

        [Doc("Move to new position")]
        public void move(IPositionable pos) => MoveImpl(pos.position, null);

        [Doc("List of adjacent segments")]
        public List<Segment> segments => ((CachedObj<List<Segment>>)_._cachedSegments).Get;

        public override void refresh()
        {
            ObjectStorage.Instance.Nodes.RefreshInstance(id);
        }

        internal override void AssignData(InstanceDataBase<uint> data, string initializationErrorMsg = null)
        {
            base.AssignData(data, initializationErrorMsg);
            if(initializationErrorMsg == null) {
                // fuj
                _._cachedSegments = new CachedObj<List<Segment>>(() =>
                    ClientHandler.Instance.SynchronousCall<List<NetSegmentData>>(Contracts.GetSegmentsForNodeId, id).Select((obj) =>
                            ObjectStorage.Instance.Segments.SaveData(obj)
                        ).ToList()
                    );
            }
        }

        public Node()
        {
            if(!CitiesObjectController.AllowInstantiation) {
                throw new Exception("Instantiation is not allowed!");
            }
        }
    }
}
