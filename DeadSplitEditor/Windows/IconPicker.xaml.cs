using DeadSplitEditor.DataContext;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DeadSplitEditor.Windows
{
    /// <summary>
    /// Interaction logic for IconPicker.xaml
    /// </summary>
    public partial class IconPicker : Window
    {
        IconContext Context
        {
            get => DataContext as IconContext;
            set => DataContext = value;
        }

        public IconPicker(string iconFolder)
        {
            InitializeComponent();

            Context = new IconContext(EnsureTrailingSlash(iconFolder));
            Context.Load();
        }

        private static string EnsureTrailingSlash(string path)
        {
            var newPath = path.Trim();
            if (newPath.EndsWith('\\') || newPath.EndsWith('/'))
            {
                return newPath;
            }
            else
            {
                return newPath + '\\';
            }
        }


        public string SelectedIcon;

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2 && sender is Image img && img.DataContext is string path)
            {
                SelectedIcon = path.Substring(Context.IconFolder.Length);
                DialogResult = true;
            }
        }
    }
}
