using System;
using System.IO;
using System.Net;
using System.Drawing;

namespace Converting
{
    class Program
    {
        static void Main(string[] args)
        {
            var formula = @"\int H(x, x')\psi(x')dx' = -\frac{\hbar^2}{2m}\frac{d^2}{dx^2} \psi(x) + V(x)\psi(x) + 1";
            var url = "http://latex.codecogs.com/gif.latex?" + formula;

            using (WebClient client = new WebClient())
            {
                var bytes = client.DownloadData(url);
                File.WriteAllBytes("image.gif", bytes);
                ReplaceTransparency("image.gif", Color.White).Save("image2.gif");
            }

            Console.WriteLine("Hello World!");
        }

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
