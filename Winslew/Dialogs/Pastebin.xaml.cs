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

namespace Winslew.Dialogs
{
    /// <summary>
    /// Interaction logic for Pastebin.xaml
    /// </summary>
    public partial class Pastebin : Window
    {
        public static RoutedCommand EscapePressed = new RoutedCommand();

        Api.Pastebin api;
        Dictionary<string, string> translatorSuffixToHighlighter;

        public Pastebin()
        {
            InitializeComponent();

            translatorSuffixToHighlighter = new Dictionary<string, string>();
            fillTranslatorForHighlighter();

            api = new Api.Pastebin();
            comboBox_syntaxHighlighting.ItemsSource = api.AvailableFormats;
            comboBox_syntaxHighlighting.DisplayMemberPath = "Value";
            comboBox_syntaxHighlighting.SelectedValuePath = "Key";
            comboBox_syntaxHighlighting.SelectedValue = "";
            comboBox_syntaxHighlighting.UpdateLayout();

            textBox_title.Focus();

            EscapePressed.InputGestures.Add(new KeyGesture(Key.Escape));
        }


        private void button_sendToPastebin_Click(object sender, RoutedEventArgs e)
        {
            if (textBox_text.Text == "")
            {
                MessageBox.Show("Please enter some text into the textbox first", "No text entered", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            else
            {
                bool isPrivate = true;
                if(comboBox_exposure.SelectedValue.ToString() == "Public") {
                    isPrivate = false;
                }
                Api.Pastebin.Result result = api.AddToPastebin(textBox_text.Text, textBox_title.Text, textBox_email.Text, textBox_subdomain.Text, isPrivate, comboBox_expiration.SelectedValue.ToString(), comboBox_syntaxHighlighting.SelectedValue.ToString());
                if (result.PastebinUrl.StartsWith("http"))
                {
                    if (textBox_RiLTags.Text != "")
                    {
                        AppController.Current.addToListWithTags(result.PastebinUrl, textBox_title.Text, textBox_RiLTags.Text);
                    }
                    else
                    {
                        AppController.Current.addItem(result.PastebinUrl, textBox_title.Text);
                    }
                    Close();
                }
                else
                {
                    MessageBox.Show(result.ErrorText, "Sending to Pastebin failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void button_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void textBox_text_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBox_title != null && textBox_text != null)
            {

                if (textBox_text.Text != "" && textBox_title.Text != "")
                {
                    button_sendToPastebin.IsEnabled = true;
                }
                else
                {
                    button_sendToPastebin.IsEnabled = false;
                }
            }
            else
            {
                button_sendToPastebin.IsEnabled = false;
            }
        }

        public void selectHighlighter(string selector) {
            selector = selector.ToLower();
            selector = selector.TrimStart('.');
            if (translatorSuffixToHighlighter.ContainsKey(selector))
            {
                selector = translatorSuffixToHighlighter[selector];
            }
            if(api.AvailableFormats.ContainsKey(selector)) {
                comboBox_syntaxHighlighting.SelectedValue = selector;
            }
        }

        private void fillTranslatorForHighlighter()
        {
            translatorSuffixToHighlighter.Add("as", "actionscript3");
            translatorSuffixToHighlighter.Add("scpt", "applescript");
            translatorSuffixToHighlighter.Add("au3", "autoit");
            translatorSuffixToHighlighter.Add("aut", "autoit");
            translatorSuffixToHighlighter.Add("sh", "bash");
            translatorSuffixToHighlighter.Add("dil", "delphi");
            translatorSuffixToHighlighter.Add("cs", "csharp");
            translatorSuffixToHighlighter.Add("e", "eiffel");
            translatorSuffixToHighlighter.Add("rb", "ruby");
        }

        private void textBox_text_KeyDown(object sender, KeyEventArgs e)
        {
            textBox_text.Focus();
        }

        private void EscapePressed_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            button_cancel_Click(null, null);
        }
    }
}
