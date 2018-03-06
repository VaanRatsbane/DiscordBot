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
            killables.Add(moduleManager);

            //Discord Client
            _discord = new DiscordClient(new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
            });

            //CommandsNext
            {
                var prefix = cfg.GetValue("prefix");
                _commands = _discord.UseCommandsNext(new CommandsNextConfiguration
                {
                    StringPrefix = prefix != null ? prefix : "!"
                });
            }

            //Interactivity
            _interactivity = _discord.UseInteractivity(new InteractivityConfiguration()
            {
                
            });

            RegisterCommands();

            await _discord.ConnectAsync();
            
            //Log channel registry
            {
                var guildText = cfg.GetValue("guild");
                var logchannelText = cfg.GetValue("logchannel");
                ulong guild, logchannel;

                if (guildText != null && logchannelText != null && ulong.TryParse(guildText, out guild) && ulong.TryParse(logchannelText, out logchannel))
                    Log.SetLogChannel((await _discord.GetGuildAsync(guild)).GetChannel(logchannel));
                else
                    Log.Warning("Couldn't load logchannel settings.");
            }

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
            _commands.RegisterCommands<BotControlModule>(); //botcontrol always loaded
            if(moduleManager.ModuleState("math")) _commands.RegisterCommands<MathModule>();
            if (moduleManager.ModuleState("admin")) _commands.RegisterCommands<AdminModule>();
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
            catch (TaskCanceledException) { } //caught when token is cancelled remotely, here to prevent crash
            catch(Exception e) //if a different exception happens, then it's time to worry
            {
                Log.Error("Task cancellation dun goofd");
                if (cfg.Debug())
                    Log.Error(e.ToString());
            }
        }

    }
}
