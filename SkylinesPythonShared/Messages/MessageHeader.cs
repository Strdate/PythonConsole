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
    public class MessageHeader
    {
        [XmlElement("version")] public string version { get; set; }
        [XmlElement("messageType")] public string messageType { get; set; }
        [XmlElement("requestId")] public long requestId { get; set; }
        [XmlElement("payload")] public object payload { get; set; }
    }
}
