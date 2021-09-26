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

            Nodes = new CitiesObjectStorage<NetNodeData, Node, uint>("node");
            Buildings = new CitiesObjectStorage<BuildingData, Building, uint>("building");
            Segments = new CitiesObjectStorage<NetSegmentData, Segment, uint>("Segments");
            Props = new CitiesObjectStorage<PropData, Prop, uint>("prop");
            Trees = new CitiesObjectStorage<TreeData, Tree, uint>("tree");
            NetPrefabs = new CitiesObjectStorage<NetPrefabData, NetPrefab, string>("net prefab");

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
