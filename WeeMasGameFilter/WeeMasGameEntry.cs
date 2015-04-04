using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WeeMasGameFilter
{
    class WeeMasGameEntry
    {
        private Color m_TextColor;

        public static Color PERFECT_MATCH = Colors.Black;
        public static Color UNMATCHED = Colors.Red;
        public static Color POSSIBLE_MATCH = Colors.Green;

        public string WeeMasName { get; set; }
        public string WellmanName { get; set; }
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

    }
}
