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

        public async Task ExecuteGroupAsync(CommandContext ctx, string command = null)
        {
            if (command != null && command.Length > 0)
            {
                var splices = command.Split(' ');
                await Program._commands.DefaultHelpAsync(ctx, splices);
            }
            else
                await ctx.RespondAsync(embed: HelpEmbeds.help);
        }
        
        [Command("commands")]
        public async Task PrintAll(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            if(ctx.Member.IsOwner)
                await ctx.RespondAsync(embed: HelpEmbeds.commands);
            else
            {
                var dm = await ctx.Member.CreateDmChannelAsync();
                await dm.SendMessageAsync(embed: HelpEmbeds.commands);
                await ctx.Message.DeleteAsync();
            }
        }

        [Command("admin")]
        public async Task Admin(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            await ctx.RespondAsync(embed: HelpEmbeds.admin);
        }

        [Command("api")]
        public async Task API(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            await ctx.RespondAsync(embed: HelpEmbeds.api);
        }

        [Command("bot")]
        public async Task Bot(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            await ctx.RespondAsync(embed: HelpEmbeds.bot);
        }

        [Command("chat")]
        public async Task Chat(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            await ctx.RespondAsync(embed: HelpEmbeds.chat);
        }

        [Command("info")]
        public async Task Info(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            await ctx.RespondAsync(embed: HelpEmbeds.info);
        }

        [Command("math")]
        public async Task Math(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            await ctx.RespondAsync(embed: HelpEmbeds.math);
        }

        [Command("tools")]
        public async Task Tools(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            var msg = await ctx.RespondAsync(embed: HelpEmbeds.tools);
        }

        [Command("scheduler")]
        public async Task Scheduler(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            var msg = await ctx.RespondAsync(embed: HelpEmbeds.scheduler);
        }


    }
}
