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
    [XmlInclude(typeof(NaturalResourceCellBase))]
    public class BatchObjectMessage
    {
        public object[] array { get; set; }

        public uint lastVisitedIndex { get; set; }

        public bool endOfStream { get; set; }
    }
}
