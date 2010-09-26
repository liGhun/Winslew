using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics;

namespace Winslew
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : Window
    {
        public About()
        {
            InitializeComponent();
            this.label_Winslew_version.Content = "Winslew " + Formatter.prettyVersion.getNiceVersionString(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }

        private void button_visitHomepage_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://www.li-ghun.de/");
        }

        private void button_visitRIL_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://readitlaterlist.com/");
        }

        private void button_humanIconTheme_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://packages.ubuntu.com/search?keywords=human-icon-theme");
        }

        private void button_snarlCsharp_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://www.fullphat.net/developer/developerGuide/win32API/ExampleIncludes/index.html");
        }

        private void button_snarl_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://www.fullphat.net/");
        }


    }
}
