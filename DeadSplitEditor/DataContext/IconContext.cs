using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Documents.DocumentStructures;

namespace DeadSplitEditor.DataContext
{
    class IconContext : Context
    {
        private const string ICON_DIRECTORY = "Icons/";

        public List<IconCollection> IconCollections
        {
            get => _iconCollections;
            set
            {
                _iconCollections = value;
                NotifyPropertyChanged();
            }
        }
        private List<IconCollection> _iconCollections;
        public string IconFolder { get; set; }

        public IconContext(string iconFolder)
        {
            IconFolder = iconFolder;
            IconCollections = new List<IconCollection>();
        }

        public void Load()
        {
            var directories = Directory.GetDirectories(IconFolder, ICON_DIRECTORY + "*", new EnumerationOptions() { RecurseSubdirectories = true });
            AddDirectory(Path.Combine(IconFolder, ICON_DIRECTORY));
            foreach (var directory in directories)
            {
                AddDirectory(directory);
            }
            NotifyPropertyChanged(nameof(IconCollections));
        }

        /// <summary>
        /// Adds a directory to the IconCollection if it contains any .png files
        /// </summary>
        /// <param name="dir">The directory to check</param>
        private void AddDirectory(string dir)
        {
            if (Directory.Exists(dir))
            {
                var name = Path.GetDirectoryName(dir + "/").Substring(IconFolder.Length).TrimStart('/', '\\', ' ');

                var collection = new IconCollection(dir, name);
                if (collection.Load())
                {
                    IconCollections.Add(collection);
                }
            }
        }
    }

    class IconCollection : Context
    {
        private string collectionPath;

        public string CollectionName
        {
            get => _collectionName;
            set
            {
                _collectionName = value;
                NotifyPropertyChanged();
            }
        }
        private string _collectionName;

        public ObservableCollection<string> IconPaths
        {
            get => _iconPaths;
            set
            {
                _iconPaths = value;
                NotifyPropertyChanged();
            }
        }
        private ObservableCollection<string> _iconPaths;

        public IconCollection(string path, string name)
        {
            collectionPath = path;
            CollectionName = name;
            IconPaths = new ObservableCollection<string>();
        }

        /// <summary>
        /// Loads all .png files in the specified folder
        /// </summary>
        /// <returns>True if the folder contained any .png files</returns>
        public bool Load()
        {
            var icons = Directory.GetFiles(collectionPath, "*.png");
            if (icons.Length > 0)
            {
                foreach (var icon in icons)
                {
                    IconPaths.Add(icon);
                }
                return true;
            }
            return false;
        }
    }
}
