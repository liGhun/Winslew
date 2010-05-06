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

            string notAvailableText = "<html><head><title>View not available...</title>";
            notAvailableText += "</head><body>\n";
            notAvailableText += "<strong>Sorry, this view is currently not available for this item...</strong>";
            notAvailableText += "<p>Please choose another view for this item</p>";
            notAvailableText += "<p>Check if cache update is currently running or update the cache manually</p>";
            notAvailableText += "</body></html>";
            StreamWriter fhUpd = File.CreateText(appDataPath + "\\Cache\\NotAvailable.html");
            fhUpd.Write(notAvailableText);
            fhUpd.Close();

            CachedItemContent storedCacheItem = new CachedItemContent();


            // Initial load of already stored caches into general cache store
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
                        storedCacheItem.LessUpdated = File.GetLastWriteTime(appDataPath + "\\Cache\\" + id.ToString() + "\\" + id.ToString() + "-less.html");
                    }
                    else
                    {
                        storedCacheItem.LessVersion = null;
                    }

                    if (File.Exists(appDataPath + "\\Cache\\" + id.ToString() + "\\" + id.ToString() + "-more.html"))
                    {
                        storedCacheItem.MoreVersion = appDataPath + "\\Cache\\" + id.ToString() + "\\" + id.ToString() + "-more.html";
                        storedCacheItem.MoreUpdated = File.GetLastWriteTime(appDataPath + "\\Cache\\" + id.ToString() + "\\" + id.ToString() + "-more.html");
                    }
                    else
                    {
                        storedCacheItem.MoreVersion = null;
                    }

                    if (File.Exists(appDataPath + "\\Cache\\" + id.ToString() + "\\full\\index.html"))
                    {
                        storedCacheItem.FullVersion = appDataPath + "\\Cache\\" + id.ToString() + "\\full\\index.html";
                        storedCacheItem.FullUpdated = File.GetLastWriteTime(appDataPath + "\\Cache\\" + id.ToString() + "\\full\\index.html");
                    }
                    else
                    {
                        storedCacheItem.FullVersion = null;
                    }



                    if (File.Exists(appDataPath + "\\Cache\\" + id.ToString() + "\\favicon.ico"))
                    {
                        storedCacheItem.FavIconPath = appDataPath + "\\Cache\\" + id.ToString() + "\\favicon.ico";
                    }
                    Cache.Add(storedCacheItem);
                }
            }

        }

        public CachedItemContent LoadStoredCache(Item item) {
            CachedItemContent returnValue = new CachedItemContent();

            if (Cache != null)
            {
                IEnumerable<CachedItemContent> alreadyInCache = Cache.Where((CachedItemContent temp) => temp.Id == item.id);
                if (alreadyInCache.Count() > 0)
                {
                    return alreadyInCache.FirstOrDefault();
                }
            }
            CachedItemContent emptyOne = new CachedItemContent();
            return emptyOne;
        }

        public CachedItemContent addToCache(Item itemToBeCached, bool overrideExisting, bool readOnlyFromHdd, bool doNotFetchFullVersion)
        {
            CachedItemContent returnValue = new CachedItemContent();

            if (Cache != null)
            {
                IEnumerable<CachedItemContent> alreadyInCache = Cache.Where((CachedItemContent temp) => temp.Id == itemToBeCached.id);
                if (!overrideExisting)
                {
                    if (alreadyInCache.Count() > 0)
                    {
                        returnValue = alreadyInCache.FirstOrDefault();
                        if (returnValue.IsComplete)
                        {
                            return returnValue;
                        }
                    }
                }
            }

            if (readOnlyFromHdd)
            {
                return returnValue;
            }

            returnValue.Id = itemToBeCached.id;
            string cacheDir = appDataPath + "\\Cache\\" + returnValue.Id.ToString();
            if (!Directory.Exists(cacheDir))
            {
                Directory.CreateDirectory(cacheDir);
            }

            if (!doNotFetchFullVersion)
            {
                if (returnValue.FullVersion == null || overrideExisting)
                {
                    FetchWebpage myFetcher = new FetchWebpage();
                    if (myFetcher.FullFetch(itemToBeCached.url, cacheDir + "\\full\\"))
                    {
                        returnValue.FullVersion = cacheDir + "\\full\\index.html";
                        returnValue.FullUpdated = DateTime.Now;
                    }
                    else
                    {
                        returnValue.FullVersion = null;
                    }
                }
            }

            string fullText = "";
            if (returnValue.LessVersion == null || overrideExisting)
            {
                Response cachedLessContent = AppController.Current.getCacheText(itemToBeCached.url, "less");
                if (cachedLessContent.Content != null && cachedLessContent.Content != "")
                {
                    fullText = "<html><head><title>" + cachedLessContent.TextTitle + "</title>";
                    fullText += "<meta http-equiv=\"Content-Type\" content=\"" + cachedLessContent.TextContentType + "\"></head><body>\n";
                    fullText += cachedLessContent.Content;
                    fullText += "\n</body></html>";

                    returnValue.LessVersion = cacheDir + "\\" + returnValue.Id.ToString() + "-less.html";
                    returnValue.LessUpdated = DateTime.Now;

                    StreamWriter fh = File.CreateText(returnValue.LessVersion);
                    fh.Write(fullText);
                    fh.Close();
                }
                else
                {
                    returnValue.LessVersion = null;
                }
            }

            if (returnValue.MoreVersion == null || overrideExisting)
            {
                Response cachedMoreContent = AppController.Current.getCacheText(itemToBeCached.url, "more");
                if (cachedMoreContent.Content != "" && cachedMoreContent.Content != null)
                {
                    fullText = "<html><head><title>" + cachedMoreContent.TextTitle + "</title>";
                    fullText += "<meta http-equiv=\"Content-Type\" content=\"" + cachedMoreContent.TextContentType + "\"></head><body>\n";
                    fullText += cachedMoreContent.Content;
                    fullText += "\n</body></html>";

                    returnValue.MoreVersion = cacheDir + "\\" + returnValue.Id.ToString() + "-more.html";
                    returnValue.MoreUpdated = DateTime.Now;

                    StreamWriter fhMore = File.CreateText(returnValue.MoreVersion);
                    fhMore.Write(fullText);
                    fhMore.Close();

                }
                else
                {
                    returnValue.MoreVersion = pathToNAPage;
                }
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
