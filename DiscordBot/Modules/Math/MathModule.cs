using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NCalc;
using DSharpPlus.Entities;

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

        [Command("decvalues"), Description("Gets a decimal number in other bases.")]
        public async Task DecValues(CommandContext ctx, int dec)
        {
            string bin = Convert.ToString(dec, 2);
            string hex = dec.ToString("X");
            string oct = Convert.ToString(dec, 8);

            var embed = new DiscordEmbedBuilder()
                .WithAuthor($"{ctx.Client.CurrentUser.Username}#{ctx.Client.CurrentUser.Discriminator}")
                .AddField("Decimal", dec.ToString())
                .AddField("Binary", bin)
                .AddField("Hexadecimal", hex)
                .AddField("Octal", oct);

            await ctx.RespondAsync(embed: embed);
        }

        [Command("binvalues"), Description("Gets a binary number in other bases.")]
        public async Task DecValues(CommandContext ctx, string bin)
        {
            try
            {
                int dec = Convert.ToInt32(bin, 2);
                string hex = dec.ToString("X");
                string oct = Convert.ToString(dec, 8);

                var embed = new DiscordEmbedBuilder()
                    .WithAuthor($"{ctx.Client.CurrentUser.Username}#{ctx.Client.CurrentUser.Discriminator}")
                    .AddField("Decimal", dec.ToString())
                    .AddField("Binary", bin)
                    .AddField("Hexadecimal", hex)
                    .AddField("Octal", oct);

                await ctx.RespondAsync(embed: embed);
            }
            catch(Exception)
            {
                await ctx.RespondAsync("That is not a valid binary number.");
            }
        }

        [Command("hexvalues"), Description("Gets an hexadecimal number in other bases.")]
        public async Task HexValues(CommandContext ctx, string hex)
        {
            try
            {
                hex = hex.ToUpperInvariant();
                int dec = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                string bin = Convert.ToString(dec, 2);
                string oct = Convert.ToString(dec, 8);

                var embed = new DiscordEmbedBuilder()
                    .WithAuthor($"{ctx.Client.CurrentUser.Username}#{ctx.Client.CurrentUser.Discriminator}")
                    .AddField("Decimal", dec.ToString())
                    .AddField("Binary", bin)
                    .AddField("Hexadecimal", hex)
                    .AddField("Octal", oct);

                await ctx.RespondAsync(embed: embed);
            }
            catch (Exception)
            {
                await ctx.RespondAsync("That is not a valid binary number.");
            }
        }
                
        [Command("octvalues"), Description("Gets an octal number in other bases.")]
        public async Task OctalValues(CommandContext ctx, string oct)
        {
            try
            {
                int dec = Convert.ToInt32(oct, 8);
                string bin = Convert.ToString(dec, 2);
                string hex = dec.ToString("X");

                var embed = new DiscordEmbedBuilder()
                    .WithAuthor($"{ctx.Client.CurrentUser.Username}#{ctx.Client.CurrentUser.Discriminator}")
                    .AddField("Decimal", dec.ToString())
                    .AddField("Binary", bin)
                    .AddField("Hexadecimal", hex)
                    .AddField("Octal", oct);

                await ctx.RespondAsync(embed: embed);
            }
            catch (Exception)
            {
                await ctx.RespondAsync("That is not a valid binary number.");
            }
        }

    }
}
