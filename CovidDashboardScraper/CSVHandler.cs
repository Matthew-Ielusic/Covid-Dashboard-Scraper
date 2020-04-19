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
        private const string HEADER_LINE = "Date, TOTAL, New York, Washington, California, New Jersey, Illinois, Michigan, Florida, Louisiana, Texas, Massachusetts, Georgia, Colorado, Tennessee, Pennsylvania, Wisconsin, Ohio, Connecticut, North Carolina, Maryland, Virginia, Mississippi, Indiana, South Carolina, Nevada, Utah, Minnesota, Arkansas, Oregon, Arizona, Missouri, Alabama, District Of Columbia, Kentucky, Iowa, Maine, Rhode Island, Oklahoma, Kansas, New Hampshire, New Mexico, Hawaii, Vermont, Nebraska, Delaware, Diamond Princess Ship, Idaho, Montana, North Dakota, Guam, Wyoming, Puerto Rico, Alaska, Grand Princess Ship, South Dakota, West Virginia, United States Virgin Islands, Wuhan Repatriated, US Military, Northern Mariana Islands, Navajo Nation";
        
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
