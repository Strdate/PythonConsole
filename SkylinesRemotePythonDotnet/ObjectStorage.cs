using SkylinesPythonShared;
using SkylinesRemotePython.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkylinesRemotePython
{
    public class ObjectStorage
    {
        private ClientHandler _client;

        [ThreadStatic]
        public static ObjectStorage Instance;
        public ObjectStorage(ClientHandler client)
        {
            _client = client;
            Instance = this;

            // feature - remove hardcoded limits
            Nodes = new CitiesObjectStorage<NetNodeData, Node, uint>("node", new ArrayStorage<NetNodeData>(32768));
            Buildings = new CitiesObjectStorage<BuildingData, Building, uint>("building", new ArrayStorage<BuildingData>(49152));
            Segments = new CitiesObjectStorage<NetSegmentData, Segment, uint>("segment", new ArrayStorage<NetSegmentData>(36864));
            Props = new CitiesObjectStorage<PropData, Prop, uint>("prop", new ArrayStorage<PropData>(65536));
            Trees = new CitiesObjectStorage<TreeData, Tree, uint>("tree", new ArrayStorage<TreeData>(262144));
            NetPrefabs = new CitiesObjectStorage<NetPrefabData, NetPrefab, string>("net prefab", new DictionaryStorage<string, NetPrefabData>());

            NaturalResources = new NaturalResourcesManager(client);
        }

        public CitiesObjectStorage<NetNodeData, Node, uint> Nodes;

        public CitiesObjectStorage<BuildingData, Building, uint> Buildings;

        public CitiesObjectStorage<NetSegmentData, Segment, uint> Segments;

        public CitiesObjectStorage<PropData, Prop, uint> Props;

        public CitiesObjectStorage<TreeData, Tree, uint> Trees;

        public CitiesObjectStorage<NetPrefabData, NetPrefab, string> NetPrefabs;

        public NaturalResourcesManager NaturalResources;
    }
}
