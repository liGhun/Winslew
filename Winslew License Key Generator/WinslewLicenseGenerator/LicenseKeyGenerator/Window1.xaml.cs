using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LicenseKeyGenerator
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        private void button_generate_Click(object sender, RoutedEventArgs e)
        {
            textBox_key.Text = generateLicense(textBox_account.Text);
            if (checkLicense(textBox_account.Text, textBox_key.Text))
            {
                label_testResult.Content = "OK";
            }
            else
            {
                label_testResult.Content = "NOK";
            }
        }

        private string generateLicense(string username)
        {
            
            string licenseToCheck = "Winslew-SomeSaltAndPepper3253245-" + username.ToLower();
            string expectedLicenseCode = GetMD5Hash(licenseToCheck).Substring(2, 10);
            
            return expectedLicenseCode;
        }


        public bool checkLicense(string username, string licenseCode)
        {
            
            if (licenseCode.Length != 10)
            {
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
                return true;
            }
            return false;
        }

        private string GetMD5Hash(string input)
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

        private void button_sendMail_Click(object sender, RoutedEventArgs e)
        {
            if (textBox_key.Text == "")
            {
                generateLicense(textBox_account.Text);
            }
            MailComposer mailComposer = new MailComposer(textBox_account.Text, GetMD5Hash(textBox_account.Text), textBox_email.Text, textBox_name.Text, textBox_lastname.Text, textBox_key.Text);
            mailComposer.Show();
            
        }


    }
}
