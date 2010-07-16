using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Winslew.Api
{
    public class Imgur
    {
        public static ImgurData uploadImage(string path)
        {
            Response result = HttpCommunications.SendPostRequest(@"http://imgur.com/api/upload", new
                {
                    key = "b4bbe31bdb063506ebb97a18e516a9cc",
                    image = EncodeAsBase64String(path)
                }, false);

            XmlDocument xmlDoc = new XmlDocument();

            if (!result.Success)
            {
                if (!string.IsNullOrEmpty(result.Content))
                {
                    try
                    {
                        xmlDoc.LoadXml(result.Content);
                        XmlNode imagePage = xmlDoc.SelectSingleNode("/rsp/error_msg");
                        string errorText = imagePage.InnerText;
                        System.Windows.Forms.MessageBox.Show(errorText, "Sending to Imgur failed");
                    }
                    catch
                    {
                        System.Windows.Forms.MessageBox.Show("Unknown error while sending image to Imgur image hosting service.", "Sending to Imgur failed");
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Unknown error while sending image to Imgur image hosting service.", "Sending to Imgur failed");
                }
                return null;
            }
            else
            {
                xmlDoc.LoadXml(result.Content);
                ImgurData responseData = new ImgurData(xmlDoc);
                
                return responseData;
            }
        }


        public static string EncodeAsBase64String(string inputFileName)
        {
            System.IO.FileStream inFile;
            byte[] binaryData;

            try
            {
                inFile = new System.IO.FileStream(inputFileName,
                                              System.IO.FileMode.Open,
                                              System.IO.FileAccess.Read);
                binaryData = new Byte[inFile.Length];
                long bytesRead = inFile.Read(binaryData, 0,
                                        (int)inFile.Length);
                inFile.Close();
                try
                {
                    string base64String = System.Convert.ToBase64String(binaryData, 0, binaryData.Length);
                    return base64String;
                }
                catch (System.ArgumentNullException)
                {
                    System.Console.WriteLine("Binary data array is null.");
                    return "";
                }
            }
            catch
            {
                return "";
            }
        }
    }

    public class ImgurData
    {
        public string imageHash { get; set; }
        public string deleteHash { get; set; }
        public string originalImage { get; set; }
        public string largeThumbnail { get; set; }
        public string smallThumbnail { get; set; }
        public string imgurPage { get; set; }
        public string deletePage { get; set; }

        public ImgurData(XmlDocument xmlData)
        {
            XmlNode xmlNode = xmlData.SelectSingleNode("/rsp/image_hash");
            imageHash = xmlNode.InnerText;

            xmlNode = xmlData.SelectSingleNode("/rsp/delete_hash");
            deleteHash = xmlNode.InnerText;

            xmlNode = xmlData.SelectSingleNode("/rsp/original_image");
            originalImage = xmlNode.InnerText;

            xmlNode = xmlData.SelectSingleNode("/rsp/large_thumbnail");
            largeThumbnail = xmlNode.InnerText;

            xmlNode = xmlData.SelectSingleNode("/rsp/small_thumbnail");
            smallThumbnail = xmlNode.InnerText;

            xmlNode = xmlData.SelectSingleNode("/rsp/imgur_page");
            imgurPage = xmlNode.InnerText;

            xmlNode = xmlData.SelectSingleNode("/rsp/delete_page");
            deletePage = xmlNode.InnerText;
        }
    }
}
