
using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using PuppeteerSharp;
using System.Xml.Linq;
using System.Linq;
using Microsoft.Extensions.Configuration;
using ISS.Common;
using ISS.Common.WebHelp;
using ISS.BusinessPurpose;
using System.Text.RegularExpressions;
using ISS.Common.GoogleSheet;


// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
await WebCrawler.Main(null);
class WebCrawler
{

    private static readonly HttpClient client = new HttpClient();
    //public string T_filename { get; set; }
    public static async Task Main(string[] args)
    {
        
        try
        {
            var configuration = new ConfigurationBuilder()
                               .SetBasePath(Directory.GetCurrentDirectory())
                               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                               .Build();
                    
            var filter = configuration.GetSection("USTASearchFilter").Get<USTASearchFilter>();

            //USTA.ProcessSearchResultPage("", filter);

            var mainapp = configuration.GetSection("MainApp").Get<string>();
            if (mainapp == "Test")
            {
                GoogleSheet.test();
                return;
            }
            else if(mainapp == "PlayerUpdate")
            {
                await USTA.updateWatchList(filter);
                return;
            }
            else if (mainapp == "Player")
            {
                #region Process one tournament
                //Console.WriteLine("Enter the tournament players URL or the Tournament ID:");
                string url = "Start";// Console.ReadLine();
                while (url?.ToUpper() != "END" )// OneTournament:
                {
                    Console.WriteLine("Please type more Tournament ID or Update to run results into watchlist \n or enter END to stop:");
                    url = Console.ReadLine();
                    if(url.Equals("update", StringComparison.OrdinalIgnoreCase))//?.ToUpper() == "END")
                    {
                        await USTA.updateWatchList(filter);
                        return; 
                    }
                    if (!url.Contains(USTA.urlBasePlayer))  //string.IsNullOrEmpty(url))
                    {
                        url = USTA.urlBasePlayer + url;// "https://playtennis.usta.com/Competitions/gamesettennis/Tournaments/players/2266573C-1CB1-4BEA-BEC4-8601B404E3FB";
                    } 
                    var players = await USTA.GetOneTournamentPlayers(url, filter);
                    //if (url?.ToUpper() == "END")
                    //    await PuppeteerHelper.CloseBrowserAsync();
                    //else
                    //    goto OneTournament;
                }
                await PuppeteerHelper.CloseBrowserAsync();

                #endregion
                return;
            }
            else if(mainapp == "USTASearch")
            {
                //var filter = configuration.GetSection("USTASearchFilter").Get<USTASearchFilter>();

                await USTA.ProcessUSTASearch(filter);
                return;
            }


 

        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message);
        }
    }

    public static async Task<List<string>> CrawlAsync(string url)
    {


        var links = new List<string>();
        var html = await FetchHtmlAsync(url);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        foreach (var node in doc.DocumentNode.SelectNodes("//a[@href]"))
        {
            var href = node.GetAttributeValue("href", string.Empty);
            if (!string.IsNullOrEmpty(href) && Uri.IsWellFormedUriString(href, UriKind.RelativeOrAbsolute))
            {
                links.Add(href);
                Logger.Log(href);
            }
        }

        return links;
    }

    private static async Task<string> FetchHtmlAsync(string url)
    {
         var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");

        var response = await client.SendAsync(request);
       //var response = await client.GetAsync(url);
       // response.EnsureSuccessStatusCode();        
        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<Dictionary<string, int>> GetOneTournamentPlayers_obsolete(string url, USTASearchFilter filter)
    {

        var html = await //getContentByPuppeteer(url);// FetchHtmlAsync(url);
            PuppeteerHelper.GetContentByPuppeteerAsync(url);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var T_time = doc.DocumentNode.SelectSingleNode("//p[@class='_dates_ayfke_177']");//September 28 - 29, 2024
        var T_Level = doc.DocumentNode.SelectSingleNode("//h1");
        var T_location = doc.DocumentNode.SelectSingleNode("//p[@class='_location_ayfke_185']");
        var T_filename = T_Level.InnerText.Split(':')[0].Replace(" ", "_") 
            + "_" + T_time.InnerText.Replace(" ", "_").Replace(",", "_")
            + "_" + T_location.InnerText.Split(',')[T_location.InnerText.Split(',').Length - 3].Trim() + ".txt";
        string filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, T_filename);
        string Tournament_id =   url.Substring(url.LastIndexOf('/')+1) ; 
            
        var Prevplayers = Logger.ReadPlayersFromFile(filename);// new Dictionary<string, string>();
        var players = new Dictionary<string, int>();
        players[Tournament_id] = 0;

        //if (Prevplayers.Count == 0 || !players.ContainsKey(Tournament_id))
        //{
        //    players[Tournament_id] =0;
        //}
        var rows = doc.DocumentNode.SelectNodes("//table[@id='player-list']/tbody/tr[not(@class='_rowGroup_1nqit_262')]");
        foreach (var row in rows)   
        {
            try
            {
                var nameNode = row.SelectSingleNode(".//td[1]/strong/a");
                var eventNode = row.SelectSingleNode(".//td[2]/span");
                var cityNode = row.SelectSingleNode(".//td[3]");
                var genderNode = row.SelectSingleNode(".//td[4]");

                if (nameNode != null && eventNode != null && cityNode != null && genderNode != null)
                {
                    //var participant = new Participant
                    //{
                    //    Name = nameNode.InnerText.Trim(),
                    //    Event = eventNode.InnerText.Trim(),
                    //    City = cityNode.InnerText.Trim(),
                    //    Gender = genderNode.InnerText.Trim()
                    //};

                    //participants.Add(participant);
                    var name = nameNode.InnerText.Trim();
                    if ( !eventNode.InnerText.Contains(filter.Gender +"' " + filter.Age.FirstOrDefault().Replace("U",""))) //Boys' 18)
                    {                       
                        continue;
                    }
                    if (Prevplayers.ContainsKey(name))
                    {
                        players[name] = Prevplayers[name];
                        continue;
                    }
                    var href = nameNode.GetAttributeValue("href", string.Empty);
                    if (!string.IsNullOrEmpty(href) && Uri.IsWellFormedUriString(href, UriKind.RelativeOrAbsolute))
                    {
                        //players[name] = href;
                        var url1 = href + @"&rankings-catalogId=JUNIOR_NULL_M_SEEDING_Y16_UNDER_SINGLES_NULL_NULL";
                        var html1 = await //getContentByPuppeteer(url1);// FetchHtmlAsync(url);
                            PuppeteerHelper.GetContentByPuppeteerAsync(url1);
                        var doc1 = new HtmlDocument();
                        doc1.LoadHtml(html1);
                        var node1 = doc1.DocumentNode.SelectSingleNode("//div[@class='v-grid-cell__content']//h4");

                        if (node1 != null)
                        {
                            players[name] = int.Parse(node1.InnerText.Trim());
                        }
                        Logger.LogPlayersToFile(players, filename);
                    }
                }
            }catch(Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }
        players[Tournament_id] = -(players.Count-1);
        Logger.LogPlayersToFile(players, filename);
        return players; 

        //// XPath to select all <a> tags
        //foreach (var node in doc.DocumentNode.SelectNodes("//a"))
        //{
        //    var href = node.GetAttributeValue("href", string.Empty);
        //    if (href.Contains("https://www.usta.com/en/home/play/player-search/profile.html"))
        //    {
        //        var name = node.InnerText.Trim();
        //        if (!string.IsNullOrEmpty(href) && Uri.IsWellFormedUriString(href, UriKind.RelativeOrAbsolute))
        //        {
        //            players[name] = href;
        //            var url1 = href + @"&rankings-catalogId=JUNIOR_NULL_M_SEEDING_Y16_UNDER_SINGLES_NULL_NULL";
        //            var html1 = await getContentByPuppeteer(url1);// FetchHtmlAsync(url);
        //            var doc1 = new HtmlDocument();
        //            doc1.LoadHtml(html1);
        //            var node1 = doc1.DocumentNode.SelectSingleNode("//div[@class='v-grid-cell__content']//h4");

        //            if (node1 != null)
        //            {
        //                players[name]= node1.InnerText.Trim();
        //            }
        //        }
        //    }
        //}


    }
    public static async Task<string> getContentByPuppeteer_obsolete(string url)
    {
        url = url ?? "https://playtennis.usta.com/Competitions/gamesettennis/Tournaments/players/2266573C-1CB1-4BEA-BEC4-8601B404E3FB";

        await new BrowserFetcher().DownloadAsync();// BrowserFetcher.DefaultRevision);
        var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = false });
        var page = await browser.NewPageAsync();
        // Set a higher timeout
        page.DefaultNavigationTimeout = 60000; // 60 seconds
//        await page.SetDefaultNavigationTimeoutAsync(60000); // 60 seconds

        
        await page.GoToAsync(url, WaitUntilNavigation.Networkidle0);
        // Wait for a specific element to ensure the page is fully loaded
        //await page.WaitForSelectorAsync("div.v-grid-cell__content");


        //await page.GoToAsync(url);
        var content = await page.GetContentAsync();
         await browser.CloseAsync();
       //Logger.Log(content);
        return content;
    }


}



