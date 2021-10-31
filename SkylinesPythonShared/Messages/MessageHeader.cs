using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace SkylinesPythonShared
{
    [Serializable]
    [XmlInclude(typeof(RunScriptMessage))]
    [XmlInclude(typeof(GetObjectMessage))]
    [XmlInclude(typeof(DeleteObjectMessage))]
    [XmlInclude(typeof(CreateBuildingMessage))]
    [XmlInclude(typeof(CreateNodeMessage))]
    [XmlInclude(typeof(CreatePropMessage))]
    [XmlInclude(typeof(CreateSegmentMessage))]
    [XmlInclude(typeof(CreateTreeMessage))]
    [XmlInclude(typeof(GetObjectsFromIndexMessage))]
    [XmlInclude(typeof(MoveMessage))]
    [XmlInclude(typeof(RenderCircleMessage))]
    [XmlInclude(typeof(RenderVectorMessage))]
    [XmlInclude(typeof(SetNaturalResourceMessage))]
    public class MessageHeader
    {
        [XmlElement("version")] public string version { get; set; }
        [XmlElement("messageType")] public string messageType { get; set; }
        [XmlElement("requestId")] public long requestId { get; set; }
        [XmlElement("payload")] public object payload { get; set; }
    }
}
