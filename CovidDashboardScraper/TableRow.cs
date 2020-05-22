using HtmlAgilityPack;
using System;
using System.Globalization;

namespace CovidDashboardScraper
{     
    public class TableRow
    {
        public string Name { get; set; }
        public int Confirmed { get; set; }

        internal static TableRow Parse_tr_Element(HtmlNode trElement)
        {
            const int NAME_INDEX = 3;
            const int TOTAL_INDEX = 5;
            const string STAR_HTML = "&#9733;";

            string name = trElement.ChildNodes[NAME_INDEX].InnerText.Trim();
            string rawTotal = trElement.ChildNodes[TOTAL_INDEX].InnerText.Trim();

            // The rows for states start with a little star character that needs to be removed
            // The html for a star character is "&#9733;
            if (name.StartsWith(STAR_HTML))
            {
                name = name.Substring(STAR_HTML.Length).Trim();
            }

            return new TableRow { Name = name, Confirmed = Int32.Parse(rawTotal, NumberStyles.AllowThousands) };
        }

        public override string ToString()
        {
            return $"{{{Name}: {Confirmed}}}";
        }
    }
}
