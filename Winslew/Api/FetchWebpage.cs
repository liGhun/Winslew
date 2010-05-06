using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Winslew.Api
{
    class FetchWebpage
    {
        private string OpenUrl = "http://www.heise.de";


        public bool FullFetch(string url, string pathToSaveIn)
        {
            
            try
            {
                OpenUrl = url;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "FetchWebpage 1.0";

                string responseStream = "";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Encoding origEncoding;

                if (String.IsNullOrEmpty(response.CharacterSet))
                {
                    origEncoding = Encoding.Default;
                }
                else
                {
                    try
                    {
                        origEncoding = Encoding.GetEncoding(response.CharacterSet);
                    }
                    catch
                    {
                        origEncoding = Encoding.Default;
                    }
                }

                using (StreamReader reader = new StreamReader(response.GetResponseStream(), origEncoding))
                {
                    responseStream = reader.ReadToEnd();
                }

                List<string> allImages = FindImagesInHtml(responseStream);

                string ReplacedHtmlSource = ReplaceImagesWithTempPaths(responseStream, allImages, pathToSaveIn, "images", OpenUrl);

                List<string> allCssFiles = FindExternalCssFiles(ReplacedHtmlSource);

                ReplacedHtmlSource = SaveAndParseCss(ReplacedHtmlSource, allCssFiles, pathToSaveIn, "css", OpenUrl);
                ReplacedHtmlSource = FindLinksInHtmlAndReplaceThem(ReplacedHtmlSource);

                //StreamWriter fh = File.CreateText("C:\\tmp\\index.html");
                StreamWriter fh = new StreamWriter(pathToSaveIn + "\\index.html", false, origEncoding);
                fh.Write(ReplacedHtmlSource);
                fh.Close();
            }
            catch
            {
                return false;
            }

            return true;
        }

        #region Images in HTML

        private List<string> FindImagesInHtml(string htmlSource)
        {
            List<string> returnList = new List<string>();

            Regex PatternForImageTags = new Regex("<img .*src=\"?(?<url>[^ \"]*)", RegexOptions.IgnoreCase);

            MatchCollection AllImages = PatternForImageTags.Matches(htmlSource);
            foreach (Match SingleImageUrl in AllImages)
            {

                if (!returnList.Contains(SingleImageUrl.Groups["url"].Value.Trim()))
                {
                    returnList.Add(SingleImageUrl.Groups["url"].Value.Trim());
                }
            }
            return returnList;
        }

        private string ReplaceImagesWithTempPaths(string htmlSource, List<string> listOfImageUrls, string absolutePathToStorage, string relativePath, string hostUrl)
        {
            int ImageCounter = 1;


            if (!Directory.Exists(absolutePathToStorage + "\\" + relativePath))
            {
                // Console.Writeline("Creating directory for images");
                Directory.CreateDirectory(absolutePathToStorage + "\\" + relativePath);
            }


            foreach (string imageUrl in listOfImageUrls)
            {
                try
                {
                    string cleanedImageUrl = imageUrl;
                    // Console.Writeline("Original image url: " + imageUrl);

                    string suffix = "";
                    if (imageUrl.Contains("?"))
                    {
                        Console.Write(" - contains a ? at position ");
                        int positionOfQuestionMark = Math.Max(0, imageUrl.IndexOf("?"));
                        Console.Write(positionOfQuestionMark.ToString() + " of " + imageUrl.Length.ToString() + " characters\r\n");
                        cleanedImageUrl = imageUrl.Substring(0, positionOfQuestionMark);
                        // Console.Writeline("  -> new url: " + cleanedImageUrl);
                    }

                    int indexOfLastPoint = cleanedImageUrl.LastIndexOf(".");
                    if (indexOfLastPoint >= 0)
                    {
                        suffix = cleanedImageUrl.Substring(indexOfLastPoint);
                        // Console.Writeline("Extracted suffix: " + suffix);
                        string imageFullPath = imageUrl;
                        if (!imageFullPath.ToLower().StartsWith("http"))
                        {
                            imageFullPath = hostUrl + imageFullPath;
                        }
                        SaveImageToFile(imageFullPath, absolutePathToStorage + "\\" + relativePath + "\\" + ImageCounter.ToString() + suffix);
                        htmlSource = htmlSource.Replace(imageUrl, relativePath + "/" + ImageCounter.ToString() + suffix);
                        // Console.Writeline("------------");
                        ImageCounter++;
                    }
                    else
                    {
                        htmlSource = htmlSource.Replace(imageUrl, "myDefaultReplacement.png");
                        // Console.Writeline("!! Didn't get the suffix");
                    }
                }
                catch (Exception exp)
                {
                    Trace.WriteLine("Exeption: " + exp.Message);
                }
            }
            return htmlSource;
        }

        #endregion

        #region CSS

        private List<string> FindExternalCssFiles(string htmlSource)
        {
            List<string> returnList = new List<string>();

            Regex PatternForCssLinkTags = new Regex(@"<link [^>]*rel=""?stylesheet""?.*>", RegexOptions.IgnoreCase);
            Regex PatternForCssHref = new Regex(@"<link [^>]*href=""?(?<url>[^"" ]*)""? .*>", RegexOptions.IgnoreCase);

            MatchCollection AllCssLinkTags = PatternForCssLinkTags.Matches(htmlSource);
            foreach (Match linkLine in AllCssLinkTags)
            {
                MatchCollection CssHrefs = PatternForCssHref.Matches(linkLine.Value);
                foreach (Match SingleHref in CssHrefs)
                {
                    // Console.Writeline("Found css in " + SingleHref.Groups["url"].Value);
                    returnList.Add(SingleHref.Groups["url"].Value);
                }
            }
            return returnList;
        }

        private string SaveAndParseCss(string htmlSource, List<string> listOfCssFiles, string absolutePathToStorage, string relativePath, string hostUrl)
        {
            int CssCounter = 1;

            if (!Directory.Exists(absolutePathToStorage + "\\" + relativePath))
            {
                // Console.Writeline("Creating directory for css");
                Directory.CreateDirectory(absolutePathToStorage + "\\" + relativePath);
            }

            string returnValue = htmlSource;

            WebClient Client = new WebClient();

            foreach (string cssFile in listOfCssFiles)
            {
                if (!Directory.Exists(absolutePathToStorage + "\\" + relativePath + "\\" + CssCounter.ToString()))
                {
                    // Console.Writeline("Creating directory for css");
                    Directory.CreateDirectory(absolutePathToStorage + "\\" + relativePath + "\\" + CssCounter.ToString());
                }
                string fullPath = CreateFullQualifiedLink(OpenUrl, cssFile);

                try
                {
                    // Console.Writeline("Trying to download " + fullPath);
                    //Client.DownloadFile(fullPath, absolutePathToStorage + "\\" + relativePath + "\\" + CssCounter.ToString() + "\\cachedCss.css");
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(fullPath);
                    request.UserAgent = "FetchWebpage 1.0";

                    string cssSource = "";

                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        cssSource = reader.ReadToEnd();
                    }

                    cssSource = CssReplaceImagesWithTempPaths(cssSource, fullPath, absolutePathToStorage + "\\" + relativePath + "\\" + CssCounter.ToString() + "\\", OpenUrl);
                    StreamWriter fh = File.CreateText(absolutePathToStorage + "\\" + relativePath + "\\" + CssCounter.ToString() + "\\cachedCss.css");
                    fh.Write(cssSource);
                    fh.Close();
                    returnValue = returnValue.Replace(cssFile, relativePath + "/" + CssCounter.ToString() + "/cachedCss.css");
                    CssCounter++;
                }
                catch (Exception exp)
                {
                    
                    Trace.WriteLine(exp.Message);
                }
            }

            return returnValue;
        }


        private string CssReplaceImagesWithTempPaths(string cssSource, string cssSourceUrl, string absolutePathToStorage, string fullQualifiedHostUrl)
        {
            int ImageCounter = 1;

            List<string> returnList = new List<string>();

            Regex PatternForImageTags = new Regex(@"url\(['""]?(?<url>[^ '""\)]*)", RegexOptions.IgnoreCase);

            MatchCollection AllImages = PatternForImageTags.Matches(cssSource);
            foreach (Match SingleImageUrl in AllImages)
            {

                if (!returnList.Contains(SingleImageUrl.Groups["url"].Value.Trim()))
                {
                    returnList.Add(SingleImageUrl.Groups["url"].Value.Trim());
                }
            }


            if (!Directory.Exists(absolutePathToStorage + "\\images"))
            {
                // Console.Writeline("Creating directory for images");
                Directory.CreateDirectory(absolutePathToStorage + "\\images");
            }


            foreach (string imageUrl in returnList)
            {
                try
                {
                    string cleanedImageUrl = imageUrl;
                    // Console.Writeline("Original image url: " + imageUrl);

                    string suffix = "";
                    if (imageUrl.Contains("?"))
                    {
                        Console.Write(" - contains a ? at position ");
                        int positionOfQuestionMark = Math.Max(0, imageUrl.IndexOf("?"));
                        Console.Write(positionOfQuestionMark.ToString() + " of " + imageUrl.Length.ToString() + " characters\r\n");
                        cleanedImageUrl = imageUrl.Substring(0, positionOfQuestionMark);
                        // Console.Writeline("  -> new url: " + cleanedImageUrl);
                    }

                    int indexOfLastPoint = cleanedImageUrl.LastIndexOf(".");
                    if (indexOfLastPoint >= 0)
                    {
                        suffix = cleanedImageUrl.Substring(indexOfLastPoint);
                        // Console.Writeline("Extracted suffix: " + suffix);
                        string imageFullPath = CreateFullQualifiedLink(cssSourceUrl, imageUrl);
                        SaveImageToFile(imageFullPath, absolutePathToStorage + "\\images\\" + ImageCounter.ToString() + suffix);
                        cssSource = cssSource.Replace(imageUrl, "images/" + ImageCounter.ToString() + suffix);
                        // Console.Writeline("------------");
                        ImageCounter++;
                    }
                    else
                    {
                        cssSource = cssSource.Replace(imageUrl, "/images/myDefaultReplacement.png");
                        // Console.Writeline("!! Didn't get the suffix");
                    }
                }
                catch (Exception exp)
                {
                    Trace.WriteLine("Exeption: " + exp.Message);
                }
            }
            return cssSource;
        }




        #endregion

        private bool SaveImageToFile(string url, string fullPath)
        {
            // Console.Writeline("************");
            // Console.Writeline("Reading url " + url);
            // Console.Writeline("Saving it to " + fullPath);
            WebClient Client = new WebClient();
            try
            {
                Client.DownloadFile(url, fullPath);
            }
            catch
            {
                return false;
            }
            return true;
        }

        private string FindLinksInHtmlAndReplaceThem(string htmlSource)
        {

            Regex PatternForImageTags = new Regex(@"<a .*href=""?(?<url>[^ ""]*)", RegexOptions.IgnoreCase);

            MatchCollection AllImages = PatternForImageTags.Matches(htmlSource);
            foreach (Match SingleImageUrl in AllImages)
            {
                if (SingleImageUrl.Groups["url"].Value.Trim().Length == 0)
                {
                    continue;
                }
                string originalLink = SingleImageUrl.Groups["url"].Value.Trim();
                string newLink = CreateFullQualifiedLink(OpenUrl, SingleImageUrl.Groups["url"].Value.Trim());
                // Console.Writeline("******************");
                // Console.Writeline("Original link: " + originalLink);
                // Console.Writeline("New link:      " + newLink);
                //Console.ReadLine();
                htmlSource = htmlSource.Replace("href=\"" + originalLink, "href=\"" + newLink);
            }
            return htmlSource;
        }

        private string CreateFullQualifiedLink(string ReferingLink, string TargetLink)
        {
            if (TargetLink.ToLower().StartsWith("http"))
            {
                return TargetLink;
            }

            if (!TargetLink.StartsWith("/"))
            {
                if (ReferingLink.EndsWith("/"))
                {
                    return ReferingLink.TrimEnd('/') + TargetLink;
                }
                else
                {
                    if (ReferingLink.Count(f => f == '/') < 3)
                    {
                        return ReferingLink + "/" + TargetLink;
                    }
                    else
                    {
                        int positionOfLastSlash = ReferingLink.LastIndexOf('/');
                        if (positionOfLastSlash < 0)
                        {
                            return TargetLink;
                        }
                        else
                        {
                            return ReferingLink.Substring(0, positionOfLastSlash) + TargetLink;
                        }

                    }
                }
            }
            else
            {
                if (ReferingLink.Count(f => f == '/') < 3)
                {
                    return ReferingLink + "/" + TargetLink;
                }
                else if (ReferingLink.Count(f => f == '/') == 3)
                {
                    return ReferingLink + TargetLink;
                }
                else
                {

                    while (ReferingLink.Count(f => f == '/') > 3)
                    {
                        ReferingLink = ReferingLink.Substring(0, ReferingLink.LastIndexOf('/'));
                    }


                    return ReferingLink + TargetLink;

                }
            }
        }


    }
}
