using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Reflection;


namespace LicenseKeyGenerator
{
    class HttpCommunications
    {
        public static Api.Response SendPostRequest(string url, object data, bool allowAutoRedirect)
        {
            try
            {
                string formData = string.Empty;
                HttpCommunications.GetProperties(data).ToList().ForEach(x =>
                {
                    string key = x.Key;
                    if (x.Key == "newone") { 
                        // this is a workaround as new is a command in C#...
                        key = "new"; 
                    }
                    formData += string.Format("{0}={1}&", key, x.Value);
                });
                formData = formData.TrimEnd('&');

                url = ProcessUrl(url);

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Method = "POST";
                request.AllowAutoRedirect = allowAutoRedirect;
                request.Accept = "*/*";
                request.UserAgent = "Winslew license key generator (http://www.li-ghun.de/)";
                request.ContentType = "application/x-www-form-urlencoded";

                byte[] encodedData = new UTF8Encoding().GetBytes(formData);
                request.ContentLength = encodedData.Length;

                using (Stream newStream = request.GetRequestStream())
                {
                    newStream.Write(encodedData, 0, encodedData.Length);
                }

                Api.Response returnValue = GetResponse(request);
                return returnValue;
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
                Api.Response nullResponse = new LicenseKeyGenerator.Api.Response();
                nullResponse.Success = false;
                nullResponse.Error = e.Message;
                return nullResponse;
            }
        }

        #region Private

        private static string ProcessUrl(string url)
        {
            string quesytionMarkSymbol = "?";
            if (url.Contains(quesytionMarkSymbol))
            {
                url = url.Replace(quesytionMarkSymbol, System.Web.HttpUtility.UrlEncode(quesytionMarkSymbol));
            }
            return url;
        }

        private static Api.Response GetResponse(HttpWebRequest request)
        {

            HttpWebResponse response;
            try
            {
                HttpWebResponse responseTemp = (HttpWebResponse)request.GetResponse();
                response = responseTemp;
            }
            catch (System.Exception e)
            {
                // some proxys have problems with Continue-100 headers
                request.ProtocolVersion = HttpVersion.Version10;
                request.ServicePoint.Expect100Continue = false;
                System.Net.ServicePointManager.Expect100Continue = false;
                HttpWebResponse responseTemp = (HttpWebResponse)request.GetResponse();
                response = responseTemp;
                System.Console.WriteLine(e.Message);
            }

            Api.Response returnValue = new LicenseKeyGenerator.Api.Response();

            returnValue.FullHeaders = response.Headers;
            returnValue = parseHeaders(returnValue);

            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                returnValue.Content = reader.ReadToEnd();
                return returnValue;
            }
        }

        private static Api.Response parseHeaders(Api.Response response)
        {
            for (int i = 0; i < response.FullHeaders.Count; i++)
            {
                KeyValuePair<string, string> header = new KeyValuePair<string, string>(response.FullHeaders.GetKey(i), response.FullHeaders.Get(i));
                switch (header.Key)
                {
                    case "Status":
                        response.Status = header.Value;
                        if(header.Value.StartsWith("200")) {
                            response.Success = true;
                        }
                        else
                        {
                            response.Success = false;
                        }
                        break;

                    case "X-Limit-User-Limit":
                        response.LimitUserLimit = Convert.ToInt32(header.Value);
                        break;

                    case "X-Limit-User-Remaining":
                        response.LimitUserRemanining = Convert.ToInt32(header.Value);
                        break;

                    case "X-Limit-User-Reset":
                        response.LimitUserReset = Convert.ToInt32(header.Value);
                        break;

                    case "X-Limit-Key-Limit":
                        response.LimitKeyLimit = Convert.ToInt32(header.Value);
                        break;

                    case "X-Limit-Key-Remaining":
                        response.LimitKeyRemaining = Convert.ToInt32(header.Value);
                        break;

                    case "X-Limit-Key-Reset":
                        response.LimitKeyReset = Convert.ToInt32(header.Value);
                        break;

                    case "X-Error":
                        response.Error = header.Value;
                        break;

                    case "X-Title":
                        response.TextTitle = header.Value;
                        break;

                    case "X-Login-Found":
                        response.TextLoginFound = true;
                        break;

                    case "X-Next":
                        response.TextNext = header.Value;
                        break;

                    case "Content-Type":
                        response.TextContentType = header.Value;
                        break;

                    default:
                        // any other response we are not interested...
                        break;
                }
            }
            return response;
        }

        private static IEnumerable<KeyValuePair<string, string>> GetProperties(object o)
        {
            foreach (var p in o.GetType().GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                yield return new KeyValuePair<string, string>(p.Name.TrimStart('_'), System.Web.HttpUtility.UrlEncode(p.GetValue(o, null).ToString()));
            }
        }

        #endregion
    }
}
