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
        const string URL = "https://ncov2019.live/data";
        private const string FILE_PATH = "historical.csv";
        private const string usaDataTable_xPath = "//table[@id='sortable_table_mobile_unitedstates']";

        // One hour in milliseconds = 1000 ms/s * 60 s/min * 60 min/hr
        private const int FOUR_HOURS = 4 * (1000 * 60 * 60);

        public static async Task Main(string[] args)
        {
            try
            {
                await MainLoop();
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an exception in parsing the HTML.");
                Console.WriteLine("The exception was:");
                Console.WriteLine(e);
                Console.WriteLine();
                Console.WriteLine("Press any key to abort...");
                Console.Read();
                throw;
            }
        }

        private static async Task MainLoop()
        {
            while (true)
            {
                Task<string> scrapeTask = ScrapeAsync();

                if (!File.Exists(FILE_PATH))
                {
                    CSVHandler.Create(FILE_PATH);
                }

                string scrapedPage = await scrapeTask;
                HtmlNodeCollection search = FindUsaData(scrapedPage);
                if (search.Count != 1)
                {
                    throw new FormatException("Found " + search.Count + "tables??!");
                }
                else
                {
                    HtmlNode tableBody = search[0].SelectSingleNode("tbody");

                    List<TableRow> processedHTML = new List<TableRow>();
                    foreach (HtmlNode row in tableBody.ChildNodes)
                    {
                        if (row.Name.Equals("tr"))
                        {
                            TableRow processed = TableRow.Parse_tr_Element(row);
                            processedHTML.Add(processed);
                        }
                    }

                    CSVHandler handler = CSVHandler.MakeHandler(FILE_PATH);
                    DateTime now = DateTime.Now;
                    handler.Write(processedHTML, now);
                    Console.WriteLine($"Finished a scraping at {now}");

                    await Task.Delay(FOUR_HOURS);
                }
            }
        }

        private static HtmlNodeCollection FindUsaData(string scrapedPage)
        {
            HtmlDocument traverser = new HtmlDocument();
            traverser.LoadHtml(scrapedPage);
            HtmlNodeCollection search = traverser.DocumentNode.SelectNodes(usaDataTable_xPath);
            return search;
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
