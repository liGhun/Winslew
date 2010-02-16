using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Winslew;
using System.Linq;
using Snarl;

namespace NativeWindowApplication
{

    // Summary description for SnarlMsgWnd.
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]


    public class SnarlMsgWnd : NativeWindow
    {
        CreateParams cp = new CreateParams();

        public string pathToIcon = "";


        int SNARL_GLOBAL_MESSAGE;

        public SnarlMsgWnd()
        {
            // Create the actual window
            this.CreateHandle(cp);
            this.SNARL_GLOBAL_MESSAGE = Snarl.SnarlConnector.GetGlobalMsg();
            pathToIcon = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\Images\\Winslew.png";
        }

        /*
        public void memorizeNotificatedItem(UnreadItem item)
        {
            // notifiedItems.Add(item);
        }

        public IEnumerable<UnreadItem> FindItemsIoSnarlId(Int32 snarlId)
        {
           /* return from item in notifiedItems
                   where item.SnarlNotificationId == snarlId
                   select item;
            * 
        }

    */

        protected override void WndProc(ref Message m)
        {

            if (m.Msg == this.SNARL_GLOBAL_MESSAGE)
            {
                if ((int)m.WParam == Snarl.SnarlConnector.SNARL_LAUNCHED)
                {
                    // Snarl has been (re)started 
                    SnarlConnector.GetSnarlWindow(true);
                    SnarlConnector.RegisterConfig(this.Handle, "Winslew", Snarl.WindowsMessage.WM_USER + 55, pathToIcon);
                    SnarlConnector.RegisterAlert("Winslew", "New item");
                    SnarlConnector.RegisterAlert("Winslew", "Item marked as read");
                    SnarlConnector.RegisterAlert("Winslew", "Item marked as unread");
                    SnarlConnector.RegisterAlert("Winslew", "Item tags changed");
                    SnarlConnector.RegisterAlert("Winslew", "Item deleted");
                }
            }
            else if (m.Msg == (int)Snarl.WindowsMessage.WM_USER + 45)
            {
                // single news item
                if ((int)m.WParam == Snarl.SnarlConnector.SNARL_NOTIFICATION_ACK)
                {

                }
                else if ((int)m.WParam == Snarl.SnarlConnector.SNARL_NOTIFICATION_CLICKED)
                {

                }
                else if (m.Msg == (int)Snarl.WindowsMessage.WM_USER + 46)
                {
                    // number of new items
                    if ((int)m.WParam == Snarl.SnarlConnector.SNARL_NOTIFICATION_ACK || (int)m.WParam == Snarl.SnarlConnector.SNARL_NOTIFICATION_CLICKED)
                    {

                    }
                }
                else if (
        (int)m.WParam == Snarl.SnarlConnector.SNARL_NOTIFICATION_TIMED_OUT ||
        (int)m.WParam == Snarl.SnarlConnector.SNARL_NOTIFICATION_CANCELLED
                    /*||
        (int)m.WParam == Snarl.SnarlConnector.SNARL_NOTIFICATION_CLOSED ||
        (int)m.WParam == Snarl.SnarlConnector.SNARL_NOTIFICATION_MIDDLE_MOUSE */
        )
                {

                }
            }
            base.WndProc(ref m);

        }


    }
}


