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

namespace Winslew
{
    /// <summary>
    /// Interaction logic for RemoveTags.xaml
    /// </summary>
    public partial class RemoveTags : Window
    {
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
                        AvailableTags.Add(new TagValue(tag));
                    }
                }
            }

            listViewTags.ItemsSource = AvailableTags;

            items = itemsToEdit;
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

        public class TagValue
        {
            public bool ShallBeDeleted { get; set; }
            public string tag { get; set; }

            public TagValue(string text)
            {
                this.tag = text;
                this.ShallBeDeleted = false;
            }
        }
    }
}
