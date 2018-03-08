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

namespace DiscordBot.Modules
{
    class APIModule
    {

        [Command("weather"), Description("Gets weather info on a given location.")]
        public async Task Weather(CommandContext ctx, [RemainingText]string location)
        {
            Console.WriteLine("Checkpoint1");
            IGeocoder geocoder = new GoogleGeocoder() { ApiKey = Program.keys.GetKey("googlemaps") };
            IEnumerable<Address> addresses = await geocoder.GeocodeAsync(location.ToLowerInvariant());
            var e = addresses.GetEnumerator();
            Console.WriteLine("Checkpoint2");
            if (e.Current != null)
            {
                Console.WriteLine("Checkpoint3");
                var darksky = new DarkSkyService(Program.keys.GetKey("darksky"));
                Forecast result = await darksky.GetWeatherDataAsync(e.Current.Coordinates.Latitude, e.Current.Coordinates.Longitude, Unit.SI);

                Console.WriteLine("Checkpoint4");
                var embed = new DiscordEmbedBuilder()
                    .WithAuthor("Powered by Dark Sky", "https://darksky.net", "https://darksky.net/images/app/logo.png")
                    .WithTitle($"Weather in {e.Current.FormattedAddress}")
                    .WithDescription(result.Currently.Summary)
                    .AddField("Temperature", result.Currently.Temperature.ToString())
                    .AddField("Wind Speed", result.Currently.WindSpeed.ToString() + "Meters / second")
                    .WithFooter($"Data as of {result.Currently.Time.ToString("yyyy-MM-dd HH:mm:ss")}");

                if (result.Currently.PrecipitationIntensity > 0)
                    embed = new DiscordEmbedBuilder(embed).AddField("Precipitation", $"{result.Currently.PrecipitationIntensity} Milimeters/hour");

                Console.WriteLine("Checkpoint5");
                await ctx.RespondAsync(embed: embed);
            }
            else
            {
                Console.WriteLine("Checkpoint0");
                await ctx.RespondAsync("Please redefine your query.");
            }
        }

    }
}
