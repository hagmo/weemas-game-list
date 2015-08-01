using System;
using System.Windows;

namespace WeeMasGameFilter
{
    /// <summary>
    /// Interaction logic for GoogleAuthWindow.xaml
    /// </summary>
    public partial class GoogleAuthWindow : Window
    {
        private Uri authUri;
        public GoogleAuthWindow(Uri authUri)
        {
            InitializeComponent();
            this.authUri = authUri;
            BrowserControl.Source = authUri;
        }

        public string AuthCode { get; set; }

        private void BrowserControl_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri != authUri)
            {
                dynamic doc = BrowserControl.Document;
                string htmlText = doc.title;
                int i = htmlText.IndexOf('=');
                if (i > 0)
                {
                    AuthCode = htmlText.Substring(i + 1);
                    Close();
                }
            }
        }
    }
}
