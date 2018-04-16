﻿using DiscordBot.Modules.Chat.Classes;
using DiscordBot.Modules.Classes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Imgur.API.Authentication.Impl;
using Imgur.API.Endpoints.Impl;
using Imgur.API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    class ChatModule
    {

        private static ImgurClient client;

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

            if (message == null)
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

                await ctx.RespondAsync(embed: embed);
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
            switch (value % 3)
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
            await ctx.Message.DeleteAsync();
            await ctx.TriggerTypingAsync();
            string result = "";
            foreach (var c in text.ToLowerInvariant())
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

        [Command("echo"), Description("I'll repeat what you say... to the best of my capabilities.")]
        public async Task Echo(CommandContext ctx, [RemainingText]string message)
        {
            await ctx.Message.DeleteAsync();
            await ctx.RespondAsync(message);
        }

        [Command("dab"), Description("Feels dab man.")]
        public async Task Dab(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            await ctx.RespondAsync((await ctx.Guild.GetEmojiAsync(410860287852412928)).GetDiscordName());
        }

        [Command("cookie"), Description("Gives a cookie 🍪yum🍪")]
        public async Task Cookie(CommandContext ctx, DiscordMember member)
        {
            if (ctx.Member.Id == member.Id)
            {
                await ctx.RespondAsync("You cannot send cookies to yourself, fatty!");
                return;
            }

            Program.cookies.AddCookie(ctx.Member, member);
            await ctx.RespondAsync($"🍪 {ctx.Member.DisplayName} gave {member.DisplayName} a cookie! 🍪");
        }

        [Command("cookies"), Description("How many cookies have been going around.")]
        public async Task Cookies(CommandContext ctx, DiscordMember member = null)
        {
            int given, received;
            Program.cookies.GetCookie(member, out given, out received);
            if (given == -1 && received == -1)
                await ctx.RespondAsync("That user has never sent or received cookies. Awww!");
            else
            {
                if (member == null)
                    await ctx.RespondAsync($"In this server {given} cookies have been sent!");
                else
                    await ctx.RespondAsync($"{member.DisplayName} has sent {given} cookies and received {received} cookies! " + (received > given ? "How greedy!" : "How nice!"));
            }
        }

        [Command("trbmb"), Aliases("thatreally"), Description("That really blanks my blank.")]
        public async Task ThatReally(CommandContext ctx)
        {
            using(WebClient client = new WebClient())
            {
                var result = client.DownloadString("http://api.chew.pro/trbmb");
                await ctx.RespondAsync(result.Substring(2, result.Length - 4));
            }
        }

        [Command("dog"), Aliases(new string[] { "doge", "pupper", "doggo" }), Description("woof")]
        public async Task Dog(CommandContext ctx)
        {
            //await SendImgurItem(ctx, Program.cfg.GetValue("dogimgur"));
            using (WebClient client = new WebClient())
            {
                var json = client.DownloadString(@"https://dog.ceo/api/breeds/image/random");
                var doggo = JsonConvert.DeserializeObject<Dog>(json);
                if(doggo.status.ToLower().Equals("success"))
                    await ctx.RespondAsync(doggo.message);
            }
        }

        [Command("cat"), Aliases(new string[] { "kitty" }), Description("meow")]
        public async Task Cat(CommandContext ctx)
        {
            WebClient client = null;
            try
            {
                client = new WebClient();
                var xml = client.DownloadString(@"http://thecatapi.com/api/images/get?format=xml&results_per_page=1");
                await ctx.RespondAsync(xml.Between("<url>", "</url>"));
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

        private async Task SendImgurItem(CommandContext ctx, string album)
        {
            if(client == null)
            {
                client = new ImgurClient(Program.keys.GetKey("imgurclient"), Program.keys.GetKey("imgursecret"));
            }

            var endpoint = new GalleryEndpoint(client);
            var images = await endpoint.GetGalleryAlbumAsync(album);
            var count = images.ImagesCount;
            var selected = Program.rng.Next(count);
            var image = new List<IImage>(images.Images)[selected];
            await ctx.RespondAsync(image.Animated ? image.Gifv : image.Link);
        }

    }
}
