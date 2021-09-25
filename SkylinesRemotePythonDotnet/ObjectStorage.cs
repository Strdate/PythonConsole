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

            Nodes = new ObjectInstanceStorage<NetNodeData, Node>("node");
            Buildings = new ObjectInstanceStorage<BuildingData, Building>("building");
            Segments = new ObjectInstanceStorage<NetSegmentData, Segment>("Segments");
            Props = new ObjectInstanceStorage<PropData, Prop>("prop");
            Trees = new ObjectInstanceStorage<TreeData, Tree>("tree");

            NaturalResources = new NaturalResourcesManager(client);
        }

        public ObjectInstanceStorage<NetNodeData, Node> Nodes;

        public ObjectInstanceStorage<BuildingData, Building> Buildings;

        public ObjectInstanceStorage<NetSegmentData, Segment> Segments;

        public ObjectInstanceStorage<PropData, Prop> Props;

        public ObjectInstanceStorage<TreeData, Tree> Trees;

        public NaturalResourcesManager NaturalResources;
    }
}
