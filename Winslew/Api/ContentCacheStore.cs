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

        Dictionary<string, ImgurData> LocallyAddedContent;
        

        public ContentCacheStore()
        {
            Cache = new List<CachedItemContent>();
            pathToNAPage = appDataPath + "\\Cache\\NotAvailable.html";
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            if (!Directory.Exists(appDataPath + "\\Cache"))
            {
                Directory.CreateDirectory(appDataPath + "\\Cache");
            }

            LocallyAddedContent = new Dictionary<string, ImgurData>();

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

            if(!File.Exists(appDataPath + "\\Cache\\actualStylesheet.css")) {
                string defaultCss = "body {\n  background-color:white;\n  color:black;}\n\n#WinslewTitle {\n  display:none;\n}";
                StreamWriter fhCss = File.CreateText(appDataPath + "\\Cache\\actualStylesheet.css");
                fhCss.Write(defaultCss);
                fhCss.Close();
            }

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
                    if (LocallyAddedContent.ContainsKey(itemToBeCached.url))
                    {
                        returnValue.FullVersion = cacheDir + "\\full\\index.html";
                        if (!Directory.Exists(cacheDir + "\\full"))
                        {
                            Directory.CreateDirectory(cacheDir + "\\full");
                        }
                        returnValue.FullUpdated = DateTime.Now;

                        string content = CreateCacheVersionOfLocalImage(cacheDir, LocallyAddedContent[itemToBeCached.url]);

                        StreamWriter fh = File.CreateText(returnValue.FullVersion);
                        fh.Write(content);
                        fh.Close();
                    }
                    else
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
            }

            string fullText = "";
            if (returnValue.LessVersion == null || overrideExisting)
            {
                if(LocallyAddedContent.ContainsKey(itemToBeCached.url)) {
                        returnValue.LessVersion = cacheDir + "\\" + returnValue.Id.ToString() + "-less.html";
                        returnValue.LessUpdated = DateTime.Now;

                        string content = CreateCacheVersionOfLocalImage(cacheDir,LocallyAddedContent[itemToBeCached.url]);

                        StreamWriter fh = File.CreateText(returnValue.LessVersion);
                        fh.Write(content);
                        fh.Close();
                }
                else
                {
                    fullText = generateLessContent(itemToBeCached.url);
                    if (!string.IsNullOrEmpty(fullText))
                    {
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
            }

            if (returnValue.MoreVersion == null || overrideExisting)
            {
                if(LocallyAddedContent.ContainsKey(itemToBeCached.url)) {
                    returnValue.MoreVersion = cacheDir + "\\" + returnValue.Id.ToString() + "-more.html";
                    returnValue.LessUpdated = DateTime.Now;

                    string content = CreateCacheVersionOfLocalImage(cacheDir,LocallyAddedContent[itemToBeCached.url]);

                    StreamWriter fh = File.CreateText(returnValue.MoreVersion);
                    fh.Write(content);
                    fh.Close();
                }
                else
                {
                    fullText = generateMoreContent(itemToBeCached.url);
                    if(!string.IsNullOrEmpty(fullText)) {

                        returnValue.MoreVersion = cacheDir + "\\" + returnValue.Id.ToString() + "-more.html";
                        returnValue.MoreUpdated = DateTime.Now;

                        StreamWriter fhMore = File.CreateText(returnValue.MoreVersion);
                        fhMore.Write(fullText);
                        fhMore.Close();

                    }
                    else
                    {
                        returnValue.MoreVersion = null;
                    }
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

        public static string generateLessContent(string Url)
        {
            string fullText = "";
            if (string.IsNullOrEmpty(Url))
            {
                return "No valid url specified";
            }
            Response cachedLessContent = AppController.Current.getCacheText(Url, "less");
            if (cachedLessContent.Content != null && cachedLessContent.Content != "")
            {
                fullText = "<html>\n<head>\n<title>" + cachedLessContent.TextTitle + "</title>\n";
                fullText += "<meta http-equiv=\"Content-Type\" content=\"" + cachedLessContent.TextContentType + "\">\n";
                fullText += "<link rel=\"stylesheet\" type=\"text/css\" href=\"../actualStylesheet.css\" />\n</head>\n";
                fullText += "<body>\n";
                fullText += "<div id=\"WinslewTitle\"><h1>" + cachedLessContent.TextTitle + "</h1></div>\n";
                fullText += "<div id=\"WinslewBody\">" + cachedLessContent.Content + "</div>";
                fullText += "\n</body>\n</html>";
            }
            return fullText;
        }

        public static string generateMoreContent(string Url)
        {
            string fullText = "";
            if (string.IsNullOrEmpty(Url))
            {
                return "No valid url specified";
            }
            Response cachedMoreContent = AppController.Current.getCacheText(Url, "more");
            if (cachedMoreContent.Content != null && cachedMoreContent.Content != "")
            {
                fullText = "<html>\n<head>\n<title>" + cachedMoreContent.TextTitle + "</title>\n";
                fullText += "<meta http-equiv=\"Content-Type\" content=\"" + cachedMoreContent.TextContentType + "\">\n";
                fullText += "<link rel=\"stylesheet\" type=\"text/css\" href=\"../actualStylesheet.css\" />\n</head>\n";
                fullText += "<body>\n";
                fullText += "<div id=\"WinslewTitle\"><h1>" + cachedMoreContent.TextTitle + "</h1></div>\n";
                fullText += "<div id=\"WinslewBody\">" + cachedMoreContent.Content + "</div>";
                fullText += "\n</body>\n</html>";
            }
            return fullText;
        }

        public string CreateCacheVersionOfLocalImage(string cacheDir, ImgurData imgurData)
        {
            string localFilePath = imgurData.originalLocalImagePath;
            string returnValue = "";
            if (File.Exists(localFilePath) || File.Exists(Path.Combine(cacheDir, Path.GetFileName(localFilePath))))
            {
                if (!File.Exists(Path.Combine(cacheDir, Path.GetFileName(localFilePath))))
                {
                    File.Copy(localFilePath, Path.Combine(cacheDir, Path.GetFileName(localFilePath)));
                }
                returnValue = "<html><head><title>" + Path.GetFileNameWithoutExtension(localFilePath) + "</title>\n";
                returnValue += "<meta http-equiv=\"Content-Type\" content=\"utf-8\">\n";
                returnValue += "<link rel=\"stylesheet\" type=\"text/css\" href=\"../actualStylesheet.css\" />\n</head>\n";
                returnValue += "<div id=\"WinslewTitle\"><h1>" + Path.GetFileNameWithoutExtension(localFilePath) + "</h1></div>\n";
                returnValue += "<div id=\"WinslewBody\"><img src=\"" + Path.Combine(cacheDir, Path.GetFileName(localFilePath)) + "\"</img></div>";
                returnValue += "<div id=\"WinslewDeleteImgurImage\"><a href=\"" + imgurData.deletePage + "\">Delete this image from Imgur</a> (will not remove it from Winslew cache or Pocket)</div>\n";
                returnValue += "\n</body>\n</html>";
            }
            return returnValue;
        }

        public void MemorizeImgurUpload(ImgurData imgData) {
            LocallyAddedContent.Add(imgData.originalImage, imgData);
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
