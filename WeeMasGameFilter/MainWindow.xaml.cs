using Microsoft.Win32;
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

        private ObservableCollection<WeeMasGameEntry> m_GameList;
        private string m_WeeMasURL;
        private string m_GameCollectionFilePath;

        private ObservableCollection<string> m_WeemasNames;
        private ObservableCollection<string> m_WellmanNames;

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
            m_GameList = new ObservableCollection<WeeMasGameEntry>();
            m_WeemasNames = new ObservableCollection<string>();
            m_WellmanNames = new ObservableCollection<string>();
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

        public ObservableCollection<WeeMasGameEntry> GameList
        {
            get { return m_GameList; }
            set
            {
                if (m_GameList != value)
                {
                    m_GameList = value;
                    NotifyPropertyChanged("GameList");
                }
            }
        }

        public ObservableCollection<string> WeemasNames
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

        public ObservableCollection<string> WellmanNames
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

                WorksheetQuery query = new WorksheetQuery(key, "private", "basic");
                WorksheetFeed feed = service.Query(query);

                WeemasNames.Clear();

                WorksheetEntry worksheet = (WorksheetEntry)feed.Entries[0];
                AtomLink listFeedLink = worksheet.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
                ListQuery listQuery = new ListQuery(listFeedLink.HRef.ToString());
                ListFeed listFeed = service.Query(listQuery);
                for (int i = 0; i < listFeed.Entries.Count; i++)
                {
                    var row = listFeed.Entries[i];
                    WeemasNames.Add(row.Title.Text);
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

                FileStream stream = File.Open(dialog.FileName, FileMode.Open, FileAccess.Read);
                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);

                List<int> gameNameColumns = new List<int>();
                int row = 0;
                int firstGameNameRow = int.MaxValue;
                while (excelReader.Read())
                {
                    for (int column = 0; column < excelReader.FieldCount; column++)
                    {
                        //Remember all columns that have "Spel", since those contain the game names.
                        string data = excelReader.GetString(column);
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
                                WellmanNames.Add(data);
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

        private void MatchNamesButton_Click(object sender, RoutedEventArgs e)
        {
            //Maybe populate XXXNames with the Node objects instead so we don't have to iterate through them twice

            //Step 1: Calculate all possible string matchings (o_O)
            //Step 2: Remove nodes on one side so that we have equal amount of nodes on each side.
            //          Should be possible by summing all possible matchings for each node on the dominant side and remove the highest ranking ones
            //Step 3: Minimum-Cost Perfect Matching problem on the resulting graph

            //Clean strings: Keep only letters and numbers. Remove parentheses if the string between contains ntsc, komplett, kassett, jap, tysk, box, repro
            //Maybe better idea: Put the parentheses' contents as a separate alternate title and try to match on it.

            List<GameNameNode> weemasNodes = new List<GameNameNode>();
            List<GameNameNode> wellmanNodes = new List<GameNameNode>();
            List<GameNameEdge> edges = new List<GameNameEdge>();
            foreach (var weemasName in WeemasNames)
            {
                var weemasNode = new GameNameNode()
                {
                    Value = weemasName,
                    Price = 0
                };
                weemasNodes.Add(weemasNode);
            }

            foreach (var wellmanName in WellmanNames)
            {
                var wellmanNode = new GameNameNode()
                {
                    Value = wellmanName,
                    Price = -1 //to be determined later
                };
                wellmanNodes.Add(wellmanNode);
            }

            foreach (var wellmanNode in wellmanNodes)
            {
                int minimumScore = int.MaxValue;

                foreach (var weemasNode in weemasNodes)
                {
                    int score = CalculateAlignmentScore(weemasNode.Value, wellmanNode.Value);
                    if (score < minimumScore)
                        minimumScore = score;

                    edges.Add(new GameNameEdge()
                    {
                        Node1 = weemasNode,
                        Node2 = wellmanNode,
                        Cost = score
                    });
                }
                wellmanNode.Price = minimumScore;
            }

            //This is the starting point of the algorithm
            //While we still have unmatched nodes
        }

        private int CalculateAlignmentScore(string n1, string n2)
        {
            return 0;
        }

        private void ShowWeemasListButton_Click(object sender, RoutedEventArgs e)
        {
            ListWindow window = new ListWindow(m_WeemasNames.ToList());
            window.ShowDialog();
        }

        private void ShowWellmanListButton_Click(object sender, RoutedEventArgs e)
        {
            ListWindow window = new ListWindow(m_WellmanNames.ToList());
            window.ShowDialog();
        }
    }
}
