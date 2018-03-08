using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot
{
    class HelpEmbeds
    {

        public static DiscordEmbed help, admin, bot, chat, info, api, math;

        public static void Initialize()
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
        }

        private static void BuildHelp(string authorName, string authorIcon)
        {
            help = new DiscordEmbedBuilder()
                .WithAuthor(authorName, null, authorIcon)
                .WithColor(DiscordColor.PhthaloBlue)
                .WithTitle("Command groups.")
                .WithDescription("Use !help < group > to see the available commands.")
                .AddField("Groups", "admin, api, bot, chat, help, info, math");
        }

        private static void BuildAdmin(string authorName, string authorIcon)
        {
            admin = new DiscordEmbedBuilder()
                .WithAuthor(authorName, null, authorIcon)
                .WithColor(DiscordColor.IndianRed)
                .WithTitle("Administrative commands.")
                .WithDescription("Use !help < command > to learn more.")
                .AddField("Commands", "dumplog, inviterolelink, removerolelink, prune, " +
                "softban, pardon, pardonroles, listsoftbans, wipe");
        }

        private static void BuildBot(string authorName, string authorIcon)
        {
            bot = new DiscordEmbedBuilder()
                .WithAuthor(authorName, null, authorIcon)
                .WithColor(DiscordColor.SapGreen)
                .WithTitle("Commands that control the bot's behaviour.")
                .WithDescription("Use !help < command > to learn more.")
                .AddField("Commands", "setnick, setstate, setgame, flag, toggleflag, createflag, deleteflag, " +
                "listflags, setting, setsetting, createsetting, deletesetting, listsetting, " +
                "key, setkey, createkey, deletekey, listkeys, quit, enablemodule, disablemodule");
        }

        private static void BuildChat(string authorName, string authorIcon)
        {
            chat = new DiscordEmbedBuilder()
                .WithAuthor(authorName, null, authorIcon)
                .WithColor(DiscordColor.Yellow)
                .WithTitle("Fun chat shenanigans.")
                .WithDescription("Use !help < command > to learn more.")
                .AddField("Commands", "savequote, randomquote, removequote, choose, 8ball, bspeak");
        }

        private static void BuildInfo(string authorName, string authorIcon)
        {
            info = new DiscordEmbedBuilder()
                .WithAuthor(authorName, null, authorIcon)
                .WithColor(DiscordColor.Teal)
                .WithTitle("Get information on various subjects.")
                .WithDescription("Use !help < command > to learn more.")
                .AddField("Commands", "about, status");
        }

        private static void BuildAPI(string authorName, string authorIcon)
        {
            api = new DiscordEmbedBuilder()
                .WithAuthor(authorName, null, authorIcon)
                .WithColor(DiscordColor.Cyan)
                .WithTitle("Commands to poll information from the internet.")
                .WithDescription("Use !help < command > to learn more.")
                .AddField("Commands", "weather");
        }

        private static void BuildMath(string authorName, string authorIcon)
        {
            math = new DiscordEmbedBuilder()
                .WithAuthor(authorName, null, authorIcon)
                .WithColor(DiscordColor.Wheat)
                .WithTitle("Do your homework.")
                .WithDescription("Use !help < command > to learn more.")
                .AddField("Commands", "calc, rolldice");
        }

    }
}
