using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WeeMasGameFilter
{
    public class WeeMasGameEntry : IEquatable<WeeMasGameEntry>, INotifyPropertyChanged
    {
        private Color m_TextColor;

        public static Color PERFECT_MATCH = Colors.Black;
        public static Color UNMATCHED = Colors.Red;
        public static Color POSSIBLE_MATCH = Colors.Green;

        private string m_WeeMasName;
        private string m_WellmanName;

        public string WeeMasName
        {
            get { return m_WeeMasName; }
            set
            {
                if (m_WeeMasName != value)
                {
                    m_WeeMasName = value;
                    NotifyPropertyChanged("WeeMasName");
                }
            }
        }

        public string WellmanName
        { get { return m_WellmanName; }
            set
            {
                if (m_WellmanName != value)
                {
                    m_WellmanName = value;
                    NotifyPropertyChanged("WellmanName");
                }
            }
        }

        public Color TextColor
        {
            get
            {
                if (m_TextColor == null)
                {
                    if (WeeMasName == null || WellmanName == null)
                        return UNMATCHED;
                    else if (WeeMasName == WellmanName)
                        return PERFECT_MATCH;
                    else
                        return POSSIBLE_MATCH;
                }
                else
                {
                    return m_TextColor;
                }
            }
            set
            {
                m_TextColor = value;
            }
        }

        public bool Equals(WeeMasGameEntry other)
        {
            return WeeMasName == other.WeeMasName && WellmanName == other.WellmanName;
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
            int weeMasCode = WeeMasName == null ? 0 : WeeMasName.GetHashCode();
            int wellmanCode = WellmanName == null ? 0 : WellmanName.GetHashCode();
            return weeMasCode * 31 + wellmanCode;
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
