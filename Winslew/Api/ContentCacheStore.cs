using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Net;

namespace Winslew.Api
{
    public class ContentCacheStore
    {

        public List<CachedItemContent> Cache { get; set; }
        private string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Winslew";
        public string pathToNAPage = "";


        public ContentCacheStore()
        {
            Cache = new List<CachedItemContent>();
            pathToNAPage = appDataPath + "\\Cache\\NotAvailable.html";
            if (!Directory.Exists(appDataPath + "\\Cache"))
            {
                Directory.CreateDirectory(appDataPath + "\\Cache");
            }
            if (!File.Exists(appDataPath + "\\Cache\\NotAvailable.html"))
            {
                string notAvailableText = "<html><head><title>Not available...</title>";
                notAvailableText += "</head><body>\n";
                notAvailableText += "<strong>Sorry, this view is not available for this item...</strong>";
                notAvailableText += "<p>Please choose another view for this item</p>";
                notAvailableText += "</body></html>";
                StreamWriter fhUpd = File.CreateText(appDataPath + "\\Cache\\NotAvailable.html");
                fhUpd.Write(notAvailableText);
                fhUpd.Close();
            }

            CachedItemContent storedCacheItem = new CachedItemContent();
            foreach (string dir in Directory.GetDirectories(appDataPath + "\\Cache"))
            {
                if (Directory.GetFiles(dir).Count() > 0)
                {
                    string id = "";
                    id = dir.Substring(dir.LastIndexOf("\\") + 1);
                    storedCacheItem = new CachedItemContent();
                    storedCacheItem.Id = id;

                    if (File.Exists(appDataPath + "\\Cache\\" + id.ToString() + "\\" + id.ToString() + "-less.html"))
                    {
                        storedCacheItem.LessVersion = appDataPath + "\\Cache\\" + id.ToString() + "\\" + id.ToString() + "-less.html";
                        storedCacheItem.Updated = File.GetLastWriteTime(appDataPath + "\\Cache\\" + id.ToString() + "\\" + id.ToString() + "-less.html");
                    }
                    else
                    {
                        storedCacheItem.LessVersion = pathToNAPage;
                    }

                    if (File.Exists(appDataPath + "\\Cache\\" + id.ToString() + "\\" + id.ToString() + "-more.html"))
                    {
                        storedCacheItem.MoreVersion = appDataPath + "\\Cache\\" + id.ToString() + "\\" + id.ToString() + "-more.html";
                        storedCacheItem.Updated = File.GetLastWriteTime(appDataPath + "\\Cache\\" + id.ToString() + "\\" + id.ToString() + "-more.html");
                    }
                    else
                    {
                        storedCacheItem.MoreVersion = pathToNAPage;
                    }

                    if (File.Exists(appDataPath + "\\Cache\\" + id.ToString() + "\\favicon.ico"))
                    {
                        storedCacheItem.FavIconPath = appDataPath + "\\Cache\\" + id.ToString() + "\\favicon.ico";
                    }

                    if (storedCacheItem.Updated != null)
                    {
                        storedCacheItem.UpdatedHumanReadable = string.Format("{0} {1}", storedCacheItem.Updated.ToShortDateString(), storedCacheItem.Updated.ToShortTimeString());
                    }

                    Cache.Add(storedCacheItem);
                }
            }

        }

        public CachedItemContent addToCache(Item itemToBeCached, bool overrideExisting)
        {
            CachedItemContent returnValue = new CachedItemContent();

            if (Cache != null)
            {
                IEnumerable<CachedItemContent> alreadyInCache = Cache.Where((CachedItemContent temp) => temp.Id == itemToBeCached.id);
                if (!overrideExisting)
                {
                    if (alreadyInCache.Count() > 0)
                    {
                        return alreadyInCache.FirstOrDefault();
                    }
                }
            }

            returnValue.Id = itemToBeCached.id;
            string cacheDir = appDataPath + "\\Cache\\" + returnValue.Id.ToString();
            if (!Directory.Exists(cacheDir))
            {
                Directory.CreateDirectory(cacheDir);
            }

            string fullText = "";

            Response cachedLessContent = AppController.Current.getCacheText(itemToBeCached.url, "less");
            if (cachedLessContent.Content != null && cachedLessContent.Content != "")
            {
                fullText = "<html><head><title>" + cachedLessContent.TextTitle + "</title>";
                fullText += "<meta http-equiv=\"Content-Type\" content=\"" + cachedLessContent.TextContentType + "\"></head><body>\n";
                fullText += cachedLessContent.Content;
                fullText += "\n</body></html>";

                returnValue.LessVersion = cacheDir + "\\" + returnValue.Id.ToString() + "-less.html";
                StreamWriter fh = File.CreateText(returnValue.LessVersion);
                fh.Write(fullText);
                fh.Close();
            }
            else
            {
                returnValue.LessVersion = pathToNAPage;
            }

            Response cachedMoreContent = AppController.Current.getCacheText(itemToBeCached.url, "more");
            if (cachedMoreContent.Content != "" && cachedLessContent.Content != null)
            {
                returnValue.MoreVersion = cacheDir + "\\" + returnValue.Id.ToString() + "-more.html";

                fullText = "<html><head><title>" + cachedLessContent.TextTitle + "</title>";
                fullText += "<meta http-equiv=\"Content-Type\" content=\"" + cachedMoreContent.TextContentType + "\"></head><body>\n";
                fullText += cachedLessContent.Content;
                fullText += "\n</body></html>";

                StreamWriter fhMore = File.CreateText(returnValue.MoreVersion);
                fhMore.Write(fullText);
                fhMore.Close();
            }
            else
            {
                returnValue.MoreVersion = pathToNAPage;
            }

            Image favIcon = GetFavicon(itemToBeCached.url);
            if (favIcon != null)
            {
                try
                {
                    favIcon.Save(cacheDir + "\\favicon.ico");
                    returnValue.FavIconPath = cacheDir + "\\favicon.ico";
                }
                catch
                {
                }
            }

            returnValue.Updated = DateTime.Now;
            returnValue.UpdatedHumanReadable = string.Format("{0} {1}", returnValue.Updated.ToShortDateString(), returnValue.Updated.ToShortTimeString());

            return returnValue;
        }


        private Image GetFavicon(string Inurl)
        {
            Uri url = new Uri(Inurl);
            string urlHost = url.Host;
            Image BookmarkIcon = null;
            if (url.HostNameType == UriHostNameType.Dns)
            {
                string iconUrl = "http://" + urlHost + "/favicon.ico";
                try
                {
                    HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(iconUrl);
                    System.Net.HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    System.IO.Stream stream = response.GetResponseStream();
                    BookmarkIcon = Image.FromStream(stream);
                }
                catch
                {

                }
                return BookmarkIcon;
            }
            else
            {
                return null;
            }
        }
    }
}
