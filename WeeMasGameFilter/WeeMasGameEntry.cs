using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;

namespace WeeMasGameFilter
{
    public class WeeMasGameEntry : IEquatable<WeeMasGameEntry>, INotifyPropertyChanged
    {
        private static readonly string stripCharacters = ":-+–.,!'’*/#";
        private static readonly string[] romanNumerals = new string[]
        {
            "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI", "XII", "XIII", "XIV", "XV", "XVI", "XVII", "XVIII", "XIX", "XX"
        };

        private WeeMasGameEntry m_Match;
        private bool m_IsManual;

        public WeeMasGameEntry(string data, bool fromWellman)
        {
            OriginalName = data;

            data = string.Concat(data.Where(c => !stripCharacters.Contains(c)));

            for (int i = 0; i < romanNumerals.Length; i++)
            {
                if (data.EndsWith(" " + romanNumerals[i]) || data.EndsWith(" " + romanNumerals[i] + ")"))
                    data = data.Replace(" " + romanNumerals[i], " " + (i + 1));
                else
                    data = data.Replace(" " + romanNumerals[i] + " ", " " + (i + 1) + " ");
            }

            data = data.ToLower();
            data = System.Text.RegularExpressions.Regex.Replace(data, @"\s+", " ");

            if (fromWellman)
            {
                int leftParenthesis = data.IndexOf('(');
                int rightParenthesis = leftParenthesis > 0 ? data.IndexOf(')', leftParenthesis) : -1;
                if (leftParenthesis > 0 && rightParenthesis > 0)
                {
                    AlternateName = data.Substring(leftParenthesis + 1, rightParenthesis - leftParenthesis - 1);
                    AlternateName = AlternateName.Trim();
                }

                if (leftParenthesis > 0)
                    data = data.Substring(0, leftParenthesis);
            }
            else
            {
                data = string.Concat(data.Where(c => c != '(' && c != ')'));
            }

            data = data.Trim();
            Name = data;
        }

        public string OriginalName { get; set; }
        public string Name { get; set; }
        public string AlternateName { get; set; }
        public string Console { get; set; }
        public bool IsManual
        {
            get { return m_IsManual; }
            set
            {
                if (m_IsManual != value)
                {
                    m_IsManual = value;
                    NotifyPropertyChanged("BackgroundColor");
                }
            }
        }
        public WeeMasGameEntry Match
        {
            get { return m_Match; }
            set
            {
                if (m_Match != value)
                {
                    m_Match = value;
                    NotifyPropertyChanged("BackgroundColor");
                }
            }
        }
        public Brush BackgroundColor
        {
            get
            {
                if (Match == null)
                    return Brushes.Transparent;
                else if (Match.Console.ToLower() == Console.ToLower())
                    return IsManual ? new SolidColorBrush(Properties.Settings.Default.ManualConsoleMatchColor) : new SolidColorBrush(Properties.Settings.Default.AutoConsoleMatchColor);
                else
                    return IsManual ? new SolidColorBrush(Properties.Settings.Default.ManualMatchColor) : new SolidColorBrush(Properties.Settings.Default.AutoMatchColor);
            }
        }
        public int StringAligment { get; set; }

        public bool Equals(WeeMasGameEntry other)
        {
            return Name == other.Name;
        }

        public override bool Equals(Object other)
        {
            WeeMasGameEntry entry = other as WeeMasGameEntry;
            if (entry == null)
            {
                return false;
            }
            return Equals(entry);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        private void NotifyPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
