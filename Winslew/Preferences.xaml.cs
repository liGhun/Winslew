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
using System.Windows.Shapes;

namespace Winslew
{
    /// <summary>
    /// Interaction logic for Preferences.xaml
    /// </summary>
    public partial class Preferences : Window
    {
        bool loginHasBeenTestedSuccessfully = false;
        private Api.Api apiAccess = new Api.Api();

        public Preferences()
        {
            InitializeComponent();

            passwordBox_RILpassword.Password = Crypto.ToInsecureString(Crypto.DecryptString(Properties.Settings.Default.Password));
            textBox_RILusername.Text = Properties.Settings.Default.Username;
            
            if (Properties.Settings.Default.LoginHasBeenTestedSuccessfully)
            {
                toggleSaveButton(true);
                button_createAccount.IsEnabled = false;
                button_RILTest.IsEnabled = false;
                button_RILTest.Content = "Login valid";
                loginHasBeenTestedSuccessfully = true;
            }
        }

        private void passwordBox_RILpassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            toggleSaveButton(false);
            button_RILTest.IsEnabled = true;
            button_RILTest.Content = "Login with existing account";
            button_createAccount.IsEnabled = true;
        }

        private void button_RILTest_Click(object sender, RoutedEventArgs e)
        {
            if (apiAccess.checkLoginData(textBox_RILusername.Text, passwordBox_RILpassword.Password))
            {
                toggleSaveButton(true);
                loginHasBeenTestedSuccessfully = true;
            }
            else
            {
                toggleSaveButton(false);
                loginHasBeenTestedSuccessfully = false;
            }
        }

        private void toggleSaveButton(Boolean loginIsValid)
        {
            if(loginIsValid)
            {
                button_RILTest.Content = "Login valid";
                button_save.IsEnabled = true;
                button_RILTest.IsEnabled = false;
                button_createAccount.IsEnabled = false;
            }
            else
            {
                button_RILTest.Content = "Login not valid";
                button_save.IsEnabled = false;
                button_RILTest.IsEnabled = true;
                button_createAccount.IsEnabled = true;
            }
        }

        private void button_save_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Password = Crypto.EncryptString(Crypto.ToSecureString(passwordBox_RILpassword.Password));
            Properties.Settings.Default.Username = textBox_RILusername.Text;
            Properties.Settings.Default.LoginHasBeenTestedSuccessfully = loginHasBeenTestedSuccessfully;
            Properties.Settings.Default.Save();
            AppController.Current.credentialsSavedSuccessfully();
            this.Close();
        }

        private void button_createAccount_Click(object sender, RoutedEventArgs e)
        {
            if(apiAccess.createAccount(textBox_RILusername.Text, passwordBox_RILpassword.Password)) {
                toggleSaveButton(true);
                button_createAccount.IsEnabled = false;
                button_createAccount.Content = "Account has been created";
            }
            else
            {
                toggleSaveButton(false);
            }
        }

        private void textBox_RILusername_TextChanged(object sender, TextChangedEventArgs e)
        {
            toggleSaveButton(false);
            button_RILTest.IsEnabled = true;
            button_RILTest.Content = "Login with existing account";
            button_createAccount.IsEnabled = true;
        }
    }
}
