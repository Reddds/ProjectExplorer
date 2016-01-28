using System.Collections.Generic;
using System.ComponentModel;
using ProjectExplorer.Models;

namespace ProjectExplorer
{
    public class FolderItem : INotifyPropertyChanged
    {
        public FolderItem Parent { get; set; }

        public string Name { get; set; }
        public List<FolderItem> SubDirs { get; set; }
        public List<ProjectBase> Projects { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    NotifyPropertyChanged("IsExpanded");
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
