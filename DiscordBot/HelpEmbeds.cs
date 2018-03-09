using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    class HelpEmbeds
    {

        public static DiscordEmbed help, admin, bot, chat, info, api, math, tools;

        public static async Task Initialize(DiscordGuild guild)
        {
            var authorName = $"{Program._discord.CurrentUser.Username}#{Program._discord.CurrentUser.Discriminator}";
            var authorIcon = Program._discord.CurrentUser.AvatarUrl;

            BuildHelp(authorName, authorIcon);
            BuildAdmin(authorName, authorIcon);
            BuildBot(authorName, authorIcon);
            BuildChat(authorName, authorIcon);
            BuildInfo(authorName, authorIcon);
            BuildAPI(authorName, authorIcon);
            BuildMath(authorName, authorIcon);
            BuildTools(authorName, authorIcon);

            //update
            var channel = guild.GetChannel(ulong.Parse(Program.cfg.GetValue("instructionschannel")));

            var helpMsg = await channel.GetMessageAsync(ulong.Parse(Program.cfg.GetValue("helpembed")));
            var adminMsg = await channel.GetMessageAsync(ulong.Parse(Program.cfg.GetValue("adminembed")));
            var botMsg = await channel.GetMessageAsync(ulong.Parse(Program.cfg.GetValue("botembed")));
            var chatMsg = await channel.GetMessageAsync(ulong.Parse(Program.cfg.GetValue("chatembed")));
            var infoMsg = await channel.GetMessageAsync(ulong.Parse(Program.cfg.GetValue("infoembed")));
            var apiMsg = await channel.GetMessageAsync(ulong.Parse(Program.cfg.GetValue("apiembed")));
            var mathMsg = await channel.GetMessageAsync(ulong.Parse(Program.cfg.GetValue("mathembed")));
            var toolsMsg = await channel.GetMessageAsync(ulong.Parse(Program.cfg.GetValue("toolsembed")));

            await helpMsg.ModifyAsync(content : "",embed: help);
            await adminMsg.ModifyAsync(content: "", embed: admin);
            await botMsg.ModifyAsync(content: "", embed: bot);
            await chatMsg.ModifyAsync(content: "", embed: chat);
            await infoMsg.ModifyAsync(content: "", embed: info);
            await apiMsg.ModifyAsync(content: "", embed: api);
            await mathMsg.ModifyAsync(content: "", embed: math);
            await toolsMsg.ModifyAsync(content: "", embed: tools);
        }

        private static void BuildHelp(string authorName, string authorIcon)
        {
            help = new DiscordEmbedBuilder()
                .WithAuthor(authorName, null, authorIcon)
                .WithColor(DiscordColor.PhthaloBlue)
                .WithTitle("Command groups.")
                .WithDescription("Use !help < group > to see the available commands.")
                .AddField("Groups", "admin, api, bot, chat, help, info, math, tools");
        }

        private static void BuildAdmin(string authorName, string authorIcon)
        {
            admin = new DiscordEmbedBuilder()
                .WithAuthor(authorName + " - Admin", null, authorIcon)
                .WithColor(DiscordColor.IndianRed)
                .WithTitle("Administrative commands.")
                .WithDescription("Use !help < command > to learn more.")
                .AddField("Commands", "dumplog, inviterolelink, removerolelink, prune, " +
                "softban, pardon, pardonroles, listsoftbans, wipe");
        }

        private static void BuildBot(string authorName, string authorIcon)
        {
            bot = new DiscordEmbedBuilder()
                .WithAuthor(authorName + " - BotControl", null, authorIcon)
                .WithColor(DiscordColor.SapGreen)
                .WithTitle("Commands that control the bot's behaviour.")
                .WithDescription("Use !help < command > to learn more.")
                .AddField("Commands", "setnick, setstate, setgame, setavatar, getavatar, flag, toggleflag, createflag, deleteflag, " +
                "listflags, setting, setsetting, createsetting, deletesetting, listsetting, " +
                "key, setkey, createkey, deletekey, listkeys, quit, enablemodule, disablemodule, listmodules");
        }

        private static void BuildChat(string authorName, string authorIcon)
        {
            chat = new DiscordEmbedBuilder()
                .WithAuthor(authorName + " - Chat", null, authorIcon)
                .WithColor(DiscordColor.Yellow)
                .WithTitle("Fun chat shenanigans.")
                .WithDescription("Use !help < command > to learn more.")
                .AddField("Commands", "savequote, randomquote, removequote, choose, 8ball, bspeak, echo, dab");
        }

        private static void BuildInfo(string authorName, string authorIcon)
        {
            info = new DiscordEmbedBuilder()
                .WithAuthor(authorName + " - Info", null, authorIcon)
                .WithColor(DiscordColor.Teal)
                .WithTitle("Get information on various subjects.")
                .WithDescription("Use !help < command > to learn more.")
                .AddField("Commands", "about, status, server");
        }

        private static void BuildAPI(string authorName, string authorIcon)
        {
            api = new DiscordEmbedBuilder()
                .WithAuthor(authorName + " - API", null, authorIcon)
                .WithColor(DiscordColor.Cyan)
                .WithTitle("Commands to poll information from the internet.")
                .WithDescription("Use !help < command > to learn more.")
                .AddField("Commands", "weather");
        }

        private static void BuildMath(string authorName, string authorIcon)
        {
            math = new DiscordEmbedBuilder()
                .WithAuthor(authorName + " - Math", null, authorIcon)
                .WithColor(DiscordColor.Wheat)
                .WithTitle("Do your homework.")
                .WithDescription("Use !help < command > to learn more.")
                .AddField("Commands", "calc, rolldice, decvalues, octvalues, binvalues, hexvalues");
        }

        private static void BuildTools(string authorName, string authorIcon)
        {
            tools = new DiscordEmbedBuilder()
                .WithAuthor(authorName + " - Tools", null, authorIcon)
                .WithColor(DiscordColor.Chartreuse)
                .WithTitle("Tools for the user.")
                .WithDescription("Use !help < command > to learn more.")
                .AddField("Commands", "color, uncolor, listcolors, tinyurl, lmgtfy");
        }

    }
}
