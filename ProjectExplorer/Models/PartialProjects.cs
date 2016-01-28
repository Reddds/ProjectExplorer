using System.ComponentModel;
using System.Xml.Serialization;

namespace ProjectExplorer.Models
{
   
    public partial class ProjectBase : INotifyPropertyChanged
    {
        [XmlIgnore]
        public FolderItem Folder { get; set; }


        private bool _isSelected;
        [XmlIgnore]
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

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

    }
}
