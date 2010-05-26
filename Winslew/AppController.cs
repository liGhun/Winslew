using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Snarl;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Threading;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Net.Sockets;
using System.Diagnostics;

namespace Winslew
{
    public class AppController
    {
        MainWindow mainWindow;
        public static AppController Current { get; private set; }
        private NativeWindowApplication.SnarlMsgWnd snarlMsgWindow;
        private IntPtr SnarlMessageWindowHandle = IntPtr.Zero;
        private string pathToIcon = "";
        public Api.Api apiAccess = new Api.Api();
        public DateTime expirationDate = new DateTime(2010, 04, 01);

        private BackgroundWorker backgroundWorker1;
        private List<Item> queueOfToBeUpdatedItems = new List<Item>();

        private bool isInitialFetch = true;
        public ObservableCollection<Item> itemsCollection
        {
            get;
            set;
        }

        private double SumOfIncompleteCaches = 0;

        private DateTime ApiResetUser = DateTime.Now;
        private DateTime ApiResetApp = DateTime.Now;

        private string appDataPath = "";
        private string appProgramPath = "";
        Dictionary<string, string> AvailableStyles;
       // FileSystemWatcher fsWatcherStyles;

        private delegate void UpdateProgressBarDelegate(
        System.Windows.DependencyProperty dp, Object value);


        private delegate void UpdateLabelDelegate(
        System.Windows.DependencyProperty dp, Object value);

        public Api.ContentCacheStore cacheStore = new Api.ContentCacheStore();

        #region Startup and initialization

        public static void Start()
        {
            Current = new AppController();
        }

        public AppController()
        {
            Current = this;

            try
            {



                // Checking and (if needed) creating preferences directories and files

                appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Winslew\\";
                appProgramPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
                if(!System.IO.Directory.Exists(appDataPath))
                {
                    System.IO.Directory.CreateDirectory(appDataPath);
                }
                string alreadyMigratedSettingsTriggerFile = appDataPath + "\\PreferencesMigrated-" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + ".upd";
                if (!System.IO.File.Exists(alreadyMigratedSettingsTriggerFile))
                {
                    Properties.Settings.Default.Upgrade();
                    System.IO.File.Create(alreadyMigratedSettingsTriggerFile);
                }   
                Properties.Settings.Default.Save();

                backgroundWorker1 = new BackgroundWorker();
                backgroundWorker1.WorkerReportsProgress = true;
                backgroundWorker1.WorkerSupportsCancellation = true;

                backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
                backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
                backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
            }
            catch (Exception exp)
            {
                Trace.WriteLine(exp.Message);
                Trace.WriteLine(exp.StackTrace);
            }

            LicenseChecker.checkLicense(Properties.Settings.Default.Username, Properties.Settings.Default.LicenseCode);

            if (Properties.Settings.Default.LoginHasBeenTestedSuccessfully && Properties.Settings.Default.Username != "" && Properties.Settings.Default.Password != "")
            {
                credentialsSavedSuccessfully();
            }
            else
            {
                Preferences myPrefWindow = new Preferences();
                myPrefWindow.Show();
            }

            this.pathToIcon = pathToIcon = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\images\\Winslew.png";
            snarlMsgWindow = new NativeWindowApplication.SnarlMsgWnd();
            SnarlMessageWindowHandle = snarlMsgWindow.Handle;
           
            UpdateAvailable myUpdateCheck = new UpdateAvailable();
        }



