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
        public static RoutedCommand EscapePressed = new RoutedCommand();

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

            textBox_itemTags.Focus();
            textBox_itemTags.Select(textBox_itemTags.Text.Length, 0);

            EscapePressed.InputGestures.Add(new KeyGesture(Key.Escape));
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

        private void textBox_itemTags_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.End || e.Key == Key.Return)
            {
                button_save_Click(null, null);
            }
        }

        private void EscapePressed_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            button_cancel_Click(null, null);
        }
    }
}
