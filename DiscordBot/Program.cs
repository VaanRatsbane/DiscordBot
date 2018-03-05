using DiscordBot.Modules;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot
{
    class Program
    {

        //list of objects to safely kill at the end of execution (without relying on unreliable deconstructors)
        static List<Killable> killables;

        public static DiscordClient _discord;
        public static CommandsNextModule _commands;
        public static InteractivityModule _interactivity;
        public static Keys keys;
        public static ConfigLoader cfg;
        public static ModuleManager moduleManager;

        public static CancellationTokenSource quitToken;

        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            Log.Info("Booting...");

            killables = new List<Killable>();

            //Always load config first
            cfg = new ConfigLoader();
            killables.Add(cfg);

            //And keys second
            keys = new Keys();
            killables.Add(keys);
            var token = keys.GetKey("discord");

            //Load modules
            moduleManager = new ModuleManager();

            _discord = new DiscordClient(new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
            });
            _commands = _discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = "$"
            });
            _interactivity = _discord.UseInteractivity(new InteractivityConfiguration()
            {
                
            });

            RegisterCommands();

            await _discord.ConnectAsync();

            Log.SetLogChannel((await _discord.GetGuildAsync(222498451752091650)).GetChannel(420209782654500874));

            quitToken = new CancellationTokenSource();

            Log.Info("Fully loaded.");
            await TaskDelay(quitToken.Token);

            //kill 'em all
            foreach (var ded in killables)
                ded.Kill();

            Log.Info("Closing...");

            await _discord.DisconnectAsync();
        }

        private static void RegisterCommands()
        {
            _commands.RegisterCommands<BotControlModule>();
            _commands.RegisterCommands<MathModule>();
        }

        private static async Task TaskDelay(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(1000, token);
                }
            }
            catch { }
        }

    }
}
