using System;
using System.Collections.ObjectModel;
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
using System.ComponentModel;

namespace Winslew
{
    /// <summary>
    /// Interaction logic for RemoveTags.xaml
    /// </summary>
    public partial class RemoveTags : Window
    {
        public static RoutedCommand EscapePressed = new RoutedCommand();

        List<Item> items;
        ObservableCollection<TagValue> AvailableTags;

        public RemoveTags(List<Item> itemsToEdit)
        {
            InitializeComponent();

            if (itemsToEdit == null)
            {
                this.Close();
            }

            AvailableTags = new ObservableCollection<TagValue>();
            List<string> alreadyStoredTags = new List<string>();
            char[] delimiters = new char[] { ',' };
            
            int checkboxId = 0;
            foreach(Item item in itemsToEdit) {
                foreach (string tag in item.tags.Split(delimiters, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (alreadyStoredTags.Contains(tag))
                    {
                        continue;
                    }
                    else
                    {
                        alreadyStoredTags.Add(tag);
                        AvailableTags.Add(new TagValue(tag,checkboxId));
                        checkboxId ++;
                    }
                }
            }

            listViewTags.ItemsSource = AvailableTags;

            items = itemsToEdit;

            listViewTags.Focus();
            listViewTags.PreviewKeyDown += CheckBox_KeyDown;

            EscapePressed.InputGestures.Add(new KeyGesture(Key.Escape));
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void buttonRemove_Click(object sender, RoutedEventArgs e)
        {
            List<Dictionary<string, string>> toBeUpdatedItems = new List<Dictionary<string, string>>();
            
            Dictionary<string, string> thisItem = new Dictionary<string, string>();

            IEnumerable<TagValue> ToBeDeletedTags = AvailableTags.Where((TagValue tagValue) => tagValue.ShallBeDeleted == true); 

            foreach(Item item in items) {
                thisItem = new Dictionary<string, string>();
                if (item.tags != "")
                {
                    foreach (TagValue singleTag in ToBeDeletedTags)
                    {
                        item.removeTag(singleTag.tag);
                    }
                }               
            }

            foreach(Item item in items.Where((Item it) => it.TagsHaveBeenUpdated == true)) {
                thisItem = new Dictionary<string, string>();
                thisItem.Add(item.url, item.tags);
                toBeUpdatedItems.Add(thisItem);
            }

            if(toBeUpdatedItems.Count > 0) {
                AppController.Current.addTags(toBeUpdatedItems);
            }

            this.Close();
        }

        public class TagValue : INotifyPropertyChanged
        {
            public bool ShallBeDeleted { get; set; }
            public string tag { get; set; }
            public string Id {get; set; }

            public TagValue(string text, int id)
            {
                this.tag = text;
                this.ShallBeDeleted = false;
                Id = "ch_" + id.ToString();
                
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        private void CheckBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                buttonRemove_Click(null, null);
            } 
            else if (e.Key == Key.Space)
            {
                ListView listView = sender as ListView;
                if (listView != null)
                {
                    TagValue listViewItem = listView.SelectedItem as TagValue;
                    if (listViewItem != null)
                    {
                        int selectedIndex = listView.SelectedIndex;
                        listViewItem.ShallBeDeleted = !listViewItem.ShallBeDeleted;
                        listView.Items.Refresh();
                        listView.SelectedIndex = selectedIndex;
                    }
                }
            }
        }

        private void EscapePressed_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            button_cancel_Click(null, null);
        }
    }
}
