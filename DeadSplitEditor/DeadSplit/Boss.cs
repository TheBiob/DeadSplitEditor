using DeadSplitEditor.DataContext;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Xml.Serialization;

using DeadSplitEditor.Extensions;

namespace DeadSplitEditor.DeadSplit
{
    public class Boss : Context, IChildItem<Fangame>
    {
        [XmlAttribute("name")]
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                NotifyPropertyChanged();
            }
        }
        private string _name;

        [XmlElement(nameof(Attack), typeof(Attack))]
        public ChildItemCollection<Boss, Attack> BossAttacks { get; private set; }
        
        [XmlElement(nameof(StartTrigger), typeof(StartTrigger))]
        public StartTrigger StartTrigger { get; set; }

        [XmlIgnore] public Fangame Parent { get => _parent; set => _parent = value; }
        [XmlIgnore] public bool IsSelecting { get; private set; }
        [XmlIgnore] public Attack SelectedAttack { get; private set; }

        private Fangame _parent;

        /// <summary>
        /// Sets the selected attack
        /// </summary>
        /// <param name="attack">The attack to select</param>
        /// <returns>true if the provided attack got the selected status. Returns false if the attack is already the selected one or not found in the bosses collection</returns>
        public bool Select(Attack attack)
        {
            SelectedAttack = attack;

            if (BossAttacks.Contains(attack))
            {
                IsSelecting = true;
                foreach (Attack curAttack in BossAttacks)
                {
                    curAttack.IsSelected = curAttack.Equals(attack);
                }
                IsSelecting = false;
            }

            return false;
        }

        public Boss()
        {
            BossAttacks = new ChildItemCollection<Boss, Attack>(this);
        }
    }
}
