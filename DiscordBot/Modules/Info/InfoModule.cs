using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

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
                    .AddField("Japan", (now + new TimeSpan(+8, 0, 0)).ToString("yyyy-MM-dd HH:mm:ss"), true);
                await ctx.RespondAsync(embed: embed);
            } catch(Exception e) { Console.WriteLine(e.ToString()); };
        }

    }
}
