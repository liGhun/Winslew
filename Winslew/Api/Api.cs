using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.IO;
using System.Net;
using System.Reflection;
using System.Drawing;

namespace Winslew.Api
{
    public class Api
    {
         private string apiKey = "362pfT45AcqNFe9553gZW53G52dkO8fY";
         private bool isOnline;
         private long lastRetrievalTime = 0;

         public void addToList(string newUrl, string newTitle)
        {
            if (newUrl != null)
            {
                Response result = HttpCommunications.SendPostRequest(@"https://readitlaterlist.com/v2/add", new
                {
                    username = Properties.Settings.Default.Username,
                    password = getPassword(),
                    apikey = apiKey,
                    url = newUrl,
                    title = newTitle

                }, false);
               // bool success = (!string.IsNullOrEmpty(result) && result.ToLowerInvariant() == "200 ok");
                if (!result.Success)
                {
                    System.Windows.Forms.MessageBox.Show("Error sending item to Read It Later", "Read it later error");
                }

            }
        }

        public bool checkLoginData(string RIL_username, string RIL_password)
        {
            if (RIL_username != "" && RIL_password != "")
            {


                Response result = HttpCommunications.SendPostRequest(@"https://readitlaterlist.com/v2/auth", new
                {
                    username = RIL_username,
                    password = RIL_password,
                    apikey = apiKey,

                }, false);
                // bool success = (!string.IsNullOrEmpty(result) && result.ToLowerInvariant() == "200 ok");
                if (!result.Success)
                {
                    System.Windows.Forms.MessageBox.Show(result.Error, "Login to Read It Later failed");
                    return false;
                }
                return true;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Missing username or password", "Login to Read It Later failed");
                return false;
            }
        }

         public bool createAccount(string RIL_username, string RIL_password)
        {
            if (RIL_username != "" && RIL_password != "")
            {

                Response result = HttpCommunications.SendPostRequest(@"https://readitlaterlist.com/v2/signup", new
                {
                    username = RIL_username,
                    password = RIL_password,
                    apikey = apiKey,

                }, false);
                // bool success = (!string.IsNullOrEmpty(result) && result.ToLowerInvariant() == "200 ok");
                if (!result.Success)
                {
                    // System.Windows.Forms.MessageBox.Show("Unable to create account with the supplied credentials. Pleaste try another username", "Account creation failed");
                    System.Windows.Forms.MessageBox.Show(result.Error, "Account creation failed");
                }
                return result.Success;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Please provide both a username and a password", "Account creation failed");
                return false;
            }
        }

         public bool sendNew(Dictionary<string, string> data)
        {
            if (Properties.Settings.Default.Username != "" && Properties.Settings.Default.Password != "")
            {

                string request = createJson(data);
                Response result = HttpCommunications.SendPostRequest(@"https://readitlaterlist.com/v2/send", new
                {
                    username = Properties.Settings.Default.Username,
                    password = getPassword(),
                    apikey = apiKey,
                    newone = request

                }, false);
                // bool success = (!string.IsNullOrEmpty(result) && result.ToLowerInvariant() == "200 ok");
                if (!result.Success)
                {
                    System.Windows.Forms.MessageBox.Show(result.Error, "Login to Read It Later failed");
                }
                return result.Success;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Missing username or password", "Setup your credentials in the Preferences first");
                return false;
            }
        }

        public bool markRead(Dictionary<string, string> data)
        {
            if (Properties.Settings.Default.Username != "" && Properties.Settings.Default.Password != "")
            {
                
                string request = createJson(data);
                Response result;

                result = HttpCommunications.SendPostRequest(@"https://readitlaterlist.com/v2/send", new
                {
                    username = Properties.Settings.Default.Username,
                    password = getPassword(),
                    apikey = apiKey,
                    read = request

                }, false);

                // bool success = (!string.IsNullOrEmpty(result) && result.ToLowerInvariant() == "200 ok");
                if (!result.Success)
                {
                    System.Windows.Forms.MessageBox.Show(result.Error, "Login to Read It Later failed");
                }
                return result.Success;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Missing username or password", "Setup your credentials in the Preferences first");
                return false;
            }
        }

