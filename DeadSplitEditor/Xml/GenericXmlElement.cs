using System.Xml;
using System.Xml.Serialization;

namespace DeadSplitEditor.Xml
{
    public class GenericXmlElement
    {
        [XmlAnyElement] public XmlElement[] XmlElementCollection { get; set; }
        [XmlAnyAttribute] public XmlAttribute[] XmlAttributeCollection { get; set; }
    }
}
