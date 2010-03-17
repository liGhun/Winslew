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
using System.Net.Sockets;

namespace Winslew
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        

        public Api.Api apiAccess = new Api.Api();
       
        public MainWindow()
        {
            InitializeComponent();

            this.Top = Properties.Settings.Default.MainWindowTop;
            this.Left = Properties.Settings.Default.MainWindowLeft;
            this.Width = Properties.Settings.Default.MainWindowWidth;
            this.Height = Properties.Settings.Default.MainWindowHeight;

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

            if (Properties.Settings.Default.CurrentView == "full")
            {
                button_full_Click(null, null);
            }
            else if (Properties.Settings.Default.CurrentView == "more")
            {
                button_more_Click(null, null);
            }
            else
            {
                button_less_Click(null, null);
            }

        }

        ~MainWindow() {
            Properties.Settings.Default.Save();
            AppController.Current.revokeSnarl();
        }
            

        public void refreshItems() {
            
             listView_Items.ItemsSource = AppController.Current.itemsCollection.Where((Item bq) => 
                 bq.read == Properties.Settings.Default.ShowReadItems && 
                 bq.title.ToLower().Contains(textBox_filterTitle.Text.ToLower()) &&
                 bq.tags.ToLower().Contains(textBox_filterTags.Text.ToLower())
                 );  
            if (!Properties.Settings.Default.IsValidLicense)
            {
                if (listView_Items.Items.Count > 10)
                {                    
                    if (!Properties.Settings.Default.BuyLicensePopupShown)
                    {
                        Properties.Settings.Default.BuyLicensePopupShown = true;
                        BuyLicense myBuyWindow = new BuyLicense();
                        myBuyWindow.Show();
                    }
                    
                }
                listView_Items.ItemsSource = AppController.Current.itemsCollection.Where((Item bq) => 
                    bq.read == Properties.Settings.Default.ShowReadItems &&
                    bq.title.ToLower().Contains(textBox_filterTitle.Text.ToLower()) &&
                    bq.tags.ToLower().Contains(textBox_filterTags.Text.ToLower())
                    ).Take(10);
            }
          
            this.Title = listView_Items.Items.Count.ToString() + " items - Winslew";
      
            if (AppController.Current.itemsCollection.Count > 0 && listView_Items.SelectedItem == null)
            {
                try
                {
                    listView_Items.SelectedItem = listView_Items.Items[listView_Items.Items.Count - 1];
                }
                catch
                {

                }
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
                updateViewOfFrame();
                label_TitleOfItem.Content = currentItem.title;
                if (currentItem.read)
                {
                    toggleReadIcon(true);
                }
                else
                {
                    toggleReadIcon(false);
                }
                if (currentItem.contentCache != null)
                {
                    if (currentItem.contentCache.Updated != null)
                    {
                        button_less.ToolTip = currentItem.contentCache.UpdatedHumanReadable;
                        button_more.ToolTip = currentItem.contentCache.UpdatedHumanReadable;
                    }
                }
            }
            else
            {
                button_less.ToolTip = "";
                button_more.ToolTip = "";
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
            AppController.Current.SetData(false);
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
                    AppController.Current.itemsCollection.Remove(currentItem);
                    toggleReadIcon(true);
                }
                refreshItems();
            }
        }

        private void button_help_Click(object sender, RoutedEventArgs e)
        {
            About myHelpWIndow = new About();
            if (Properties.Settings.Default.IsValidLicense)
            {
                myHelpWIndow.label_license.Content = "This copy is licensed to " + Properties.Settings.Default.Username;
            }
            else
            {
                myHelpWIndow.label_license.Content = "This is copy is unlicensed and in trial mode";
            }
            myHelpWIndow.Show();
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

        private void button_full_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.CurrentView = "full";
            button_full.IsEnabled = false;
            button_full.Background = Brushes.Black;
            button_full.Foreground = Brushes.White;
            button_more.IsEnabled = true;
            button_more.Background = Brushes.White;
            button_more.Foreground = Brushes.Black;
            button_less.IsEnabled = true;
            button_less.Background = Brushes.White;
            button_less.Foreground = Brushes.Black;
            updateViewOfFrame();
        }

        private void button_more_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.CurrentView = "more";
            button_full.IsEnabled = true;
            button_full.Background = Brushes.White;
            button_full.Foreground = Brushes.Black;
            button_more.IsEnabled = false;
            button_more.Background = Brushes.Black;
            button_more.Foreground = Brushes.White;
            button_less.IsEnabled = true;
            button_less.Background = Brushes.White;
            button_less.Foreground = Brushes.Black;
            updateViewOfFrame();
        }

        private void button_less_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.CurrentView = "less";
            button_full.IsEnabled = true;
            button_full.Background = Brushes.White;
            button_full.Foreground = Brushes.Black;
            button_more.IsEnabled = true;
            button_more.Background = Brushes.White;
            button_more.Foreground = Brushes.Black;
            button_less.IsEnabled = false;
            button_less.Background = Brushes.Black;
            button_less.Foreground = Brushes.White;
            updateViewOfFrame();
        }

        public void updateViewOfFrame()
        {
            if (listView_Items.SelectedItem != null)
            {
                var currentItem = listView_Items.SelectedItem as Item;
                if (currentItem.contentCache != null || Properties.Settings.Default.CurrentView == "full")
                {

                    if (Properties.Settings.Default.CurrentView == "full" && apiAccess.checkIfOnline())
                    {
                        frame_content.Source = new Uri(currentItem.url);
                    }
                    else if (Properties.Settings.Default.CurrentView == "more" && System.IO.File.Exists(currentItem.contentCache.MoreVersion))
                    {
                        frame_content.Source = new Uri(currentItem.contentCache.MoreVersion);
                    }
                    else if (System.IO.File.Exists(currentItem.contentCache.LessVersion))
                    {
                        frame_content.Source = new Uri(currentItem.contentCache.LessVersion);
                    }
                    else
                    {
                        frame_content.Source = new Uri(AppController.Current.cacheStore.pathToNAPage);
                    }
                }
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Properties.Settings.Default.MainWindowWidth = this.Width;
            Properties.Settings.Default.MainWindowHeight = this.Height;
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MainWindowTop = this.Top;
            Properties.Settings.Default.MainWindowLeft = this.Left;
        }

        private void button_updateCache_Click(object sender, RoutedEventArgs e)
        {
            if (listView_Items.SelectedItem != null)
            {
                List<Item> temoList = new List<Item>();
                var currentItem = listView_Items.SelectedItem as Item;
                temoList.Add(currentItem);
                AppController.Current.updateCache(temoList, true);
            }
            frame_content.Refresh();
        }



        private void textBox_filterTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            refreshItems();
        }

        private void textBox_filterTags_TextChanged(object sender, TextChangedEventArgs e)
        {
            refreshItems();
        }


    }
}
