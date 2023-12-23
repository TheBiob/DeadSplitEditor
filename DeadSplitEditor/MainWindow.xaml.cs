using DeadSplitEditor.DataContext;
using DeadSplitEditor.DeadSplit;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using DeadSplitEditor.Windows;
using System.CodeDom;
using System.IO;
using System;

namespace DeadSplitEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string DEADSPLIT_FOLDER_CONFIG_FILE = "./deadsplit_path.txt"; // TODO: Add a proper configuration

        public DeadSplitContext ActiveContext
        {
            get => DataContext as DeadSplitContext;
            set => DataContext = value;
        }

        public MainWindow()
        {
            InitializeComponent();
            if (File.Exists(DEADSPLIT_FOLDER_CONFIG_FILE))
            {
                try
                {
                    using var file = File.OpenText(DEADSPLIT_FOLDER_CONFIG_FILE);
                    var path = file.ReadLine();
                    if (!Directory.Exists(path))
                    {
                        throw new Exception(path + " does not exist");
                    }
                    ActiveContext.DeadSplitPath = path;
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Failed to load configuration: " + ex.Message, "DeadSplit Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void SelectDeadSplitFolder_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog ofd = new CommonOpenFileDialog() { IsFolderPicker = true };

            if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                ActiveContext.DeadSplitPath = ofd.FileName;
                try
                {
                    File.WriteAllText(DEADSPLIT_FOLDER_CONFIG_FILE, ofd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to save configuration: " + ex.Message, "DeadSplit Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox box)
            {
                ActiveContext.LoadLayout(box.SelectedItem as string);
            }
        }

        private void OpenBossEditor(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox box && box.SelectedItem is Boss boss)
            {
                ShowAvoidanceEditor(boss);
            }
        }

        private void NewBoss_Click(object sender, RoutedEventArgs e)
        {
            var newBoss = new Boss() { Name = "New Boss" };
            ActiveContext.Fangame.Bosses.Add(newBoss);

            ShowAvoidanceEditor(newBoss);
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            ActiveContext.SaveLayout();
        }

        private void ShowAvoidanceEditor(Boss boss)
        {
            var dialog = new AvoidanceBossEditor() { DataContext = boss };
            Hide();
            dialog.ShowDialog();
            Show();
        }
    }
}
