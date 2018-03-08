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

        public static Random rng;

        public static CancellationTokenSource quitToken;

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

            Log.Info("Closing...");

            await _discord.DisconnectAsync();
        }

        private static void SaveTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Save(false);
        }

        private static async Task _discord_GuildAvailable(DSharpPlus.EventArgs.GuildCreateEventArgs e)
        {
            var guildText = cfg.GetValue("guild");
            if (guildText != null && ulong.TryParse(guildText, out ulong guild) && e.Guild.Id == guild)
            {
                var members = await e.Guild.GetAllMembersAsync();
                if (members.Count == 0)
                    Log.Warning("0 members in GetAllMembersAsync");
                else
                    autoPrune.Initialize(members);
            }

            softbans.SolvePardons();
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

        private static Task _discord_PresenceUpdated(DSharpPlus.EventArgs.PresenceUpdateEventArgs e)
        {
            autoPrune.Logged(e.Member.Id);
            return Task.CompletedTask;
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
        }

        private static void Save(bool finalSave)
        {
            //kill 'em all
            foreach (var ded in killables)
                ded.Kill();

            if (finalSave)
                saveTimer.Stop();
        }

        private static void RegisterCommands()
        {
            _commands.RegisterCommands<BotControlModule>(); //botcontrol always loaded
            if(moduleManager.ModuleState("math")) _commands.RegisterCommands<MathModule>();
            if (moduleManager.ModuleState("admin")) _commands.RegisterCommands<AdminModule>();
            if (moduleManager.ModuleState("chat")) _commands.RegisterCommands<ChatModule>();
            if (moduleManager.ModuleState("info")) _commands.RegisterCommands<InfoModule>();
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
