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
    /// Interaction logic for EditTags.xaml
    /// </summary>
    public partial class EditTags : Window
    {
        private Item oldItem;
        private Item newItem;

        public EditTags(Item itemToEdit)
        {
            InitializeComponent();
            if (itemToEdit == null)
            {
                this.Close();
            }
            oldItem = itemToEdit;
            label_itemTitle.Content = itemToEdit.title;
            textBox_itemTags.Text = itemToEdit.tags;
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void button_save_Click(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> thisItem = new Dictionary<string, string>();
            thisItem.Add("url", oldItem.url);
            thisItem.Add("tags", textBox_itemTags.Text);
            AppController.Current.changeTags(thisItem);
            
            newItem = oldItem;
            newItem.tags = textBox_itemTags.Text;
            AppController.Current.updateItem(oldItem, newItem);

            this.Close();
        }
    }
}
