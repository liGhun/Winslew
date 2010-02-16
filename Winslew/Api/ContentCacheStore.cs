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
        public string pathToUpdatingSite = "";


        public ContentCacheStore()
        {
            Cache = new List<CachedItemContent>();
            pathToUpdatingSite = appDataPath + "\\Cache\\UpdatingCache.html";
            if (!Directory.Exists(appDataPath + "\\Cache"))
            {
                Directory.CreateDirectory(appDataPath + "\\Cache");
            }
            if(!File.Exists(appDataPath + "\\Cache\\UpdatingCache.html")) {
                string updatingText = "<html><head><title>Updating cache...</title>";
                updatingText += "</head><body>\n";
                updatingText += "<h1>Updating content cache...</h1>";
                updatingText += "</body></html>";
                StreamWriter fhUpd = File.CreateText(appDataPath + "\\Cache\\UpdatingCache.html");
                fhUpd.Write(updatingText);           
                fhUpd.Close();
            }

            CachedItemContent storedCacheItem = new CachedItemContent();
            foreach(string dir in Directory.GetDirectories(appDataPath + "\\Cache")) {
                if (Directory.GetFiles(dir).Count() > 0)
                {
                    string id = "";
                    id = dir.Substring(dir.LastIndexOf("\\")+1);
                    storedCacheItem = new CachedItemContent();
                    storedCacheItem.Id = id;
                    storedCacheItem.LessVersion = appDataPath + "\\Cache\\" + id.ToString() + "\\" + id.ToString() + "-less.html";
                    storedCacheItem.MoreVersion = appDataPath + "\\Cache\\" + id.ToString() + "-more.html";
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

            Response cachedLessContent = AppController.Current.getCacheText(itemToBeCached.url, "less");
            string fullText = "<html><head><title>" + cachedLessContent.TextTitle + "</title>";
            fullText += "<meta http-equiv=\"Content-Type\" content=\"" + cachedLessContent.TextContentType + "\"></head><body>\n";
            fullText += cachedLessContent.Content;
            fullText += "\n</body></html>";

            returnValue.LessVersion = cacheDir + "\\" + returnValue.Id.ToString() + "-less.html";
            StreamWriter fh = File.CreateText(returnValue.LessVersion);
            fh.Write(fullText);           
            fh.Close();

            Response cachedMoreContent = AppController.Current.getCacheText(itemToBeCached.url, "more");
            fullText = "<html><head><title>" + cachedLessContent.TextTitle + "</title>";
            fullText += "<meta http-equiv=\"Content-Type\" content=\"" + cachedLessContent.TextContentType + "\"></head><body>\n";
            fullText += cachedLessContent.Content;
            fullText += "\n</body></html>";

            returnValue.MoreVersion = cacheDir + "\\" + returnValue.Id.ToString() + "-more.html";
            StreamWriter fhMore = File.CreateText(returnValue.MoreVersion);
            fhMore.Write(fullText);
            fhMore.Close();

            return returnValue;
        }
    }
}
