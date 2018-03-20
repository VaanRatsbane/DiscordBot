using DiscordBot.Modules;
using DiscordBot.Modules.Classes;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
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
        public static InviteRoles inviteRoles;
        public static AutoPrune autoPrune;
        public static Softbans softbans;
        public static Quotes quotes;
        public static CookieManager cookies;
        public static SchedulerManager scheduler;

        public static Random rng;

        public static CancellationTokenSource quitToken;
        public static bool reboot = false;

        private static System.Timers.Timer saveTimer;

        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            Log.Info("Booting...");
            Load(); //Load files

            //Discord Client
            _discord = new DiscordClient(new DiscordConfiguration
            {
                Token = keys.GetKey("discord"),
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true
            });

            //CommandsNext
            {
                var prefix = cfg.GetValue("prefix");
                if (prefix == null)
                    cfg.CreateValue("prefix", "!");
                _commands = _discord.UseCommandsNext(new CommandsNextConfiguration
                {
                    StringPrefix = prefix != null ? prefix : "!",
                    EnableDefaultHelp = false
                });
            }

            //Interactivity
            _interactivity = _discord.UseInteractivity(new InteractivityConfiguration()
            {
                
            });

            RegisterCommands();

            //Events
            _discord.PresenceUpdated += _discord_PresenceUpdated;
            _discord.GuildMemberAdded += _discord_GuildMemberAdded;
            _discord.GuildRoleDeleted += _discord_GuildRoleDeleted;
            _discord.ChannelDeleted += _discord_ChannelDeleted;
            _discord.GuildAvailable += _discord_GuildAvailable;

            rng = new Random();

            await _discord.ConnectAsync();

            //Log channel registry and InviteRoles/AutoPrune Init
            {
                var guildText = cfg.GetValue("guild");
                var logchannelText = cfg.GetValue("logchannel");

                if (guildText != null && ulong.TryParse(guildText, out ulong guild))
                {
                    var guildObj = await _discord.GetGuildAsync(guild);

                    inviteRoles.Initialize(await guildObj.GetInvitesAsync()); //take advantage of having guild obj
                    
                    if (logchannelText != null && ulong.TryParse(logchannelText, out ulong logchannel))
                        Log.SetLogChannel(guildObj.GetChannel(logchannel));
                    else
                        Log.Warning("Couldn't load logchannel settings.");
                }
            }

            quitToken = new CancellationTokenSource();

            saveTimer = new System.Timers.Timer();
            saveTimer.Interval = 3600000; //save every hour
            saveTimer.AutoReset = true;
            saveTimer.Elapsed += SaveTimer_Elapsed;
            saveTimer.Start();

            Log.Info("Fully loaded.");
            await TaskDelay(quitToken.Token);

            Save(true);

            if (reboot)
                Log.Info("Rebooting...");
            else
                Log.Info("Closing...");

            try
            {
                await _discord.DisconnectAsync();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadKey();
            }

            if (reboot)
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    WorkingDirectory = Environment.CurrentDirectory,
                    FileName = "dotnet",
                    Arguments = "run"
                };
                System.Diagnostics.Process.Start(startInfo);
            }

            Environment.Exit(0);
        }

        private static void SaveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Save(false);
        }

        private static async Task _discord_GuildAvailable(DSharpPlus.EventArgs.GuildCreateEventArgs e)
        {
            try
            {
                var guildText = cfg.GetValue("guild");
                if (guildText != null && ulong.TryParse(guildText, out ulong guild) && e.Guild.Id == guild)
                {
                    var members = await e.Guild.GetAllMembersAsync();
                    if (members.Count == 0)
                        Log.Warning("0 members in GetAllMembersAsync");
                    else
                        autoPrune.Initialize(members);

                    await scheduler.SolveReminders();
                    scheduler.SetReminderTimer();
                    await _discord.UpdateStatusAsync(game: new DiscordGame($"{cfg.GetValue("prefix")}help to learn more."));
                }

                softbans.SolvePardons();
                HelpEmbeds.Initialize(e.Guild); //Setup help embeds
            }
            catch(Exception exc)
            {
                Log.Error(exc.ToString());
            }
        }

        private static Task _discord_ChannelDeleted(DSharpPlus.EventArgs.ChannelDeleteEventArgs e)
        {
            inviteRoles.RemoveChannel(e.Channel.Id);
            return Task.CompletedTask;
        }

        private static Task _discord_GuildRoleDeleted(DSharpPlus.EventArgs.GuildRoleDeleteEventArgs e)
        {
            inviteRoles.RemoveRole(e.Role.Id);
            return Task.CompletedTask;
        }

        private static async Task _discord_GuildMemberAdded(DSharpPlus.EventArgs.GuildMemberAddEventArgs e)
        {
            var ban = softbans.GetBan(e.Member);
            if (ban != null)
            {
                var softbanRole = e.Guild.GetRole(ulong.Parse(cfg.GetValue("softbanrole")));
                await e.Member.ReplaceRolesAsync(new List<DiscordRole>() { softbanRole }, "Rejoined while still softbanned, to the woodshed with thee!");
            }
            else
            {
                var invites = await e.Guild.GetInvitesAsync();
                var roleID = inviteRoles.UpdateUsages(invites);
                if (roleID != 0)
                {
                    var role = e.Guild.GetRole(roleID);
                    await e.Member.GrantRoleAsync(role, "Auto assigned by bot.");
                }
            }
        }

        private static async Task _discord_PresenceUpdated(DSharpPlus.EventArgs.PresenceUpdateEventArgs e)
        {
            try
            {
                autoPrune.Logged(e.Member.Id);

                await TwitchNotifs.AddNotificationAsync(e);
                await Task.CompletedTask;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void Load()
        {
            killables = new List<Killable>();

            //Always load config first
            cfg = new ConfigLoader();
            killables.Add(cfg);

            //And keys second
            keys = new Keys();
            killables.Add(keys);

            //Load modules
            moduleManager = new ModuleManager();
            killables.Add(moduleManager);

            //Load invite role links
            inviteRoles = new InviteRoles();
            killables.Add(inviteRoles);

            //Auto Pruning
            autoPrune = new AutoPrune();
            killables.Add(autoPrune);

            //Softbans
            softbans = new Softbans();
            killables.Add(softbans);

            //Quotes
            quotes = new Quotes();
            killables.Add(quotes);

            //Cookies
            cookies = new CookieManager();
            killables.Add(cookies);

            //Scheduler
            scheduler = new SchedulerManager();
            killables.Add(scheduler);

        }

        private static void Save(bool finalSave)
        {
            if (finalSave)
            {
                saveTimer.Stop();
                foreach (var ded in killables)
                    ded.Kill();
            }
            else
            {
                foreach (var kindaalive in killables)
                    kindaalive.Save();
            }
        }

        private static void RegisterCommands()
        {
            _commands.RegisterCommands<BotControlModule>(); //botcontrol always loaded
            _commands.RegisterCommands<HelpModule>(); //help always loaded
            if(moduleManager.ModuleState("math")) _commands.RegisterCommands<MathModule>();
            if (moduleManager.ModuleState("admin")) _commands.RegisterCommands<AdminModule>();
            if (moduleManager.ModuleState("chat")) _commands.RegisterCommands<ChatModule>();
            if (moduleManager.ModuleState("info")) _commands.RegisterCommands<InfoModule>();
            if (moduleManager.ModuleState("api")) _commands.RegisterCommands<APIModule>();
            if (moduleManager.ModuleState("tools")) _commands.RegisterCommands<ToolsModule>();
            if (moduleManager.ModuleState("scheduler")) _commands.RegisterCommands<SchedulerModule>();
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