        public void credentialsSavedSuccessfully()
        {
            if (mainWindow == null)
            {
                mainWindow = new MainWindow();
            }

            if (SnarlConnector.GetSnarlWindow() != IntPtr.Zero)
            {
                SnarlConnector.RegisterConfig(SnarlMessageWindowHandle, "Winslew", Snarl.WindowsMessage.WM_USER + 55, pathToIcon);
                SnarlConnector.RegisterAlert("Winslew", "New item");
                SnarlConnector.RegisterAlert("Winslew", "Item added");
                SnarlConnector.RegisterAlert("Winslew", "Item marked as read");
                SnarlConnector.RegisterAlert("Winslew", "Item marked as unread");
                SnarlConnector.RegisterAlert("Winslew", "Item tags changed");
                SnarlConnector.RegisterAlert("Winslew", "Item deleted");
                SnarlConnector.RegisterAlert("Winslew", "Cache has been updated");
                SnarlConnector.RegisterAlert("Winslew", "User API limit critical");
                SnarlConnector.RegisterAlert("Winslew", "Application API limit critical");

                hideSnarlHint();
            }


            if (apiAccess.checkIfOnline())
            {

                SetData(false);
                isInitialFetch = false;

                List<Item> tempList = new List<Item>();
                if (itemsCollection != null)
                {
                    foreach (Item item in itemsCollection)
                    {
                        tempList.Add(item);
                    }
                }

                updateCache(tempList, false);

            }
            else
            {
                string storePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Winslew\\ContentCache.xml";
                if (File.Exists(storePath))
                {
                    List<Item> tempList = new List<Item>();

                    XmlSerializer xmlSerializer = new
                    XmlSerializer(typeof(List<Item>), new
                    XmlRootAttribute("ItemsCollection"));
                    FileStream fileStram = new FileStream(storePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    XmlReader reader = new XmlTextReader(fileStram);

                    tempList = (List<Item>)xmlSerializer.Deserialize(reader);
                    reader.Close();
                    fileStram.Close();

                    itemsCollection = new ObservableCollection<Item>();
                    foreach (Item storedItem in tempList)
                    {
                        itemsCollection.Add(storedItem);
                    }
                }
            }

            // xxx Load combobox styles
            LoadComboBoxStyles();

            mainWindow.refreshItems();
            mainWindow.updateViewOfFrame();

            mainWindow.Show();
            mainWindow.Focus();
            StartCacheUpdate(null);

        }

        private void LoadComboBoxStyles()
        {
            AvailableStyles = new Dictionary<string, string>();
            if (mainWindow != null)
            {
                string[] availableDefaultStyles = Directory.GetFiles(appProgramPath.Substring(6) + "\\Styles", "*.css");
                foreach (string style in availableDefaultStyles)
                {
                    AvailableStyles.Add(Path.GetFileNameWithoutExtension(style), style);
                }
                if(Directory.Exists(appDataPath + "\\Cache\\Styles")) {
                    string[] availableUserDefinedStyles = Directory.GetFiles(appDataPath + "\\Cache\\Styles");
                    foreach (string style in availableUserDefinedStyles)
                    {
                        if(AvailableStyles.ContainsKey(Path.GetFileNameWithoutExtension(style))) {
                            // user defined ones override default styles
                            AvailableStyles[Path.GetFileNameWithoutExtension(style)] = style;
                        }
                        else
                        {
                            AvailableStyles.Add(Path.GetFileNameWithoutExtension(style), style);
                        }
                    }
                }
                foreach(string styleName in AvailableStyles.Keys) {
                   mainWindow.comboBox_chooseStyle.Items.Add(styleName);
                }
                if(!Directory.Exists(appDataPath + "\\Cache\\Styles")) {
                    Directory.CreateDirectory(appDataPath + "\\Cache\\Styles");
                }
                /*
                fsWatcherStyles = new FileSystemWatcher();
                fsWatcherStyles.Path = appDataPath + "\\Cache\\Styles";
                fsWatcherStyles.Filter = "*.css";
                fsWatcherStyles.SynchronizingObject = ;
                fsWatcherStyles.Created += (obj, e) =>AvailableStyles.BeginInvoke(DispatcherPriority.Send, new Action(() =>
  {
    // Code to handle Changed event
  }));
                fsWatcherStyles.Deleted += new FileSystemEventHandler(fsWatcherStyles_Deleted);
                fsWatcherStyles.EnableRaisingEvents = true;
                */
                mainWindow.comboBox_chooseStyle.SelectedValue = Properties.Settings.Default.Style;
            }
        }

        public void selectAnotherStyle(string styleName)
        {
            if(AvailableStyles.ContainsKey(styleName)) {
                if (File.Exists(AvailableStyles[styleName]))
                {
                    File.Copy(AvailableStyles[styleName], appDataPath + "\\Cache\\actualStylesheet.css",true);
                    mainWindow.frame_content.Refresh();
                    Properties.Settings.Default.Style = styleName;
                }
            }
        }

        private void fsWatcherStyles_Created(object source, FileSystemEventArgs e)
        {
            if (AvailableStyles.ContainsKey(Path.GetFileNameWithoutExtension(e.FullPath)))
            {
                // user defined ones override default styles
                AvailableStyles[Path.GetFileNameWithoutExtension(e.FullPath)] = e.FullPath;
            }
            else
            {
                AvailableStyles.Add(Path.GetFileNameWithoutExtension(e.FullPath), e.FullPath);
                // AppController.Current.mainWindow.comboBox_chooseStyle.Items.Add(Path.GetFileNameWithoutExtension(e.FullPath));
            }
        }

        private void fsWatcherStyles_Deleted(object source, FileSystemEventArgs e)
        {
            if (AvailableStyles.ContainsKey(Path.GetFileNameWithoutExtension(e.FullPath)))
            {
                AvailableStyles.Remove(Path.GetFileNameWithoutExtension(e.FullPath));
                // AppController.Current.mainWindow.comboBox_chooseStyle.Items.Remove(Path.GetFileNameWithoutExtension(e.FullPath));
            }
        }

        private void loadCacheStoreFromHdd(List<Item> listOfItems)
        {
            updateCache(listOfItems, false);
        }

        #endregion

        public void updateCache(List<Item> listOfItems, bool updateWithNewerVersion) {
            if (updateWithNewerVersion)
            {
                StartCacheUpdate(listOfItems);
                return;
            }
            UpdatingCache myUpdateCacheWindow = new UpdatingCache();

            myUpdateCacheWindow.label2.Content = "Winslew " + Formatter.prettyVersion.getNiceVersionString(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            myUpdateCacheWindow.label1.Content = "Loading stored cache...";
            myUpdateCacheWindow.Show();
            myUpdateCacheWindow.progressBar1.Maximum = listOfItems.Count();
            
            UpdateProgressBarDelegate updatePbDelegate =
                 new UpdateProgressBarDelegate(myUpdateCacheWindow.progressBar1.SetValue);

            double i = 1;
            foreach (Item item in listOfItems)
            {
                myUpdateCacheWindow.label1.Content = "Loading cache " + i.ToString() + " of " + listOfItems.Count();
                myUpdateCacheWindow.label_itemTitle.Content = item.title;
                myUpdateCacheWindow.UpdateLayout();
                Dispatcher.CurrentDispatcher.Invoke(updatePbDelegate,
                System.Windows.Threading.DispatcherPriority.Background,
                new object[] { ProgressBar.ValueProperty, i });
                if (!updateWithNewerVersion)
                item.contentCache = cacheStore.LoadStoredCache(item);
             
                i++;
            }

            myUpdateCacheWindow.Close();
        }

        private void StartCacheUpdate(List<Item> ToBeUpdatedItems) {
            if(!apiAccess.checkIfOnline())
            {
                return;
            }
            bool isInitialUpdate = false;
            if (ToBeUpdatedItems == null)
            {
                isInitialUpdate = true;
                IEnumerable<Item> NextIncompleteItems = itemsCollection.Where((Item bq) => bq.contentCache.IsComplete == false);
                 
                ToBeUpdatedItems = new List<Item>();
                foreach (Item item in NextIncompleteItems)
                {
                    ToBeUpdatedItems.Add(item);
                }
            }

            if (backgroundWorker1.IsBusy)
            {
                queueOfToBeUpdatedItems.AddRange(ToBeUpdatedItems);
                SumOfIncompleteCaches += ToBeUpdatedItems.Count();
                return;
            }

            SumOfIncompleteCaches = ToBeUpdatedItems.Count();

            mainWindow.LabelCurrentAction.Content = "Updating 1 of " + SumOfIncompleteCaches.ToString();
            mainWindow.LabelCurrentAction.Visibility = System.Windows.Visibility.Visible;
            mainWindow.ProgressBarCurrentAction.Value = 0;
            mainWindow.ProgressBarCurrentAction.Visibility = System.Windows.Visibility.Visible;
            if (isInitialUpdate)
            {
                backgroundWorker1.RunWorkerAsync(null);
            }
            else
            {
                backgroundWorker1.RunWorkerAsync(ToBeUpdatedItems);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            List<Item> ToBeWorkedItems = new List<Item>();
            bool initialFastFetch = true;
            if (e.Argument == null)
            {
                IEnumerable<Item> NextIncompleteItems = itemsCollection.Where((Item bq) => bq.contentCache.IsComplete == false);

                foreach (Item item in NextIncompleteItems)
                {
                    ToBeWorkedItems.Add(item);
                }
            }
            else
            {
                ToBeWorkedItems = (List<Item>)e.Argument;
                initialFastFetch = false;
            }
            List<Item> Return = new List<Item>();


            int done = 0;
            foreach (Item currentItem in ToBeWorkedItems)
            {
                System.Threading.Thread.Sleep(10);                
                currentItem.contentCache = cacheStore.addToCache(currentItem, false, false, initialFastFetch);
                AppController.Current.sendSnarlNotification("Cache has been updated", "Cache of more and less version has been updated", currentItem.title);
                done++;
                Return.Add(currentItem);
                backgroundWorker1.ReportProgress(done, currentItem);
            }

            if (initialFastFetch)
            {
                Return = new List<Item>();

                done = 0;
                foreach (Item currentItem in ToBeWorkedItems)
                {
                    System.Threading.Thread.Sleep(10);
                    currentItem.contentCache = cacheStore.addToCache(currentItem, false, false, !initialFastFetch);
                    AppController.Current.sendSnarlNotification("Cache has been updated", "Cache of full version has been updated", currentItem.title);
                    done++;
                    Return.Add(currentItem);
                    backgroundWorker1.ReportProgress(done, currentItem);
                }

            }

            e.Result = Return;
        }


        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
            }
            else if (e.Cancelled)
            {
                
            }
            else
            {
                mainWindow.LabelCurrentAction.Visibility = System.Windows.Visibility.Collapsed;
                mainWindow.ProgressBarCurrentAction.Visibility = System.Windows.Visibility.Collapsed;
            }
            if (queueOfToBeUpdatedItems.Count != 0)
            {
                StartCacheUpdate(queueOfToBeUpdatedItems);
                queueOfToBeUpdatedItems = new List<Item>();
            }
        }

        // This event handler updates the progress bar.
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            double newPercentage = e.ProgressPercentage * 100 / SumOfIncompleteCaches;
            mainWindow.ProgressBarCurrentAction.Value = newPercentage;
            Item UpdatedOne = (Item)e.UserState;
            mainWindow.LabelCurrentAction.Content = "Updating cache " + e.ProgressPercentage.ToString() + " of " + SumOfIncompleteCaches;
            int index = itemsCollection.IndexOf(itemsCollection.Where((Item bq) => bq.id == UpdatedOne.id).FirstOrDefault());
            if(index < 0) {
                itemsCollection.Add(UpdatedOne);
                mainWindow.refreshItems();  
            }
            else
            {
                itemsCollection[index] = UpdatedOne;
                mainWindow.refreshItems();  
                if (mainWindow.listView_Items.SelectedItem == UpdatedOne)
                {
                    mainWindow.frame_content.Refresh();
                }
            }
        }


        // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^


        public void revokeSnarl()
        {
            SnarlConnector.RevokeConfig(SnarlMessageWindowHandle);
        }

        ~AppController()
        {
            if (itemsCollection != null)
            {
                string storePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Winslew\\ContentCache.xml";
                List<Item> tempList = new List<Item>();
                foreach (Item item in itemsCollection)
                {
                    tempList.Add(item);
                }
                XmlSerializer xmlSerializer = new
                XmlSerializer(typeof(List<Item>), new
                XmlRootAttribute("ItemsCollection"));
                TextWriter writer = new StreamWriter(storePath);
                xmlSerializer.Serialize(writer, tempList);
                writer.Close();
            }
        }

        public void addToCache(Item item, bool updateWithNewerVersion)
        {
            Item newItem = item;
            item.contentCache = cacheStore.addToCache(item, updateWithNewerVersion, false,false);
        }



        public void sendSnarlNotification(string className, string title, string text)
        {
            SnarlConnector.ShowMessageEx(className, title, text, 10, pathToIcon, SnarlMessageWindowHandle, WindowsMessage.WM_USER + 47, "");
        }

        public void refreshMainWindow()
        {
            mainWindow.refreshItems();
        }

        public void SetData(bool freshRefresh)
        {
            IEnumerable<Item> data = apiAccess.getList(freshRefresh, false, true);
            if (isInitialFetch || freshRefresh)
            {
                itemsCollection = new ObservableCollection<Item>(data);
                List<Item> tempList = new List<Item>();
                foreach (Item newItem in itemsCollection)
                {
                    tempList.Add(newItem);
                    
                }
                AppController.Current.updateCache(tempList, false);
            }
            else
            {
                foreach (Item newItem in data)
                {
                    IEnumerable<Item> alreadyExistingItemWithId = itemsCollection.Where((Item bq) => bq.id == newItem.id);

                    if (alreadyExistingItemWithId == null)
                    {
                        itemsCollection.Add(newItem);
                        List<Item> tempList = new List<Item>();
                        tempList.Add(newItem);
                        AppController.Current.updateCache(tempList, true);
                    }
                    else if (alreadyExistingItemWithId.Count() == 0)
                    {
                        itemsCollection.Add(newItem);
                        List<Item> tempList = new List<Item>();
                        tempList.Add(newItem);
                        AppController.Current.updateCache(tempList, true);
                        AppController.Current.sendSnarlNotification("New item", "New item added", newItem.title);
                        AppController.Current.sendSnarlNotification("Cache has been updated", "Cache has been updated", newItem.title);
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
                            updateItem(oldItem, newItem);
                            mainWindow.updateViewOfFrame();
                        }
                    }
                }
            }
        }

 

