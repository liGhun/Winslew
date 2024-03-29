﻿using System;
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
using System.Windows.Shapes;
using System.Net;
using System.Net.Mail;

namespace LicenseKeyGenerator
{
    /// <summary>
    /// Interaction logic for MailComposer.xaml
    /// </summary>
    public partial class MailComposer : Window
    {
        private string Username;
        private string Firstname;
        private string Lastname;
        private string LicenseCode;
        private string UsernameHash;
        private string Email;

        public MailComposer(string username, string usernamehash, string email, string firstname, string lastname, string licenseCode) {
            InitializeComponent();
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            this.textBox_to.Text = string.Format("{0} {1} <{2}>",firstname,lastname,email);
            this.Username = username;
            this.UsernameHash = usernamehash;
            this.Email = email;
            this.Firstname = firstname;
            this.Lastname = lastname;
            this.LicenseCode = licenseCode;
            this.textBox_userhash.Text = usernamehash;
            textBox_text.Text = emailText(username, firstname, licenseCode);
        }

        private string emailText(string username, string firstname, string licenseKey)
        {
            return string.Format("Hello {0},\n\nThanks for having purchased a full license of Winslew. With this mail I am sending you your individual license code which will be valid for an unlimited time and include all updates of Winslew in the future.\n\n  Your Pocket username: {1}\n  Your license code: {2}\n\nYou can also use this license code on as many different PCs as you like at once as long they use the same user account.\n\nYou can either enter this code into the Winslew preferences manually or just (re)start Winslew and it will pick up the license automatically online from my database.\n\nHave fun with Winslew\n\nSven Walther", firstname, username, licenseKey);
        }

        private bool storeInDatabase()
        {
            Api.Response response = HttpCommunications.SendPostRequest("http://www.li-ghun.de/Winslew/api/checkLicense/admin/storeLicense.php",
                             new
                       {
                           userhash = this.UsernameHash,
                           data = "weg547",
                           username = this.Username,
                           firstname = this.Firstname,
                           lastname = this.Lastname,
                           email = this.Email,
                           licenseCode = this.LicenseCode,
                       }, true);
            if (response.Content != null)
            {
                if (response.Content != "")
                {
                    if (response.Content.StartsWith("Storage successfull"))
                    {
                        MessageBox.Show(response.Content, "Storing successfull");
                        return true;
                    }
                    else
                    {
                        MessageBox.Show(response.Content, "Store failed");
                    }
                }
                else
                {
                    MessageBox.Show("Empty response", "Store failed");
                }
            }
            else
            {
                MessageBox.Show("NULL response", "Store failed");
            }
            return false;

        }
        

        private bool sendEmail()
        {
            SmtpClient client = new SmtpClient("sven-walther.de", 25);
            client.EnableSsl = true;
            

            client.Credentials = new NetworkCredential("sven", passwordBox_mailServer.Password);
            
            using (MailMessage msg = new MailMessage())
            {
                msg.From = new MailAddress(textBox_from.Text);
                msg.Subject = textBox_subject.Text;
                msg.Body = textBox_text.Text;            
                msg.To.Add(new MailAddress(textBox_to.Text));
                msg.Bcc.Add("info@li-ghun.de");
                
                msg.Sender = new MailAddress(textBox_from.Text);
                try
                {
                    client.Send(msg);
                }
                catch (Exception exp)
                {
                    MessageBox.Show(exp.Message, "Sending failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    MessageBox.Show(exp.StackTrace, "Sending failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            return true;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            sendEmail();
        }

        private void button_storeOnly_Click(object sender, RoutedEventArgs e)
        {
            storeInDatabase();
        }

        private void button_storeAndSend_Click(object sender, RoutedEventArgs e)
        {
            storeInDatabase();
            sendEmail();
        }
    }
}
