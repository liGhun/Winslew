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
using System.ComponentModel;

namespace Winslew
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        

        public Api.Api apiAccess = new Api.Api();
        private GridViewColumnHeader _CurSortCol = null;
        private SortAdorner _CurAdorner = null;
       
        public MainWindow()
        {
            InitializeComponent();

            this.Top = Properties.Settings.Default.MainWindowTop;
            this.Left = Properties.Settings.Default.MainWindowLeft;
            this.Width = Properties.Settings.Default.MainWindowWidth;
            this.Height = Properties.Settings.Default.MainWindowHeight;

            ListViewColumnTitle.Width = Properties.Settings.Default.ListViewWidthTitle;
            ListViewColumnTags.Width = Properties.Settings.Default.ListViewWidthTags;
            ListViewColumnAdded.Width = Properties.Settings.Default.ListViewWidthAdded;
            ListViewColumnUpdated.Width = Properties.Settings.Default.ListViewWidthUpdated;

            // = Properties.Settings.Default.TopGridHeight;

            if (Properties.Settings.Default.ShowReadItems)
            {
                CurrentView.Content = "Read view";
            }
            else
            {
                CurrentView.Content = "Unread view";
            }

            if (Properties.Settings.Default.CurrentView != "")
            {
                comboBox_browserView.Text = Properties.Settings.Default.CurrentView;
            }
            else
            {
                comboBox_browserView.Text = "More";
            }

           // frame_content.DataContextChanged += BrowserStartLoad;
           // frame_content.LoadCompleted += BrowserOnLoadCompleted;

        }

        ~MainWindow() {

            Properties.Settings.Default.Save();
            AppController.Current.revokeSnarl();
        }

        private void BrowserOnLoadCompleted(object sender, NavigationEventArgs navigationEventArgs)
        {
            Snarl.SnarlConnector.ShowMessage("Site lodaded", "fine", 10, "", IntPtr.Zero, 0);    
        }

        private void BrowserStartLoad(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (frame_content.DataContext != null)
            {
                Snarl.SnarlConnector.ShowMessage("Site loding", "just a second please", 10, "", IntPtr.Zero, 0);
            }
        }

        public void refreshItems() {
            if (listView_Items != null)
                if (AppController.Current.itemsCollection != null)
                {
                    {
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
                }
        }

        public void updateSelectedItem()
        {
            listView_Items_SelectionChanged(null, null);
        }

        private void comboBox_chooseStyle_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox_chooseStyle.SelectedValue != null)
            {
                AppController.Current.selectAnotherStyle(comboBox_chooseStyle.SelectedValue.ToString());
            }
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
                        comboBox_browserView.ToolTip = "Select to be displayed cache version\r\nCache updated: " + currentItem.contentCache.UpdatedHumanReadable;
                    }
                }
            }
            else
            {
                comboBox_browserView.ToolTip = "Select to be displayed cache version";
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

        private void button_changeTitle_Click(object sender, RoutedEventArgs e)
        {
            if (listView_Items.SelectedItem != null)
            {
                var currentItem = listView_Items.SelectedItem as Item;
                ChangeTitle myEditWindow = new ChangeTitle(currentItem);
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
            }
            else
            {
                CurrentView.Content = "Unread view";
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

        public void updateViewOfFrame()
        {
            if (listView_Items.SelectedItem != null && frame_content != null)
            {
                var currentItem = listView_Items.SelectedItem as Item;
                if (currentItem.contentCache != null || Properties.Settings.Default.CurrentView.ToLower().EndsWith("online"))
                {

                    if (Properties.Settings.Default.CurrentView.ToLower().EndsWith("online") && apiAccess.checkIfOnline())
                    {
                        frame_content.Source = new Uri(currentItem.url);
                    }
                    else if (Properties.Settings.Default.CurrentView.ToLower().EndsWith("full") && System.IO.File.Exists(currentItem.contentCache.FullVersion))
                    {
                        frame_content.Source = new Uri(currentItem.contentCache.FullVersion);
                    }
                    else if (Properties.Settings.Default.CurrentView.ToLower().EndsWith("more") && System.IO.File.Exists(currentItem.contentCache.MoreVersion))
                    {
                        frame_content.Source = new Uri(currentItem.contentCache.MoreVersion);
                    }
                    else if (Properties.Settings.Default.CurrentView.ToLower().EndsWith("less") && System.IO.File.Exists(currentItem.contentCache.LessVersion))
                    {
                        frame_content.Source = new Uri(currentItem.contentCache.LessVersion);
                    }
                    else
                    {
                        frame_content.Source = new Uri(AppController.Current.cacheStore.pathToNAPage);
                    }
                }
                else
                {
                    frame_content.Source = new Uri(AppController.Current.cacheStore.pathToNAPage);
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
                List<Item> tempList = new List<Item>();
                var currentItem = listView_Items.SelectedItem as Item;
                tempList.Add(currentItem);
                AppController.Current.updateCache(tempList, true);
                AppController.Current.sendSnarlNotification("Cache has been updated", "Cache has been updated", currentItem.title);
                if (currentItem.contentCache != null)
                {
                    if (currentItem.contentCache.Updated != null)
                    {
                        comboBox_browserView.ToolTip = "Select to be displayed cache version\r\nCache updated: " + currentItem.contentCache.UpdatedHumanReadable;
                    }
                }
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

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = sender as GridViewColumnHeader;
            String field = column.Tag as String;

            if (_CurSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(_CurSortCol).Remove(_CurAdorner);
                listView_Items.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (_CurSortCol == column && _CurAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            _CurSortCol = column;
            _CurAdorner = new SortAdorner(_CurSortCol, newDir);
            AdornerLayer.GetAdornerLayer(_CurSortCol).Add(_CurAdorner);
            listView_Items.Items.SortDescriptions.Add(new SortDescription(field, newDir));
        }

        public class SortAdorner : Adorner
        {
            private readonly static Geometry _AscGeometry =
                Geometry.Parse("M 0,0 L 10,0 L 5,5 Z");

            private readonly static Geometry _DescGeometry =
                Geometry.Parse("M 0,5 L 10,5 L 5,0 Z");

            public ListSortDirection Direction { get; private set; }

            public SortAdorner(UIElement element, ListSortDirection dir)
                : base(element)
            { Direction = dir; }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                if (AdornedElement.RenderSize.Width < 20)
                    return;

                drawingContext.PushTransform(
                    new TranslateTransform(
                      AdornedElement.RenderSize.Width - 15,
                      (AdornedElement.RenderSize.Height - 5) / 2));

                drawingContext.DrawGeometry(Brushes.Black, null,
                    Direction == ListSortDirection.Ascending ?
                      _AscGeometry : _DescGeometry);

                drawingContext.Pop();
            }
        }

        private void GridViewColumnHeader_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Properties.Settings.Default.ListViewWidthTitle = ListViewColumnTitle.Width;
            Properties.Settings.Default.ListViewWidthTags = ListViewColumnTags.Width;
            Properties.Settings.Default.ListViewWidthAdded = ListViewColumnAdded.Width;
            Properties.Settings.Default.ListViewWidthUpdated = ListViewColumnUpdated.Width;
        }

        private void comboBox_browserView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem cbi = comboBox_browserView.SelectedItem as ComboBoxItem;
            if (cbi != null)
            {
                    Properties.Settings.Default.CurrentView = cbi.Content.ToString();
                    updateViewOfFrame();
            }
        }

        private void button_openInBrowser_Click(object sender, RoutedEventArgs e)
        {
            if (listView_Items.SelectedItem != null)
            {
                var currentItem = listView_Items.SelectedItem as Item;
                System.Diagnostics.Process.Start(currentItem.url);
            }
        }

        private void button_copyUrlToClipboard_Click(object sender, RoutedEventArgs e)
        {
            if (listView_Items.SelectedItem != null)
            {
                var currentItem = listView_Items.SelectedItem as Item;
                Clipboard.SetText(currentItem.url);
            }
        }

        #region ContextMenu ListView

        private void ContextCopyUrl_Click(object sender, RoutedEventArgs e)
        {
            button_copyUrlToClipboard_Click(null, null);
        }

        private void ContextOpenInBrower_Click(object sender, RoutedEventArgs e)
        {
            button_openInBrowser_Click(null, null);
        }

        private void ContextToggleReadState_Click(object sender, RoutedEventArgs e)
        {
            button_markRead_Click(null, null);
        }

        private void ContextUpdateCache_Click(object sender, RoutedEventArgs e)
        {
            button_updateCache_Click(null, null);
        }

        private void ContextDelete_Click(object sender, RoutedEventArgs e)
        {
            button_delete_Click(null, null);
        }

        private void ContextEditTags_Click(object sender, RoutedEventArgs e)
        {
            button_editTags_Click(null, null);
        }


        private void ContextChangeTitle_Click(object sender, RoutedEventArgs e)
        {
            button_changeTitle_Click(null, null);
        }

        private void ContextPrint_Click(object sender, RoutedEventArgs e)
        {
            button_printPage_Click(null, null);
        }

        #endregion

        private void button_openSnarl_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.fullphat.net");
        }

        private void button_printPage_Click(object sender, RoutedEventArgs e)
        {
            if (listView_Items.SelectedItem != null)
            {
                try
                {
                    var currentItem = listView_Items.SelectedItem as Item;
                    /* xxx Printing not working anymore
                    mshtml.IHTMLDocument2 doc = frame_content.Document as mshtml.IHTMLDocument2;
                    doc.execCommand("Print", true, null); */
                }
                catch (Exception exp)
                {
                    MessageBox.Show(exp.Message, "Printing failed");
                }
            }
        }

        private void button_addTags_Click(object sender, RoutedEventArgs e)
        {
            if (listView_Items.SelectedItems.Count > 0)
            {
                List<Item> items = new List<Item>();
                foreach (Item item in listView_Items.SelectedItems)
                {
                    items.Add(item);
                }
                AddTags myWnd = new AddTags(items);
                myWnd.Show();
            }
        }

        private void button_removeTags_Click(object sender, RoutedEventArgs e)
        {
            if (listView_Items.SelectedItems.Count > 0)
            {
                List<Item> items = new List<Item>();
                foreach (Item item in listView_Items.SelectedItems)
                {
                    items.Add(item);
                }
                RemoveTags myWnd = new RemoveTags(items);
                myWnd.Show();
            }
        }

        private void ContextAddTags_Click(object sender, RoutedEventArgs e)
        {
            button_addTags_Click(null, null);
        }

        private void ContextRemoveTags_Click(object sender, RoutedEventArgs e)
        {
            button_removeTags_Click(null, null);
        }

    }
}
