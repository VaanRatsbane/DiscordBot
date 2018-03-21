using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Hef.Math;
using DiscordBot.Modules.Math.Classes;
using System.IO;

namespace DiscordBot.Modules
{
    class MathModule
    {

        static Interpreter interpreter;

        [Command("calc"), Description("Calculates a formula. Check https://github.com/fsegaud/Hef.Math.Interpreter#annex---handled-operations for syntax.")]
        public async Task Add(CommandContext ctx, [RemainingText]string formula)
        {
            try
            {
                formula = formula.ToLower();

                if (formula.StartsWith("setv"))
                    return;

                if (interpreter == null) interpreter = new Interpreter();
                double result = interpreter.Calculate(formula);

                await ctx.RespondAsync(result.ToString());
            }
            catch
            {

            }
        }

        [Command("plot")]
        public async Task Plot(CommandContext ctx)
        {
            try
            {
                Graphs.DrawGraph(interpreter, "x * x", -10, 10, -10, 10);
                using (FileStream fs = new FileStream("tempGraph.bmp", FileMode.Open))
                {
                    await ctx.RespondWithFileAsync(fs);
                }
                File.Delete("tempGraph.bmp");
            }
            catch(Exception)
            {

            }
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
            catch (Exception)
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
