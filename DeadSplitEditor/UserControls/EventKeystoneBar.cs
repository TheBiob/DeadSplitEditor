using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;

namespace DeadSplitEditor.UserControls
{
    public class EventKeystoneBar : FrameworkElement
    {
        public event EventHandler<EventKeystone> EventKeystoneClicked;

        public ControlTemplate KeystoneControlTemplate
        {
            get { return (ControlTemplate)GetValue(KeystoneControlTemplateProperty); }
            set { SetValue(KeystoneControlTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for KeystoneControlTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeystoneControlTemplateProperty =
            DependencyProperty.Register("KeystoneControlTemplate", typeof(ControlTemplate), typeof(EventKeystoneBar), new FrameworkPropertyMetadata(null, KeystoneTemplateChanged));

        private static void KeystoneTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EventKeystoneBar bar && e.NewValue is ControlTemplate template)
            {
                bar.OnKeystoneTemplateChanged(e.OldValue, template);
            }
        }

        private void OnKeystoneTemplateChanged(object oldValue, ControlTemplate template)
        {
            var keyCopy = new object[_keystones.Count];
            _keystones.Keys.CopyTo(keyCopy, 0);
            foreach (var o in keyCopy)
            {
                AddEventKeystone(o);
            }
        }

        public IEnumerable<object> Items
        {
            get { return (IEnumerable<object>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(IEnumerable<object>), typeof(EventKeystoneBar), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsArrange, OnItemsChanged));

        private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EventKeystoneBar bar)
            {
                bar.OnItemsPropertyChanged(e.OldValue, e.NewValue);
            }
        }

        private void OnItemsPropertyChanged(object oldValue, object newValue)
        {
            if (oldValue is IEnumerable<object> oldEnumerable)
            {
                if (oldEnumerable is INotifyCollectionChanged oldChangable)
                {
                    oldChangable.CollectionChanged -= new NotifyCollectionChangedEventHandler(OnItemCollectionChanged);
                }
                foreach (var o in _keystones.Values)
                {
                    RemoveEventKeystone(o);
                }
                _keystones.Clear();

            }

            if (newValue is IEnumerable<object> newEnumerable) {
                if (newEnumerable is INotifyCollectionChanged newChangable)
                {
                    newChangable.CollectionChanged += new NotifyCollectionChangedEventHandler(OnItemCollectionChanged);
                }
                foreach (var o in newEnumerable)
                {
                    AddEventKeystone(o);
                }
            }
        }

        private void OnItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (object o in e.NewItems)
                    {
                        AddEventKeystone(o);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (object o in e.OldItems)
                    {
                        RemoveEventKeystone(o);
                    }
                    break;
            }
        }

        /// <summary>
        /// DependencyProperty for <see cref="StartOffset" /> property.
        /// </summary>
        [CommonDependencyProperty]
        public static readonly DependencyProperty StartOffsetProperty = DependencyProperty.Register(
                        "StartOffset",
                        typeof(double),
                        typeof(EventKeystoneBar),
                        new FrameworkPropertyMetadata(
                                0.0d,
                                FrameworkPropertyMetadataOptions.AffectsArrange,
                                new PropertyChangedCallback(OnStartOffsetChanged)),
                                new ValidateValueCallback(IsValidDoubleValue));

