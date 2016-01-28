using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using MarkdownSharp;

namespace ProjectExplorer.Windows
{
    /// <summary>
    /// Interaction logic for ReadmeWindow.xaml
    /// </summary>
    public partial class ReadmeWindow
    {
        private readonly string _readmePath;

        private readonly string _stylePath;

        readonly MarkdownOptions _options = new MarkdownOptions
        {
            AutoHyperlink = true,
            AutoNewlines = true,
            LinkEmails = true,
            QuoteSingleLine = true,
            StrictBoldItalic = true
        };

        public ReadmeWindow(string readmePath)
        {
            _readmePath = readmePath;
            InitializeComponent();

            var strAppDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
            _stylePath = Path.Combine(strAppDir, "ReadMeStyle.less");
            TbSource.Text = File.ReadAllText(readmePath);
            ShowReadme();
        }

        private void ShowReadme()
        {

            var source = File.ReadAllText(_readmePath);



            var mark = new Markdown(_options);
            var headerHtml = @"<!DOCTYPE html>
<head>
    <meta charset='utf-8'>
<link rel='stylesheet' type='text/css' href='
"
                             +
                             _stylePath
                             +
                             @"' />
</head>
<body>
";
            const string footerHtml = @"</body>";
            var htmlString = headerHtml + mark.Transform(source) + footerHtml;

            WbReadme.NavigateToString(htmlString);
        }


        private void SaveClick(object sender, RoutedEventArgs e)
        {
            SaveReadme();
        }

        private void SaveReadme()
        {
            File.WriteAllText(_readmePath, TbSource.Text);
            ShowReadme();
        }

        private void ReadmeWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) && e.Key == Key.S)
                SaveReadme();
        }
    }
}
