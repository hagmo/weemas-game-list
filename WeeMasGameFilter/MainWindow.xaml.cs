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
using System.Data.OleDb;
using System.Data;
using System.IO;
using Excel;

namespace WeeMasGameFilter
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
        private OAuth2Parameters parameters;
        private SpreadsheetsService service;

        private string m_WeeMasURL;
        private string m_GameCollectionFilePath;

        private ObservableCollection<WeeMasGameEntry> m_WeemasNames;
        private ObservableCollection<WeeMasGameEntry> m_WellmanNames;
        private string m_WeemasSearchString;
        private string m_WellmanSearchString;

        private WeeMasGameEntry m_SelectedWeemasItem;
        private WeeMasGameEntry m_SelectedWellmanItem;

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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Authentication failed: " + ex.Message, "Error");
            }
            m_WeemasNames = new ObservableCollection<WeeMasGameEntry>();
            m_WellmanNames = new ObservableCollection<WeeMasGameEntry>();
            InitializeComponent();
            WeeMasURL = "https://docs.google.com/spreadsheets/d/1JU7BwpP47sE62wFO79B7ZXhG56h0Mj393eYqRmUZH2s/";
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

        public ObservableCollection<WeeMasGameEntry> WeemasNames
        {
            get { return m_WeemasNames; }
            set
            {
                if (m_WeemasNames != value)
                {
                    m_WeemasNames = value;
                    NotifyPropertyChanged("WeemasNames");
                }
            }
        }

        public ObservableCollection<WeeMasGameEntry> WellmanNames
        {
            get { return m_WellmanNames; }
            set
            {
                if (m_WellmanNames != value)
                {
                    m_WellmanNames = value;
                    NotifyPropertyChanged("WellmanNames");
                }
            }
        }

        public string WeemasSearchString
        {
            get { return m_WeemasSearchString; }
            set
            {
                if (m_WeemasSearchString != value)
                {
                    m_WeemasSearchString = value;
                    NotifyPropertyChanged("FilteredWeemasNames");
                }
            }
        }

        public IEnumerable<WeeMasGameEntry> FilteredWeemasNames
        {
            get
            {
                if (!string.IsNullOrEmpty(m_WeemasSearchString))
                {
                    var filteredEntries = m_WeemasNames.Where(e => e.OriginalName.ToLower().Contains(m_WeemasSearchString.ToLower()));
                    filteredEntries = filteredEntries.Union(m_WeemasNames.Where(e => e.Console.ToLower().Contains(m_WeemasSearchString.ToLower())));
                    return filteredEntries;
                }
                else
                {
                    return m_WeemasNames;
                }
            }
        }

        public string WellmanSearchString
        {
            get { return m_WellmanSearchString; }
            set
            {
                if (m_WellmanSearchString != value)
                {
                    m_WellmanSearchString = value;
                    NotifyPropertyChanged("FilteredWellmanNames");
                }
            }
        }

        public IEnumerable<WeeMasGameEntry> FilteredWellmanNames
        {
            get
            {
                if (!string.IsNullOrEmpty(m_WellmanSearchString))
                {
                    var filteredEntries = m_WellmanNames.Where(e => e.OriginalName.ToLower().Contains(m_WellmanSearchString.ToLower()));
                    filteredEntries = filteredEntries.Union(m_WellmanNames.Where(e => e.Console.ToLower().Contains(m_WellmanSearchString.ToLower())));
                    return filteredEntries;
                }
                else
                {
                    return m_WellmanNames;
                }
            }
        }

        public int NbrOfMatches
        {
            get
            {
                return m_WeemasNames.Where(e => e.Match != null).Count();
            }
        }

        public WeeMasGameEntry SelectedWeemasItem
        {
            get { return m_SelectedWeemasItem; }
            set
            {
                if (m_SelectedWeemasItem != value)
                {
                    m_SelectedWeemasItem = value;
                    NotifyPropertyChanged("SelectedWeemasItem");
                }
            }
        }

        public WeeMasGameEntry SelectedWellmanItem
        {
            get { return m_SelectedWellmanItem; }
            set
            {
                if (m_SelectedWellmanItem != value)
                {
                    m_SelectedWellmanItem = value;
                    NotifyPropertyChanged("SelectedWellmanItem");
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
                        if (key.EndsWith("/"))
                            key = key.Remove(key.Length - 1);
                        break;
                    }
                }
                if (key == null)
                {
                    var queryString = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    key = queryString["key"];
                }

                WorksheetQuery query = new WorksheetQuery(key, "private", "full");
                WorksheetFeed feed = service.Query(query);

                WeemasNames.Clear();

                WorksheetEntry worksheet = (WorksheetEntry)feed.Entries[0];

                AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
                ListFeed listFeed = service.Query(listQuery);
                foreach (ListEntry row in listFeed.Entries)
                {
                    var gameEntry = new WeeMasGameEntry(row.Title.Text.Trim(), false);
                    gameEntry.Console = ParseConsoleName(row.Elements[1].Value);
                    WeemasNames.Add(gameEntry);
                }
            }
            catch
            {
                MessageBox.Show("Invalid URL. Please copy the link from the address bar in your browser after opening the spreadsheet in Google Docs.");
                return;
            }
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
            dialog.Filter = "Excel OpenXML Files (*.xlsx)|*.xlsx";
            dialog.ShowDialog();
            if (dialog.FileName != null && dialog.FileName != string.Empty)
            {
                WellmanNames.Clear();

                FileStream stream = null;
                try
                {
                    stream = File.Open(dialog.FileName, FileMode.Open, FileAccess.Read);
                }
                catch (IOException ex)
                {
                    MessageBox.Show(ex.Message + "\n\n(Please ensure that the file is not already open in Excel.)");
                    return;
                }
                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

                List<int> gameNameColumns = new List<int>();
                List<string> consoleNames = new List<string>();
                int row = 0;
                int firstGameNameRow = int.MaxValue;
                while (excelReader.Read())
                {
                    bool consoleRow = false;
                    for (int column = 0; column < excelReader.FieldCount; column++)
                    {
                        string data = excelReader.GetString(column);
                        if (data == "NES")
                        {
                            consoleRow = true;
                        }

                        if (consoleRow == true && column % 2 != 0)
                        {
                            consoleNames.Add(ParseConsoleName(data));
                        }
                        //Remember all columns that have "Spel", since those contain the game names.
                        if (row < firstGameNameRow && data == "Spel")
                        {
                            gameNameColumns.Add(column);
                            firstGameNameRow = row + 1;
                        }
                        else if (row >= firstGameNameRow)
                        {
                            //If we're inside the section with game names, save all fields in the columns that had "Spel" at the top
                            if (data != null && gameNameColumns.Contains(column))
                            {
                                var entry = new WeeMasGameEntry(data, true);
                                entry.Console = consoleNames[gameNameColumns.IndexOf(column)];
                                if (!WellmanNames.Contains(entry))
                                    WellmanNames.Add(entry);
                            }
                        }
                    }
                    row++;
                }
                excelReader.Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void AutoMatchButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var entry in m_WeemasNames)
            {
                try
                {
                    //prioritize console matches
                    var match = m_WellmanNames.SingleOrDefault(ent => ent.Name == entry.Name && ent.Console == entry.Console && ent.Match == null);

                    if (match == null)
                        match = m_WellmanNames.SingleOrDefault(ent => ent.AlternateName == entry.Name && ent.Console == entry.Console && ent.Match == null);

                    //exact name matches
                    if (match == null)
                        match = m_WellmanNames.SingleOrDefault(ent => ent.Name == entry.Name && ent.Match == null);
                    
                    if (match == null)
                        match = m_WellmanNames.SingleOrDefault(ent => ent.AlternateName == entry.Name && ent.Match == null);

                    if (match != null)
                    {
                        entry.Match = match;
                        match.Match = entry;
                    }
                }
                catch (Exception ex)
                {
                    var result = MessageBox.Show(string.Format("{0} on {1}: {2}. Show stacktrace?", ex, entry.OriginalName, ex.Message), "Error", MessageBoxButton.YesNo);
                    if (result == MessageBoxResult.Yes)
                        MessageBox.Show(ex.StackTrace);
                }
            }
            NotifyPropertyChanged("NbrOfMatches");
        }

        private string ParseConsoleName(string s)
        {
            string sl = s.ToLower();
            if (sl == "xbox360")
                return "X360";
            else if (sl == "nintendo 64")
                return "N64";
            else if (sl == "gamecube")
                return "GCN";
            else if (sl == "game boy")
                return "GB";
            else if (sl == "virtual boy")
                return "VB";
            else if (sl == "game boy color")
                return "GBC";
            else if (sl == "game boy advance micro")
                return "GBA";
            else if (sl == "nintendo ds lite")
                return "NDS";
            else if (sl == "nintendo 3ds + xl*")
                return "3DS";
            else if (sl == "sega master system")
                return "SMS";
            else if (sl == "sega genesis")
                return "SMD";
            else if (sl == "md")
                return "SMD";
            else if (sl == "sega cd")
                return "SCD";
            else if (sl == "sega saturn")
                return "SAT";
            else if (sl == "saturn")
                return "SAT";
            else if (sl == "dreamcast")
                return "DC";
            else if (sl == "game gear")
                return "GG";
            else if (sl == "playstation")
                return "PSX";
            else if (sl == "playstation 2")
                return "PS2";
            else if (sl == "playstation 3")
                return "PS3";
            else if (sl == "playstation 4")
                return "PS4";
            else if (sl == "playstation portable")
                return "PSP";
            else if (sl == "playstation vita")
                return "PSV";
            else if (sl == "playstation")
                return "PSX";
            else return s;
        }

        private void UnmatchAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var entry in WellmanNames)
                entry.Match = null;
            foreach (var entry in WeemasNames)
                entry.Match = null;
        }

        private void NameSortButton_Click(object sender, RoutedEventArgs e)
        {
            SortLists(delegate(WeeMasGameEntry x, WeeMasGameEntry y)
            {
                return x.Name.CompareTo(y.Name);
            });
        }

        private void ConsoleSortButton_Click(object sender, RoutedEventArgs e)
        {
            SortLists(delegate(WeeMasGameEntry x, WeeMasGameEntry y)
            {
                return x.Console.CompareTo(y.Console);
            });
        }

        private void MatchSortButton_Click(object sender, RoutedEventArgs e)
        {
            SortLists(delegate(WeeMasGameEntry x, WeeMasGameEntry y)
            {
                if (x.Match != null && y.Match == null)
                    return -1;
                if (x.Match == null && y.Match != null)
                    return 1;
                return 0;
            });
        }

        private void SortLists(Comparison<WeeMasGameEntry> comparison)
        {
            SortWeemasList(comparison);
            SortWellmanList(comparison);
        }

        private void SortWellmanList(Comparison<WeeMasGameEntry> comparison)
        {
            var sorted = WellmanNames.ToList();
            sorted.Sort(comparison);
            WellmanNames.Clear();
            foreach (var entry in sorted)
                WellmanNames.Add(entry);
        }

        private void SortWeemasList(Comparison<WeeMasGameEntry> comparison)
        {
            var sorted = WeemasNames.ToList();
            sorted.Sort(comparison);
            WeemasNames.Clear();
            foreach (var entry in sorted)
                WeemasNames.Add(entry);
        }

        private void UnmatchSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedWeemasItem != null && SelectedWellmanItem != null)
            {
                SelectedWeemasItem.Match = null;
                SelectedWellmanItem.Match = null;
            }
        }

        private void MatchSelectedButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedWeemasItem != null && SelectedWellmanItem != null)
            {
                SelectedWeemasItem.Match = SelectedWellmanItem;
                SelectedWellmanItem.Match = SelectedWeemasItem;
            }
        }

        private int StringAlignment(string s1, string s2)
        {
            int[,] alignments = new int[s1.Length + 1, s2.Length + 1];

            for (int j = 0; j <= s2.Length; j++)
            {
                alignments[0, j] = j;
            }

            for (int i = 1; i <= s1.Length; i++)
            {
                alignments[i, 0] = i;
                for (int j = 1; j <= s2.Length; j++)
                {
                    alignments[i, j] = -1;
                }
            }

            return StringAlignment(alignments, s1, s2);
        }

        private int StringAlignment(int[,] alignments, string s1, string s2)
        {
            if (alignments[s1.Length, s2.Length] != -1)
                return alignments[s1.Length, s2.Length];

            for (int i = 1; i <= s1.Length; i++)
            {
                for (int j = 1; j <= s2.Length; j++)
                {
                    int penalty = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    int v1 = penalty + StringAlignment(alignments, s1.Substring(0, i - 1), s2.Substring(0, j - 1));
                    int v2 = 1 + StringAlignment(alignments, s1.Substring(0, i - 1), s2.Substring(0, j));
                    int v3 = 1 + StringAlignment(alignments, s1.Substring(0, i), s2.Substring(0, j - 1));
                    alignments[i, j] = Math.Min(v1, Math.Min(v2, v3));
                }
            }
            return alignments[s1.Length, s2.Length];
        }

        private void WeemasListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedWeemasItem == null)
            {
                return;
            }

            if (SelectedWeemasItem.Match != null)
            {
                WellmanListView.ScrollIntoView(SelectedWeemasItem.Match);
            }
            else
            {
                foreach (var wellmanItem in WellmanNames)
                {
                    wellmanItem.StringAligment = StringAlignment(wellmanItem.Name, SelectedWeemasItem.Name);
                }
                SortWellmanList(delegate(WeeMasGameEntry x, WeeMasGameEntry y)
                {
                    return x.StringAligment - y.StringAligment;
                });
                if (WellmanNames.Count > 0)
                    WellmanListView.ScrollIntoView(WellmanNames[0]);
            }
        }

        private void WellmanListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedWellmanItem == null)
            {
                return;
            }

            if (SelectedWellmanItem.Match != null)
            {
                SelectedWeemasItem = SelectedWellmanItem.Match;
                WeemasListView.ScrollIntoView(SelectedWeemasItem);
            }
            else
            {
                foreach (var weemasItem in WeemasNames)
                {
                    weemasItem.StringAligment = StringAlignment(weemasItem.Name, SelectedWellmanItem.Name);
                }
                SortWeemasList(delegate(WeeMasGameEntry x, WeeMasGameEntry y)
                {
                    return x.StringAligment - y.StringAligment;
                });
                if (WeemasNames.Count > 0)
                    WeemasListView.ScrollIntoView(WeemasNames[0]);
            }
        }
    }
}
