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

        // We will have to sleep for an hour.
        // Twelve hours in milliseconds = 1000 ms/s * 60 s/min * 60 min/hr * 12
        private const int FOUR_HOURS = 1000 * 60 * 60 * 4;

        public static async Task Main(string[] args)
        {
            try
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
                        throw new Exception("Found " + search.Count + "tables??!");
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
                        System.Threading.Thread.Sleep(FOUR_HOURS);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception was thrown, and this error handling is not sophisticated enough to figure out what went wrong.");
                Console.WriteLine("Here is the exception:");
                Console.WriteLine(e);
                Console.ReadLine();
                throw e;
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
