using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DiscordBot.Modules.Math.Classes
{
    public class LatexClass
    {
        public static Bitmap ReplaceTransparency(string file, System.Drawing.Color background)
        {
            return ReplaceTransparency(Image.FromFile(file), background);
        }

        public static System.Drawing.Bitmap ReplaceTransparency(System.Drawing.Image image, System.Drawing.Color background)
        {
            return ReplaceTransparency((System.Drawing.Bitmap)image, background);
        }

        public static System.Drawing.Bitmap ReplaceTransparency(System.Drawing.Bitmap bitmap, System.Drawing.Color background)
        {
            /* Important: you have to set the PixelFormat to remove the alpha channel.
             * Otherwise you'll still have a transparent image - just without transparent areas */
            var result = new System.Drawing.Bitmap(bitmap.Size.Width, bitmap.Size.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            var g = System.Drawing.Graphics.FromImage(result);

            g.Clear(background);
            g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            g.DrawImage(bitmap, 0, 0);

            return result;
        }
    }
}
