using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Winslew
{
    class LicenseChecker
    {
        public static bool checkLicense(string username, string licenseCode)
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
    }
}
