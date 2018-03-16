using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Geocoding;
using Geocoding.Google;
using DarkSkyApi;
using DarkSkyApi.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using System.Net;
using DiscordBot.Modules.API.Classes;
using Newtonsoft.Json;
using System.Linq;
using SourceQuery;

namespace DiscordBot.Modules
{
    class APIModule
    {

        [Command("weather"), Description("Gets weather info on a given location.")]
        public async Task Weather(CommandContext ctx, [RemainingText]string location)
        {
            IGeocoder geocoder = new GoogleGeocoder() { ApiKey = Program.keys.GetKey("googlemaps") };
            IEnumerable<Address> addresses = await geocoder.GeocodeAsync(location.ToLowerInvariant());

            Address address = null;

            foreach(var item in addresses)
            {
                address = item;
                break;
            }
            
            if (address != null)
            {
                var darksky = new DarkSkyService(Program.keys.GetKey("darksky"));
                Forecast result = await darksky.GetWeatherDataAsync(address.Coordinates.Latitude, address.Coordinates.Longitude, Unit.SI);

                string url = Program.cfg.GetValue($"weather-{result.Currently.Icon.ToLowerInvariant()}");
                if (url == null) { url = ""; Console.WriteLine("Oops"); };

                string colorCode;

                var temp = result.Currently.Temperature;
                if (temp > 40)
                    colorCode = "ff0000";
                else if (temp > 30)
                    colorCode = "ff7f00";
                else if (temp > 20)
                    colorCode = "ffff00";
                else if (temp > 10)
                    colorCode = "7fff00";
                else if (temp > 0)
                    colorCode = "00ff7f";
                else if (temp > 10)
                    colorCode = "007fff";
                else
                    colorCode = "0000ff";

                var embed = new DiscordEmbedBuilder()
                    .WithAuthor("Powered by Dark Sky", "https://darksky.net", "https://darksky.net/images/app/logo.png")
                    .WithTitle($"Weather in {address.FormattedAddress}")
                    .WithDescription(result.Currently.Summary)
                    .AddField("Temperature", result.Currently.Temperature.ToString() + " Celsius")
                    .AddField("Wind Speed", result.Currently.WindSpeed.ToString() + "Meters / second")
                    .WithThumbnailUrl(url)
                    .WithColor(new DiscordColor(colorCode))
                    .WithFooter($"Data as of {result.Currently.Time.ToString("yyyy-MM-dd HH:mm:ss")}");

                if (result.Currently.PrecipitationIntensity > 0)
                    embed = new DiscordEmbedBuilder(embed).AddField("Precipitation", $"{result.Currently.PrecipitationIntensity} Milimeters/hour");

                await ctx.RespondAsync(embed: embed);
            }
            else
            {
                await ctx.RespondAsync("Please redefine your query.");
            }
        }

        [Command("ff"), Aliases("finalfantasy"), Description("Search the Final Fantasy wikia.")]
        public async Task FFWiki(CommandContext ctx, [RemainingText]string query)
        {
            await GetWikiaEmbed(ctx, DiscordColor.Cyan, "finalfantasy", "Final Fantasy", "https://vignette.wikia.nocookie.net/finalfantasy/images/b/bc/Wiki.png", query);
        }

        [Command("tf2"), Aliases("teamfortress2"), Description("Search the Team Fortress 2 wikia.")]
        public async Task TF2Wiki(CommandContext ctx, [RemainingText]string query)
        {
            await GetWikiaEmbed(ctx, DiscordColor.Orange, "teamfortress", "Team Fortress 2", "https://steamuserimages-a.akamaihd.net/ugc/594789945355042791/A016D9748918786B9933858543B27EEAFFB056D2/", query);
        }

        [Command("ow"), Aliases("overwatch"), Description("Search the Overwatch wikia.")]
        public async Task OWWiki(CommandContext ctx, [RemainingText]string query)
        {
            await GetWikiaEmbed(ctx, DiscordColor.Gold, "overwatch", "Overwatch", "https://blitzesports.com/assets/img/icons/ow_light.png", query);
        }

        [Command("mc"), Aliases("minecraft"), Description("Search the Minecraft wikia.")]
        public async Task MCWiki(CommandContext ctx, [RemainingText]string query)
        {
            await GetWikiaEmbed(ctx, DiscordColor.Gold, "minecraft", "Minecraft", "", query);
        }

