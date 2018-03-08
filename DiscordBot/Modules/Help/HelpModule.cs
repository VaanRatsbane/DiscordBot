using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    [Group("help", CanInvokeWithoutSubcommand = true), Description("List the different command types.")]
    class HelpModule
    {

        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            await ctx.RespondAsync(embed: HelpEmbeds.help);
        }

        [Command("admin")]
        public async Task Admin(CommandContext ctx)
        {
            await ctx.RespondAsync(embed: HelpEmbeds.admin);
        }

        [Command("bot")]
        public async Task Bot(CommandContext ctx)
        {
            await ctx.RespondAsync(embed: HelpEmbeds.bot);
        }

        [Command("chat")]
        public async Task Chat(CommandContext ctx)
        {
            await ctx.RespondAsync(embed: HelpEmbeds.chat);
        }

        [Command("info")]
        public async Task Info(CommandContext ctx)
        {
            await ctx.RespondAsync(embed: HelpEmbeds.info);
        }


    }
}
