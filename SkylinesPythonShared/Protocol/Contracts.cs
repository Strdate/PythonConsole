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
        public static Contract CreateProp = new Contract() { FuncName = "CreateProp", RetType = "Prop", CanRunAsync = true };
        public static Contract CreateTree = new Contract() { FuncName = "CreateTree", RetType = "Tree", CanRunAsync = true };
        public static Contract CreateBuilding = new Contract() { FuncName = "CreateBuilding", RetType = "Building", CanRunAsync = true };
        public static Contract CreateNode = new Contract() { FuncName = "CreateNode", RetType = "NetNode", CanRunAsync = true };
        public static Contract CreateSegment = new Contract() { FuncName = "CreateSegment", RetType = "Segment", CanRunAsync = true };
        public static Contract CreateSegments = new Contract() { FuncName = "CreateSegments", RetType = "SegmentList", CanRunAsync = true };
        public static Contract MoveObject = new Contract() { FuncName = "MoveObject", RetType = "AnyObject", CanRunAsync = true };
        public static Contract DeleteObject = new Contract() { FuncName = "DeleteObject", RetType = "bool", CanRunAsync = true };
        public static Contract ExistsPrefab = new Contract() { FuncName = "ExistsPrefab", RetType = "bool" };
        public static Contract GetTerrainHeight = new Contract() { FuncName = "GetTerrainHeight", RetType = "float" };
        public static Contract GetWaterLevel = new Contract() { FuncName = "GetWaterLevel", RetType = "float" };
        public static Contract RenderVector = new Contract() { FuncName = "RenderVector", RetType = "int", CanRunAsync = true };
        public static Contract RenderCircle = new Contract() { FuncName = "RenderCircle", RetType = "int", CanRunAsync = true };
        public static Contract GetNaturalResourceCells = new Contract() { FuncName = "GetNaturalResourceCells", RetType = "NaturalResourceCellBaseArray" };
        public static Contract GetNaturalResourceCellSingle = new Contract() { FuncName = "GetNaturalResourceCellSingle", RetType = "NaturalResourceCellBase" };
        public static Contract RemoveRenderedObject = new Contract() { FuncName = "RemoveRenderedObject", RetType = null };
        public static Contract SetNaturalResource = new Contract() { FuncName = "SetNaturalResource", RetType = null };
        public static Contract GetTreesStartingFromIndex = new Contract() { FuncName = "GetTreesStartingFromIndex", RetType = "BatchTreeMessage", IsBackgroundAsync = true };
        public static Contract GetPropsStartingFromIndex = new Contract() { FuncName = "GetPropsStartingFromIndex", RetType = "BatchPropMessage", IsBackgroundAsync = true };
        public static Contract GetBuildingsStartingFromIndex = new Contract() { FuncName = "GetBuildingsStartingFromIndex", RetType = "BatchBuildingsMessage", IsBackgroundAsync = true };
        public static Contract GetNodesStartingFromIndex = new Contract() { FuncName = "GetNodesStartingFromIndex", RetType = "BatchNodeMessage", IsBackgroundAsync = true };
        public static Contract GetSegmentsStartingFromIndex = new Contract() { FuncName = "GetSegmentsStartingFromIndex", RetType = "BatchSegmentMessage", IsBackgroundAsync = true };
    }

    public class Contract
    {
        public string FuncName { get; internal set; }

        public string RetType { get; internal set; }

        public bool CanRunAsync { get; internal set; }

        public bool IsBackgroundAsync { get; internal set; }
    }
}
