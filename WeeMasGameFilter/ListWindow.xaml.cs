using System.Collections.ObjectModel;
using System.Windows;

namespace WeeMasGameFilter
{
    /// <summary>
    /// Interaction logic for ListWindow.xaml
    /// </summary>
    public partial class ListWindow : Window
    {
        public ListWindow(ObservableCollection<WeeMasGameEntry> list)
        {
            GameList = list;
            InitializeComponent();
        }

        public ObservableCollection<WeeMasGameEntry> GameList { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
