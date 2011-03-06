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
    /// Interaction logic for BuyLicense.xaml
    /// </summary>
    public partial class BuyLicense : Window
    {
        public static RoutedCommand EscapePressed = new RoutedCommand();

        public BuyLicense()
        {
            InitializeComponent();

            EscapePressed.InputGestures.Add(new KeyGesture(Key.Escape));
        }

        private void button_buyit_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.li-ghun.de/Winslew/Download/");
            this.Close();
        }

        private void EscapePressed_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
    }
}
