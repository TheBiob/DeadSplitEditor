using DeadSplitEditor.DataContext;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace DeadSplitEditor.DeadSplit
{
    public class Time : Context, IXmlSerializable
    {
        [XmlAttribute("start")]
        public decimal? StartTime
        {
            get => _startTime;
            set
            {
                _startTime = value;
                NotifyPropertyChanged();
            }
        }
        private decimal? _startTime;

        [XmlAttribute("end")]
        public decimal? EndTime
        {
            get => _endTime;
            set
            {
                _endTime = value;
                NotifyPropertyChanged();
            }
        }
        private decimal? _endTime;

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToFirstAttribute();
            do
            {
                switch (reader.LocalName)
                {
                    case "start":
                        StartTime = Convert.ToDecimal(reader.Value);
                        break;
                    case "end":
                        EndTime = Convert.ToDecimal(reader.Value);
                        break;
                }
            } while (reader.MoveToNextAttribute());
        }

        public void WriteXml(XmlWriter writer)
        {
            if (StartTime.HasValue)
                writer.WriteAttributeString("start", StartTime.Value.ToString());

            if (EndTime.HasValue)
                writer.WriteAttributeString("end", EndTime.Value.ToString());
        }
    }
}
