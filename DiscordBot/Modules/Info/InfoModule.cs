using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DiscordBot.Modules.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

namespace DiscordBot.Modules
{
    class InfoModule
    {

        [Command("about"), Description("Learn more about myself.")]
        public async Task About(CommandContext ctx)
        {

            var author = $"{ctx.Guild.Owner.Username}#{ctx.Guild.Owner.Discriminator}";

            var embed = new DiscordEmbedBuilder()
                .WithAuthor(author, "https://www.steamcommunity.com/id/Vaan", ctx.Guild.Owner.AvatarUrl)
                .WithColor(DiscordColor.Azure)
                .WithTitle($"About {ctx.Client.CurrentUser.Username}#{ctx.Client.CurrentUser.Discriminator}")
                .WithDescription($"Hey there! I was developed by {author}, using the unofficial .NET " +
                "wrapper [DSharpPlus](https://dsharpplus.emzi0767.com/). This has been an ongoing project " +
                "since 05/09/2016. If you like this bot and would like to donate, please click on this " +
                "[link](https://www.youtube.com/watch?v=2YYNPnql9YI). Thank you!");

            await ctx.RespondAsync(embed: embed);
        }

        [Command("status"), Description("Bot status information.")]
        public async Task Status(CommandContext ctx)
        {
            var p = Process.GetCurrentProcess();
            var memoryUsed = p.WorkingSet64 / 1024 / 1024;
            var os = RuntimeInformation.OSDescription;
            var cores = $"{Environment.ProcessorCount}";

            var embed = new DiscordEmbedBuilder()
                .WithAuthor($"{ctx.Client.CurrentUser.Username}#{ctx.Client.CurrentUser.Discriminator}")
                .WithTitle("Status Information")
                .WithDescription("Learn more about my entrails!")
                .WithColor(DiscordColor.Gold)
                .AddField("Uptime", (DateTime.Now - p.StartTime).ToString("d'd:'h'h:'m'm:'s's'") + " (since " + p.StartTime.ToString("yyyy-MM-dd HH:mm:ss") + ")")
                .AddField("Running in", os)
                .AddField("Cores", cores)
                .AddField("Memory used", $"{memoryUsed}M")
                .WithFooter(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            await ctx.RespondAsync(embed: embed);
        }

        [Command("uptime"), Description("How long I've been running.")]
        public async Task Uptime(CommandContext ctx)
        {
            var p = Process.GetCurrentProcess();
            await ctx.RespondAsync("I've been running for " + (DateTime.Now - p.StartTime).ToString("d'd:'h'h:'m'm:'s's'") + " (since " + p.StartTime.ToString("yyyy-MM-dd HH:mm:ss") + ")");
        }

        [Command("server"), Aliases("guild"), Description("Information about this server.")]
        public async Task Server(CommandContext ctx)
        {
            var embed = new DiscordEmbedBuilder()
                .WithTitle(ctx.Guild.Name)
                .WithDescription("Information about this server.")
                .AddField("Created by", ctx.Guild.Owner.DisplayName)
                .AddField("Created in", ctx.Guild.CreationTimestamp.ToString())
                .AddField("Region", ctx.Guild.RegionId)
                .AddField("Users", ctx.Guild.MemberCount.ToString());

            await ctx.RespondAsync(embed: embed);
        }

        [Command("time"), Aliases(new string[]{"timezone", "timezones"}), Description("Various timezones.")]
        public async Task Timezones(CommandContext ctx)
        {
            try
            {
                var now = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
                var embed = new DiscordEmbedBuilder()
                    .WithAuthor(ctx.Client.CurrentUser.GetFullIdentifier(), null, ctx.Client.CurrentUser.AvatarUrl)
                    .WithTitle("Times around the world")
                    .WithDescription("Times are in UTC format (no summer hours)")
                    .AddField("UK / PT", (now + new TimeSpan(-1, 0, 0)).ToString("yyyy-MM-dd HH:mm:ss"), true)
                    .AddField("USA East Coast", (now + new TimeSpan(-5, 0, 0)).ToString("yyyy-MM-dd HH:mm:ss"), true)
                    .AddField("USA West Coast", (now + new TimeSpan(-8, 0, 0)).ToString("yyyy-MM-dd HH:mm:ss"), true)
                    .AddField("Japan / South Korea", (now + new TimeSpan(+8, 0, 0)).ToString("yyyy-MM-dd HH:mm:ss"), true);
                await ctx.RespondAsync(embed: embed);
            } catch(Exception e) { Console.WriteLine(e.ToString()); };
        }

        [Command("currencies"), Description("Shows current currency exchange rates. Updates daily.")]
        public async Task GetCurrencies(CommandContext ctx)
        {
            if(Currencies.lastUpdated == null || Currencies.lastUpdated < DateTime.UtcNow)
                Currencies.Update(ctx);

            if (Currencies.embed != null)
                await ctx.RespondAsync(embed: Currencies.embed);
            
        }
        
        [Command("convertcurrency"), Description("Converts one currency into another.")]
        public async Task ConvertCurrency(CommandContext ctx, double value, string currency)
        {
            try
            {
                if (Currencies.currencies == null)
                    Currencies.Update(ctx);

                if (currency.ToUpper().Equals("EUR") || Currencies.HasCurrency(currency))
                {
                    var c = currency.ToUpper().Equals("EUR") ? 1 : Currencies.currencies[currency];
                    DiscordEmbed embed = new DiscordEmbedBuilder()
                        .WithAuthor(ctx.Client.CurrentUser.GetFullIdentifier())
                        .WithTitle("Using " + currency.ToUpper() + " as a base. Last updated " + Currencies.lastUpdated)
                        .WithDescription($"Converting {value} {currency.ToUpper()}")
                        .WithFooter("Powered by http://www.ecb.europa.eu")
                        .WithColor(DiscordColor.Gold);

                    decimal exchangeRate = 1;
                    if (!currency.ToUpper().Equals("EUR"))
                    {
                        exchangeRate = 1 / Currencies.currencies[currency.ToUpper()];
                        embed = new DiscordEmbedBuilder(embed).AddField("EUR", (exchangeRate * (decimal)value).ToCurrency().ToString(), true);
                    }
                    foreach (var cur in Currencies.displayCurrencies)
                        if (cur != currency.ToUpper() && Currencies.currencies.ContainsKey(cur))
                            embed = new DiscordEmbedBuilder(embed).AddField(cur, (exchangeRate * Currencies.currencies[cur] * (decimal)value).ToCurrency().ToString(), true);
                    await ctx.RespondAsync(embed: embed);
                }
                else
                    await ctx.RespondAsync("Use one of the following currencies:\n" + Currencies.ListCurrencies());
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
        }

    }
}
