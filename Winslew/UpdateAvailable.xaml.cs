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
using System.Net;
using System.Xml;

namespace Winslew
{
    /// <summary>
    /// Interaction logic for UpdateAvailable.xaml
    /// </summary>
    public partial class UpdateAvailable : Window
    {
        private string newVersion = "";

        public UpdateAvailable()
        {
            InitializeComponent();
            Version installedVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
           
            XmlDocument XMLdoc = null;
            HttpWebRequest request;
            HttpWebResponse response = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(string.Format("http://www.li-ghun.de/Downloads/winslewUpdateInfo.xml"));
                request.UserAgent = @"Winslew update check " + installedVersion.ToString();
                response = (HttpWebResponse)request.GetResponse();
                XMLdoc = new XmlDocument();
                XMLdoc.Load(response.GetResponseStream());

                string availableVersionString = XMLdoc.SelectSingleNode("Winslew/Version").InnerText;

                if (availableVersionString == Properties.Settings.Default.ignoredNewVersion)
                {
                    // this version shall be ignored
                    Close();
                }
                Version availableVersion = new Version(availableVersionString);
                if (availableVersion > installedVersion)
                {
                    label_oldNew.Content = string.Format("Installed version: {0}  - now available: {1}", Formatter.prettyVersion.getNiceVersionString(installedVersion.ToString()),  Formatter.prettyVersion.getNiceVersionString(availableVersionString));
                    textBox_news.Text =  XMLdoc.SelectSingleNode("Winslew/Description").InnerText;
                    newVersion = availableVersionString;
                    Show();
                }
                else
                {
                    Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Close();
            }
        }

        private void button_getUpdate_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.li-ghun.de/Winslew/Download/");
        }

        private void button_ignore_Click(object sender, RoutedEventArgs e)
        {
            if (!(bool)checkBox_remindMeAgain.IsChecked)
            {
                Properties.Settings.Default.ignoredNewVersion = newVersion; 
            }
            Close();
        }
    }
}
