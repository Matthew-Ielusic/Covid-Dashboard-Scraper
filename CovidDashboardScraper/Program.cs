using CsvHelper;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace CovidDashboardScraper
{
    public class Program
    {
        static HttpClient client;
        const string URL = "https://ncov2019.live/data";
        static async Task Main(string[] args)
        {
            Console.WriteLine(DateTime.Now);
            var fileIOTask = CSVHandler.MakeHandlerAsync("../../../historical.csv");
            var scrapeTask = ScrapeAsync();
            
            List<TableRow> scrapedData = new List<TableRow>();
            HtmlDocument traverser = new HtmlDocument();
            string scrapedPage = await scrapeTask;
            traverser.LoadHtml(scrapedPage);
            string usaDataTable_xPath = "//table[@id='sortable_table_USA']";
            HtmlNodeCollection search = traverser.DocumentNode.SelectNodes(usaDataTable_xPath);
            if (search.Count != 1)
            {
                Console.WriteLine("Found " + search.Count + "tables??!");
            }
            else
            {
                HtmlNode tableBody = search[0].SelectSingleNode("tbody");
                foreach (HtmlNode row in tableBody.ChildNodes)
                {
                    if (row.Name.Equals("tr"))
                    {
                        TableRow processed = TableRow.Parse_tr_Element(row);
                        scrapedData.Add(processed);
                    }
                }
                CSVHandler fileIO = await fileIOTask;
                await fileIO.Write(scrapedData, DateTime.Now);
            }
        }

        private static async Task FromHTMLFile()
        {
            List<TableRow> scrapedData = new List<TableRow>();
            using (System.IO.StreamReader stream = new System.IO.StreamReader("./response.html"))
            {
                Task<String> tData = stream.ReadToEndAsync();
                Console.WriteLine("reading...");
                string data = await tData;
                HtmlDocument x = new HtmlDocument();
                x.LoadHtml(data);
                string usaDataTable_xPath = "//table[@id='sortable_table_USA']";
                HtmlNodeCollection search = x.DocumentNode.SelectNodes(usaDataTable_xPath);
                if (search.Count != 1)
                {
                    Console.WriteLine("Found " + search.Count + "tables??!");
                }
                else
                {
                    HtmlNode tableBody = search[0].SelectSingleNode("tbody");
                    foreach (HtmlNode row in tableBody.ChildNodes)
                    {
                        if (row.Name.Equals("tr"))
                        {
                            TableRow processed = TableRow.Parse_tr_Element(row);
                            scrapedData.Add(processed);
                        }
                    }
                }
            }
        }

        private static async Task<Dictionary<String, Int32>> ParseCSVHeaders(string path)
        {
            string header;
            using (StreamReader reader = new StreamReader(path))
            {
                header = await reader.ReadLineAsync();
            }
            Console.Out.WriteLine(header);
            string[] columns = header.Split(", ");
            Dictionary<String, Int32> output = new Dictionary<string, int>();
            for(int i = 0; i < columns.Length; i++)
            {
                output.Add(columns[i], i);
            }
            return output;
        }

        private static void WriteToFile(string data)
        {
            using (System.IO.StreamWriter file =
             new System.IO.StreamWriter(@"./response.html"))
            {
                file.WriteLine(data);
            }
        }

        private static async Task<string> ScrapeAsync()
        {
            var client = new HttpClient();
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            HttpResponseMessage response = await client.GetAsync(URL);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
    }


}
