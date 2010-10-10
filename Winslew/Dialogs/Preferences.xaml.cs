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
        bool isInit = true;
        bool unlicensed = false;

        bool loginHasBeenTestedSuccessfully = false;
        private Api.Api apiAccess = new Api.Api();

        public Preferences()
        {
            InitializeComponent();
            try
            {
                passwordBox_RILpassword.Password = Crypto.ToInsecureString(Crypto.DecryptString(Properties.Settings.Default.Password));
                textBox_RILusername.Text = Properties.Settings.Default.Username;
                isInit = false;
                textBox_licenseCode.Text = Properties.Settings.Default.LicenseCode;
            }
            catch
            {
            }

            try
            {
                if (Properties.Settings.Default.LoginHasBeenTestedSuccessfully)
                {
                    toggleSaveButton(true);
                    button_createAccount.IsEnabled = false;
                    button_RILTest.IsEnabled = false;
                    button_RILTest.Content = "Login valid";
                    loginHasBeenTestedSuccessfully = true;
                    if (!Properties.Settings.Default.IsValidLicense)
                    {
                        LicenseChecker.checkOnlineForLicense();
                    }
                }
            }
            catch
            {

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
                Properties.Settings.Default.Password = Crypto.EncryptString(Crypto.ToSecureString(passwordBox_RILpassword.Password));
                Properties.Settings.Default.Username = textBox_RILusername.Text;
                Properties.Settings.Default.Save();
                bool licenseValid = LicenseChecker.checkLicense(textBox_RILusername.Text, textBox_licenseCode.Text);
                if (!licenseValid)
                {
                    if (LicenseChecker.checkOnlineForLicense())
                    {
                        licenseValid = true;
                        textBox_licenseCode.Text = Properties.Settings.Default.LicenseCode;
                    }
                }
                if (licenseValid)
                {
                    licenseIsValid();
                }
                else
                {
                    licenseIsNotValid();
                }
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
            Properties.Settings.Default.LicenseCode = textBox_licenseCode.Text;
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
            Properties.Settings.Default.Username = textBox_RILusername.Text;
            textBox_licenseCode_TextChanged(null, null);
        }

        private void textBox_licenseCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInit)
            {
                return;
            }
            Properties.Settings.Default.LicenseCode = textBox_licenseCode.Text;
            if(LicenseChecker.checkLicense(Properties.Settings.Default.Username, textBox_licenseCode.Text)) {
                licenseIsValid();
            }
            else
            {
                licenseIsNotValid();
            }
            Properties.Settings.Default.Save();
        }

        private void licenseIsValid()
        {
            label_licValid.Content = "License valid";
            label_licValid.Foreground = new SolidColorBrush(Colors.Green);
            button_getLicense.IsEnabled = false;
            button_getLicense.Content = "Thank you";
            checkBox_retrieveLicense.IsEnabled = false;
            textBox_licenseCode.Text = Properties.Settings.Default.LicenseCode;
            Properties.Settings.Default.Save();
            if (unlicensed && Properties.Settings.Default.LoginHasBeenTestedSuccessfully)
            {
                AppController.Current.UpdateItemsFromRiL(true);
                AppController.Current.refreshMainWindow();
            }
            unlicensed = false;
            
        }

        private void licenseIsNotValid()
        {
            label_licValid.Content = "Not licensed";
            label_licValid.Foreground = new SolidColorBrush(Colors.Orange);
            button_getLicense.IsEnabled = true;
            button_getLicense.Content = "Buy license";
            checkBox_retrieveLicense.IsEnabled = true;
            unlicensed = true;
        }

        private void button_getLicense_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.li-ghun.de/Winslew/Download/");
        }


        bool AreAllValidNumericChars(string str)
        {
            bool ret = true;


            int l = str.Length;
            for (int i = 0; i < l; i++)
            {
                char ch = str[i];
                ret &= Char.IsDigit(ch);
            }

            return ret;
        }

        private void textBox_autoRefresh_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !AreAllValidNumericChars(e.Text);
            base.OnPreviewTextInput(e); 
        }

        private void checkBox_autoRefresh_Checked(object sender, RoutedEventArgs e)
        {
            if (IsInitialized)
            {
                if (checkBox_autoRefresh.IsChecked == true)
                {
                    AppController.Current.startAutoRefreshTimer(Properties.Settings.Default.AutoRefreshInterval);
                }
                else
                {
                    AppController.Current.stopAutoRefreshTimer();
                }
            }
        }
    }
}
