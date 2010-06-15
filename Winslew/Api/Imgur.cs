using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Winslew.Api
{
    public class Imgur
    {
        public static string uploadImage(string path)
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
                return "";
            }
            else
            {
                xmlDoc.LoadXml(result.Content);
                XmlNode imagePage = xmlDoc.SelectSingleNode("/rsp/imgur_page");
                string imageUrl = imagePage.InnerText;
                return imageUrl;
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
}
