using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ProjectExplorer.Models;

namespace ProjectExplorer.Windows
{
    /// <summary>
    /// Interaction logic for OptionsDialog.xaml
    /// </summary>
    public partial class OptionsDialog
    {
        public OptionsDialog()
        {
            InitializeComponent();
        }

        public void SetTags(IEnumerable<ProjectCollectionTag> tags)
        {
            DgTags.ItemsSource = tags;
        }

        private void OkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}