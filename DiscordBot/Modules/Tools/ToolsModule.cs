using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    class ToolsModule
    {

        readonly List<string> colors = new List<string> {"RED", "GREEN", "YELLOW", "PURPLE", "ORANGE", "WHITE", 
            "PINK", "SALMON", "CYAN"};

        [Command("color"), Aliases("colour"), Description("Assign yourself a color. Use listcolors to see the available ones.")]
        public async Task Color(CommandContext ctx, string color)
        {
            await ctx.TriggerTypingAsync();
            color = color.ToUpperInvariant();
            if (colors.Contains(color))
            {
                foreach (var role in ctx.Guild.Roles)
                    if (role.Name.ToUpperInvariant() == color)
                    {
                        await ctx.Member.GrantRoleAsync(role, "Color attribution.");
                        return;
                    }
                await ctx.RespondAsync("Something went wrong. Contact an administrator.");
            }
            else
                await ctx.RespondAsync("That is not a valid color.");
        }

        [Command("uncolor"), Aliases("uncolour"), Description("Unassign yourself a color.")]
        public async Task Uncolor(CommandContext ctx, string color)
        {
            await ctx.TriggerTypingAsync();
            color = color.ToUpperInvariant();
            if (colors.Contains(color))
            {
                foreach (var role in ctx.Member.Roles)
                    if (role.Name.ToUpperInvariant() == color)
                    {
                        await ctx.Member.RevokeRoleAsync(role, "Color removal.");
                        return;
                    }
                await ctx.RespondAsync("You don't have a color to remove.");
            }
            else
                await ctx.RespondAsync("That is not a valid color.");
        }

        [Command("listcolors"), Aliases("listcolours"), Description("List the available colours.")]
        public async Task ListColors(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await ctx.RespondAsync("Available colors:");
            await ctx.RespondAsync(String.Join(' ', colors));
        }

        [Command("tinyurl"), Description("Shorten a link.")]
        public async Task TinyUrl(CommandContext ctx, [RemainingText]string url)
        {
            await ctx.TriggerTypingAsync();
            try
            {
                using(WebClient client = new WebClient())
                {
                    string tiny = client.DownloadString("http://tinyurl.com/api-create.php?url=" + url);
                    await ctx.RespondAsync($"<{tiny}>");
                }
            }
            catch(Exception)
            {
                await ctx.RespondAsync("That is not a valid link.");
            }
        }

        [Command("lmgtfy"), Aliases("helpjas"), Description("I'll do the hard work for you.")]
        public async Task LMGTFY(CommandContext ctx, [RemainingText]string url)
        {
            await ctx.TriggerTypingAsync();
            await TinyUrl(ctx, "http://letmegooglethatforyou.com/?q="+url);
        }

        //Nadeko
        [Command("togethertube"), Aliases("totube"), Description("Watch videos with your friends!")]
        public async Task ToTube(CommandContext ctx, [RemainingText]string url)
        {
            await ctx.TriggerTypingAsync();
            Uri uri;
            using (var client = new HttpClient())
            {
                var res = await client.GetAsync("https://togethertube.com/room/create").ConfigureAwait(false);
                uri = res.RequestMessage.RequestUri;
            }

            var embed = new DiscordEmbedBuilder()
                .WithAuthor("TogetherTube", "https://togethertube.com/", "https://togethertube.com/assets/img/favicons/favicon-32x32.png")
                .WithDescription("You're okay, get in!\n" + uri);

            await ctx.RespondAsync(embed: embed);
        }

    }
}
