using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CovidDashboardScraper
{
    public class CSVHandler
    {
        private const string HEADER_LINE = "Date, TOTAL, Alabama, Alaska, Arizona, Arkansas, California, Colorado, Connecticut, Delaware, District of Columbia, Federal Prisons, Florida, Georgia, Guam, Hawaii, Idaho, Illinois, Indiana, Iowa, Kansas, Kentucky, Louisiana, Maine, Maryland, Massachusetts, Michigan, Minnesota, Mississippi, Missouri, Montana, Navajo Nation, Nebraska, Nevada, New Hampshire, New Jersey, New Mexico, New York, North Carolina, North Dakota, Northern Mariana Islands, Ohio, Oklahoma, Oregon, Pennsylvania, Puerto Rico, Rhode Island, South Carolina, South Dakota, Tennessee, Texas, U.S. Military, United States Virgin Islands, Utah, Vermont, Veteran Affairs, Virgin Islands, Virginia, Washington, West Virginia, Wisconsin, Wuhan Repatriated, Wyoming";



        private string path;
        private List<String> stateNames;
        public static CSVHandler MakeHandler(String path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string header = reader.ReadLine(); 
                
                CSVHandler output = new CSVHandler();
                output.path = path;

                output.stateNames = new List<String>(header.Split(", "));
                output.stateNames.Remove("Date");
                return output;
            }
        }


        // Smashes any file that happens to be located at 'path'
        internal static void Create(string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
                writer.WriteLine(HEADER_LINE);
        }

        public void Write(IEnumerable<TableRow> data, DateTime timestamp)
        {
            Dictionary<String, Int32> dataIndexer = new Dictionary<String, Int32>();
            foreach (var d in data)
            {
                dataIndexer[d.Name] = d.Confirmed;
            }

            List<String> fields = new List<String>();
            fields.Add(timestamp.ToString());
            foreach (string name in stateNames)
            {
                fields.Add(dataIndexer[name].ToString());
            }
            string line = fields.Aggregate((accumulator, next) => accumulator + ", " + next);

            using (StreamWriter writer = new StreamWriter(this.path, append: true))
            {
                writer.WriteLine(line);
            }
        }
    }
}
