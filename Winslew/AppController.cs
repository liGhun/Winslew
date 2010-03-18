using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Snarl;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Net.Sockets;

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

        private bool isInitialFetch = true;
        public ObservableCollection<Item> itemsCollection
        {
            get;
            set;
        }

        private DateTime ApiResetUser = DateTime.Now;
        private DateTime ApiResetApp = DateTime.Now;

        private delegate void UpdateProgressBarDelegate(
        System.Windows.DependencyProperty dp, Object value);
        
        public Api.ContentCacheStore cacheStore = new Api.ContentCacheStore();
        

        public AppController()
        {
            Current = this;

            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if(!System.IO.Directory.Exists(appDataPath + "\\Winslew\\"))
                {
                    System.IO.Directory.CreateDirectory(appDataPath + "\\Winslew\\");
                }
                string alreadyMigratedSettingsTriggerFile = appDataPath + "\\Winslew\\PreferencesMigrated-" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() + ".upd";
                if (!System.IO.File.Exists(alreadyMigratedSettingsTriggerFile))
                {
                    Properties.Settings.Default.Upgrade();
                    System.IO.File.Create(alreadyMigratedSettingsTriggerFile);
                }
                Properties.Settings.Default.Save();
            }
            catch 
            {
                
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
            }

            UpdateAvailable myUpdateCheck = new UpdateAvailable();
        }

        public void updateCache(List<Item> listOfItems, bool updateWithNewerVersion) {
            UpdatingCache myUpdateCacheWindow = new UpdatingCache();

            myUpdateCacheWindow.label2.Content = "Winslew " + Formatter.prettyVersion.getNiceVersionString(System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            myUpdateCacheWindow.Show();
            myUpdateCacheWindow.progressBar1.Maximum = listOfItems.Count();
            double i = 1;

            UpdateProgressBarDelegate updatePbDelegate =
                new UpdateProgressBarDelegate(myUpdateCacheWindow.progressBar1.SetValue);

            foreach (Item item in listOfItems)
            {
                myUpdateCacheWindow.label1.Content = "Updating cache " + i.ToString() + " of " + listOfItems.Count();
                myUpdateCacheWindow.label_itemTitle.Content = item.title;
                myUpdateCacheWindow.UpdateLayout();
                Dispatcher.CurrentDispatcher.Invoke(updatePbDelegate,
                System.Windows.Threading.DispatcherPriority.Background,
                new object[] { ProgressBar.ValueProperty, i });
                addToCache(item, updateWithNewerVersion);
                i++;
            }
            myUpdateCacheWindow.Close();
        }

        public void credentialsSavedSuccessfully()
        {
            if (mainWindow == null)
            {
                mainWindow = new MainWindow();
                

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

                    // updateCache(tempList, false);

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
                mainWindow.refreshItems();
                mainWindow.updateViewOfFrame();
                
                mainWindow.Show();
            }

        }

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
            item.contentCache = cacheStore.addToCache(item, updateWithNewerVersion);
            updateItem(item, newItem);
        }

        public static void Start()
        {
            Current = new AppController();
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
                            itemsCollection.Remove(oldItem);
                            itemsCollection.Add(newItem);
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
            AppController.Current.itemsCollection.Remove(oldItem);
            AppController.Current.itemsCollection.Add(newItem);
            mainWindow.refreshItems();  
        }

        public void changeTags(Dictionary<string, string> data)
        {
            apiAccess.changeTags(data);
            sendSnarlNotification("Item tags changed", "Item tags have been changed", "");
        }

        public void addItem(Item newItem)
        {
            AppController.Current.itemsCollection.Add(newItem);
            mainWindow.refreshItems();
        }

        public Api.Response getCacheText(string urlToBeCached, string type)
        {
            string oldTitle = "";
            Api.Response returnValue = new Winslew.Api.Response();
            if (mainWindow != null)
            {
                oldTitle = mainWindow.Title;
                mainWindow.Title = "Caching items";
                returnValue = apiAccess.getCacheText(urlToBeCached, type);
                if (mainWindow != null)
                {
                    mainWindow.Title = oldTitle;
                }
            }
            return returnValue;
        }

        public void updateApiStatusBar(int userAvailable, int userLimit, int userReset, int appAvailable, int appLimit, int appReset) {
            if (mainWindow != null)
            {
                if (mainWindow.label_apiLimits != null && appLimit != 0 && userLimit != 0)
                {
                    DateTime now = DateTime.Now;
                    mainWindow.label_apiLimits.Content = string.Format("Api usage: User: {0} of {1} left, next reset at {2} - Winslew: {3} of {4} left, next reset at {5}",
                        userAvailable.ToString(),
                        userLimit.ToString(),
                        now.AddSeconds(userReset).ToLongTimeString(),
                        appAvailable.ToString(),
                        appLimit.ToString(),
                        now.AddSeconds(appReset).ToLongTimeString());

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
    }
}
