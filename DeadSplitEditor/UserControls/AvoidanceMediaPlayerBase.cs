using HanumanInstitute.MediaPlayer.Wpf;
using HanumanInstitute.MediaPlayer.Wpf.Mvvm;
using HanumanInstitute.MediaPlayer.Wpf.Mpv;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Input;
using DeadSplitEditor.CustomCommands;

namespace DeadSplitEditor.UserControls
{
    public class AvoidanceMediaPlayerBase : MediaPlayerBase
    {
        public MpvPlayerHost MpvHost => PlayerHost as MpvPlayerHost;

        public ICommand NextFrameCommand => CommandHelper.InitCommand(ref _frameAdvanceCommand, NextFrame, MediaLoaded);
        private RelayCommand _frameAdvanceCommand;
        public ICommand PreviousFrameCommand => CommandHelper.InitCommand(ref _frameBackwardCommand, PreviousFrame, MediaLoaded);
        private RelayCommand _frameBackwardCommand;
        public ICommand SkipForwardCommand => CommandHelper.InitCommand(ref _skipForwardCommand, SkipForward, MediaLoaded);
        private RelayCommand _skipForwardCommand;
        public ICommand SkipBackwardCommand => CommandHelper.InitCommand(ref _skipBackwardCommand, SkipBackward, MediaLoaded);
        private RelayCommand _skipBackwardCommand;

        protected bool MediaLoaded() => PlayerHost?.IsMediaLoaded ?? false;

        private void NextFrame()
        {
            MpvHost.Player.NextFrame();
        }
        private void PreviousFrame()
        {
            MpvHost.Player.PreviousFrame();
        }
        private void SkipForward()
        {
            PlayerHost.Position = PlayerHost.Position.Add(new TimeSpan(0, 0, 10));
        }
        private void SkipBackward()
        {
            PlayerHost.Position = PlayerHost.Position.Add(new TimeSpan(0, 0, -10));
        }

        //protected override string FormatTime(TimeSpan t)
        //{
        //    if (PositionDisplay == TimeDisplayStyle.Standard)
        //    {
        //        if (t.TotalHours >= 1)
        //        {
        //            return t.ToString("h\\:mm\\:ss\\.ff", CultureInfo.InvariantCulture);
        //        }
        //        else
        //        {
        //            return t.ToString("m\\:ss\\.ff", CultureInfo.InvariantCulture);
        //        }
        //    }
        //    else if (PositionDisplay == TimeDisplayStyle.Seconds)
        //    {
        //        return t.TotalSeconds.ToString(CultureInfo.InvariantCulture);
        //    }
        //    else
        //    {
        //        return string.Empty;
        //    }
        //}
    }
}
