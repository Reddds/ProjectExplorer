using System;
using System.IO;
using System.Reflection;
using MarkdownSharp;

namespace ProjectExplorer.Windows
{
    /// <summary>
    /// Interaction logic for ReadmeWindow.xaml
    /// </summary>
    public partial class ReadmeWindow
    {
        public ReadmeWindow(string readmePath)
        {
            InitializeComponent();

            var strAppDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
            var strFullPathToMyFile = Path.Combine(strAppDir, "ReadMeStyle.less");

            var source = File.ReadAllText(readmePath);

            var options = new MarkdownOptions
            {
                AutoHyperlink = true,
                AutoNewlines = true,
                LinkEmails = true,
                QuoteSingleLine = true,
                StrictBoldItalic = true
            };

            var mark = new Markdown(options);
            var headerHtml = @"<!DOCTYPE html>
<head>
    <meta charset='utf-8'>
<link rel='stylesheet' type='text/css' href='
"
+
strFullPathToMyFile
 +
@"' />
</head>
<body>
";
            const string footerHtml = @"</body>";
            var htmlString = headerHtml + mark.Transform(source) + footerHtml;


            WbReadme.NavigateToString(htmlString);
        }
    }
}