        public bool addItem(string url, string title)
        {
            apiAccess.addToList(url, title);
            
            SetData(false);
            
            mainWindow.refreshItems();
            return true;
        }

        public void updateItem(Item oldItem, Item newItem)
        {
            int index = itemsCollection.IndexOf(oldItem);
            if (index < 0)
            {
                itemsCollection.Add(newItem);
            }
            else
            {
                itemsCollection[index] = newItem;
            }
            mainWindow.refreshItems();  
            if (mainWindow.listView_Items.SelectedItem == newItem)
            {
                mainWindow.updateViewOfFrame();
            }
            
        }

        public void changeTags(Dictionary<string, string> data)
        {
            apiAccess.changeTags(data);
            sendSnarlNotification("Item tags changed", "Item tags have been changed", "");
        }

        public void addTags(List<Dictionary<string, string>> data)
        {
            apiAccess.addTags(data);
            SetData(false);
        }

        public void changeTitle(Dictionary<string, string> data)
        {
            apiAccess.changeTitle(data);
        }

        public void addItem(Item newItem)
        {
            AppController.Current.itemsCollection.Add(newItem);
            mainWindow.refreshItems();
        }

        public Api.Response getCacheText(string urlToBeCached, string type)
        {
            Api.Response returnValue = new Winslew.Api.Response();
            returnValue = apiAccess.getCacheText(urlToBeCached, type);
            return returnValue;
        }