        public bool changeTags(Dictionary<string, string> data)
        {
            if (Properties.Settings.Default.Username != "" && Properties.Settings.Default.Password != "")
            {

                string request = createJson(data);
                Response result = HttpCommunications.SendPostRequest(@"https://readitlaterlist.com/v2/send", new
                {
                    username = Properties.Settings.Default.Username,
                    password = getPassword(),
                    apikey = apiKey,
                    update_tags = request

                }, false);
                // bool success = (!string.IsNullOrEmpty(result) && result.ToLowerInvariant() == "200 ok");
                if (!result.Success)
                {
                    System.Windows.Forms.MessageBox.Show(result.Error, "Login to Read It Later failed");
                }
                return result.Success;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Missing username or password", "Setup your credentials in the Preferences first");
                return false;
            }
        }


        public bool delete(Dictionary<string, string> data)
        {
            if (Properties.Settings.Default.Username != "" && Properties.Settings.Default.Password != "")
            {

                string request = createJson(data);
                Response result = HttpCommunications.SendPostRequest(@"https://readitlaterlist.com/v2/send", new
                {
                    username = Properties.Settings.Default.Username,
                    password = getPassword(),
                    apikey = apiKey,
                    delete = request

                }, false);
                // bool success = (!string.IsNullOrEmpty(result) && result.ToLowerInvariant() == "200 ok");
                if (!result.Success)
                {
                    System.Windows.Forms.MessageBox.Show(result.Error, "Login to Read It Later failed");
                }
                return result.Success;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Missing username or password", "Setup your credentials in the Preferences first");
                return false;
            }
        }

