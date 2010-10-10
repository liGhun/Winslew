using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Winslew
{
    class LicenseChecker
    {
        public static bool checkLicense(string username, string licenseCode) {
            if (checkLicenseOffline(username, licenseCode))
            {
                return true;
            }
            else
            {
                return checkOnlineForLicense();
            }
        }

        private static bool checkLicenseOffline(string username, string licenseCode)
        {
            Properties.Settings.Default.IsValidLicense = false;
            if(licenseCode.Length != 10) {
                return false;
            }
            string expectedLicenseCode = "";
            string licenseToCheck = "Winslew-SomeSaltAndPepper3253245-" + username.ToLower();
            if (licenseToCheck.Length > 16)
            {
                expectedLicenseCode = GetMD5Hash(licenseToCheck).Substring(2, 10);
            }
            if (licenseCode == expectedLicenseCode)
            {
                Properties.Settings.Default.IsValidLicense = true;
                return true;
            }
            return false;
        }

        private static string GetMD5Hash(string input)
        {
            System.Security.Cryptography.MD5CryptoServiceProvider x = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(input);
            bs = x.ComputeHash(bs);
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            foreach (byte b in bs)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            return s.ToString();
        }

        public static bool checkOnlineForLicense()
        {
            if (Properties.Settings.Default.CheckForLicense && Properties.Settings.Default.Username != "" && Properties.Settings.Default.Password != "")
            {
                try
                {
                    Api.Response response = HttpCommunications.SendPostRequest("http://www.li-ghun.de/Winslew/api/checkLicense/",
                        new
                        {
                            userhash = LicenseChecker.GetMD5Hash(Properties.Settings.Default.Username),
                            data = "jh6i7bi8bo"
                        }, true);
                    if (response.Content != null)
                    {
                        if (response.Content != "")
                        {
                            bool licenseValid = LicenseChecker.checkLicenseOffline(Properties.Settings.Default.Username, response.Content.Trim());
                            if (licenseValid)
                            {
                                Properties.Settings.Default.LicenseCode = response.Content.Trim();
                                Properties.Settings.Default.IsValidLicense = true;
                                Properties.Settings.Default.Save();
                                return true;
                            }
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
    }
}
