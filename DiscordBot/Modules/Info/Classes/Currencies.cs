using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DiscordBot.Modules.Classes
{
    public static class Currencies
    {

        public static DateTime lastUpdated;
        public static DiscordEmbed embed;
        public static readonly List<string> displayCurrencies = new List<string>() { "USD", "JPY", "GBP", "CNY" };
        public static Dictionary<string, decimal> currencies;

        public static void Update(CommandContext ctx)
        {
            WebClient client = null;
            try
            {
                client = new WebClient();
                var xml = client.DownloadString(@"http://www.ecb.europa.eu/stats/eurofxref/eurofxref-daily.xml");
                var result = xml.XmlDeserializeFromString(typeof(Envelope)) as Envelope;

                embed = new DiscordEmbedBuilder()
                    .WithAuthor(ctx.Client.CurrentUser.GetFullIdentifier(), null, ctx.Client.CurrentUser.AvatarUrl)
                    .WithTitle("Currency Exchange")
                    .WithDescription("Using Euro € as base. Last updated " + result.Cube.Cube1.time)
                    .WithFooter("Powered by http://www.ecb.europa.eu")
                    .WithColor(DiscordColor.Gold);

                currencies = new Dictionary<string, decimal>();
                foreach (var cube in result.Cube.Cube1.Cube)
                {
                    if (displayCurrencies.Contains(cube.currency))
                        embed = new DiscordEmbedBuilder(embed).AddField(cube.currency, cube.rate.ToString(), true);
                    currencies.Add(cube.currency.ToUpper(), cube.rate);
                }

                lastUpdated = result.Cube.Cube1.time;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (client != null)
                    client.Dispose();
            }
        }

        public static bool HasCurrency(string currency)
        {
            return currencies.ContainsKey(currency.ToUpper());
        }
        
        public static string ListCurrencies()
        {
            return String.Join(' ', displayCurrencies);
        }

    }
}