        public bool checkIfOnline()
        {
            if (isOnline)
            {
                return true;
            }
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("http://www.li-ghun.de/");
            request.Timeout = 2000;
            try
            {
                request.GetResponse();
                isOnline = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Response getCacheText(string urlToBeCached, string type)
        {
            string modeToBeUsed = "more";
            if(type.ToLower() == "less") {
                modeToBeUsed = "less";
            }
            Response result = HttpCommunications.SendPostRequest(@"https://text.readitlaterlist.com/v2/text", new
            {
                apikey = apiKey,
                url = urlToBeCached,
                mode = modeToBeUsed
            }, false);

            return result;
        }


        private  string createJson(Dictionary<string, string> data) {
            Dictionary<string, object> container = new Dictionary<string, object>();
            container.Add("0", data);
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            return serializer.Serialize(container);
        }

         public IEnumerable<Item> getList(bool freshRefresh, bool includeRead, bool includeUnread)
        {
            if (freshRefresh)
            {
                lastRetrievalTime = 0;
            }
            if (Properties.Settings.Default.Username != "" && Properties.Settings.Default.Password != "")
            {
                Response result = HttpCommunications.SendPostRequest(@"https://readitlaterlist.com/v2/get", new
                {
                    username = Properties.Settings.Default.Username,
                    password = getPassword(),
                    apikey = apiKey,
                    since = lastRetrievalTime.ToString(),
                    tags = "1"

                }, false);
                // bool success = (!string.IsNullOrEmpty(result));
                if (!result.Success)
                {
                    System.Windows.Forms.MessageBox.Show(result.Error, "Login to Read It Later failed");
                }

                return DeserializeItemsJson(result.Content);

                
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Missing username or password", "Setup ypur credentials in the Preferences first");
                return new List<Item>();
            }
        }

        // private  IEnumerable<Item> DeserializeItemsJson(string data, Func<string, bool> checkAction)
        private  IEnumerable<Item> DeserializeItemsJson(string data)
        {
            List<Item> ListOfNewItems = new List<Item>();
            if (data == null)
            {
                return ListOfNewItems;
            }
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            serializer.MaxJsonLength = data.Length;
            var obj = serializer.DeserializeObject(data) as Dictionary<string, object>;
            if (obj != null)
            {
                string status = obj["status"].ToString();
                lastRetrievalTime = Convert.ToInt64(obj["since"].ToString());
                if (status == "2")
                {
                    // no updates since last refresh - nothing to do
                    return ListOfNewItems;
                }
                var itemsCollection = obj["list"] as Dictionary<string, object>;
                if(itemsCollection != null) {
                    foreach (var i in itemsCollection as Dictionary<string, object>)
                    {
                        Console.WriteLine(i.Key + " " + i.Value);
                        var content = i.Value as Dictionary<string, object>;
                        Console.WriteLine(i.Key + " " + i.Value);
                        // var item = i as Dictionary<string, object>;
                        if (content != null)
                        {
                            string itemUrl = "";
                            string itemTitle = "";
                            Int64 itemTimeUpdated = 0;
                            string itemTimeUpdatedHumanReadable = "";
                            Int64 itemTimeAdded = 0;
                            string itemTimeAddedHumanReadbale = "";
                            string itemTags = "";
                            bool itemState = false;
                            string itemId = "";

                            if (content.ContainsKey("url"))
                            {
                                itemUrl = content["url"].ToString();
                            }
                            if (content.ContainsKey("title"))
                            {
                                itemTitle = content["title"].ToString().Trim();
                            }
                            if (content.ContainsKey("item_id"))
                            {
                                itemId = content["item_id"].ToString();
                            }
                            if (content.ContainsKey("tags"))
                            {
                                itemTags = content["tags"].ToString();
                            }
                            if (content.ContainsKey("time_updated"))
                            {
                                itemTimeUpdated = Convert.ToInt64(content["time_updated"].ToString());
                                DateTime myDateObject = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                                myDateObject = myDateObject.AddSeconds(itemTimeUpdated);
                                itemTimeUpdatedHumanReadable = string.Format("{0} {1}", myDateObject.ToShortDateString(), myDateObject.ToShortTimeString());
                            }
                            if (content.ContainsKey("time_added"))
                            {
                                itemTimeAdded = Convert.ToInt64(content["time_added"].ToString());
                                DateTime myDateObject = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
                                myDateObject = myDateObject.AddSeconds(itemTimeAdded);
                                itemTimeAddedHumanReadbale = string.Format("{0} {1}", myDateObject.ToShortDateString(), myDateObject.ToShortTimeString());
                            }
                            if (content.ContainsKey("state"))
                            {
                                if(content["state"].ToString() == "1") {
                                    itemState = true;
                                }
                            }

                            char[] separators = { ',' };

                            Item thisItem = new Item();
                            thisItem.id = itemId;
                            thisItem.read = itemState;
                            thisItem.tags = itemTags;
                            thisItem.timeAdded = itemTimeAdded;
                            thisItem.timeAddedHumanReadable = itemTimeAddedHumanReadbale;
                            thisItem.timeUpdated = itemTimeUpdated;
                            thisItem.timeUpdatedHumanReadable = itemTimeUpdatedHumanReadable;
                            thisItem.title = itemTitle;
                            thisItem.url = itemUrl;
                            ListOfNewItems.Add(thisItem);
                        }
                    }
                }
                return ListOfNewItems;
            }
            else
            {
                return null;
            }
            /*
 
                            yield return new Item()
                            {
                               id  = "111"
                            };

            }*/
        }

         private string getPassword()
        {
            return Crypto.ToInsecureString(Crypto.DecryptString(Properties.Settings.Default.Password));
        }

        private void updateGuiApiLimits(Response respone) {
            AppController.Current.updateApiStatusBar(respone.LimitUserRemanining, respone.LimitUserLimit, respone.LimitUserReset, respone.LimitKeyRemaining, respone.LimitKeyLimit, respone.LimitKeyReset);
        }
    }
}
