using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace DeadSplitEditor.DeadSplit
{
    public enum TriggerType
    {
        GameRestart,
        PrevAttackEnd,
        PrevAttackStart,
        AttackEnd,
        AttackStart,
        BossStart,
        ParentStart,
        PrevBossClear,
        AllAttacksClear,
        TimePassed,
        BossShow,
        Null,
        GameSave,
        CounterInitialize,
        AddressChange,
    }

    public class StartTrigger
    {
        [XmlAnyAttribute]
        public XmlAttribute[] Attributes { get; set; }
        [XmlAnyElement]
        public XmlElement[] Elements { get; set; }



        [XmlAttribute("type")]
        public TriggerType Type { get; set; }

        [XmlAttribute("addressName")]
        public string AddressName { get; set; }

        [XmlAttribute("targetValue")]
        public int TargetValue { get; set; }
    }
}
