using DeadSplitEditor.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace DeadSplitEditor.DeadSplit
{
    public class AddressSpace : GenericXmlElement
    {
        [XmlIgnore]
        public string[] AddressNames
        {
            get => XmlElementCollection.Select(e => e.Name).ToArray();
        }
    }
}
