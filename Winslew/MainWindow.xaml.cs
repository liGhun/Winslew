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
using System.Timers;
using System.Windows.Threading;

namespace Winslew
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static RoutedCommand OnRefreshRiL = new RoutedCommand();
        public static RoutedCommand OnToggleReadUnreadView = new RoutedCommand();
        public static RoutedCommand OnChangeTitle = new RoutedCommand();
        public static RoutedCommand OnEditTags = new RoutedCommand();
        public static RoutedCommand OnAddTags = new RoutedCommand();
        public static RoutedCommand OnRemoveTags = new RoutedCommand();
        public static RoutedCommand OnNewItem = new RoutedCommand();
        public static RoutedCommand OnUploadImage = new RoutedCommand();
        public static RoutedCommand OnNewPastebin = new RoutedCommand();
        public static RoutedCommand OnUploadPastebin = new RoutedCommand();
        public static RoutedCommand OnDelete = new RoutedCommand();
        public static RoutedCommand OnFilter = new RoutedCommand();
        public static RoutedCommand OnPreferences = new RoutedCommand();
        public static RoutedCommand OnHelp = new RoutedCommand();
        public static RoutedCommand OnToggleReadUnreadState = new RoutedCommand();
        public static RoutedCommand OnAddCurrentPage = new RoutedCommand();
        public static RoutedCommand OnCopyUrl = new RoutedCommand();
        public static RoutedCommand OnOpenInBrowser = new RoutedCommand();
        public static RoutedCommand OnRefreshCache = new RoutedCommand();
        public static RoutedCommand OnChangeStyle = new RoutedCommand();
        public static RoutedCommand OnChangeViewType = new RoutedCommand();
        public static RoutedCommand OnSelectListView = new RoutedCommand();
        public static RoutedCommand OnSelectBrowser = new RoutedCommand();

        public Api.Api apiAccess = new Api.Api();
        private GridViewColumnHeader _CurSortCol = null;
        private SortAdorner _CurAdorner = null;

        private WindowState m_storedWindowState = WindowState.Normal;
        private System.Windows.Forms.NotifyIcon m_notifyIcon;
        private System.Windows.Forms.ContextMenu m_notifyMenu;
        private bool isInRunningMode = false;

        private System.Windows.Forms.Integration.WindowsFormsHost _MyHost;
        public WebKit.WebKitBrowser webKitBrowser;

        public DispatcherTimer dispatcherTimer;

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

            _MyHost = new System.Windows.Forms.Integration.WindowsFormsHost();
            webKitBrowser = new WebKit.WebKitBrowser();
            _MyHost.Child = webKitBrowser;
            WebkitBrowserGrid.Children.Add(_MyHost);

            // tray icon stuff
            m_notifyIcon = new System.Windows.Forms.NotifyIcon();
            m_notifyIcon.Text = "Winslew";
            m_notifyIcon.Icon = new System.Drawing.Icon("Winslew.ico");
            m_notifyIcon.DoubleClick += new EventHandler(m_notifyIcon_Click);
            System.Windows.Forms.MenuItem showMainMenu = new System.Windows.Forms.MenuItem("Show main window", new System.EventHandler(trayContextShow));

            m_notifyMenu = new System.Windows.Forms.ContextMenu();
            m_notifyMenu.MenuItems.Add("Winslew"); 
            m_notifyMenu.MenuItems.Add("-");
            m_notifyMenu.MenuItems.Add(showMainMenu);
            m_notifyMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Refresh now", new System.EventHandler(trayContextRefresh)));
            m_notifyMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Preferences", new System.EventHandler(trayContextPreferences)));
            m_notifyMenu.MenuItems.Add("-");
            m_notifyMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Add new item url", new System.EventHandler(trayContextAddItem)));
            m_notifyMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Upload image", new System.EventHandler(trayContextUploadImage)));
            m_notifyMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Store text online", new System.EventHandler(trayContextNewPastebin)));
            m_notifyMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Upload text online", new System.EventHandler(trayContextUploadPastebin)));
            m_notifyMenu.MenuItems.Add("-");
            m_notifyMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("Quit", new System.EventHandler(trayContextQuit)));

            m_notifyIcon.ContextMenu = m_notifyMenu;

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);

            webKitBrowser.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(webKitBrowser_Navigated);
            webKitBrowser.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(webKitBrowser_Navigating);
            webKitBrowser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(webKitBrowser_DocumentCompleted);
            webKitBrowser.UserAgent = "Winslew " + Formatter.prettyVersion.getNiceVersionString(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()) + " (http://www.li-ghun.de/Winslew/)";
            webKitBrowser.AllowDownloads = false;
            webKitBrowser.AllowNewWindows = false;

            OnRefreshRiL.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control));
            OnRefreshRiL.InputGestures.Add(new KeyGesture(Key.F5));
            OnToggleReadUnreadView.InputGestures.Add(new KeyGesture(Key.Tab, ModifierKeys.Control));
            OnChangeTitle.InputGestures.Add(new KeyGesture(Key.T, ModifierKeys.Control));
            OnEditTags.InputGestures.Add(new KeyGesture(Key.E, ModifierKeys.Control));
            OnAddTags.InputGestures.Add(new KeyGesture(Key.W, ModifierKeys.Control));
            OnRemoveTags.InputGestures.Add(new KeyGesture(Key.Q, ModifierKeys.Control));
            OnNewItem.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            OnUploadImage.InputGestures.Add(new KeyGesture(Key.I, ModifierKeys.Control));
            OnNewPastebin.InputGestures.Add(new KeyGesture(Key.J, ModifierKeys.Control));
            OnUploadPastebin.InputGestures.Add(new KeyGesture(Key.K, ModifierKeys.Control));
            OnDelete.InputGestures.Add(new KeyGesture(Key.D, ModifierKeys.Control));
            OnFilter.InputGestures.Add(new KeyGesture(Key.F, ModifierKeys.Control));
            OnPreferences.InputGestures.Add(new KeyGesture(Key.F1));
            OnHelp.InputGestures.Add(new KeyGesture(Key.H, ModifierKeys.Control));
            OnToggleReadUnreadState.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            OnAddCurrentPage.InputGestures.Add(new KeyGesture(Key.F2));
            OnCopyUrl.InputGestures.Add(new KeyGesture(Key.F3));
            OnOpenInBrowser.InputGestures.Add(new KeyGesture(Key.Return, ModifierKeys.Control));
            OnRefreshCache.InputGestures.Add(new KeyGesture(Key.F4));
            OnChangeStyle.InputGestures.Add(new KeyGesture(Key.F6));
            OnChangeViewType.InputGestures.Add(new KeyGesture(Key.F7));
            OnSelectListView.InputGestures.Add(new KeyGesture(Key.F8));
            OnSelectBrowser.InputGestures.Add(new KeyGesture(Key.F9));

            textboxCurrentUrl.KeyDown += new KeyEventHandler(textboxCurrentUrl_KeyDown);

            isInRunningMode = true;
        }

        void textboxCurrentUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e != null)
            {
                if (e.Key == Key.Return || e.Key == Key.Enter)
                {
                    webKitBrowser.Navigate(textboxCurrentUrl.Text);
                }
            }
        }

        void webKitBrowser_DocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e)
        {
            progressBarLoadingPage.Visibility = Visibility.Collapsed;
            labelLoadingPage.Visibility = Visibility.Collapsed;
            label_TitleOfItem.Text = webKitBrowser.DocumentTitle;
            //textboxCurrentUrl.Text = webKitBrowser.Url.AbsoluteUri;
            buttonBrowserStop.IsEnabled = false;
        }

        void webKitBrowser_Navigating(object sender, System.Windows.Forms.WebBrowserNavigatingEventArgs e)
        {
            progressBarLoadingPage.Visibility = Visibility.Visible;
            labelLoadingPage.Visibility = Visibility.Visible;
            buttonBrowserStop.IsEnabled = true;
            if (e != null)
            {
                if (e.Url != null)
                {
                    textboxCurrentUrl.Text = e.Url.AbsoluteUri;
                }
            }
        }

        void webKitBrowser_Navigated(object sender, System.Windows.Forms.WebBrowserNavigatedEventArgs e)
        {
            button_addCurrentViewedPage.Visibility = Visibility.Collapsed;
            Image_AddCurrentlyViewPage.Visibility = Visibility.Collapsed;
            // Snarl.SnarlConnector.ShowMessage("Site lodaded", "fine", 10, "", IntPtr.Zero, 0);    
            if (e.Url.AbsoluteUri.ToString().StartsWith("http"))
            {
                if (AppController.Current.itemsCollection.Where(i => i.url == e.Url.AbsoluteUri).Count() == 0)
                {
                    button_addCurrentViewedPage.Visibility = Visibility.Visible;
                    Image_AddCurrentlyViewPage.Visibility = Visibility.Visible;
                }
            }

        }

        void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            button_refresh_Click(null, null);
        }

        ~MainWindow() {
            isInRunningMode = false;
            if (m_notifyIcon != null)
            {
                try
                {
                    m_notifyIcon.Visible = false;
                }
                catch
                {
                }
            }
            
            Properties.Settings.Default.Save();
            AppController.Current.revokeSnarl();
        }

        
        private void BrowserStartLoad(object sender, DependencyPropertyChangedEventArgs e)
        {
                // Snarl.SnarlConnector.ShowMessage("Site loding", "just a second please", 10, "", IntPtr.Zero, 0);
                button_addCurrentViewedPage.Visibility = Visibility.Collapsed;
                Image_AddCurrentlyViewPage.Visibility = Visibility.Collapsed;
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
                label_TitleOfItem.Text = currentItem.title;
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
                        comboBox_browserView.ToolTip = "Select to be displayed cache version (F7)\r\nCache updated: " + currentItem.contentCache.UpdatedHumanReadable;
                    }
                }
            }
            else
            {
                comboBox_browserView.ToolTip = "Select to be displayed cache version (F7)";
            }
        }

        private void toggleReadIcon(bool isCurrentItemRead)
        {
            if(isCurrentItemRead)
                {
                    Uri src = new Uri(@"/Winslew;component/Images/markedRead.png", UriKind.Relative);
                    BitmapImage img = new BitmapImage(src);
                    ImageReadState.Source = img;
                    ImageReadState.ToolTip = "This item is marked as read - click to toggle state (Ctrl-s)";
                }
                else
                {
                    Uri src = new Uri(@"/Winslew;component/Images/unread.png", UriKind.Relative);
                    BitmapImage img = new BitmapImage(src);
                    ImageReadState.Source = img;
                    ImageReadState.ToolTip = "This item is marked as unread - click to toggle state (Ctrl-s)";
                }
        }

        #region Buttons

        private void button_refresh_Click(object sender, RoutedEventArgs e)
        {
            AppController.Current.UpdateItemsFromRiL(false);
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
            if (listView_Items.SelectedItems.Count > 0)
            {
                foreach (Item currentItem in listView_Items.SelectedItems)
                {
                    Dictionary<string, string> thisItem = new Dictionary<string, string>();
                    thisItem.Add("url", currentItem.url);
                    if (apiAccess.delete(thisItem))
                    {
                        AppController.Current.sendSnarlNotification("Item deleted", "Item has been deleted", currentItem.title);
                        AppController.Current.itemsCollection.Remove(currentItem);
                        toggleReadIcon(true);
                    }
                }
                refreshItems();
            }
        }

        private void button_help_Click(object sender, RoutedEventArgs e)
        {
            About myHelpWIndow = new About();
            if (Properties.Settings.Default.IsValidLicense)
            {
                myHelpWIndow.label_license.Text = "This copy is licensed to " + Properties.Settings.Default.Username;
            }
            else
            {
                myHelpWIndow.label_license.Text = "This is copy is unlicensed and in trial mode";
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
            if (listView_Items.SelectedItem != null && webKitBrowser != null)
            {
                var currentItem = listView_Items.SelectedItem as Item;
                if (currentItem.contentCache != null || Properties.Settings.Default.CurrentView.ToLower().EndsWith("online"))
                {

                    if (Properties.Settings.Default.CurrentView.ToLower().EndsWith("online") && apiAccess.checkIfOnline())
                    {
                        webKitBrowser.Navigate(currentItem.url);
                    }
                    else if (Properties.Settings.Default.CurrentView.ToLower().EndsWith("full") && System.IO.File.Exists(currentItem.contentCache.FullVersion))
                    {
                        webKitBrowser.Navigate(new Uri(currentItem.contentCache.FullVersion).AbsoluteUri);
                    }
                    else if (Properties.Settings.Default.CurrentView.ToLower().EndsWith("more") && System.IO.File.Exists(currentItem.contentCache.MoreVersion))
                    {
                        webKitBrowser.Navigate(new Uri(currentItem.contentCache.MoreVersion).AbsoluteUri);
                    }
                    else if (Properties.Settings.Default.CurrentView.ToLower().EndsWith("less") && System.IO.File.Exists(currentItem.contentCache.LessVersion))
                    {
                        webKitBrowser.Navigate(new Uri(currentItem.contentCache.LessVersion).AbsoluteUri);
                    }
                    else
                    {
                        webKitBrowser.Navigate(new Uri(AppController.Current.cacheStore.pathToNAPage).AbsoluteUri);
                    }
                }
                else
                {
                    webKitBrowser.Navigate(new Uri(AppController.Current.cacheStore.pathToNAPage).AbsoluteUri);
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
                currentItem.contentCache.FullVersion = null;
                tempList.Add(currentItem);
                AppController.Current.updateCache(tempList, true);
                AppController.Current.sendSnarlNotification("Cache has been updated", "Cache is being updated", currentItem.title);
                if (currentItem.contentCache != null)
                {
                    if (currentItem.contentCache.Updated != null)
                    {
                        comboBox_browserView.ToolTip = "Select to be displayed cache version\r\nCache updated: " + currentItem.contentCache.UpdatedHumanReadable;
                    }
                }
            }
            webKitBrowser.Reload();
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
                    webKitBrowser.ShowPrintDialog();
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

        private void button_uploadImage_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            
            dlg.DefaultExt = ".png"; // Default file extension
            dlg.Filter = "Images (*.png,*.jpg,*.gif,*.tif,*.bmp,*.pdf,*.xcf)|*.png;*.jpeg;*.jpg;*.gif;*.tif;*.tiff;*.bmp;*.pdf;*.xcf"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                string filename = dlg.FileName;
                int snarlId = AppController.Current.ShowSnarlNotificton("Image upload has been started", "Upload has been started", "Upload of file " + dlg.FileName + " has been started",0);
                Api.ImgurData uploadedImageData = Api.Imgur.uploadImage(dlg.FileName);
                Snarl.SnarlConnector.HideMessage(snarlId);
                if (uploadedImageData != null)
                {
                    AppController.Current.ShowSnarlNotificton("Image upload has been completed", "Upload completed", "Upload of file " + dlg.FileName + " has been completed", 10);
                    uploadedImageData.originalLocalImagePath = dlg.FileName;
                    AppController.Current.MemorizeImgurUpload(uploadedImageData);
                    AppController.Current.addItem(uploadedImageData.originalImage, System.IO.Path.GetFileNameWithoutExtension(dlg.FileName));
                }
                else
                {
                    AppController.Current.ShowSnarlNotificton("Image upload failed", "Upload failed", "Upload of file " + dlg.FileName + " failed", 10);
                }
            }
        }

        private void button_newPastebin_Click(object sender, RoutedEventArgs e)
        {
            Dialogs.Pastebin newPastebinWindow = new Dialogs.Pastebin();
            newPastebinWindow.Show();
        }


        #region Tray icon

        protected void trayContextShow(Object sender, System.EventArgs e)
        {
            m_notifyIcon_Click(null, null);
        }

        protected void trayContextRefresh(Object sender, System.EventArgs e)
        {
            button_refresh_Click(null, null);
        }


        protected void trayContextPreferences(Object sender, System.EventArgs e)
        {
            button_preferences_Click(null, null);
        }

        protected void trayContextUploadImage(Object sender, System.EventArgs e)
        {
            button_uploadImage_Click(null, null);
        }

        protected void trayContextNewPastebin(Object sender, System.EventArgs e)
        {
            button_newPastebin_Click(null, null);
        }

        protected void trayContextUploadPastebin(Object sender, System.EventArgs e)
        {
            button_uploadPastebin_Click(null, null);
        }

        protected void trayContextAddItem(Object sender, System.EventArgs e)
        {
            AddNew itemAdd = new AddNew();
            itemAdd.Show();
        }

        protected void trayContextQuit(Object sender, System.EventArgs e)
        {
            Close();
        }

        void OnStateChanged(object sender, EventArgs args)
        {
            if (!isInRunningMode)
            {
                return;
            }
            if (WindowState == WindowState.Minimized)
            {
                if (Properties.Settings.Default.MinimizeToTray)
                {
                    Hide();
                }
            }
            else if (WindowState != WindowState.Minimized)
            {
                ShowTrayIcon(false);
                m_storedWindowState = WindowState;
            }
        }
        void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            CheckTrayIcon();
        }

        void m_notifyIcon_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = m_storedWindowState;
        }
        void CheckTrayIcon()
        {
            ShowTrayIcon(!IsVisible);
        }

        void ShowTrayIcon(bool show)
        {
            if (m_notifyIcon != null)
                m_notifyIcon.Visible = show;
        }

        #endregion

        private void button_addCurrentViewedPage_Click(object sender, RoutedEventArgs e)
        {
            AddNew newItem = new AddNew();
            newItem.SetUrl(webKitBrowser.Url.AbsoluteUri);
            newItem.SetTitle(webKitBrowser.DocumentTitle);
            newItem.Show();
        }

        private void button_uploadPastebin_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.Filter = "Textfiles (*.txt,*.xml,*.*)|*.txt;*.xml;*.*"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                string filename = dlg.FileName;
                string text = "";
                System.IO.StreamReader textReader = System.IO.File.OpenText(dlg.FileName);

                // int snarlId = AppController.Current.ShowSnarlNotificton("Image upload has been started", "Upload has been started", "Upload of file " + dlg.FileName + " has been started", 0);
                string input = null;
                while ((input = textReader.ReadLine()) != null)
                {
                    text += input + "\n";
                }
                textReader.Close();
                textReader = null;

                text = text.TrimEnd();

                Dialogs.Pastebin myPastebin = new Dialogs.Pastebin();
                myPastebin.textBox_text.Text = text;
                myPastebin.textBox_title.Text = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
                myPastebin.selectHighlighter(System.IO.Path.GetExtension(dlg.FileName));
                myPastebin.Show();
            }
        }

        private void ContextUploadImage_Click(object sender, RoutedEventArgs e)
        {
            button_uploadImage_Click(null, null);
        }

        private void ContextNewPastebin_Click(object sender, RoutedEventArgs e)
        {
            button_newPastebin_Click(null, null);
        }

        private void ContextUploadPastebin_Click(object sender, RoutedEventArgs e)
        {
            button_uploadPastebin_Click(null, null);
        }

        #region Keyboard shortcuts

        private void RefreshRiLExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            button_refresh_Click(null, null);
        }

        private void ToggleReadUnreadViewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            button_toggleView_Click(null, null);
        }

        private void ChangeTitleExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            button_changeTitle_Click(null, null);
        }

   

        private void EditTagsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            button_editTags_Click(null, null);
        }

        private void AddTagsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            button_addTags_Click(null, null);
        }

        private void RemoveTagsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            button_removeTags_Click(null, null);
        }

        private void NewItemExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            button_addNew_Click(null, null);
        }

        private void UploadImageExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            button_uploadImage_Click(null, null);
        }

        private void NewPastebinExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            button_newPastebin_Click(null, null);
        }

        private void UploadPastebinExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            button_uploadPastebin_Click(null, null);
        }

        private void DeleteExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            button_delete_Click(null, null);
        }

        private void FilterExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.textBox_filterTags != null && this.textBox_filterTitle != null)
            {
                if (this.textBox_filterTitle.IsFocused)
                {
                    this.textBox_filterTags.Focus();
                }
                else
                {
                    this.textBox_filterTitle.Focus();
                }
            }
        }

        private void PreferencesExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            button_preferences_Click(null, null);
        }

        private void HelpExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            button_help_Click(null, null);
        }

        private void ToggleReadUnreadStateExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            button_markRead_Click(null, null);
        }

        private void AddCurrentPageExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            button_addCurrentViewedPage_Click(null, null);
        }

        private void CopyUrlExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            button_copyUrlToClipboard_Click(null, null);
        }

        private void RefreshCacheExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            button_updateCache_Click(null, null);
        }

        private void ChangeStyleExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.comboBox_chooseStyle != null)
            {
                this.comboBox_chooseStyle.Focus();
            }
        }

        private void ChangeViewTypeExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.comboBox_browserView != null)
            {
                this.comboBox_browserView.Focus();
            }
        }



        private void OpenInBrowserExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            button_openInBrowser_Click(null, null);
        }


        private void SelectListViewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            listView_Items.Focus();
        }

        private void SelectBrowserExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            webKitBrowser.Focus();
        }

        #endregion

        private void buttonBrowserReload_Click(object sender, RoutedEventArgs e)
        {
            webKitBrowser.Reload();
        }

        private void buttonBrowserBack_Click(object sender, RoutedEventArgs e)
        {
            webKitBrowser.GoBack();
        }

        private void buttonBrowserNext_Click(object sender, RoutedEventArgs e)
        {
            webKitBrowser.GoForward();
        }

        private void buttonBrowserStop_Click(object sender, RoutedEventArgs e)
        {
            webKitBrowser.Stop();
            progressBarLoadingPage.Visibility = Visibility.Collapsed;
        }
    }

}
