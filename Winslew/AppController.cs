using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Snarl;


namespace Winslew
{
    public class AppController
    {
        MainWindow mainWindow;
        public static AppController Current { get; private set; }
        private NativeWindowApplication.SnarlMsgWnd snarlMsgWindow;
        private IntPtr SnarlMessageWindowHandle = IntPtr.Zero;
        private string pathToIcon = "";
        
        public Api.ContentCacheStore cacheStore = new Api.ContentCacheStore();
        

        public AppController()
        {
            Current = this;
            mainWindow = new MainWindow();
            mainWindow.Show();

            

            List<Item> tempList = new List<Item>();
            foreach (Item item in mainWindow.UnreadItems)
            {
                tempList.Add(item);
            }
            
            int i = 1;
            foreach (Item item in tempList)
            {
                mainWindow.label_updateCache.Content = "Updating cache " + i.ToString() + " of " + tempList.Count();
                addToCache(item);
            }

            mainWindow.label_updateCache.Visibility = System.Windows.Visibility.Collapsed;
            mainWindow.frame_content.Visibility = System.Windows.Visibility.Visible;


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

        public bool addItem(string url, string title)
        {
            mainWindow.apiAccess.addToList(url, title);
            Item newItem = new Item();
            newItem.title = title;
            newItem.url = url;
            newItem.timeAddedHumanReadable = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
            mainWindow.UnreadItems.Add(newItem);
            mainWindow.refreshItems();
            return true;
        }

        public void updateItem(Item oldItem, Item newItem)
        {
            mainWindow.UnreadItems.Remove(oldItem);
            mainWindow.UnreadItems.Add(newItem);
            mainWindow.refreshItems();  
        }

        public void changeTags(Dictionary<string, string> data)
        {
            mainWindow.apiAccess.changeTags(data);
            sendSnarlNotification("Item tags changed", "Item tags have been changed", "");
        }

        public void addItem(Item newItem)
        {
            mainWindow.UnreadItems.Add(newItem);
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
                returnValue = mainWindow.apiAccess.getCacheText(urlToBeCached, type);
                if (mainWindow != null)
                {
                    mainWindow.Title = oldTitle;
                }
            }
            return returnValue;
        }
    }
}
