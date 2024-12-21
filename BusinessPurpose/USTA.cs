using ISS.Common;
using ISS.Common.WebHelp;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;
using PuppeteerSharp;
using System;
using System.Text.RegularExpressions;
using System.Text;
using System.Runtime.CompilerServices;

namespace ISS.BusinessPurpose
{
    public class USTAWeb
    {
        public static string html_RegistrationStatus="//div[@class='_registration_ayfke_222']";
        public static string html_playername = ".//td[1]/strong/a";
        public static string html_eventnode = ".//td[2]/span";
        public static string html_citynode = ".//td[3]";
        public static string html_gendernode = ".//td[4]";
    }
    public class USTA
    {
        public static string urlBase = "https://playtennis.usta.com/";
        public static string urlBasePlayer = urlBase + "Competitions/gamesettennis/Tournaments/players/";
        public static List<USTATournament> ListOfTournament = new List<USTATournament>();

        
        public static async Task<string> updateWatchList(USTASearchFilter filter)
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"WatchList");
            var files = Directory.GetFiles(path, "*.txt");
            foreach (var file in files)
            {
                try
                {
                    var Prevplayers = Logger.ReadPlayersFromFile(file);// new Dictionary<string, string>();
                    var url = USTA.urlBasePlayer;
                    foreach (var player in Prevplayers)
                    {
                        var match = Regex.Match(player.Key, @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");
                        if (match.Success)
                        {
                            url += player.Key;
                        }
                    }
                    if (url.Length > USTA.urlBasePlayer.Length) // + currentTournament;
                    {
                        var players = await USTA.GetOneTournamentPlayers(url, filter);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message);
                }


            }
            await PuppeteerHelper.CloseBrowserAsync();


            return "UPDATED";
        }
        public static async Task<Dictionary<string, int>> GetOneTournamentPlayers(string url, USTASearchFilter filter)
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
            string filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"WatchList", T_filename);
            string Tournament_id = url.Substring(url.LastIndexOf('/') + 1);

            var Prevplayers = Logger.ReadPlayersFromFile(filename);// new Dictionary<string, string>();
            var players = new Dictionary<string, int>();
            players[Tournament_id] = 0;

            //if (Prevplayers.Count == 0 || !players.ContainsKey(Tournament_id))
            //{
            //    players[Tournament_id] =0;
            //}
            var rows = doc.DocumentNode.SelectNodes("//table[@id='player-list']/tbody/tr[not(@class='_rowGroup_1nqit_262')]");
            var tdCountBase = doc.DocumentNode.SelectSingleNode(USTAWeb.html_RegistrationStatus)
                .InnerText.Contains("closed", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
            foreach (var row in rows)
            {
                try
                {

                    var nameNode = row.SelectSingleNode(USTAWeb.html_playername.To_IncreaseNumber(tdCountBase));// ".//td[1]/strong/a");
                    var eventNode = row.SelectSingleNode(USTAWeb.html_eventnode.To_IncreaseNumber(tdCountBase));// ".//td[2]/span");
                    var cityNode = row.SelectSingleNode(USTAWeb.html_citynode.To_IncreaseNumber(tdCountBase));//".//td[3]");
                    var genderNode = row.SelectSingleNode(USTAWeb.html_gendernode.To_IncreaseNumber(tdCountBase));//".//td[4]");

                    if (nameNode != null && eventNode != null && cityNode != null && genderNode != null)
                    {

                        //participants.Add(participant);
                        var name = nameNode.InnerText.Trim();
                        if (!eventNode.InnerText.Contains(filter.Gender + "' " + filter.Age.FirstOrDefault().Replace("U", ""))) //Boys' 18)
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
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message);
                }
            }
            players[Tournament_id] = -(players.Count - 1);
            Logger.LogPlayersToFile(players, filename);
            LogPlayersToHTML(players, filename.Replace(".txt", ".html"));
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

        public static async Task<string> ProcessUSTASearch(USTASearchFilter filter)
        {

            string url = filter.getSearchURL();
            var manager = new PuppeteerManager();
            await manager.InitializeBrowserAsync();
            int AutoReTryCount = 0;
            AutoReTryStart:
            AutoReTryCount++;
            bool AutoReTry = false;

            var page = await manager.browser.NewPageAsync();
            page.DefaultNavigationTimeout = 60000; // 60 seconds
            try
            {
                await page.GoToAsync(url, new NavigationOptions
                {
                    Timeout = 60000, // 30 seconds
                    WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
                });
                //await page.GoToAsync(url, WaitUntilNavigation.Networkidle0);

                // Wait for a specific element to ensure the page is fully loaded
                //await page.WaitForSelectorAsync("div.v-grid-cell__content");

                var content = await page.GetContentAsync();
                #region loop through all the filters
                // Loop throug the filter levels
                foreach (var level in filter.Level)
                {
                    // Perform a click action on a specific element
                    var levelSelector = "#tournament-level_00000000-0000-0000-0000-00000000000" + level;
                    // Wait for the checkbox to be available in the DOM
                    await page.WaitForSelectorAsync(levelSelector);
                    // Check if the checkbox is already checked
                    bool isChecked = await page.EvaluateFunctionAsync<bool>($@"
    () => document.querySelector('{levelSelector}').checked");

                    // Click the checkbox if it is not checked
                    if (!isChecked)
                    {
                        await page.ClickAsync(levelSelector);
                    }
                    bool hasSearchResult = false;
                    try
                    {
                        var contentSelector = ".csa-search-results-title"; //"csa-search-no-results-title";
                                                                           // Wait for the content element to become visible
                        await page.WaitForSelectorAsync(contentSelector, new WaitForSelectorOptions { Visible = true });
                        hasSearchResult = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Error in find result no result means has result ");
                    }
                    if (hasSearchResult)
                    {
                        content = await page.GetContentAsync();
                        //get all the tournament links
                    }
                }
                #endregion

                //return content;
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                //return string.Empty;

                if (AutoReTryCount < 3)
                    AutoReTry = true;

            }
            finally
            {
                await page.CloseAsync();
            }
            if (AutoReTry)
            {
                goto AutoReTryStart;
            }

            return "ok";

        }
        public static List<USTATournament> ProcessSearchResultPage(string content, USTASearchFilter filter)
        {
            var TList = new List<USTATournament>();
            content = Mockup.getMockupFile("searchresult.txt");
            var doc = new HtmlDocument();
            doc.LoadHtml(content);
            //return "true";class="csa-search-result-item panel csa-search-result-tournament"
            var list = doc.DocumentNode.SelectNodes("//div[@class='csa-search-result-item panel csa-search-result-tournament']");// panel csa-search-result-tournament\"");
            foreach (var tnmt in list)
            {
                var location = tnmt.SelectSingleNode("//p[@class='csa-location']");
                var tnmtdetail = tnmt.SelectSingleNode("//h3/a");
                var href = tnmtdetail.GetAttributeValue("href", string.Empty);
                var match = Regex.Match(href, @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");
                if (match.Success)
                {  
                    var _dt = tnmt.SelectSingleNode("//li[@class='csa-date-v2']");
                    var _Sdate = _dt.InnerText.Split("-")[0].Split(',')[1];//Fri,Dec 27-Sat,Dec 28,2024
                    var _Edate = _dt.InnerText.Split("-")[1].Split(',')[1];//Fri,Dec 27-Sat,Dec 28,2024
                    var _year = _dt.InnerText.Split("-")[1].Split(',')[2];//Fri,Dec 27-Sat,Dec 28,2024
                    var Sdate = DateTime.Parse(_Sdate + "," + _year);
                    USTATournament obj = new USTATournament
                    {
                         ID = match.Value,
                         Level = filter.Level[0],
                         Date = _year + Sdate.Month.ToString("00") + Sdate.Day.ToString("00"),
                    };
                    Logger.Log($"Extracted GUID: {href}");
                }
                //var id=lnk.hr
            }

            var T_time = doc.DocumentNode.SelectSingleNode("//p[@class='csa-search-result-item']");//September 28 - 29, 2024
            var T_Level = doc.DocumentNode.SelectSingleNode("//h1");
            var T_location = doc.DocumentNode.SelectSingleNode("//p[@class='_location_ayfke_185']");
            var T_filename = T_Level.InnerText.Split(':')[0].Replace(" ", "_")
                + "_" + T_time.InnerText.Replace(" ", "_").Replace(",", "_")
                + "_" + T_location.InnerText.Split(',')[T_location.InnerText.Split(',').Length - 3].Trim() + ".txt";
            //string filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, T_filename);
            //string Tournament_id = url.Substring(url.LastIndexOf('/') + 1);

            //var players = Logger.ReadPlayersFromFile(filename);// new Dictionary<string, string>();

            //if (players.Count == 0 || !players.ContainsKey(Tournament_id))
            //{
            //    players[Tournament_id] = 0;
            //}
            //var rows = doc.DocumentNode.SelectNodes("//table[@id='player-list']/tbody/tr[not(@class='_rowGroup_1nqit_262')]");
            //foreach (var row in rows)
            //{
            //    try
            //    {
            //        var nameNode = row.SelectSingleNode(".//td[1]/strong/a");
            //        var eventNode = row.SelectSingleNode(".//td[2]/span");
            //        var cityNode = row.SelectSingleNode(".//td[3]");
            //        var genderNode = row.SelectSingleNode(".//td[4]");

            //        if (nameNode != null && eventNode != null && cityNode != null && genderNode != null)
            //        {
            //            //var participant = new Participant
            //            //{
            //            //    Name = nameNode.InnerText.Trim(),
            //            //    Event = eventNode.InnerText.Trim(),
            //            //    City = cityNode.InnerText.Trim(),
            //            //    Gender = genderNode.InnerText.Trim()
            //            //};

            //            //participants.Add(participant);
            //            var name = nameNode.InnerText.Trim();
            //            if (!eventNode.InnerText.Contains("Boys' 18") || players.ContainsKey(name))
            //            {
            //                continue;
            //            }
            //            var href = nameNode.GetAttributeValue("href", string.Empty);
            //            if (!string.IsNullOrEmpty(href) && Uri.IsWellFormedUriString(href, UriKind.RelativeOrAbsolute))
            //            {
            //                //players[name] = href;
            //                var url1 = href + @"&rankings-catalogId=JUNIOR_NULL_M_SEEDING_Y16_UNDER_SINGLES_NULL_NULL";
            //                var html1 = await //getContentByPuppeteer(url1);// FetchHtmlAsync(url);
            //                    PuppeteerHelper.GetContentByPuppeteerAsync(url1);
            //                var doc1 = new HtmlDocument();
            //                doc1.LoadHtml(html1);
            //                var node1 = doc1.DocumentNode.SelectSingleNode("//div[@class='v-grid-cell__content']//h4");

            //                if (node1 != null)
            //                {
            //                    players[name] = int.Parse(node1.InnerText.Trim());
            //                }
            //                Logger.LogPlayersToFile(players, filename);
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Logger.Log(ex.Message);
            //    }
            //}
            // players[Tournament_id] = -(players.Count - 1);
            //Logger.LogPlayersToFile(players, filename);
            return TList;

        }
        public static void LogPlayersToHTML(Dictionary<string, int> participants, string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    var sortedlist = participants.OrderByDescending(p => p.Value).ToList();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("<html><head><title>Players</title></head><body><table border='1'>");
                    for (int i = 0; i < sortedlist.Count(); i++)
                    {
                        var href = sortedlist[i].Key.Trim();
                        var match = Regex.Match(href, @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");
                        if (match.Success)
                        {
                            sb.Append($"<tr><td>{i}</td><td><a href='{urlBasePlayer}{sortedlist[i].Key.Trim()}'>{sortedlist[i].Key.Trim()}</a></td><td>{sortedlist[i].Value}</td></tr>");
                        }
                        else
                        {
                            sb.Append($"<tr><td>{i}</td><td>{sortedlist[i].Key.Trim()}</td><td>{sortedlist[i].Value}</td></tr>");
                            //writer.WriteLine($"{i}|{sortedlist[i].Key.Trim()} |  {sortedlist[i].Value}");
                        }
                    }
                    sb.Append("</table></body></html>");
                    writer.WriteLine(sb.ToString());
                }
            }

            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }
        }

    }
    public class USTATournament
    {
        public string Description { get; set; }
        public string Level { get; set; }
        public string Location { get; set; }
        public string Date { get; set; }
        public string URL { get; set; }
        public string ID { get; set; }
    }
}
