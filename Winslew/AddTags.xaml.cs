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
    /// Interaction logic for AddTags.xaml
    /// </summary>
    public partial class AddTags : Window
    {
        public static RoutedCommand EscapePressed = new RoutedCommand();

        List<Item> items;

        public AddTags(List<Item> itemsToEdit)
        {
            InitializeComponent();
            if (itemsToEdit == null)
            {
                this.Close();
            }
            items = itemsToEdit;
            label_itemTitle.Content = "You have seleceted " + itemsToEdit.Count.ToString() + " items";

            textBox_itemTags.Focus();

            EscapePressed.InputGestures.Add(new KeyGesture(Key.Escape));
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void button_save_Click(object sender, RoutedEventArgs e)
        {
            List<Dictionary<string, string>> toBeUpdatedItems = new List<Dictionary<string, string>>();

            Dictionary<string, string> thisItem = new Dictionary<string, string>();
            foreach(Item item in items) {
                thisItem = new Dictionary<string, string>();
                string oldTags = "";
                if (item.tags != "")
                {
                    oldTags = item.tags + ",";
                }
                thisItem.Add(item.url, oldTags + textBox_itemTags.Text);
                toBeUpdatedItems.Add(thisItem);
            }
            AppController.Current.addTags(toBeUpdatedItems);

            this.Close();
        }

        private void textBox_itemTags_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                if (textBox_itemTags.Text.Length > 0)
                {
                    button_save_Click(null, null);
                }
            }
        }

        private void EscapePressed_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            button_cancel_Click(null, null);
        }
    }
}
