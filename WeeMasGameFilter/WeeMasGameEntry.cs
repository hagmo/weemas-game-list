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
        private static readonly string stripCharacters = ":-+–.,!'’*/#";
        private static readonly string[] romanNumerals = new string[]
        {
            "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI", "XII", "XIII", "XIV", "XV", "XVI", "XVII", "XVIII", "XIX", "XX"
        };

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
        public WeeMasGameEntry Match { get; set; }

        public bool Equals(WeeMasGameEntry other)
        {
            return OriginalName == other.OriginalName;
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
            return OriginalName.GetHashCode();
        }

        public string DisplayString
        {
            get { return string.Format("{0} ({1})", Name, Console); }
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
