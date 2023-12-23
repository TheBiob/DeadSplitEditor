using DeadSplitEditor.DeadSplit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Xml;
using System.Xml.Serialization;

namespace DeadSplitEditor.DataContext
{
    public class DeadSplitContext : Context
    {
        private const string FANGAME_INFO_FOLDER = "FangameInfo\\";

        public string DeadSplitPath
        {
            get => _DeadSplitPath;
            set
            {
                _DeadSplitPath = value;
                UpdateFangameFolder();
                NotifyPropertyChanged();
            }
        }
        private string _DeadSplitPath;

        public string LayoutFilePath
        {
            get => _fileName;
            set
            {
                _fileName = value;
                NotifyPropertyChanged();
            }
        }
        private string _fileName;

        public List<string> FangameDirectories
        {
            get => _FangameDirectories;
            private set
            {
                _FangameDirectories = value;
                NotifyPropertyChanged();
                UpdateListFilter();
            }
        }
        private List<string> _FangameDirectories;

        public string ListFilter
        {
            get => _ListFilter;
            set
            {
                _ListFilter = value;
                NotifyPropertyChanged();
                UpdateListFilter();
            }
        }
        private string _ListFilter;

        public Fangame Fangame
        {
            get => _Fangame;
            set
            {
                _Fangame = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(FangameLoaded));
            }
        }
        private Fangame _Fangame;

        public bool FangameLoaded => Fangame != null;

        private void UpdateFangameFolder()
        {
            var layoutDir = Path.Join(DeadSplitPath, FANGAME_INFO_FOLDER);
            var newDirectories = new List<string>();

            if (Directory.Exists(layoutDir))
            {
                var directories = Directory.EnumerateDirectories(layoutDir);
                foreach (var dir in directories)
                {
                    if (File.Exists(Path.Join(dir, "layout.xml")))
                    {
                        var splitDirectoryPath = Path.GetDirectoryName(dir + "\\").Split("\\");

                        newDirectories.Add(splitDirectoryPath[splitDirectoryPath.Length - 1]);
                    }
                }
            }

            FangameDirectories = newDirectories;
        }

        private void UpdateListFilter()
        {
            var view = CollectionViewSource.GetDefaultView(FangameDirectories);
            if (string.IsNullOrWhiteSpace(ListFilter))
            {
                view.Filter = null;
            }
            else
            {
                view.Filter = (o) =>
                {
                    return o is string s && s.IndexOf(ListFilter, StringComparison.OrdinalIgnoreCase) >= 0;
                };
            }
        }

        public void LoadLayout(string dir)
        {
            LayoutFilePath = Path.Join(DeadSplitPath, FANGAME_INFO_FOLDER, dir, "layout.xml");

            if (File.Exists(LayoutFilePath))
            {
                try
                {
                    using XmlReader xmlReader = XmlReader.Create(LayoutFilePath);
                    XmlSerializer fangameSerializer = new XmlSerializer(typeof(Fangame));
                    Fangame = fangameSerializer.Deserialize(xmlReader) as Fangame;
                    Fangame.DeadsplitFolder = DeadSplitPath;
                }
                catch (InvalidOperationException e)
                {
                    MessageBox.Show(string.Format("Failed to load layout file {0}\nException: {1}", LayoutFilePath, e.Message), "Couldn't load file", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void SaveLayout()
        {
            if (File.Exists(LayoutFilePath))
            {
                using XmlWriter xmlWriter = XmlWriter.Create(LayoutFilePath, new XmlWriterSettings()
                {
                    Indent = true,
                });
                XmlSerializer fangameSerializer = new XmlSerializer(typeof(Fangame));
                fangameSerializer.Serialize(xmlWriter, Fangame);
            }
        }
    }
}