        /// <summary>
        /// Validate input value
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Returns False if value is NaN or PositiveInfinity. Otherwise, returns True.</returns>
        private static bool IsValidDoubleValue(object value)
        {
            double d = (double)value;

            return !(double.IsNaN(d) || double.IsInfinity(d));
        }
        private static void OnStartOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is EventKeystoneBar ekb)
            {
                ekb.InvalidateArrange();
            }
        }

        /// <summary>
        /// The Start offset value of the events on the eventbar
        /// </summary>
        public double StartOffset
        {
            get { return (double)GetValue(StartOffsetProperty); }
            set { SetValue(StartOffsetProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for <see cref="Maximum" /> property.
        /// </summary>
        [CommonDependencyProperty]
        public static readonly DependencyProperty MaximumProperty =
                RangeBase.MaximumProperty.AddOwner(typeof(EventKeystoneBar),
                        new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// The Maximum value of the Slider or ScrollBar
        /// </summary>
        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public RepeatButton PreStartRegion
        {
            get => _preStartRegion;
            set
            {
                UpdateComponent(_preStartRegion, value);
                _preStartRegion = value;
            }
        }
        private RepeatButton _preStartRegion;

        public RepeatButton EventArea
        {
            get => _eventArea;
            set
            {
                UpdateComponent(_eventArea, value);
                _eventArea = value;
            }
        }
        private RepeatButton _eventArea;    

        private void UpdateComponent(Visual oldValue, Visual newValue)
        {
            if (oldValue != newValue)
            {
                if (_visualChildren == null)
                {
                    _visualChildren = new List<Visual>();
                }

                if (oldValue != null)
                {
                    RemoveVisualChild(oldValue);

                    if (_visualChildren.Contains(oldValue))
                    {
                        _visualChildren.Remove(oldValue);
                    }
                }

                if (newValue != null)
                {
                    AddVisualChild(newValue);
                    _visualChildren.Add(newValue);
                }

                InvalidateArrange();
            }
        }

        public EventKeystoneBar()
        {
            _keystones = new Dictionary<object, EventKeystone>();
        }

        /// <summary>
        ///   Derived class must implement to support Visual children. The method must return
        ///    the child at the specified index. Index must be between 0 and GetVisualChildrenCount-1.
        ///
        ///    By default a Visual does not have any children.
        ///
        ///  Remark: 
        ///       During this virtual call it is not valid to modify the Visual tree. 
        /// </summary>
        protected override Visual GetVisualChild(int index)
        {
            if (_visualChildren == null || index < 0 || index >= _visualChildren.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index was out of range");
            }
            return _visualChildren[index];
        }

        private void AddEventKeystone(object context)
        {
            if (_keystones == null)
            {
                _keystones = new Dictionary<object, EventKeystone>();
            }

            EventKeystone newKeystone, oldKeystone = null;

            if (KeystoneControlTemplate != null)
            {
                newKeystone = KeystoneControlTemplate.LoadContent() as EventKeystone;
            }
            else
            {
                newKeystone = new EventKeystone();
            }

            newKeystone.MouseDown += Keystone_MouseDown;
            newKeystone.DataContext = context;
            newKeystone.Template = KeystoneControlTemplate;

            if (_keystones.ContainsKey(context))
            {
                oldKeystone = _keystones[context];
                _keystones[context] = newKeystone;
            }
            else
            {
                _keystones.Add(context, newKeystone);
            }
            UpdateComponent(oldKeystone, newKeystone);
        }

        private void Keystone_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is EventKeystone keystone)
            {
                if (e.ButtonState == System.Windows.Input.MouseButtonState.Pressed && e.ChangedButton == System.Windows.Input.MouseButton.Left && e.ClickCount < 2)
                {
                    EventKeystoneClicked?.Invoke(this, keystone);
                }
            }
        }

        private void RemoveEventKeystone(object context)
        {
            if (_keystones != null && _keystones.ContainsKey(context))
            {
                var keystone = _keystones[context];
                UpdateComponent(keystone, null);
                _keystones.Remove(context);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var size = base.ArrangeOverride(finalSize);

            var partRatio = Math.Clamp(StartOffset / Maximum, 0, 1);
            var posX = size.Width * partRatio;

            PreStartRegion?.Arrange(new Rect(new Point(Margin.Left, 0), new Size(posX, size.Height)));

            EventArea?.Arrange(new Rect(new Point(posX, 0), new Size(size.Width - posX, size.Height)));

            if (_keystones != null)
            {
                foreach (EventKeystone keystone in _keystones.Values)
                {
                    var actualValue = keystone.EventOffset + StartOffset;
                    if (actualValue > 0 && actualValue < Maximum)
                    {
                        var ratio = actualValue / Maximum;
                        var xpos = size.Width * ratio;
                        keystone.Visibility = Visibility.Visible;
                        keystone.Arrange(new Rect(new Point(xpos - size.Width / 2, 0), size));
                    }
                    else
                    {
                        keystone.Visibility = Visibility.Collapsed;
                    }
                }
            }

            return size;
        }

        /// <summary>
        ///  Derived classes override this property to enable the Visual code to enumerate 
        ///  the Visual children. Derived classes need to return the number of children
        ///  from this method.
        ///
        ///    By default a Visual does not have any children.
        ///
        ///  Remark: During this virtual method the Visual tree must not be modified.
        /// </summary>        
        protected override int VisualChildrenCount
        {
            get
            {
                if (_visualChildren == null)
                {
                    return 0;
                }
                else return _visualChildren.Count;
            }
        }

        private List<Visual> _visualChildren;
        private Dictionary<object, EventKeystone> _keystones;
    }
}
