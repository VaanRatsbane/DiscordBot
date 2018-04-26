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
using System.Net;
using System.Drawing;

namespace DiscordBot.Modules
{
    class MathModule
    {

        static Interpreter interpreter;

        [Command("calc"), Description("Calculates a formula. Check https://github.com/fsegaud/Hef.Math.Interpreter#annex---handled-operations for syntax.")]
        public async Task Add(CommandContext ctx, [RemainingText]string formula)
        {
            await ctx.TriggerTypingAsync();
            try
            {
                formula = formula.ToLower();

                if (formula.StartsWith("setv"))
                    return;

                if (interpreter == null) interpreter = new Interpreter();
                double result = interpreter.Calculate(formula);

                await ctx.RespondAsync(result.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        [Command("graph"), Aliases("plot"), Description("Draws a 2D graph based on a given formula. Use x as your variable. Check https://github.com/fsegaud/Hef.Math.Interpreter#annex---handled-operations for syntax.")]
        public async Task Plot(CommandContext ctx, [Description("The function to render. Ex: x^2")]string formula,
                                                   [Description("The low X viewport value.")] int viewXMin = -10,
                                                   [Description("The high X viewport value.")] int viewXMax = 10,
                                                   [Description("The low Y viewport value.")] int viewYMin = -10,
                                                   [Description("The high Y viewport value.")] int viewYMax = 10)
        {
            await ctx.TriggerTypingAsync();
            try
            {
                if (interpreter == null) interpreter = new Interpreter();

                Graphs.DrawGraph(interpreter, formula, viewXMin, viewXMax, viewYMin, viewYMax);
                using (FileStream fs = new FileStream("_g.png", FileMode.Open))
                {
                    await ctx.RespondWithFileAsync(fs);
                }
                File.Delete("_g.png");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        [Command("latex"), Description("Uses latex notation to output formula images. Powered by codecogs")]
        public async Task Latex(CommandContext ctx, [RemainingText]string formula)
        {
            await ctx.TriggerTypingAsync();
            try
            {
                var url = @"http://latex.codecogs.com/gif.latex?\dpi{150} " + formula;
                using (WebClient client = new WebClient())
                {
                    var bytes = client.DownloadData(url);
                    File.WriteAllBytes("image.gif", bytes);
                }
                LatexClass.ReplaceTransparency("image.gif", Color.White).Save("image2.gif");
                using (FileStream fs = new FileStream("image2.gif", FileMode.Open))
                    await ctx.RespondWithFileAsync(fs);
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                if (File.Exists("image.gif")) File.Delete("image.gif");
                if (File.Exists("image2.gif")) File.Delete("image2.gif");
            }
        }

        [Command("rolldice"), Description("Rolls a dice between two numbers. 6sided by default.")]
        public async Task RollDice(CommandContext ctx, int start = 1, int end = 6)
        {
            await ctx.TriggerTypingAsync();
            int rnd = Program.rng.Next(start, end + start);
            await ctx.RespondAsync($"🎲{rnd}🎲");
        }

        [Command("decvalues"), Description("Gets a decimal number in other bases.")]
        public async Task DecValues(CommandContext ctx, int dec)
        {
            await ctx.TriggerTypingAsync();
            string bin = Convert.ToString(dec, 2);
            string hex = dec.ToString("X");
            string oct = Convert.ToString(dec, 8);

            var embed = new DiscordEmbedBuilder()
                .WithAuthor($"{ctx.Client.CurrentUser.GetFullIdentifier()}")
                .AddField("Decimal", dec.ToString())
                .AddField("Binary", bin)
                .AddField("Hexadecimal", hex)
                .AddField("Octal", oct);

            await ctx.RespondAsync(embed: embed);
        }

        [Command("binvalues"), Description("Gets a binary number in other bases.")]
        public async Task DecValues(CommandContext ctx, string bin)
        {
            await ctx.TriggerTypingAsync();
            try
            {
                int dec = Convert.ToInt32(bin, 2);
                string hex = dec.ToString("X");
                string oct = Convert.ToString(dec, 8);

                var embed = new DiscordEmbedBuilder()
                    .WithAuthor($"{ctx.Client.CurrentUser.GetFullIdentifier()}")
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
            await ctx.TriggerTypingAsync();
            try
            {
                hex = hex.ToUpperInvariant();
                int dec = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                string bin = Convert.ToString(dec, 2);
                string oct = Convert.ToString(dec, 8);

                var embed = new DiscordEmbedBuilder()
                    .WithAuthor($"{ctx.Client.CurrentUser.GetFullIdentifier()}")
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
            await ctx.TriggerTypingAsync();
            try
            {
                int dec = Convert.ToInt32(oct, 8);
                string bin = Convert.ToString(dec, 2);
                string hex = dec.ToString("X");

                var embed = new DiscordEmbedBuilder()
                    .WithAuthor($"{ctx.Client.CurrentUser.GetFullIdentifier()}")
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
