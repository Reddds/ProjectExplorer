using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ProjectExplorer.Controls;
using ProjectExplorer.Models;
using ProjectExplorer.Windows;


namespace ProjectExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const string CollectionFileName = "projectCollection.xml";
        private string _rootDirPath;
        readonly ProjectCollection _projectCollection;
        private readonly CollectionViewSource _viewCollection;
        private readonly List<ListViewItem> _listViewItems;
        /// <summary>
        /// Искать по тегам, чтобы все теги были включены (true), или нет
        /// </summary>
        private bool _tagsAnd = false;

        private string _findText = null;

        public MainWindow()
        {
            InitializeComponent();

            _listViewItems = new List<ListViewItem>();
            _viewCollection = new CollectionViewSource();
            _viewCollection.Filter += ViewCollectionFilter;

            if (File.Exists(CollectionFileName))
            {
                _projectCollection = ProjectCollection.LoadFromFile(CollectionFileName);
                TbRootProjectDir.Text = _projectCollection.RootDir;
                ShowCollection();
                ShowTags();
            }
            else
            {
                TbRootProjectDir.Text = Properties.Settings.Default.RootPath;
                _projectCollection = new ProjectCollection();
            }
            _viewCollection.Source = _listViewItems;
            LvProjects.ItemsSource = _viewCollection.View;

        }

        private void ViewCollectionFilter(object sender, FilterEventArgs e)
        {
            var lvi = e.Item as ListViewItem;

            var collectionItem = lvi?.Content as CollectionItem;
            if (collectionItem == null)
            {
                e.Accepted = false;
                return;
            }
            if (collectionItem.IsSolution && CbShowSolutions.IsChecked != true)
            {
                e.Accepted = false;
                return;
            }
            if (!collectionItem.IsSolution && CbShowProjects.IsChecked != true)
            {
                e.Accepted = false;
                return;
            }

            if (CbShowTagLess.IsChecked == true)
            {
                if (!collectionItem.IsNoTagsSet)
                {
                    e.Accepted = false;
                    return;
                }
                return;
            }


            var noTagsChecked = WpTags.Children.Cast<CheckBox>().All(checkBox => checkBox.IsChecked != true);

            // Если не выделена ни одна метка, то показываем всё
            if (!noTagsChecked)
            {

                if (_tagsAnd)
                {
                    if (
                        WpTags.Children.Cast<CheckBox>()
                            .Any(
                                cbTag =>
                                    cbTag.IsChecked == true &&
                                    !collectionItem.IsTagSet((ProjectCollectionTag)cbTag.Tag)))
                    {
                        e.Accepted = false;
                        return;
                    }
                }
                else
                {
                    if (
                        WpTags.Children.Cast<CheckBox>()
                            .All(
                                cbTag =>
                                    cbTag.IsChecked != true || !collectionItem.IsTagSet((ProjectCollectionTag)cbTag.Tag)))
                    {
                        e.Accepted = false;
                        return;
                    }
                }
            }

            if (!string.IsNullOrEmpty(_findText))
            {
                
                if (!collectionItem.ProjectName.ToLower().Contains(_findText))
                {
                    e.Accepted = false;
                    return;
                }
            }

            //e.Accepted = false;
        }

        public void RefreshView()
        {
            _viewCollection?.View.Refresh();
        }

        private void ShowTags()
        {
            WpTags.Children.Clear();


            foreach (var tag in _projectCollection.Tags)
            {
                var content = new Label { Content = tag.Name };
                var color = ColorConverter.ConvertFromString(tag.Color);
                if (color != null)
                    content.Background = new SolidColorBrush((Color)color);
                var cbTag = new CheckBox
                {
                    Content = content,
                    Tag = tag,
                    Style = FindResource("TagCheckBoxStyle") as Style
                };
                cbTag.Click += (sender, args) =>
                {
                    RefreshView();
                };
                WpTags.Children.Add(cbTag);

            }
        }

        private void ShowTagsMenu()
        {
            // Зачищаем меню от тэгов
            for (var i = CmMain.Items.Count - 1; i >= 0; i--)
            {
                var curMenuItem = CmMain.Items[i] as MenuItem;
                if (curMenuItem?.Tag != null && ((int)curMenuItem.Tag == 100 || (int)curMenuItem.Tag == 101))
                {
                    CmMain.Items.Remove(curMenuItem);
                }
            }
            foreach (var tag in _projectCollection.Tags)
            {
                // Во всех ли выделенных проектах установлен данный тэг
                var projectsWhereTagSet = (from ListViewItem lvi in LvProjects.SelectedItems select (CollectionItem)lvi.Content).Count(collectionItem => collectionItem.IsTagSet(tag));


                var color = ColorConverter.ConvertFromString(tag.Color);
                var tagMenuItem = new MenuItem
                {
                    Header = tag.Name,
                    IsCheckable = true,
                    Tag = 100// Чтобы потом поудалять при обновлении тэгов
                };
                if (color != null)
                {
                    if (projectsWhereTagSet > 0 && projectsWhereTagSet < LvProjects.SelectedItems.Count)
                    {
                        var vb = (VisualBrush)FindResource("SomeTagsBrush");
                        var grid = (Grid)vb.Visual;
                        grid.Background = new SolidColorBrush((Color)color);
                        tagMenuItem.Background = vb;
                    }
                    else
                    {
                        if (projectsWhereTagSet == LvProjects.SelectedItems.Count)
                            tagMenuItem.IsChecked = true;
                        tagMenuItem.Background = new SolidColorBrush((Color)color);
                    }
                    //                    tagMenuItem.Background = new SolidColorBrush((Color)color);
                }
                //tagMenuItem.Style = FindResource("SomeTagsStyle") as Style;
                tagMenuItem.Click += (sender, args) =>
                {
                    var mi = (MenuItem)sender;
                    foreach (ListViewItem lvi in LvProjects.SelectedItems)
                    {
                        var collectionItem = lvi.Content as CollectionItem;

                        collectionItem?.SetTag(tag, mi.IsChecked);
                    }
                };
                CmMain.Items.Add(tagMenuItem);

            }
        }

        private ListViewItem AddNewItemInView(FrameworkElement item, object dataItem)
        {
            //LvProjects.Items
            var lvi = new ListViewItem
            {
                Content = item,
                Tag = dataItem
            };
            _listViewItems.Add(lvi);
            return lvi;
        }

        private void ShowCollection()
        {
            _listViewItems.Clear();
            //            LvProjects.Items.Clear();

            foreach (var solution in _projectCollection.Solution)
            {
                var item = new CollectionItem(solution, _projectCollection.Tags);
                var lvi = AddNewItemInView(item, solution);

                item.Remove = () =>
                {
                    _projectCollection.Solution.Remove(solution);
                    _listViewItems.Remove(lvi);
                    //LvProjects.Items.Remove(item);
                };

                _existProjects.Add(solution.FullPath.ToLower());
            }
            foreach (var project in _projectCollection.Project)
            {
                var item = new CollectionItem(project, _projectCollection.Tags);
                var lvi = AddNewItemInView(item, project);

                item.Remove = () =>
                {
                    _projectCollection.Project.Remove(project);
                    _listViewItems.Remove(lvi);
                    //LvProjects.Items.Remove(item);
                };

                _existProjects.Add(project.FullPath.ToLower());
            }
        }

        private readonly HashSet<string> _existProjects = new HashSet<string>();
        //private StringBuilder _foun;

        private class FilesInFolder
        {
            public FileInfo Readme { get; set; }
            public FileInfo Screenshot { get; set; }
        }

        private static void ScanFolder(ProjectBase project, DirectoryInfo parentDir)
        {
            var filesInFolder = new FilesInFolder();
            var readmes = parentDir.GetFiles("readme.md");
            if (readmes.Length > 0)
            {
                filesInFolder.Readme = readmes[0];
            }

            var screenshots = parentDir.GetFiles("screenshot.png");
            if (screenshots.Length > 0)
            {
                filesInFolder.Screenshot = screenshots[0];
            }

            if (filesInFolder.Readme != null)
            {
                project.ReadmePath = filesInFolder.Readme.FullName;
                project.Value = File.ReadAllText(filesInFolder.Readme.FullName);
            }
            if (filesInFolder.Screenshot != null)
                project.ImagePath = filesInFolder.Screenshot.FullName;
        }

        private void TreeScan(DirectoryInfo parentDir, BackgroundWorker curWorker)
        {
            curWorker.ReportProgress(0, parentDir.FullName);


            //            var existDir = _projectCollection.Directory.FirstOrDefault(d => d.Name == dirName);
            foreach (var f in parentDir.GetFiles("*.sln"))
            {
                // Если проект уже найден
                if (_existProjects.Contains(f.FullName.ToLower()))
                    continue;
                /*var dirName = parentDir.FullName;
                dirName = NormalizePath(dirName);


                    if (dirName == _rootDirPath)
                    {

                    }
                    else
                    {
                        var relativeDir = dirName.Substring(_rootDirPath.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                        // Ищем среди каталогов нужный, если он существует
                        if (existDir != null)
                        {
                        }
                        else // если родительский каталог не найдн, создаём его
                        {
                            var directories = relativeDir.Split(Path.DirectorySeparatorChar);
                            var createdPath = string.Empty;
                            ProjectCollectionDirectory lastDir = null;
                            foreach (var directory in directories)
                            {
                                if (string.IsNullOrEmpty(createdPath))
                                    createdPath = directory;
                                else
                                    createdPath = Path.Combine(createdPath, directory);
                                var existParentDir = _projectCollection.Directory.FirstOrDefault(d => d.Path == createdPath);
                                if (existParentDir == null)
                                {

                                    var newDir = new ProjectCollectionDirectory
                                    {
                                        Id = createdPath.GetHashCode(),//(ulong) ((DateTime.Now.ToBinary() % 1000000000000)),
                                        Name = directory,
                                        Path = createdPath,
                                    };
                                    if (lastDir != null)
                                        newDir.ParentId = lastDir.Id;
                                    else
                                        newDir.ParentId = -1;

                                    _projectCollection.Directory.Add(newDir);
                                    lastDir = newDir;
                                }
                                else
                                    lastDir = existParentDir;
                            }

                            existDir = lastDir;
                        }
                    }*/
                //Debug.Assert(existDir != null);
                var newSolution = new SolutionBase
                {
                    CategoryId = -1,//existDir?.Id ?? 
                    Name = f.Name,
                    FullPath = f.FullName,

                };
                ScanFolder(newSolution, parentDir);

                _projectCollection.Project.Add(newSolution);
                _existProjects.Add(newSolution.FullPath.ToLower());

                //TbTest.Text += f.FullName + Environment.NewLine;
            }
            foreach (var f in parentDir.GetFiles("*.csproj"))
            {
                if (_existProjects.Contains(f.FullName.ToLower()))
                    continue;

                //Debug.Assert(existDir != null);
                var newProject = new ProjectBase
                {
                    CategoryId = -1,//existDir?.Id ?? 
                    Name = f.Name,
                    FullPath = f.FullName,

                };
                ScanFolder(newProject, parentDir);

                _projectCollection.Project.Add(newProject);
                _existProjects.Add(newProject.FullPath.ToLower());

            }
            foreach (var d in parentDir.GetDirectories())
            {
                //var relativeDir = d.FullName.Substring(_rootDirPath.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                TreeScan(d, curWorker);
            }
        }

        private void ScanFoldersClick(object sender, RoutedEventArgs e)
        {
            if (
                MessageBox.Show("Просканировать каталоги в поиске проектов и решений?", "Запуск сканирования",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            _rootDirPath = TbRootProjectDir.Text.Trim();
            Properties.Settings.Default.RootPath = _rootDirPath;
            Properties.Settings.Default.Save();

            if (string.IsNullOrEmpty(_rootDirPath))
            {
                MessageBox.Show("Введите корневой каталог!");
                return;
            }
            _rootDirPath = NormalizePath(_rootDirPath);

            _projectCollection.RootDir = _rootDirPath;

            BiLoading.IsBusy = true;

            var worker = new BackgroundWorker { WorkerReportsProgress = true };

            worker.RunWorkerCompleted += (o, args) =>
            {
                SpScanning.Visibility = Visibility.Hidden;
                SaveCollection();

                ShowCollection();
                BiLoading.IsBusy = false;
            };
            worker.ProgressChanged += (o, args) =>
            {
                LCurrentDir.Content = args.UserState as string;
            };
            worker.DoWork += (o, args) =>
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                new Action(() => SpScanning.Visibility = Visibility.Visible));


                var curWorker = (BackgroundWorker)o;

                var rootDir = new DirectoryInfo((string)args.Argument);

                //BackupCollection();


                // Проверим на существование проектов из коллекции и удалим, если не будут надйены
                curWorker.ReportProgress(0, "Проверка сохранённых проектов");
                for (var i = _projectCollection.Project.Count - 1; i >= 0; i--)
                {
                    var project = _projectCollection.Project[i];
                    if (!File.Exists(project.FullPath))
                    {
                        _projectCollection.Project.Remove(project);
                    }
                }
                for (var i = _projectCollection.Solution.Count - 1; i >= 0; i--)
                {
                    var solution = _projectCollection.Solution[i];
                    if (!File.Exists(solution.FullPath))
                    {
                        _projectCollection.Solution.Remove(solution);
                    }
                }
                TreeScan(rootDir, curWorker);
            };

            worker.RunWorkerAsync(_rootDirPath);

        }

        private static void BackupCollection()
        {
            if (File.Exists(CollectionFileName))
                File.Copy(CollectionFileName,
                    CollectionFileName + "." + DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss-fff") + ".bak");
        }

        private static string NormalizePath(string path)
        {
            return Path.GetFullPath(new Uri(path).LocalPath)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            //.ToUpperInvariant();
        }

        private void OptionsClick(object sender, RoutedEventArgs e)
        {
            var optionsDialog = new OptionsDialog();
            optionsDialog.SetTags(_projectCollection.Tags);
            if (optionsDialog.ShowDialog() != true)
                return;
            ShowTags();
            UpdateTagsOnAllItems();
            SaveCollection();
        }

        private void UpdateTagsOnAllItems()
        {
            //LvProjects.Items
            foreach (ListViewItem item in _listViewItems)
            {
                var collectionItem = item.Content as CollectionItem;
                collectionItem?.UpdateTags(_projectCollection.Tags);
            }
        }

        private void SaveCollection()
        {
            BackupCollection();
            _projectCollection.SaveToFile(CollectionFileName);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SaveCollection();
        }

        private void ShowReadmeClick(object sender, RoutedEventArgs e)
        {
            if (LvProjects.SelectedItem == null)
                return;

            var listItem = (ListViewItem)LvProjects.SelectedItem;
            string readmePath = null;


            var project = listItem.Tag as ProjectBase;
            if (project != null)
            {
                readmePath = project.ReadmePath;
            }
            if (string.IsNullOrEmpty(readmePath))
            {
                MessageBox.Show("В папке проекта нет файла Readme");
                return;
            }

            var readmeWin = new ReadmeWindow(readmePath);
            readmeWin.ShowDialog();

        }

        private void OpenFolderClick(object sender, RoutedEventArgs e)
        {
            if (LvProjects.SelectedItem == null)
                return;

            var listItem = (ListViewItem)LvProjects.SelectedItem;
            string projectDir = null;


            var project = listItem.Tag as ProjectBase;
            if (project != null)
            {
                projectDir = Path.GetDirectoryName(project.FullPath);
            }

            if (projectDir != null)
                Process.Start(projectDir);
        }

        private void RemoveItem(ListViewItem lvi)
        {
            _listViewItems.Remove(lvi);
            //            LvProjects.Items.Remove(lvi);

            var project = lvi.Tag as ProjectBase;
            if (project != null)
            {
                var colution = project as SolutionBase;
                if (colution != null)
                    _projectCollection.Solution.Remove(colution);
                else
                    _projectCollection.Project.Remove(project);
                _existProjects.Remove(project.FullPath.ToLower());
                return;
            }
            RefreshView();
        }

        private void RemoveItemClick(object sender, RoutedEventArgs e)
        {
            if (
                MessageBox.Show("Действительно выбранные элементы из списка?", "Удаление", MessageBoxButton.YesNo,
                MessageBoxImage.Exclamation) != MessageBoxResult.Yes)
                return;

            for (var i = LvProjects.SelectedItems.Count - 1; i >= 0; i--)
            {
                var item = (ListViewItem)LvProjects.SelectedItems[i];
                RemoveItem(item);
            }
        }

        private void CmMainOpening(object sender, ContextMenuEventArgs e)
        {
            if (LvProjects.SelectedItem == null)
                e.Handled = true;

            ShowTagsMenu();
        }

        private void CbShowSolutionsClicked(object sender, RoutedEventArgs e)
        {
            RefreshView();
        }

        private void CbShowProjectsClicked(object sender, RoutedEventArgs e)
        {
            RefreshView();
        }

        private void TagsBoolAddClick(object sender, RoutedEventArgs e)
        {
            var rb = (RadioButton)sender;
            _tagsAnd = rb.IsChecked == true;
            RefreshView();
        }

        private void TagsBoolOrClick(object sender, RoutedEventArgs e)
        {
            var rb = (RadioButton)sender;
            _tagsAnd = rb.IsChecked != true;
            RefreshView();
        }

        private void UpdateInfoClick(object sender, RoutedEventArgs e)
        {
            for (var i = LvProjects.SelectedItems.Count - 1; i >= 0; i--)
            {
                var item = (ListViewItem)LvProjects.SelectedItems[i];
                var project = item.Tag as ProjectBase;
                if (project != null)
                {
                    var projectDir = Path.GetDirectoryName(project.FullPath);
                    if (projectDir == null)
                    {
                        continue;
                    }
                    ScanFolder(project, new DirectoryInfo(projectDir));
                }
            }
            RefreshView();
        }

        private void TbSearchOnTextInput(object sender, TextChangedEventArgs textChangedEventArgs)
        {
            var tb = (TextBox)sender;
            _findText = tb.Text.ToLower().Trim();
            RefreshView();
        }

        private void ClearFindClick(object sender, RoutedEventArgs e)
        {
            var tmp = TbSearch.Text.Trim();
            _findText = null;
            // Если текст какой-то был
            if(!string.IsNullOrEmpty(tmp))
                TbSearch.Text = string.Empty;
        }
    }
}
