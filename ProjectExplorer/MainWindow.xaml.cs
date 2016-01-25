using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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

        public MainWindow()
        {
            InitializeComponent();

            TbRootProjectDir.Text = Properties.Settings.Default.RootPath;
            if (File.Exists(CollectionFileName))
            {
                _projectCollection = ProjectCollection.LoadFromFile(CollectionFileName);
                ShowCollection();
                ShowTags();
            }
            else
            {
                _projectCollection = new ProjectCollection();
            }
        }

        private void ShowTags()
        {
            WpTags.Children.Clear();
            foreach (var tag in _projectCollection.Tags)
            {
                var content = new Label {Content = tag.Name};
                var color = ColorConverter.ConvertFromString(tag.Color);
                if (color != null)
                content.Background = new SolidColorBrush((Color)color);
                var cbTag = new CheckBox
                {
                    Content = content,
                    Tag = tag,
                    Style = FindResource("TagCheckBoxStyle") as Style
                };
                WpTags.Children.Add(cbTag);
            }
        }

        private void AddNewItemInView(FrameworkElement item, object dataItem)
        {
            LvProjects.Items.Add(new ListViewItem
            {
                Content = item,
                Tag = dataItem
            });
        }

        private void ShowCollection()
        {
            LvProjects.Items.Clear();
            foreach (var solution in _projectCollection.Solution)
            {
                var item = new Controls.CollectionItem(solution, _projectCollection.Tags);
                item.Remove = () =>
                {
                    _projectCollection.Solution.Remove(solution);
                    LvProjects.Items.Remove(item);
                };
                AddNewItemInView(item, solution);
                _existProjects.Add(solution.FullPath.ToLower());
            }
            foreach (var project in _projectCollection.Project)
            {
                var item = new Controls.CollectionItem(project, _projectCollection.Tags);
                item.Remove = () =>
                {
                    _projectCollection.Project.Remove(project);
                    LvProjects.Items.Remove(item);
                };

                AddNewItemInView(item, project);
                _existProjects.Add(project.FullPath.ToLower());
            }
        }

        private readonly HashSet<string> _existProjects = new HashSet<string>();
        //private StringBuilder _foun;


        private void TreeScan(DirectoryInfo parentDir, BackgroundWorker curWorker)
        {
            curWorker.ReportProgress(0, parentDir.FullName);
            var readmes = parentDir.GetFiles("readme.md");
            FileInfo readme = null;
            if (readmes.Length > 0)
            {
                readme = readmes[0];
            }

            var screenshots = parentDir.GetFiles("screenshot.png");
            FileInfo screenshot = null;
            if (screenshots.Length > 0)
            {
                screenshot = screenshots[0];
            }

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
                var newSolution = new ProjectCollectionSolution
                {
                    CategoryId = -1,//existDir?.Id ?? 
                    Name = f.Name,
                    FullPath = f.FullName,

                };
                if (readme != null)
                {
                    newSolution.ReadmePath = readme.FullName;
                    newSolution.Value = File.ReadAllText(readme.FullName);
                }
                if (screenshot != null)
                    newSolution.ImagePath = screenshot.FullName;
                _projectCollection.Solution.Add(newSolution);
                _existProjects.Add(newSolution.FullPath.ToLower());

                //TbTest.Text += f.FullName + Environment.NewLine;
            }
            foreach (var f in parentDir.GetFiles("*.csproj"))
            {
                if (_existProjects.Contains(f.FullName.ToLower()))
                    continue;

                //Debug.Assert(existDir != null);
                var newProject = new ProjectCollectionProject
                {
                    CategoryId = -1,//existDir?.Id ?? 
                    Name = f.Name,
                    FullPath = f.FullName,

                };
                if (readme != null)
                {
                    newProject.ReadmePath = readme.FullName;
                    newProject.Value = File.ReadAllText(readme.FullName);
                }
                if (screenshot != null)
                    newProject.ImagePath = screenshot.FullName;
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

            BiLoading.IsBusy = true;

            var worker = new BackgroundWorker {WorkerReportsProgress = true};

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
           

                var curWorker = (BackgroundWorker) o;

                var rootDir = new DirectoryInfo((string)args.Argument);

                if (File.Exists(CollectionFileName))
                    File.Copy(CollectionFileName, CollectionFileName + "." + DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss-fff") + ".bak");


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
            SaveCollection();
            ShowTags();
            UpdateTagsOnAllItems();
        }

        private void UpdateTagsOnAllItems()
        {
            foreach (ListViewItem item in LvProjects.Items)
            {
                var collectionItem = item.Content as CollectionItem;
                collectionItem?.UpdateTags(_projectCollection.Tags);
            }
        }

        private void SaveCollection()
        {
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

            var listItem = (ListViewItem) LvProjects.SelectedItem;
            string readmePath = null;


            var solution = listItem.Tag as ProjectCollectionSolution;
            if (solution != null)
            {
                readmePath = solution.ReadmePath;
            }
            var project = listItem.Tag as ProjectCollectionProject;
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


            var solution = listItem.Tag as ProjectCollectionSolution;
            if (solution != null)
            {
                projectDir = Path.GetDirectoryName(solution.FullPath);
            }
            var project = listItem.Tag as ProjectCollectionProject;
            if (project != null)
            {
                projectDir = Path.GetDirectoryName(project.FullPath);
            }

            if (projectDir != null)
                Process.Start(projectDir);
        }

        private void RemoveItem(ContentControl lvi)
        {
            LvProjects.Items.Remove(lvi);
            var solution = lvi.Tag as ProjectCollectionSolution;
            if (solution != null)
            {
                _projectCollection.Solution.Remove(solution);
                _existProjects.Remove(solution.FullPath.ToLower());
                return;
            }
            var project = lvi.Tag as ProjectCollectionProject;
            if (project != null)
            {
                _projectCollection.Project.Remove(project);
                _existProjects.Remove(project.FullPath.ToLower());
                return;
            }

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
        }
    }
}
