using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkylinesPythonShared
{
    public static class Contracts
    {
        public static Contract GetPropFromId = new Contract() { FuncName = "GetPropFromId", RetType = "Prop"};
        public static Contract GetNodeFromId = new Contract() { FuncName = "GetNodeFromId", RetType = "NetNode"};
        public static Contract CreateProp = new Contract() { FuncName = "CreateProp", RetType = "Prop"};
        public static Contract CreateNode = new Contract() { FuncName = "CreateNode", RetType = "NetNode"};
        public static Contract ExistsPrefab = new Contract() { FuncName = "ExistsPrefab", RetType = "bool" };
    }

    public class Contract
    {
        public string FuncName { get; internal set; }

        public string RetType { get; internal set; }
    }
}
