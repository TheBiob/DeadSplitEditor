using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using HanumanInstitute.MediaPlayer.Wpf;
using HanumanInstitute.MediaPlayer.Wpf.Mvvm;
using HanumanInstitute.MediaPlayer;
using DeadSplitEditor.CustomCommands;
using HanumanInstitute.MediaPlayer.Wpf.Mpv;

#nullable enable

namespace DeadSplitEditor.UserControls
{
    /// <summary>
    /// A media player graphical interface that can be used with any video host.
    /// </summary>
    [TemplatePart(Name = UIPartName, Type = typeof(Border))]
    [TemplatePart(Name = SeekBarPartName, Type = typeof(Slider))]
    //[TemplatePart(Name = MediaPlayer.SeekBarTrackPartName, Type = typeof(Track))]
    public class AvoidanceMediaPlayer : AvoidanceMediaPlayerBase, INotifyPropertyChanged
    {
        static AvoidanceMediaPlayer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AvoidanceMediaPlayer), new FrameworkPropertyMetadata(typeof(AvoidanceMediaPlayer)));
            BackgroundProperty.OverrideMetadata(typeof(AvoidanceMediaPlayer), new FrameworkPropertyMetadata(Brushes.Black));
            HorizontalAlignmentProperty.OverrideMetadata(typeof(AvoidanceMediaPlayer), new FrameworkPropertyMetadata(HorizontalAlignment.Stretch));
            VerticalAlignmentProperty.OverrideMetadata(typeof(AvoidanceMediaPlayer), new FrameworkPropertyMetadata(VerticalAlignment.Stretch));
            ContentProperty.OverrideMetadata(typeof(AvoidanceMediaPlayer), new FrameworkPropertyMetadata(ContentChanged, CoerceContent));
        }

        public ICommand SetStartOffsetCommand => CommandHelper.InitCommand(ref _setStartOffsetCommand, SetStartOffset, MediaLoaded);
        private RelayCommand? _setStartOffsetCommand;

        private static void ContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => MediaPlayerBase.OnContentChanged(d, e);

        private static object? CoerceContent(DependencyObject d, object baseValue) => baseValue as PlayerHostBase;

        public event PropertyChangedEventHandler? PropertyChanged;

        public const string KeystonesPartName = "PART_AttackKeystones";
        public EventKeystoneBar? KeystoneBar => _keystoneBar ??= (GetTemplateChild(KeystonesPartName) as EventKeystoneBar);
        private EventKeystoneBar? _keystoneBar;

        public const string UIPartName = "PART_UI";
        public Border? UI => _ui ??= (GetTemplateChild(UIPartName) as Border);
        private Border? _ui;

        public const string SeekBarPartName = "PART_SeekBar";
        public Slider? SeekBar => _seekBar ??= (GetTemplateChild(SeekBarPartName) as Slider);
        private Slider? _seekBar;

        public const string SeekBarTrackPartName = "PART_Track";
        public Track? SeekBarTrack => _seekBarTrack ??= (SeekBar?.Template.FindName(SeekBarTrackPartName, SeekBar) as Track);
        private Track? _seekBarTrack;

        public Thumb? SeekBarThumb => _seekBarThumb ??= (SeekBarTrack?.Thumb);
        private Thumb? _seekBarThumb;

        private void SetStartOffset()
        {
            if (KeystoneBar != null && SeekBar != null)
                KeystoneBar.StartOffset = SeekBar.Value;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (DesignerProperties.GetIsInDesignMode(this)) { return; }

            if (UI == null)
            {
                throw new InvalidCastException(string.Format(CultureInfo.InvariantCulture, "TemplateElementNotFound", UIPartName, typeof(Border).Name));
            }
            if (SeekBar == null)
            {
                throw new InvalidCastException(string.Format(CultureInfo.InvariantCulture, "TemplateElementNotFound", SeekBarPartName, typeof(Slider).Name));
            }

            MouseDown += UserControl_MouseDown;
            SeekBar.AddHandler(Slider.PreviewMouseDownEvent, new MouseButtonEventHandler(OnSeekBarPreviewMouseLeftButtonDown), true);
            // Thumb doesn't yet exist.
            SeekBar.Loaded += (s, e) =>
            {
                if (SeekBarThumb == null)
                {
                    throw new InvalidCastException(string.Format(CultureInfo.InvariantCulture, "TemplateElementNotFound", SeekBarTrackPartName, typeof(Track).Name));
                }

                SeekBarThumb.DragStarted += OnSeekBarDragStarted;
                SeekBarThumb.DragCompleted += OnSeekBarDragCompleted;
            };

            if (KeystoneBar != null)
            {
                KeystoneBar.EventKeystoneClicked += KeystoneBar_EventKeystoneClicked;
            }
        }

        private void KeystoneBar_EventKeystoneClicked(object? sender, EventKeystone e)
        {
            if (sender is EventKeystoneBar bar && PlayerHost is MpvPlayerHost host && host.Player != null)
            {
                var eventTime = bar.StartOffset + e.EventOffset;
                host.Player.SeekAsync(eventTime);
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            Focus();
        }

        /// <summary>
        /// Prevents the Host from receiving mouse events when clicking on controls bar.
        /// </summary>
        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
            e.Handled = true;
        }

        protected override void OnContentChanged(DependencyPropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayerHost)));
            // PlayerHost = e.NewValue as PlayerHostBase;
            if (e.OldValue != null)
            {
                ((Control)e.OldValue).MouseDown -= Host_MouseDown;
                ((Control)e.OldValue).MouseWheel -= Host_MouseWheel;
                ((Control)e.OldValue).MouseDoubleClick -= Host_MouseDoubleClick;
            }
            if (e.NewValue != null)
            {
                ((Control)e.NewValue).MouseDown += Host_MouseDown;
                ((Control)e.NewValue).MouseWheel += Host_MouseWheel;
                ((Control)e.NewValue).MouseDoubleClick += Host_MouseDoubleClick;
            }
        }

        private void Host_MouseWheel(object? sender, MouseWheelEventArgs e)
        {
            if (PlayerHost == null) { return; }

            if (ChangeVolumeOnMouseWheel)
            {
                if (e.Delta > 0)
                {
                    PlayerHost.Volume += 5;
                }
                else if (e.Delta < 0)
                {
                    PlayerHost.Volume -= 5;
                }
            }
        }

        private void Host_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
        {
            // ** Double clicks aren't working properly yet.
            HandleMouseAction(sender, e, 2);
        }

        private void Host_MouseDown(object? sender, MouseButtonEventArgs e)
        {
            HandleMouseAction(sender, e, 1);
        }

        /// <summary>
        /// Handles mouse click events for both Host and Fullscreen.
        /// </summary>
        private void HandleMouseAction(object? sender, MouseButtonEventArgs e, int clickCount)
        {
            if (IsActionPause(e, clickCount))
            {
                if (PlayPauseCommand.CanExecute(null))
                {
                    PlayPauseCommand.Execute(null);
                }

                e.Handled = true;
            }
        }

        private bool IsActionPause(MouseButtonEventArgs e, int clickCount) => IsMouseAction(MousePause, e, clickCount);

        private bool IsMouseAction(MouseTrigger a, MouseButtonEventArgs e, int clickCount)
        {
            if (clickCount != TriggerClickCount(a))
            {
                return false;
            }

            if (a == MouseTrigger.LeftClick && e.ChangedButton == MouseButton.Left)
            {
                return true;
            }

            if (a == MouseTrigger.MiddleClick && e.ChangedButton == MouseButton.Middle)
            {
                return true;
            }

            if (a == MouseTrigger.RightClick && e.ChangedButton == MouseButton.Right)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the amount of clicks represented by specified mouse trigger.
        /// </summary>
        private int TriggerClickCount(MouseTrigger a)
        {
            if (a == MouseTrigger.None)
            {
                return 0;
            }
            else if (a == MouseTrigger.LeftClick || a == MouseTrigger.MiddleClick || a == MouseTrigger.RightClick)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }

        private Panel? _uiParentCache;
        /// <summary>
        /// Returns the container of this control the first time it is called and maintain reference to that container.
        /// </summary>
        private Panel UIParentCache
        {
            get
            {
                if (_uiParentCache == null)
                {
                    _uiParentCache = UI?.Parent as Panel;
                }
                if (_uiParentCache == null)
                {
                    throw new NullReferenceException(string.Format(CultureInfo.InvariantCulture, "ParentMustBePanel", UIPartName, UI?.Parent?.GetType()));
                }
                return _uiParentCache;
            }
        }

        // TitleProperty
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(bool), typeof(AvoidanceMediaPlayer));
        public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }

        // MouseFullscreen
        public static readonly DependencyProperty MouseFullscreenProperty = DependencyProperty.Register("MouseFullscreen", typeof(MouseTrigger), typeof(AvoidanceMediaPlayer),
            new PropertyMetadata(MouseTrigger.MiddleClick));
        public MouseTrigger MouseFullscreen { get => (MouseTrigger)GetValue(MouseFullscreenProperty); set => SetValue(MouseFullscreenProperty, value); }

        // MousePause
        public static readonly DependencyProperty MousePauseProperty = DependencyProperty.Register("MousePause", typeof(MouseTrigger), typeof(AvoidanceMediaPlayer),
            new PropertyMetadata(MouseTrigger.LeftClick));
        public MouseTrigger MousePause { get => (MouseTrigger)GetValue(MousePauseProperty); set => SetValue(MousePauseProperty, value); }

        // ChangeVolumeOnMouseWheel
        public static readonly DependencyProperty ChangeVolumeOnMouseWheelProperty = DependencyProperty.Register("ChangeVolumeOnMouseWheel", typeof(bool), typeof(AvoidanceMediaPlayer),
            new PropertyMetadata(true));
        public bool ChangeVolumeOnMouseWheel { get => (bool)GetValue(ChangeVolumeOnMouseWheelProperty); set => SetValue(ChangeVolumeOnMouseWheelProperty, value); }

        // IsPlayPauseVisible
        public static readonly DependencyProperty IsPlayPauseVisibleProperty = DependencyProperty.Register("IsPlayPauseVisible", typeof(bool), typeof(AvoidanceMediaPlayer),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsParentArrange));
        public bool IsPlayPauseVisible { get => (bool)GetValue(IsPlayPauseVisibleProperty); set => SetValue(IsPlayPauseVisibleProperty, value); }

        // IsStopVisible
        public static readonly DependencyProperty IsStopVisibleProperty = DependencyProperty.Register("IsStopVisible", typeof(bool), typeof(AvoidanceMediaPlayer),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsParentArrange));
        public bool IsStopVisible { get => (bool)GetValue(IsStopVisibleProperty); set => SetValue(IsStopVisibleProperty, value); }

        // IsLoopVisible
        public static readonly DependencyProperty IsLoopVisibleProperty = DependencyProperty.Register("IsLoopVisible", typeof(bool), typeof(AvoidanceMediaPlayer),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsParentArrange));
        public bool IsLoopVisible { get => (bool)GetValue(IsLoopVisibleProperty); set => SetValue(IsLoopVisibleProperty, value); }

        // IsVolumeVisible
        public static readonly DependencyProperty IsVolumeVisibleProperty = DependencyProperty.Register("IsVolumeVisible", typeof(bool), typeof(AvoidanceMediaPlayer),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsParentArrange));
        public bool IsVolumeVisible { get => (bool)GetValue(IsVolumeVisibleProperty); set => SetValue(IsVolumeVisibleProperty, value); }

        // IsSpeedVisible
        public static readonly DependencyProperty IsSpeedVisibleProperty = DependencyProperty.Register("IsSpeedVisible", typeof(bool), typeof(AvoidanceMediaPlayer),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsParentArrange));
        public bool IsSpeedVisible { get => (bool)GetValue(IsSpeedVisibleProperty); set => SetValue(IsSpeedVisibleProperty, value); }

        // IsSeekBarVisible
        public static readonly DependencyProperty IsSeekBarVisibleProperty = DependencyProperty.Register("IsSeekBarVisible", typeof(bool), typeof(AvoidanceMediaPlayer),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsParentArrange));
        public bool IsSeekBarVisible { get => (bool)GetValue(IsSeekBarVisibleProperty); set => SetValue(IsSeekBarVisibleProperty, value); }



        public void OnSeekBarPreviewMouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            if (PlayerHost == null) { return; }
            //e.CheckNotNull(nameof(e));

            // Only process event if click is not on thumb.
            if (SeekBarThumb != null)
            {
                var pos = e.GetPosition(SeekBarThumb);
                if (pos.X < 0 || pos.Y < 0 || pos.X > SeekBarThumb.ActualWidth || pos.Y > SeekBarThumb.ActualHeight)
                {
                    // Immediate seek when clicking elsewhere.
                    IsSeekBarPressed = true;
                    PlayerHost.Position = PositionBar;
                    IsSeekBarPressed = false;
                }
            }
        }

        public void OnSeekBarDragStarted(object? sender, DragStartedEventArgs e)
        {
            if (PlayerHost == null) { return; }

            IsSeekBarPressed = true;
        }

        private DateTime _lastDragCompleted = DateTime.MinValue;
        public void OnSeekBarDragCompleted(object? sender, DragCompletedEventArgs e)
        {
            if (PlayerHost == null) { return; }

            // DragCompleted can trigger multiple times after switching to/from fullscreen. Ignore multiple events within a second.
            if ((DateTime.Now - _lastDragCompleted).TotalSeconds < 1)
            {
                return;
            }

            _lastDragCompleted = DateTime.Now;

            PlayerHost.Position = PositionBar;
            IsSeekBarPressed = false;
        }

        private static void TransferElement(Panel src, Panel dst, FrameworkElement element)
        {
            src.Children.Remove(element);
            dst.Children.Add(element);
        }
    }
}