        private static async Task GetWikiaEmbed(CommandContext ctx, DiscordColor color, string website, string siteTitle, string siteIcon, string query)
        {
            try
            {
                string json, description;
                using (var client = new WebClient())
                {
                    json = client.DownloadString(WikiaSearch(website, query));
                    var searchRoot = JsonConvert.DeserializeObject<WikiaSearch.RootObject>(json);
                    var searchItem = searchRoot.items[0];
                    description = searchItem.Snippet;
                    json = client.DownloadString(WikiaArticle(website, searchItem.id));

                    //treat json
                    int firstCutPos = json.GetNthIndex(':', 2, false) + 1;
                    int lastCutPos = json.GetNthIndex('}', 3, true) + 1;
                    json = json.Substring(firstCutPos, lastCutPos - firstCutPos);

                    Console.WriteLine(json);

                }

                var article = JsonConvert.DeserializeObject<WikiaArticle.RootObject>(json);

                var embed = new DiscordEmbedBuilder()
                    .WithAuthor($"{siteTitle} Wikia", $"http://{website}.wikia.com", siteIcon)
                    .WithTitle(article.title)
                    .WithDescription($"http://{website}.wikia.com{article.url}\n{description}")
                    .WithColor(color);

                if (article.thumbnail != null)
                    embed = new DiscordEmbedBuilder(embed).WithImageUrl(article.thumbnail.Replace("\\", "/"));

                await ctx.RespondAsync(embed: embed);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                await ctx.RespondAsync("Couldn't find anything.");
            }
        }

        private static string WikiaSearch(string website, string query)
        {
            return $"http://{website}.wikia.com/api/v1/Search/List/?query={query}&limit=1";
        }

        private static string WikiaArticle(string website, int id)
        {
            return $"http://{website}.wikia.com/api/v1/Articles/Details/?ids={id}";
        }

        [Command("reddit"), Description("Top 3 posts of the day on any given subreddit.")]
        public async Task Leddit(CommandContext ctx, string subreddit)
        {
            WebClient client = new WebClient();
            try
            {
                var json = client.DownloadString($"https://www.reddit.com/r/{subreddit}/top/.json?limit=3&t=day");
                var root = JsonConvert.DeserializeObject<Reddit.RootObject>(json);

                var embed = new DiscordEmbedBuilder()
                    .WithAuthor("Reddit")
                    .WithTitle("Top 3 posts in " + subreddit)
                    .WithColor(DiscordColor.White);

                if(root != null && root.data != null && root.data.children != null && root.data.children.Count > 0)
                {
                    foreach (var item in root.data.children)
                        embed = new DiscordEmbedBuilder(embed).AddField(item.data.title, item.data.url);
                    await ctx.RespondAsync(embed: embed);
                }
                else
                {
                    throw new Exception();
                }

            }
            catch (Exception)
            {
                await ctx.RespondAsync("Could not get what you wanted.");
            }
            finally
            {
                client.Dispose();
            }
        }

        [Command("tf2server"), Description("Information about the TF2 server.")]
        public async Task Tf2Server(CommandContext ctx)
        {
            var tf2serveraddress = Program.cfg.GetValue("tf2serverip");
            var tf2serverport = Program.cfg.GetValue("tf2serverport");
            if (tf2serveraddress == null)
            {
                await ctx.RespondAsync("The server ip is not saved in the bot's memory, contact the owner.");
            }
            else
            {
                var gs = new GameServer(new IPEndPoint(IPAddress.Parse(tf2serveraddress), int.Parse(tf2serverport)));
                DiscordEmbed embed = new DiscordEmbedBuilder()
                    .WithAuthor(gs.Name)
                    .WithDescription((gs.VACSecured ? "" : "Not ") + "VAC secured" + (gs.RequiresPassword ? " | Requires Password" : ""))
                    .WithColor(new DiscordColor("FF6600"))
                    .AddField("Map", gs.Map)
                    .AddField("Players", $"{gs.PlayerCount}/{gs.MaximumPlayerCount}")
                    .WithFooter($"{tf2serveraddress}:{tf2serverport}");

                if(gs.PlayerCount > 0)
                {
                    List<PlayerInfo> players = gs.Players.OrderByDescending(o => o.Score).ToList();
                    string highscores = "";
                    for (int i = 0; i < players.Count && i < 3; i++)
                        highscores += $"{i} - {players[i].Score} points | {players[i].Name}\n";
                    embed = new DiscordEmbedBuilder().AddField("Top 3", highscores);
                }

                await ctx.RespondAsync(embed: embed);
            }
        }

    }
}
