using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Winslew
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                AppController.Start();
            }
            catch (Exception exp)
            {
                try
                {
                    System.Windows.Forms.Clipboard.SetText(exp.Message + "\n\n" + exp.StackTrace);
                }
                catch { }
                MessageBox.Show(exp.StackTrace, exp.Message);
            }
        }
    }
}
