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
        public static Contract GetNodeFromId = new Contract() { FuncName = "GetNodeFromId", RetType = "NetNode"};
        public static Contract GetSegmentFromId = new Contract() { FuncName = "GetSegmentFromId", RetType = "NetSegment" };
        public static Contract CreateProp = new Contract() { FuncName = "CreateProp", RetType = "Prop"};
        public static Contract CreateTree = new Contract() { FuncName = "CreateTree", RetType = "Tree" };
        public static Contract CreateNode = new Contract() { FuncName = "CreateNode", RetType = "NetNode"};
        public static Contract CreateSegment = new Contract() { FuncName = "CreateSegment", RetType = "Segment" };
        public static Contract CreateSegments = new Contract() { FuncName = "CreateSegments", RetType = "SegmentList" };
        public static Contract ExistsPrefab = new Contract() { FuncName = "ExistsPrefab", RetType = "bool" };
    }

    public class Contract
    {
        public string FuncName { get; internal set; }

        public string RetType { get; internal set; }
    }
}
