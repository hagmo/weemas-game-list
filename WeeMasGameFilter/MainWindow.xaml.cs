﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Net.Http;
using System.Web;
using System.Net;
using Google.GData.Client;
using Google.GData.Spreadsheets;
using System.Collections.ObjectModel;

namespace WeeMasGameFilter

    //https://docs.google.com/spreadsheet/ccc?key=0AuDUStfkpJTBdGIxcE5nU3hqejdEUm1VcWVDSnk1ZGc&usp=drive_web#gid=0
//https://spreadsheets.google.com/tq?&tq=&key=0AuDUStfkpJTBdGIxcE5nU3hqejdEUm1VcWVDSnk1ZGc&gid=2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string CLIENT_ID = "1510761808-aurrtlm99cpv701u16um8dq2vea4f78n.apps.googleusercontent.com";
        private string CLIENT_SECRET = "xQf3e8Ozy3dgjwJT3CJ1a28V";
        private string SCOPE = "https://spreadsheets.google.com/feeds";
        private string REDIRECT_URI = "urn:ietf:wg:oauth:2.0:oob";
        private Uri m_AuthUri;
        private OAuth2Parameters parameters;
        private SpreadsheetsService service;

        private string m_GameListText;
        private string m_WeeMasURL;
        private string m_GameCollectionFilePath;
        private List<string> m_WeeMasGames;

        public MainWindow()
        {
            //Authentication...
            parameters = new OAuth2Parameters();
            parameters.ClientId = CLIENT_ID;
            parameters.ClientSecret = CLIENT_SECRET;
            parameters.RedirectUri = REDIRECT_URI;
            parameters.Scope = SCOPE;
            Uri authUri = new Uri(OAuthUtil.CreateOAuth2AuthorizationUrl(parameters));
            GoogleAuthWindow authWindow = new GoogleAuthWindow(authUri);
            authWindow.ShowDialog();

            try
            {
                if (authWindow.AuthCode == null)
                {
                    throw new Exception("Authentication cancelled.");
                }

                parameters.AccessCode = authWindow.AuthCode;
                OAuthUtil.GetAccessToken(parameters);
                string accessToken = parameters.AccessToken;
                GOAuth2RequestFactory requestFactory = new GOAuth2RequestFactory(null, "WeeMasGameList", parameters);
                service = new SpreadsheetsService("WeeMasGameList");
                service.RequestFactory = requestFactory;

                //WorksheetQuery query2 = new WorksheetQuery("1JU7BwpP47sE62wFO79B7ZXhG56h0Mj393eYqRmUZH2s", "private", "basic");
                //WorksheetFeed feed2 = service.Query(query2);

                //if (feed2.Entries.Count == 0)
                //{
                //    // TODO: There were no spreadsheets, act accordingly.
                //}
                //WorksheetEntry worksheet = (WorksheetEntry)feed2.Entries[0];
                //AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                //ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
                //ListFeed listFeed = service.Query(listQuery);
                //foreach (ListEntry row in listFeed.Entries)
                //{
                //    Console.WriteLine(row.Title.Text);
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show("Authentication failed: " + ex.Message, "Error");
            }
            m_WeeMasGames = new List<string>();
            InitializeComponent();
        }

        public string GameListText
        {
            get { return m_GameListText; }
            set
            {
                if (m_GameListText != value)
                {
                    m_GameListText = value;
                    NotifyPropertyChanged("GameListText");
                }
            }
        }

        public string WeeMasURL
        {
            get { return m_WeeMasURL; }
            set
            {
                if (m_WeeMasURL != value)
                {
                    m_WeeMasURL = value;
                    NotifyPropertyChanged("WeeMasURL");
                }
            }
        }

        public string GameCollectionFilePath
        {
            get { return m_GameCollectionFilePath; }
            set
            {
                if (m_GameCollectionFilePath != value)
                {
                    m_GameCollectionFilePath = value;
                    NotifyPropertyChanged("GameCollectionFilePath");
                }
            }
        }

        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Uri uri = new Uri(m_WeeMasURL);
                string key = null;
                for (int i = 0; i < uri.Segments.Length - 1; i++)
                {
                    var segment = uri.Segments[i];
                    if (segment == "d/")
                    {
                        key = uri.Segments[i + 1];
                        key = key.Remove(key.Length - 1);
                        break;
                    }
                }
                if (key == null)
                {
                    var queryString = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    key = queryString["key"];
                }

                WorksheetQuery query = new WorksheetQuery(key, "private", "basic");
                WorksheetFeed feed = service.Query(query);

                WorksheetEntry worksheet = (WorksheetEntry)feed.Entries[0];
                AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
                ListFeed listFeed = service.Query(listQuery);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("WeeMas completed games:");
                foreach (ListEntry row in listFeed.Entries)
                {
                    m_WeeMasGames.Add(row.Title.Text);
                    sb.AppendLine(row.Title.Text);
                }
                GameListText = sb.ToString();
            }
            catch
            {
                MessageBox.Show("Invalid URL. Please copy the link from the address bar in your browser after opening the spreadsheet in Google Docs.");
                return;
            }
            MessageBox.Show("URL seems valid");
        }

        private string BuildConnectionString(string key)
        {
            Uri uri = new Uri("https://spreadsheets.google.com/feeds/list");
            uri = new Uri(uri, key);
            uri = new Uri(uri, "private/basic");
            return uri.ToString();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.ShowDialog();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}