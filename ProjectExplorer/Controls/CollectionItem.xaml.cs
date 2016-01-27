using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ProjectExplorer.Models;
using ProjectExplorer.Windows;

namespace ProjectExplorer.Controls
{
    /// <summary>
    /// Interaction logic for CollectionItem.xaml
    /// </summary>
    public partial class CollectionItem
    {
        private readonly ProjectBase _projectInfo;
        private Dictionary<int, ProjectCollectionTag> _tagIds;

        public Action Remove;

        public CollectionItem()
        {
            InitializeComponent();
        }

        public CollectionItem(ProjectBase projectInfo, List<ProjectCollectionTag> tagsData) :this()
        {
            _projectInfo = projectInfo;
            if(_projectInfo is SolutionBase)
                ISolution.Visibility = Visibility.Visible;

            LName.Content = _projectInfo.Name;
            LName.ToolTip = _projectInfo.FullPath;

            if (!string.IsNullOrEmpty(_projectInfo.ImagePath))
            {
                IScreenshot.Source = new BitmapImage(new Uri(_projectInfo.ImagePath));
                BShowFullScreenshot.Visibility = Visibility.Visible;
            }

            ProcessTags(tagsData, _projectInfo.Tags);

        }

        public bool IsSolution => _projectInfo is SolutionBase;
        public bool IsNoTagsSet => _tagIds == null || _tagIds.Count == 0;
        public string ProjectName => _projectInfo.Name;

        /// <summary>
        /// Обновление тэгов, если их изменили в настройках
        /// </summary>
        public void UpdateTags(List<ProjectCollectionTag> tagsData)
        {
            if(_tagIds != null)
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
                var tags = tagsStr.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
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
            var curImagePath = _projectInfo.ImagePath;
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
                if (_projectInfo != null)
                    _projectInfo.Tags = tagStr;
                ShowTags();
            }
        }

        public bool IsTagSet(ProjectCollectionTag tag)
        {
            return _tagIds?.ContainsKey(tag.Id) ?? false;
        }
    }
}
