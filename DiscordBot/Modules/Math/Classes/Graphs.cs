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

        public static void DrawGraph(Interpreter interpreter, string formula, int xMin, int xMax, int yMin, int yMax)
        {
            int width = 500;
            int height = 500;

            using (Image<Rgba32> image = new Image<Rgba32>(width, height))
            {
                image.Mutate(x => x
                    .BackgroundColor(Rgba32.White));
                
                if (yMin < 0 && yMax > 0) //draw horizontal axis if in view
                {
                    var horizontalLine = new PointF[] { new PointF(0, height - (-yMin * height / (yMax - yMin))) };
                    image.Mutate(x => x
                        .DrawLines(Rgba32.Black, 1, horizontalLine));
                }

                if(xMin < 0 && xMax > 0) //draw vertical axis if in view
                {
                    var verticalLine = new PointF[] { new PointF(0, width - (-xMin * width / (xMax - xMin))) };
                    image.Mutate(x => x
                        .DrawLines(Rgba32.Black, 1, verticalLine));
                }

                for(int i = 0; i < width; i++) //draw graph
                {
                    var x = i * width / (xMax - xMin);
                    var y = (int)interpreter.Calculate(formula.Replace("x", x.ToString()));
                    var j = y * (yMax - yMin) / height;

                    image[i, j] = Rgba32.Blue;
                }

                using (var fs = new FileStream("tempGraph.bmp", FileMode.Append))
                {
                    image.Save(fs, ImageFormats.Bmp);
                }

            }

        }

    }
}
