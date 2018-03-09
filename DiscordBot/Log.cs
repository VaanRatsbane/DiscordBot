using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace DiscordBot
{
    class Log
    {

        private static readonly object ConsoleWriterLock = new object();
        private static DiscordChannel _logChannel;
        const string LOGPATH = "Files/Logs/";

        const ConsoleColor successColor = ConsoleColor.Green;
        const ConsoleColor infoColor = ConsoleColor.White;
        const ConsoleColor warningColor = ConsoleColor.Yellow;
        const ConsoleColor errorColor = ConsoleColor.Red;
        const ConsoleColor inputColor = ConsoleColor.Cyan;

        public static void SetLogChannel(DiscordChannel channel)
        {
            _logChannel = channel;
        }

        public static void Info(string text)
        {
            Print(text, infoColor);
        }

        public static void Input(string text)
        {
            Print(text, inputColor);
        }

        public static void Success(string text)
        {
            Print(text, successColor);
        }

        public static void Warning(string text)
        {
            Print(text, warningColor);
        }

        public static void Error(string text)
        {
            Print(text, errorColor);
        }

        private static void Print(string text, ConsoleColor color)
        {

            var datedText = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "] " + text;
            lock (ConsoleWriterLock)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(datedText);
                Console.ForegroundColor = infoColor;
            }

            if (_logChannel != null)
                _logChannel.SendMessageAsync(text);

            if (Program.cfg != null && Program.cfg.FileLogging())
            {
                var now = DateTime.Now;
                var path = LOGPATH + now.Year + "/" + now.Month;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                File.AppendAllLines(path + "/" + now.Day + ".txt", new string[]{datedText + "\n"});
            }
        }

        public static FileStream GetLogFile(int year, int month, int day)
        {
            var filepath = LOGPATH + year + "/" + month + "/" + day + ".txt";
            if (File.Exists(filepath))
                return new FileStream(filepath, FileMode.Open);
            else
                return null;
        }

        public static FileStream GetLogZip(int year, int month)
        {
            try
            {
                string dirpath;
                if (month != -1)
                    dirpath = LOGPATH + year + "/" + month;
                else
                    dirpath = LOGPATH + year;

                if (Directory.Exists(dirpath))
                {
                    var zipfilepath = Path.GetTempPath() + "VaanDiscordBot/";
                    var filepath = "LOG -" + year + (month != -1 ? "-" + month : "") + ".zip";

                    if (!Directory.Exists(zipfilepath))
                        Directory.CreateDirectory(zipfilepath);

                    ZipFile.CreateFromDirectory(dirpath, zipfilepath+filepath);
                    return new FileStream(zipfilepath+filepath, FileMode.Open);
                }
                else
                    return null;
            }
            catch(Exception e)
            {
                Log.Warning("Failed to get log zip.");
                if (Program.cfg.Debug())
                    Log.Warning(e.ToString());
                return null;
            }
        }

        public static void CleanTempZip()
        {
            Directory.Delete(Path.GetTempPath() + "VaanDiscordBot", true);
        }

    }
}
