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
    public partial class ChangeTitle : Window
    {
        public static RoutedCommand EscapePressed = new RoutedCommand();

        private Item oldItem;
        private Item newItem;

        public ChangeTitle(Item itemToEdit)
        {
            InitializeComponent();
            if (itemToEdit == null)
            {
                this.Close();
            }
            oldItem = itemToEdit;
            textBox_itemTitle.Text = itemToEdit.title;

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
            thisItem.Add("title", textBox_itemTitle.Text);
            AppController.Current.changeTitle(thisItem);
            
            newItem = oldItem;
            newItem.title = textBox_itemTitle.Text;
            AppController.Current.updateItem(oldItem, newItem);

            this.Close();
        }

        private void EscapePressed_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            button_cancel_Click(null, null);
        }
    }
}
