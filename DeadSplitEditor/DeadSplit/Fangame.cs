using System.Xml;
using System.Xml.Serialization;

using DeadSplitEditor.Extensions;
using DeadSplitEditor.Xml;

namespace DeadSplitEditor.DeadSplit
{
    public class Fangame : GenericXmlElement
    {
        [XmlAttribute("title")]
        public string Title { get; set; }

        [XmlElement(nameof(AddressSpace), typeof(AddressSpace))]
        public AddressSpace AddressSpace { get; set; }

        //[XmlElement("DeathDetector", IsNullable = true)]
        //public XmlElement DeathDetector { get; set; } // TODO

        [XmlElement(nameof(Boss), typeof(Boss))]
        public ChildItemCollection<Fangame, Boss> Bosses { get; private set; }

        public Fangame()
        {
            Bosses = new ChildItemCollection<Fangame, Boss>(this);
        }

        [XmlIgnore] public string DeadsplitFolder { get; internal set; }
    }
}
