using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SkylinesPythonShared
{
    [Serializable]
    [XmlInclude(typeof(BuildingData))]
    [XmlInclude(typeof(TreeData))]
    [XmlInclude(typeof(PropData))]
    [XmlInclude(typeof(BatchObjectMessage))]
    [XmlInclude(typeof(InstanceData))]
    [XmlInclude(typeof(NetNodeData))]
    [XmlInclude(typeof(NetSegmentData))]
    [XmlInclude(typeof(NetPrefabData))]
    [XmlInclude(typeof(NetSegmentListMessage))]
    public class RunScriptMessage
    {
        public string script { get; set; }

        public object[] clipboard { get; set; }

        public string[] searchPaths { get; set; }
    }
}
