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
    /// Interaction logic for AddNew.xaml
    /// </summary>
    public partial class AddNew : Window
    {
        public AddNew()
        {
            InitializeComponent();
            textBox_url.Focus();
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void button_save_Click(object sender, RoutedEventArgs e)
        {
            AppController.Current.addItem(textBox_url.Text, textBox_title.Text);
            this.Close();
        }

        private void textBox_url_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (button_save != null)
            {
                if (!(textBox_url.Text.StartsWith("http://") || textBox_url.Text.StartsWith("https://")) || textBox_url.Text.Length < 9)
                {
                    button_save.IsEnabled = false;
                }
                else
                {
                    button_save.IsEnabled = true;
                }
            }
        }
    }
}
