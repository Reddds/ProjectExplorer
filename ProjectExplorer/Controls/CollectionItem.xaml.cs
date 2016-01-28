using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ProjectExplorer.Models;
using ProjectExplorer.Windows;

namespace ProjectExplorer.Controls
{
    /// <summary>
    /// Interaction logic for CollectionItem.xaml
    /// </summary>
    public partial class CollectionItem// : INotifyPropertyChanged
    {
        private readonly List<ProjectCollectionTag> _tagsData;
        private Dictionary<int, ProjectCollectionTag> _tagIds;

        private DispatcherTimer _miniZoom;

        public Action Remove;

        public CollectionItem()
        {
            InitializeComponent();
        }

        public ProjectBase Project { get; }


        public CollectionItem(ProjectBase projectInfo, List<ProjectCollectionTag> tagsData) : this()
        {
            _tagsData = tagsData;
            Project = projectInfo;

            _miniZoom = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };
            _miniZoom.Tick += MiniZoomTick;

            Redraw();
        }

        private void MiniZoomTick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public bool IsSolution => Project is SolutionBase;
        public bool IsNoTagsSet => _tagIds == null || _tagIds.Count == 0;
        public string ProjectName => Project.Name;

        /// <summary>
        /// Обновление тэгов, если их изменили в настройках
        /// </summary>
        public void UpdateTags(List<ProjectCollectionTag> tagsData)
        {
            if (_tagIds != null)
                ShowTags();
        }

        private void ShowTags()
        {
            WpTags.Children.Clear();
            foreach (var tagObj in _tagIds.Values)
            {
                var color = ColorConverter.ConvertFromString(tagObj.Color);
                var newTag = new Label
                {
                    Content = tagObj.Name,
                    Background = new SolidColorBrush((Color?)color ?? Colors.LightGray),

                };
                WpTags.Children.Add(newTag);
            }
        }

        private void ProcessTags(List<ProjectCollectionTag> tagsData, string tagsStr)
        {
            if (!string.IsNullOrWhiteSpace(tagsStr))
            {
                var tags = tagsStr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                _tagIds = new Dictionary<int, ProjectCollectionTag>();
                for (var i = 0; i < tags.Length; i++)
                {
                    var curTag = tags[i].Trim();
                    int tagId;
                    if (!int.TryParse(curTag, out tagId))
                        continue;

                    var tagObj = tagsData.FirstOrDefault(t => t.Id == tagId);
                    if (tagObj == null)
                        continue;

                    _tagIds.Add(tagId, tagObj);
                }

                ShowTags();
            }
        }



        private void ShowFullScreenShotClick(object sender, EventArgs e)
        {
            var curImagePath = Project.ImagePath;
            if (string.IsNullOrEmpty(curImagePath))
            {
                return;
            }
            var screenShotWin = new ScreenShotWindow(curImagePath);
            screenShotWin.ShowDialog();
        }

        public void SetTag(ProjectCollectionTag tag, bool isChecked)
        {
            if (isChecked)
            {
                if (_tagIds == null)
                    _tagIds = new Dictionary<int, ProjectCollectionTag>();
                _tagIds.Add(tag.Id, tag);
            }
            else
            {
                var curTag = _tagIds.FirstOrDefault(t => t.Value == tag);
                _tagIds.Remove(curTag.Key);
            }
            if (_tagIds != null)
            {
                var tagStrs = (from t in _tagIds
                               select t.Key.ToString()).ToArray();
                var tagStr = string.Join(";", tagStrs);
                if (Project != null)
                    Project.Tags = tagStr;
                ShowTags();
            }
        }

        public bool IsTagSet(ProjectCollectionTag tag)
        {
            return _tagIds?.ContainsKey(tag.Id) ?? false;
        }

        /// <summary>
        /// Обновление внешнего вида на основе изменённых данных модели
        /// </summary>
        public void Redraw()
        {
            if (Project is SolutionBase)
                ISolution.Visibility = Visibility.Visible;

            if (!string.IsNullOrEmpty(Project.ReadmePath))
            {
                IReadme.Visibility = Visibility.Visible;
            }

            LName.Content = Project.Name;
            LName.ToolTip = Project.FullPath;

            if (!string.IsNullOrEmpty(Project.ImagePath))
            {
                IScreenshot.Source = new BitmapImage(new Uri(Project.ImagePath));
                //                BShowFullScreenshot.Visibility = Visibility.Visible;
            }

            ProcessTags(_tagsData, Project.Tags);

        }

        /*        public void ShowReadme()
                {
                    var readmePath = Project.ReadmePath;

                    if (string.IsNullOrEmpty(readmePath))
                    {
                        MessageBox.Show("В папке проекта нет файла Readme");
                        return;
                    }

                    var readmeWin = new ReadmeWindow(readmePath);
                    readmeWin.ShowDialog();
                }

                private void ShowReadmeClick(object sender, RoutedEventArgs e)
                {
                    ShowReadme();
                }*/
        /*

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

                public event PropertyChangedEventHandler PropertyChanged;

                public void NotifyPropertyChanged(string propName)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
                }
        */

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (!string.IsNullOrEmpty(Project.ImagePath))
            {
                BShowFullScreenshot.Visibility = Visibility.Visible;
            }
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            BShowFullScreenshot.Visibility = Visibility.Collapsed;
        }
    }
}
