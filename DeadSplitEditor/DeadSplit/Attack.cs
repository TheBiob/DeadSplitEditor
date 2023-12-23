using DeadSplitEditor.DataContext;
using DeadSplitEditor.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace DeadSplitEditor.DeadSplit
{
    public class Attack : Context, IComparable<Attack>, IChildItem<Boss>
    {
        [XmlElement(nameof(Time), typeof(Time))]
        public Time TimeValue { get; set; }

        [XmlAttribute("name")]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(Tooltip));
            }
        }
        private string _name;

        [XmlAttribute("icon")]
        [DefaultValue("Icons/FruitRed.png")]
        public string Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(FullIconPath));
            }
        }
        private string _icon;

        [XmlIgnore]
        public string FullIconPath
        {
            get
            {
                if (Icon != null)
                {
                    return Path.Combine(Parent.Parent.DeadsplitFolder, Icon);
                }
                else
                {
                    return Path.Combine(Parent.Parent.DeadsplitFolder, "Icons/FruitRed.png");

                }
            }
        }

        [XmlElement(nameof(Attack), typeof(Attack))]
        public ObservableCollection<Attack> SubAttacks { get; set; }

        [XmlIgnore] public string Tooltip => string.Format("Attack {0}, Time: {1}", Name, EndTime);

        [XmlIgnore] public decimal EndTime => (TimeValue != null && TimeValue.EndTime.HasValue) ? TimeValue.EndTime.Value : 0;

        [XmlIgnore] public Boss Parent { get => _parent; set => _parent = value; }
        private Boss _parent;

        [XmlIgnore] public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    if (!Parent.IsSelecting)
                    {
                        if (value)
                        {
                            Parent.Select(this);
                        }
                        else
                        {
                            Parent.Select(null);
                        }
                    }
                    NotifyPropertyChanged();
                }
            }
        }
        private bool _isSelected;

        public int CompareTo(Attack other)
        {
            var difference = EndTime - other.EndTime;
            if (difference < 0) return -1;
            if (difference > 0) return 1;
            return 0;
        }
    }
}
