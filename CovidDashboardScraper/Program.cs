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
        private const string FILE_PATH = "../../../historical.csv";

        static async Task Main(string[] args)
        {
            try
            {
                var fileIOTask = CSVHandler.MakeHandlerAsync(FILE_PATH);
                var scrapeTask = ScrapeAsync();

                List<TableRow> scrapedData = new List<TableRow>();
                HtmlDocument traverser = new HtmlDocument();
                string scrapedPage = await scrapeTask;
                traverser.LoadHtml(scrapedPage);
                string usaDataTable_xPath = "//table[@id='sortable_table_usa']";
                HtmlNodeCollection search = traverser.DocumentNode.SelectNodes(usaDataTable_xPath);
                if (search.Count != 1)
                {
                    throw new Exception("Found " + search.Count + "tables??!");
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
                    Console.WriteLine("Done.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception was thron, and this error handling is not sophisticated enough to figure out what went wrong.");
                Console.WriteLine("Here is the exception:");
                Console.WriteLine(e);
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
