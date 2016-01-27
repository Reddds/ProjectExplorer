using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ProjectExplorer.Models;

namespace ProjectExplorer.Windows
{
    /// <summary>
    /// Interaction logic for OptionsDialog.xaml
    /// </summary>
    public partial class OptionsDialog
    {
        private List<ProjectCollectionTag> _original;
        private List<ProjectCollectionTag> _localCopy;

        public OptionsDialog()
        {
            InitializeComponent();
        }

        public void SetTags(List<ProjectCollectionTag> tags)
        {
            _original = tags;
            _localCopy = tags.Select(tag => new ProjectCollectionTag
            {
                Id = tag.Id, Name = tag.Name, Color = tag.Color
            }).ToList();
            
            DgTags.ItemsSource = _localCopy;
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            // Удаляем удалённые тэги
            for (var i = _original.Count - 1; i >= 0; i--)
            {
                var origTag = _original[i];
                if (_localCopy.All(t => t.Id != origTag.Id))
                {
                    _original.Remove(origTag);
                }
            }
            // Заменяем или добавляем новые тэги
            foreach (var tag in _localCopy)
            {
                var found = _original.FirstOrDefault(t => t.Id == tag.Id);
                if (found == null)
                {
                    found = new ProjectCollectionTag {Id = tag.Id};
                    _original.Add(found);
                }
                found.Name = tag.Name;
                found.Color = tag.Color;
            }
            DialogResult = true;
        }

        private void DgTags_OnRowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            var dg = (DataGrid)sender;

            var curRow = e.Row.DataContext as ProjectCollectionTag;
            if (curRow == null)
                return;

/*
            if (e.EditAction == DataGridEditAction.Commit)
            {
                if (curRow.Id == 0)
                {
                    var tags = (List<ProjectCollectionTag>)dg.ItemsSource;
                    curRow.Id = tags.Max(t => t.Id) + 1;

                }
            }
*/
        }

        private void DgTags_OnBeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            var dg = (DataGrid)sender;

            var curRow = e.Row.DataContext as ProjectCollectionTag;

            if (curRow?.Id == 0)
            {
                var tags = (List<ProjectCollectionTag>)dg.ItemsSource;
                curRow.Id = tags.Max(t => t.Id) + 1;
            }
        }
    }
}