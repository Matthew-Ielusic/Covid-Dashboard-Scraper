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
            const int NAME_INDEX = 1;
            const int TOTAL_INDEX = 3;

            string name = trElement.ChildNodes[NAME_INDEX].InnerText.Trim();
            string rawTotal = trElement.ChildNodes[TOTAL_INDEX].InnerText.Trim();

            return new TableRow { Name = name, Confirmed = Int32.Parse(rawTotal, NumberStyles.AllowThousands) };
        }

        public override string ToString()
        {
            return $"{{{Name}: {Confirmed}}}";
        }
    }
}
