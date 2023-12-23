using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace DeadSplitEditor.CustomCommands
{
    static class CustomCommands
    {
        public static RoutedUICommand TogglePlayPause = new RoutedUICommand(
            "Toggle Play/Pause",
            nameof(TogglePlayPause),
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Space)
            });

        public static RoutedUICommand SkipForward = new RoutedUICommand(
            "Skip forward",
            nameof(SkipForward),
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Right)
            });

        public static RoutedUICommand SkipBackward = new RoutedUICommand(
            "Skip backward",
            nameof(SkipBackward),
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Left)
            });

        public static RoutedUICommand FrameForward = new RoutedUICommand(
            "Go 1 frame forward",
            nameof(FrameForward),
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Right, ModifierKeys.Control)
            });

        public static RoutedUICommand FrameBackward = new RoutedUICommand(
            "Go 1 frame back",
            nameof(FrameBackward),
            typeof(CustomCommands),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Left, ModifierKeys.Control)
            });
    }
}