        public void updateApiStatusBar(int userAvailable, int userLimit, int userReset, int appAvailable, int appLimit, int appReset) {
            if (mainWindow != null)
            {
                if (mainWindow.label_apiLimits != null && appLimit != 0 && userLimit != 0)
                {
                    DateTime now = DateTime.Now;
                    //mainWindow.label_apiLimits.Content = string.Format("Api usage: User: {0} of {1} left, next reset at {2} - Winslew: {3} of {4} left, next reset at {5}",
                    //userAvailable.ToString(),
                    //    userLimit.ToString(),
                    //    now.AddSeconds(userReset).ToLongTimeString(),
                    //    appAvailable.ToString(),
                    //    appLimit.ToString(),
                    //    now.AddSeconds(appReset).ToLongTimeString());
                    mainWindow.label_apiLimits.Content = string.Format("Api usage: {0} of {1} left, next reset at {2}",
                        userAvailable.ToString(),
                        userLimit.ToString(),
                        now.AddSeconds(userReset).ToLongTimeString()
                        );

                    int userApiUsage = (userAvailable * 100 / userLimit);
                    if(now.AddSeconds(userReset) <= ApiResetUser && userApiUsage < Properties.Settings.Default.ApiUserWarnPercentage) {
                        SnarlConnector.ShowMessageEx("User API limit critical", "Your API limits are about to be reached", "Your number of requests to the RiL API is about to exceed soon - you have " + userAvailable.ToString() + " requests left of " + userLimit.ToString() + ".\n\nNext reset will be at " + now.AddSeconds(userReset).ToLongTimeString(), 10, pathToIcon, SnarlMessageWindowHandle, WindowsMessage.WM_USER + 32, "");
                    }
                    ApiResetUser = now.AddSeconds(userReset);

                    int appApiUsage = (appAvailable * 100 / appLimit);
                    if (now.AddSeconds(appReset) <= ApiResetApp && appApiUsage < Properties.Settings.Default.ApiAppWarnPercentage)
                    {
                        SnarlConnector.ShowMessageEx("Application API limit critical", "Winslew API limits are about to be reached", "The number of requests from Winslew to the RiL API is about to exceed soon - you have " + appAvailable.ToString() + " requests left of " + appLimit.ToString() + ".\n\nNext reset will be at " + now.AddSeconds(appReset).ToLongTimeString() + "\n\nIf this happens on a regular base please contact the developer of Winslew", 10, pathToIcon, SnarlMessageWindowHandle, WindowsMessage.WM_USER + 33, "");
                    }
                    ApiResetApp = now.AddSeconds(appReset);
                }
            }
        }

        public void hideSnarlHint()
        {
            if (mainWindow != null)
            {
                mainWindow.button_openSnarlIcon.Visibility = System.Windows.Visibility.Collapsed;
                mainWindow.button_openSnarlText.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
    }
}
