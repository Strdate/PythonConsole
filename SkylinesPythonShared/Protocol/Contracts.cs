using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    public static class Contracts
    {
        public static Contract GetPropFromId = new Contract() { FuncName = "GetPropFromId", RetType = "Prop"};
        public static Contract GetTreeFromId = new Contract() { FuncName = "GetTreeFromId", RetType = "Prop" };
        public static Contract GetBuildingFromId = new Contract() { FuncName = "GetBuildingFromId", RetType = "Building" };
        public static Contract GetNodeFromId = new Contract() { FuncName = "GetNodeFromId", RetType = "NetNode"};
        public static Contract GetSegmentFromId = new Contract() { FuncName = "GetSegmentFromId", RetType = "NetSegment" };
        public static Contract GetSegmentsForNodeId = new Contract() { FuncName = "GetSegmentsForNodeId", RetType = "NetSegments" };
        public static Contract GetNetPrefabFromName = new Contract() { FuncName = "GetNetPrefabFromName", RetType = "NetPrefab" };
        public static Contract CreateProp = new Contract() { FuncName = "CreateProp", RetType = "Prop"};
        public static Contract CreateTree = new Contract() { FuncName = "CreateTree", RetType = "Tree" };
        public static Contract CreateBuilding = new Contract() { FuncName = "CreateBuilding", RetType = "Building" };
        public static Contract CreateNode = new Contract() { FuncName = "CreateNode", RetType = "NetNode"};
        public static Contract CreateSegment = new Contract() { FuncName = "CreateSegment", RetType = "Segment" };
        public static Contract CreateSegments = new Contract() { FuncName = "CreateSegments", RetType = "SegmentList" };
        public static Contract MoveObject = new Contract() { FuncName = "MoveObject", RetType = "AnyObject" };
        public static Contract DeleteObject = new Contract() { FuncName = "DeleteObject", RetType = "bool" };
        public static Contract ExistsPrefab = new Contract() { FuncName = "ExistsPrefab", RetType = "bool" };
        public static Contract GetTerrainHeight = new Contract() { FuncName = "GetTerrainHeight", RetType = "float" };
    }

    public class Contract
    {
        public string FuncName { get; internal set; }

        public string RetType { get; internal set; }
    }
}
