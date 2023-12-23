using DeadSplitEditor.DeadSplit;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DeadSplitEditor.Extensions;
using DeadSplitEditor.UserControls;
using DeadSplitEditor.Converter;
using System.ComponentModel;
using DeadSplitEditor.CustomCommands;
using HanumanInstitute.MediaPlayer.Wpf.Mvvm;
using System.Threading;

namespace DeadSplitEditor.Windows
{
    /// <summary>
    /// Interaction logic for AvoidanceBossEditor.xaml
    /// </summary>
    public partial class AvoidanceBossEditor : Window, INotifyPropertyChanged
    {
        private Boss Boss => DataContext as Boss;

        public bool RoundToNearestFrameValue
        {
            get => _roundToNearestFrameValue;
            set
            {
                _roundToNearestFrameValue = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RoundToNearestFrameValue)));
            }
        }
        private bool _roundToNearestFrameValue;

        public ICommand SetStartOffsetAttackCommand => CommandHelper.InitCommand(ref _setStartOffsetAttackCommand, SetStartOffsetAttackExecuted, CanSetStartOffsetAttack);
        private RelayCommand? _setStartOffsetAttackCommand;

        public ICommand NewAttackCommand => CommandHelper.InitCommand(ref _newAttackCommand, NewAttackCommandExecuted);
        private RelayCommand _newAttackCommand;

        public ICommand MoveSelectedAttackCommand => CommandHelper.InitCommand(ref _moveSelectedAttackCommand, MoveSelectedAttackCommandExecuted, CanExecuteMoveSelectedAttackCommand);
        private RelayCommand _moveSelectedAttackCommand;

        public ICommand NextAttackCommand => CommandHelper.InitCommand(ref _nextAttackCommand, NextAttackCommandExecuted);
        private RelayCommand _nextAttackCommand;

        public ICommand PreviousAttackCommand => CommandHelper.InitCommand(ref _previousAttackCommand, PreviousAttackCommandExecuted);
        private RelayCommand _previousAttackCommand;

        public ICommand MoveToSelectedAttackCommand => CommandHelper.InitCommand(ref _moveToSelectedAttackCommand, MoveToSelectedAttackCommandExecuted, CanExecuteMoveSelectedAttackCommand);
        private RelayCommand _moveToSelectedAttackCommand;

        public ICommand UnfocusAllCommand => CommandHelper.InitCommand(ref _unfocusAllCommand, UnfocusAllCommandExecuted);
        private RelayCommand _unfocusAllCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        public AvoidanceBossEditor()
        {
            InitializeComponent();
        }

        private bool CanExecuteMoveSelectedAttackCommand()
        {
            return Player.PlayerHost.IsMediaLoaded && Boss.SelectedAttack != null;
        }

        private void MoveSelectedAttackCommandExecuted()
        {
            var value = (decimal)Player.SeekBar.Value - (decimal)Player.KeystoneBar.StartOffset;
            if (RoundToNearestFrameValue)
            {
                Boss.SelectedAttack.TimeValue.EndTime = EndTimeConverter.RoundToNearestValue(value);
            }
            else
            {
                Boss.SelectedAttack.TimeValue.EndTime = value;
            }
        }

        private void MoveToSelectedAttackCommandExecuted()
        {
            if (Boss.SelectedAttack != null)
            {
                var time = Player.KeystoneBar.StartOffset + (double)Boss.SelectedAttack.EndTime;
                if (time >= 0 && time <= Player.MpvHost.Duration.TotalSeconds)
                {
                    Player.MpvHost.Player.SeekAsync(time);
                }
            }
        }

        private void SetStartOffsetAttackExecuted()
        {
            if (Player.KeystoneBar != null && Player.SeekBar != null)
                Player.KeystoneBar.StartOffset = Player.SeekBar.Value - (double)Boss.SelectedAttack.EndTime;
        }

        private bool CanSetStartOffsetAttack()
        {
            return (PlayerHost?.IsMediaLoaded ?? false) && Boss.SelectedAttack != null;
        }

        private void NextAttackCommandExecuted()
        {
            var newIndex = Boss.BossAttacks.IndexOf(Boss.SelectedAttack) + 1;
            if (newIndex >= Boss.BossAttacks.Count)
            {
                newIndex = 0;
            }

            Boss.BossAttacks[newIndex].IsSelected = true;
        }

        private void PreviousAttackCommandExecuted()
        {
            var newIndex = Boss.BossAttacks.IndexOf(Boss.SelectedAttack) - 1;
            if (newIndex < 0)
            {
                newIndex = Boss.BossAttacks.Count - 1;
            }

            Boss.BossAttacks[newIndex].IsSelected = true;
        }

        private void NewAttackCommandExecuted()
        {
            var attack = new Attack() { Name = "New Attack", TimeValue = new Time() };
            var actualTime = (decimal)Player.SeekBar.Value - (decimal)Player.KeystoneBar.StartOffset;
            if (actualTime >= 0)
            {
                attack.TimeValue.EndTime = actualTime;
            }

            Boss.BossAttacks.Add(attack);
            Boss.BossAttacks.Sort();
            attack.IsSelected = true;
        }

        private void UnfocusAllCommandExecuted()
        {
            Keyboard.Focus(Player);
        }

        private void LoadVideo_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Media Files | *.mp4;*.flv";
            if (ofd.ShowDialog() == true)
            {
                Player.PlayerHost.Source = ofd.FileName;
                // NOTE: There is a bug where MpvPlayerHost does not implement SourceChanged correctly and therefore doesn't reload the video automatically.
                // Once that is fixed and updated the below code should be removed, but until that happaned, run the code to load the video manually.
                Player.MpvHost.Player.Stop();
                if (!string.IsNullOrEmpty(ofd.FileName))
                {
                    //_initLoaded = true;
                    Thread.Sleep(10);
                    Player.MpvHost.Player.Load(ofd.FileName, true);
                }
            }
        }

        private void Attack_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (sender is Border border && border.DataContext is Attack attack)
                {
                    if (e.ClickCount >= 2)
                    {
                        if (Player.PlayerHost.IsMediaLoaded)
                        {
                            var eventTime = Player.KeystoneBar.StartOffset + (double)attack.EndTime;
                            PlayerHost.Player.SeekAsync(eventTime);
                        }
                    }
                    else
                    {
                        attack.IsSelected = !attack.IsSelected;
                    }
                }
            }
        }

        private void RemoveAttack_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b && b.DataContext is Attack attackToRemove)
            {
                Boss.BossAttacks.Remove(attackToRemove);
            }
        }

        private void MediaPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is AvoidanceMediaPlayer player)
            {
                player.KeystoneBar.EventKeystoneClicked += KeystoneBar_EventKeystoneClicked;
            }
        }

        private void KeystoneBar_EventKeystoneClicked(object sender, EventKeystone e)
        {
            if (e.DataContext is Attack attack)
            {
                attack.IsSelected = true;
            }
        }

        private void AttackImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right && sender is FrameworkElement dpo && dpo.DataContext is Attack attack)
            {
                e.Handled = true;

                var deadsplitFolder = attack.Parent.Parent.DeadsplitFolder;
                var iconPicker = new IconPicker(deadsplitFolder);
                if (iconPicker.ShowDialog() == true)
                {
                    attack.Icon = iconPicker.SelectedIcon;
                }
            }
        }

        private void ApplyBoss(object sender, RoutedEventArgs e)
        {
            if (RoundToNearestFrameValue)
            {
                foreach (var attack in Boss.BossAttacks)
                {
                    if (attack.TimeValue != null && attack.TimeValue.EndTime.HasValue)
                    {
                        attack.TimeValue.EndTime = EndTimeConverter.RoundToNearestValue(attack.TimeValue.EndTime.Value);
                    }
                }
            }
            DialogResult = true;
        }
    }
}
