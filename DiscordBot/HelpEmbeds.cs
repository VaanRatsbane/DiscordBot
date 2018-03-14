using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
    class HelpEmbeds
    {

        public static DiscordEmbed help, commands, admin, bot, chat, info, api, math, tools, scheduler;

        public static void Initialize(DiscordGuild guild)
        {
            var authorName = $"{Program._discord.CurrentUser.Username}#{Program._discord.CurrentUser.Discriminator}";
            var authorIcon = Program._discord.CurrentUser.AvatarUrl;

            BuildHelp(authorName, authorIcon);
            BuildCommands(authorName, authorIcon);
            BuildAdmin(authorName, authorIcon);
            BuildBot(authorName, authorIcon);
            BuildChat(authorName, authorIcon);
            BuildInfo(authorName, authorIcon);
            BuildAPI(authorName, authorIcon);
            BuildMath(authorName, authorIcon);
            BuildTools(authorName, authorIcon);
            BuildScheduler(authorName, authorIcon);
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

        private static void BuildCommands(string authorName, string authorIcon)
        {
            commands = new DiscordEmbedBuilder()
                .WithAuthor(authorName, null, authorIcon)
                .WithColor(DiscordColor.PhthaloBlue)
                .WithTitle("Commands.")
                .WithDescription("Use !help < command > to see the available commands.")
                .AddField("Admin", "dumplog, inviterolelink, removerolelink, prune, " +
                "softban, pardon, pardonroles, listsoftbans, wipe")
                .AddField("Bot", "setnick, setstate, setgame, setavatar, getavatar, flag, toggleflag, createflag, deleteflag, " +
                "listflags, setting, setsetting, createsetting, deletesetting, listsetting, " +
                "key, setkey, createkey, deletekey, listkeys, quit, enablemodule, disablemodule, listmodules")
                .AddField("Chat", "savequote, randomquote, removequote, choose, 8ball, bspeak, echo, dab, cookie, cookies")
                .AddField("Info", "about, status, server")
                .AddField("API", "weather, ff, tf2, ow, mc, reddit")
                .AddField("Math", "calc, rolldice, decvalues, octvalues, binvalues, hexvalues")
                .AddField("Tools", "color, uncolor, listcolors, tinyurl, lmgtfy, togethertube")
                .AddField("Scheduler", "remindme, listreminders, cancelreminder");
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
                .AddField("Commands", "savequote, randomquote, removequote, choose, 8ball, bspeak, echo, dab, cookie, cookies");
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
                .AddField("Commands", "weather, ff, tf2, ow, mc, reddit");
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
                .AddField("Commands", "color, uncolor, listcolors, tinyurl, lmgtfy, togethertube");
        }

        private static void BuildScheduler(string authorName, string authorIcon)
        {
            scheduler = new DiscordEmbedBuilder()
                .WithAuthor(authorName + " - Scheduler", null, authorIcon)
                .WithColor(DiscordColor.Chartreuse)
                .WithTitle("Time based notifications.")
                .WithDescription("Use !help < command > to learn more.")
                .AddField("Commands", "remindme, listreminders, cancelreminder");
        }

    }
}
