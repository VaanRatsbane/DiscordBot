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
                .AddField("Uptime", (DateTime.Now - p.StartTime).ToString("yyyy-MM-dd HH:mm:ss") + " (since " + p.StartTime.ToString("yyyy-MM-dd HH:mm:ss") + ")")
                .AddField("Running in", os)
                .AddField("Cores", cores)
                .AddField("Memory used", $"{memoryUsed}M")
                .WithFooter(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            await ctx.RespondAsync(embed: embed);
        }

    }
}
