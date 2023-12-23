using System;
using System.Windows.Input;
using HanumanInstitute.MediaPlayer.Wpf.Mvvm;

namespace DeadSplitEditor.CustomCommands
{
    internal static class CommandHelper
    {
        public static ICommand InitCommand(ref RelayCommand? cmd, Action execute)
        {
            return InitCommand(ref cmd, execute, () => true);
        }

        public static ICommand InitCommand(ref RelayCommand? cmd, Action execute, Func<bool> canExecute)
        {
            return cmd ??= new RelayCommand(execute, canExecute);
        }

        public static ICommand InitCommand<T>(ref RelayCommand<T>? cmd, Action<T> execute)
        {
            return InitCommand(ref cmd, execute, (T _) => true);
        }

        public static ICommand InitCommand<T>(ref RelayCommand<T>? cmd, Action<T> execute, Predicate<T> canExecute)
        {
            return cmd ??= new RelayCommand<T>(execute, canExecute);
        }
    }
}
