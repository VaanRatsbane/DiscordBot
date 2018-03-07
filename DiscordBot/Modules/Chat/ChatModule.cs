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

        [Command("choose"), Description("Choose from a list of things.")]
        public async Task Choose(CommandContext ctx, [Description("The list of choices, seperated by spaces. Multi worded choices delimited by quotes.")]params string[] choices)
        {
            await ctx.RespondAsync(choices[Program.rng.Next(choices.Length)]);
        }

        [Command("8ball"), Description("Ask me a question with a yes or no answer, and I shall tell you the secrets of the universe...")]
        public async Task EightBall(CommandContext ctx, [RemainingText]string query)
        {
            query = query.ToLowerInvariant(); //standardize answer
            int value = 0;
            foreach (var c in query)
                value += c;
            string answer;
            switch(value % 3)
            {
                case 0:
                    answer = "Yes.";
                    break;
                case 1:
                    answer = "No.";
                    break;
                case 2:
                    answer = "Maybe.";
                    break;
                default:
                    answer = "I... I do not know. What madness is this?!";
                    break;
            }
            await ctx.RespondAsync(answer);
        }

        [Command("bspeak"), Aliases("b"), Description("Speak like a true rudda.")]
        public async Task BSpeak(CommandContext ctx, [RemainingText]string text)
        {
            string result = "";
            foreach(var c in text.ToLowerInvariant())
            {
                if (c >= 97 && c <= 122)
                {
                    if (c == 98)
                        result += "🅱";
                    else
                        result += $":regional_indicator_{c}:";
                }
                else if (c >= 48 && c <= 57)
                {
                    switch (c)
                    {
                        case '0':
                            result += ":zero:";
                            break;
                        case '1':
                            result += ":one:";
                            break;
                        case '2':
                            result += ":two:";
                            break;
                        case '3':
                            result += ":three:";
                            break;
                        case '4':
                            result += ":four:";
                            break;
                        case '5':
                            result += ":five:";
                            break;
                        case '6':
                            result += ":six:";
                            break;
                        case '7':
                            result += ":seven:";
                            break;
                        case '8':
                            result += ":eight:";
                            break;
                        case '9':
                            result += ":nine:";
                            break;
                        default: break;
                    }
                }
                else
                    result += c;

                if (result.Length > 2000)
                    return;
            }

            await ctx.RespondAsync(result);
        }

    }
}
