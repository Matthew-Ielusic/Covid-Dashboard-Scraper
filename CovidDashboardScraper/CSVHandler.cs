using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CovidDashboardScraper
{
    class CSVHandler
    {
        private string path;
        private List<String> stateNames;
        public static async Task<CSVHandler> MakeHandlerAsync(String path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                Task<String> task = reader.ReadLineAsync();
                CSVHandler output = new CSVHandler();
                output.path = path;
                string header = await task;

                output.stateNames = new List<String>(header.Split(", "));
                output.stateNames.Remove("Date");
                return output;
            }
        }

        public async Task Write(IEnumerable<TableRow> data, DateTime timestamp)
        {
            Dictionary<String, Int32> dataIndexer = new Dictionary<String, Int32>();
            foreach (var d in data)
            {
                dataIndexer.Add(d.Name, d.Confirmed);
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
                await writer.WriteLineAsync(line);
            }
        }
    }
}
