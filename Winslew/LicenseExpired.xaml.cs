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

namespace Winslew
{
    /// <summary>
    /// Interaction logic for LicenseExpired.xaml
    /// </summary>
    public partial class LicenseExpired : Window
    {
        public LicenseExpired()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.li-ghun.de/");
            this.Close();
        }

    }
}
