using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot
{
    class Log
    {

        private static readonly object ConsoleWriterLock = new object();
        private static DiscordChannel _logChannel;

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
            lock (ConsoleWriterLock)
            {
                Console.ForegroundColor = color;
                Console.WriteLine("[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "] " + text);
                Console.ForegroundColor = infoColor;
            }

            if(_logChannel != null)
                _logChannel.SendMessageAsync(text);
        }

    }
}
