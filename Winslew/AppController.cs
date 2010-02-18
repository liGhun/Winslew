using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Snarl;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Threading;
using System.Windows.Threading;


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

        private bool isInitialFetch = true;
        public ObservableCollection<Item> itemsCollection
        {
            get;
            set;
        }

        private delegate void UpdateProgressBarDelegate(
        System.Windows.DependencyProperty dp, Object value);
        
        public Api.ContentCacheStore cacheStore = new Api.ContentCacheStore();
        

        public AppController()
        {
            Current = this;

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
            }
        }

        public void credentialsSavedSuccessfully()
        {
            if (mainWindow == null)
            {
                mainWindow = new MainWindow();
                UpdatingCache myUpdateCacheWindow = new UpdatingCache();

                myUpdateCacheWindow.label2.Content = "Winslew " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                myUpdateCacheWindow.Show();

                SetData();
                isInitialFetch = false;

                List<Item> tempList = new List<Item>();
                if (itemsCollection != null)
                {
                    foreach (Item item in itemsCollection)
                    {
                        tempList.Add(item);
                    }
                }

                myUpdateCacheWindow.progressBar1.Maximum = tempList.Count();
                double i = 1;


                UpdateProgressBarDelegate updatePbDelegate =
                    new UpdateProgressBarDelegate(myUpdateCacheWindow.progressBar1.SetValue);

                foreach (Item item in tempList)
                {
                    myUpdateCacheWindow.label1.Content = "Updating cache " + i.ToString() + " of " + tempList.Count();
                    Dispatcher.CurrentDispatcher.Invoke(updatePbDelegate,
                    System.Windows.Threading.DispatcherPriority.Background,
                    new object[] { ProgressBar.ValueProperty, i });
                    addToCache(item);
                    i++;
                }

                mainWindow.refreshItems();
                mainWindow.updateViewOfFrame();
                myUpdateCacheWindow.Close();
                mainWindow.Show();
            }

        }

        public void revokeSnarl()
        {
            SnarlConnector.RevokeConfig(SnarlMessageWindowHandle);
        }

        ~AppController()
        {
            
        }

        public void addToCache(Item item)
        {
            Item newItem = item;
            item.contentCache = cacheStore.addToCache(item, false);
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


        public void SetData()
        {
            IEnumerable<Item> data = apiAccess.getList(0, false, true);
            if (isInitialFetch)
            {
                itemsCollection = new ObservableCollection<Item>(data);
            }
            else
            {
                foreach (Item newItem in data)
                {
                    IEnumerable<Item> alreadyExistingItemWithId = itemsCollection.Where((Item bq) => bq.id == newItem.id);

                    if (alreadyExistingItemWithId == null)
                    {
                        itemsCollection.Add(newItem);
                    }
                    else if (alreadyExistingItemWithId.Count() == 0)
                    {
                        itemsCollection.Add(newItem);
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
            
            SetData();
            
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
    }
}
