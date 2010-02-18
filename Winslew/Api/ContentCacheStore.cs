using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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
                string updatingText = "<html><head><title>Not available...</title>";
                updatingText += "</head><body>\n";
                updatingText += "<h1>Sorry, this view is not available for this item...</h1>";
                updatingText += "</body></html>";
                StreamWriter fhUpd = File.CreateText(appDataPath + "\\Cache\\NotAvailable.html");
                fhUpd.Write(updatingText);           
                fhUpd.Close();
            }

            CachedItemContent storedCacheItem = new CachedItemContent();
            foreach(string dir in Directory.GetDirectories(appDataPath + "\\Cache")) {
                if (Directory.GetFiles(dir).Count() > 0)
                {
                    string id = "";
                    id = dir.Substring(dir.LastIndexOf("\\") + 1);
                    storedCacheItem = new CachedItemContent();
                    storedCacheItem.Id = id;

                    if(File.Exists(appDataPath + "\\Cache\\" + id.ToString() + "\\" + id.ToString() + "-less.html")) {
                        storedCacheItem.LessVersion = appDataPath + "\\Cache\\" + id.ToString() + "\\" + id.ToString() + "-less.html";
                    }
                    else
                    {
                        storedCacheItem.LessVersion = pathToNAPage;
                    }

                    if(File.Exists(appDataPath + "\\Cache\\" + id.ToString() + "\\" + id.ToString() + "-more.html")) {
                        storedCacheItem.MoreVersion = appDataPath + "\\Cache\\" + id.ToString() + "\\" + id.ToString() + "-more.html";
                    }
                    else
                    {
                        storedCacheItem.MoreVersion = pathToNAPage;
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
            if(!Directory.Exists(cacheDir)) {
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

            return returnValue;
        }
    }
}
