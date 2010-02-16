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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;

namespace Winslew
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private bool isInitialFetch = true;

        public ObservableCollection<Item> UnreadItems
        {
            get;
            set;
        }


        public Api.Api apiAccess = new Api.Api();
        

        public MainWindow()
        {
            InitializeComponent();
            if (Properties.Settings.Default.Username != "" && Properties.Settings.Default.Password != "")
            {
                SetData(apiAccess.getList(0, false, true));
                isInitialFetch = false;
            }
            else
            {
                Preferences myPrefWindow = new Preferences();
                myPrefWindow.Show();
                myPrefWindow.Focus();
            }
            if (Properties.Settings.Default.ShowReadItems)
            {
                CurrentView.Content = "Read view";
                ImageDelete.Visibility = Visibility.Visible;
                button_delete.Visibility = Visibility.Visible;
            }
            else
            {
                CurrentView.Content = "Unread view";
                ImageDelete.Visibility = Visibility.Hidden;
                button_delete.Visibility = Visibility.Hidden;
            }
        }

        ~MainWindow() {
            Properties.Settings.Default.Save();
            AppController.Current.revokeSnarl();
        }
            

        public void SetData(IEnumerable<Item> data)
        {
            if (isInitialFetch)
            {
                UnreadItems = new ObservableCollection<Item>(data);



            }
            else
            {
                foreach (Item newItem in data)
                {
                    IEnumerable<Item> alreadyExistingItemWithId = UnreadItems.Where((Item bq) => bq.id == newItem.id);

                    if (alreadyExistingItemWithId == null)
                    {
                        UnreadItems.Add(newItem);
                    }
                    else if (alreadyExistingItemWithId.Count() == 0)
                    {
                        UnreadItems.Add(newItem);
                        AppController.Current.addToCache(newItem);
                        AppController.Current.sendSnarlNotification("New item", "New item added", newItem.title);
                    }
                    else
                    {
                        List<Item> tempList = new List<Item>();
                        foreach (Item item in alreadyExistingItemWithId)
                        {
                            tempList.Add(item);
                        }
                        foreach (Item oldItem in tempList)
                        {
                            UnreadItems.Remove(oldItem);
                            UnreadItems.Add(newItem);
                        }
                    }
                }

                // deleting temporary items as they are created when you add an item here in the GUI
                // they can be found by searching for empty IDs
                IEnumerable<Item> temporaryAddedItems = UnreadItems.Where((Item bq) => bq.id == null);
                if (temporaryAddedItems.Count() > 0)
                {
                    List<Item> tempList = new List<Item>();
                    foreach (Item item in temporaryAddedItems)
                    {
                        tempList.Add(item);
                    }
                    foreach (Item tempItem in tempList)
                    {
                        UnreadItems.Remove(tempItem);
                    }
                }

            }
            refreshItems();
            

        }

        public void refreshItems() {
            listView_Items.ItemsSource = UnreadItems.Where((Item bq) => bq.read == Properties.Settings.Default.ShowReadItems);
            this.Title = listView_Items.Items.Count.ToString() + " items - Winslew";
            if (UnreadItems.Count > 0 && listView_Items.SelectedItem == null)
            {
                listView_Items.SelectedItem = listView_Items.Items[0];
            }
        }

        public void updateSelectedItem()
        {
            listView_Items_SelectionChanged(null, null);
        }

        private void listView_Items_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listView_Items.SelectedItem != null)
            {
                var currentItem = listView_Items.SelectedItem as Item;
                System.Uri pathToContent;
                if (currentItem.contentCache != null)
                {
                    pathToContent = new System.Uri(currentItem.contentCache.LessVersion);
                }
                else
                {
                    pathToContent = new System.Uri(currentItem.url);
                }
                frame_content.Source = pathToContent;
                label_TitleOfItem.Content = currentItem.title;
                if (currentItem.read)
                {
                    toggleReadIcon(true);
                }
                else
                {
                    toggleReadIcon(false);
                }
            }
        }

        private void toggleReadIcon(bool isCurrentItemRead)
        {
            if(isCurrentItemRead)
                {
                    Uri src = new Uri(@"/Winslew;component/Images/markedRead.png", UriKind.Relative);
                    BitmapImage img = new BitmapImage(src);
                    ImageReadState.Source = img;
                    ImageReadState.ToolTip = "This item is marked as read - click to toggle state";
                }
                else
                {
                    Uri src = new Uri(@"/Winslew;component/Images/unread.png", UriKind.Relative);
                    BitmapImage img = new BitmapImage(src);
                    ImageReadState.Source = img;
                    ImageReadState.ToolTip = "This item is marked as unread - click to toggle state";
                }
        }

        #region Buttons

        private void button_refresh_Click(object sender, RoutedEventArgs e)
        {
            SetData(apiAccess.getList(0, false, true));
        }

        private void button_editTags_Click(object sender, RoutedEventArgs e)
        {
            if (listView_Items.SelectedItem != null)
            {
                var currentItem = listView_Items.SelectedItem as Item;
                EditTags myEditWindow = new EditTags(currentItem);
                myEditWindow.Show();
                myEditWindow.Focus();
            }

        }

        private void button_addNew_Click(object sender, RoutedEventArgs e)
        {
            AddNew myAddNewWindows = new AddNew();
            myAddNewWindows.Show();
        }

        private void button_delete_Click(object sender, RoutedEventArgs e)
        {
            if (listView_Items.SelectedItem != null)
            {
                var currentItem = listView_Items.SelectedItem as Item;
                Dictionary<string, string> thisItem = new Dictionary<string, string>();
                thisItem.Add("url", currentItem.url);
                if (apiAccess.delete(thisItem))
                {
                    AppController.Current.sendSnarlNotification("Item deleted", "Item has been deleted", currentItem.title);
                    UnreadItems.Remove(currentItem);
                    toggleReadIcon(true);
                }
                refreshItems();
            }
        }

        private void button_help_Click(object sender, RoutedEventArgs e)
        {

        }

        private void button_toggleView_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.ShowReadItems = !Properties.Settings.Default.ShowReadItems;
            if (Properties.Settings.Default.ShowReadItems)
            {
                CurrentView.Content = "Read view";
                ImageDelete.Visibility = Visibility.Visible;
                button_delete.Visibility = Visibility.Visible;
            }
            else
            {
                CurrentView.Content = "Unread view";
                ImageDelete.Visibility = Visibility.Hidden;
                button_delete.Visibility = Visibility.Hidden;
            }
            refreshItems();
        }

        private void button_markRead_Click(object sender, RoutedEventArgs e)
        {
            if (listView_Items.SelectedItem != null)
            {
                var currentItem = listView_Items.SelectedItem as Item;
                Dictionary<string, string> thisItem = new Dictionary<string, string>();
                thisItem.Add("url", currentItem.url);
                if (currentItem.read)
                {
                    thisItem.Add("title", currentItem.title);
                    apiAccess.sendNew(thisItem);
                    AppController.Current.sendSnarlNotification("Item marked as unread", "Item has been marked as unread", currentItem.title);
                }
                else
                {
                    apiAccess.markRead(thisItem);
                    AppController.Current.sendSnarlNotification("Item marked as read", "Item has been marked as read", currentItem.title);
                }
                currentItem.read = !currentItem.read;
                toggleReadIcon(currentItem.read);
                refreshItems();
            }
        }

        private void button_preferences_Click(object sender, RoutedEventArgs e)
        {
            Preferences myPrefWindows = new Preferences();
            myPrefWindows.Show();
        }

        #endregion



    }
}
