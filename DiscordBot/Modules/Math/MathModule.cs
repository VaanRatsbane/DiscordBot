using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    [Group("math"), Description("Commands that calculate mathematical operations.")]
    class MathModule
    {

        [Command("add")]
        public async Task Add(CommandContext ctx)
        {
            await ctx.RespondAsync("Add");
        }

    }
}
