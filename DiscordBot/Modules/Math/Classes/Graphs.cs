using Hef.Math;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Brushes;
using SixLabors.ImageSharp.Drawing.Pens;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DiscordBot.Modules.Math.Classes
{
    public class Graphs
    {

        public static void DrawGraph(Interpreter interpreter, string formula, double xMin, double xMax, double yMin, double yMax)
        {
            int width = 300;
            int height = 300;

            using (Image<Rgba32> image = new Image<Rgba32>(width, height))
            {
                image.Mutate(x => x
                    .BackgroundColor(Rgba32.White));

                var points = new List<PointF>();

                for (int i = 0; i <= width; i++) //draw graph
                {
                    var x = (0.0 + xMax - xMin) / width * i + xMin;
                    var replaced = formula.Replace("x", $"({x.ToString()})").Replace("-", "±");
                    var y = interpreter.Calculate(replaced);
                    var j = -1 * (int)((y - yMax) * height / (yMax - yMin));

                    if (j >= 0 && j <= height)
                        points.Add(new PointF(i, j));
                }

                image.Mutate(x => x
                    .DrawLines(Rgba32.Blue, 1, points.ToArray()));

                if (yMin < 0 && yMax > 0) //draw horizontal axis if in view
                {
                    var horizontalLine = new PointF[] { new PointF(0, (int)(height - (-yMin * height / (yMax - yMin)))),
                                                            new PointF(500, height - (int)((-yMin * height / (yMax - yMin))))};
                    image.Mutate(x => x
                        .DrawLines(Rgba32.Black, 1, horizontalLine));
                }

                if (xMin < 0 && xMax > 0) //draw vertical axis if in view
                {
                    var verticalLine = new PointF[] { new PointF((int)(width - (-xMin * width/ (xMax - xMin))), 0),
                                                          new PointF(width - (int)((-xMin * width / (xMax - xMin))), 500)};
                    image.Mutate(x => x
                        .DrawLines(Rgba32.Black, 1, verticalLine));
                }

                using (var fs = new FileStream("_g.png", FileMode.Append))
                {
                    image.Save(fs, ImageFormats.Png);
                }

            }

        }

    }
}
