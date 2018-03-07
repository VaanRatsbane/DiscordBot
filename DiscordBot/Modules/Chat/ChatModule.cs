using DiscordBot.Modules.Classes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    [Group("chat"), Description("Fun chat shenanigans.")]
    class ChatModule
    {

        [Command("savequote"), Description("Saves a message for posterity.")]
        public async Task SaveQuote(CommandContext ctx, ulong messageId = 0, ulong channelId = 0)
        {
            DiscordChannel channel;
            DiscordMessage message;
            if (channelId == 0)
                channel = ctx.Channel;
            else
            {
                channel = ctx.Guild.GetChannel(channelId);
                if (channel == null)
                {
                    await ctx.RespondAsync("That channel does not exist.");
                    return;
                }
            }

            if (messageId == 0)
                message = (await ctx.Channel.GetMessagesAsync(1))[0];
            else
                message = await channel.GetMessageAsync(messageId);

            if(message == null)
            {
                await ctx.RespondAsync("That message does not exist.");
            }
            else
            {
                Program.quotes.Add(message);
                await ctx.RespondAsync("👌");
            }
        }

        [Command("randomquote"), Description("Gets a random quote.")]
        public async Task RandomQuote(CommandContext ctx)
        {
            Quote quote = Program.quotes.RandomQuote();
            if (quote == null)
                await ctx.RespondAsync("There are no saved quotes yet!");
            else
            {
                var member = await ctx.Guild.GetMemberAsync(quote.memberId);
                var embed = new DiscordEmbedBuilder()
                    .WithAuthor(member != null ? member.DisplayName : quote.username, icon_url: member != null ? member.AvatarUrl : null)
                    .WithFooter(quote.date + " | " + quote.messageId)
                    .WithDescription(quote.message);

                await ctx.RespondAsync(embed:embed);
            }
        }

        [Command("removequote"), Description("Remove a quote."), RequireOwner]
        public async Task RemoveQuote(CommandContext ctx, ulong messageId)
        {
            if (Program.quotes.RemoveQuote(messageId))
                await ctx.RespondAsync("👌");
            else
                await ctx.RespondAsync("🤷");
        }

    }
}
