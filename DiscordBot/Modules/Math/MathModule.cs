using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NCalc;

namespace DiscordBot.Modules
{
    [Group("math"), Description("Commands that calculate mathematical operations.")]
    class MathModule
    {

        [Command("calc"), Description("Calculates a formula. Rounded to 2 decimal places. Check https://archive.codeplex.com/?p=ncalc for documentation.")]
        public async Task Add(CommandContext ctx, [RemainingText]string formula)
        {
            var expr = new Expression(formula);
            string result = expr.Evaluate().ToString();

            if(double.TryParse(result, out double number))
            {
                double rounded = Math.Round(number, 2);
                if ((rounded > 0 && rounded < number) || (rounded < 0 && rounded > number))
                    rounded += rounded > 0 ? 0.01 : -0.01;
                result = rounded.ToString();
            }
            
            await ctx.RespondAsync(result);
        }

        [Command("rolldice"), Description("Rolls a dice between two numbers. 6sided by default.")]
        public async Task RollDice(CommandContext ctx, int start = 1, int end = 6)
        {
            int rnd = Program.rng.Next(start, end + start);
            await ctx.RespondAsync($"🎲{rnd}🎲");
        }

    }
}
