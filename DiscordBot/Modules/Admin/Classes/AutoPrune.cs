using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Timers;
using System.IO;
using Newtonsoft.Json;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace DiscordBot.Modules.Classes
{
    class AutoPrune : Killable
    {

        static ConcurrentDictionary<ulong, DateTime> lastLogins;
        Timer timer;

        const string LASTLOGINS_FILE = "Files\\Admin\\lastlogins.json";

        public AutoPrune()
        {
            if(Program.cfg.GetValue("prunelimit") == null)
            {
                Program.cfg.SetValue("prunelimit", 30.ToString());
            }

            timer = new Timer();
            timer.Interval = 1800000; //every 30 minutes, check
            timer.Elapsed += OnTimedEvent;
            timer.AutoReset = true;

            try
            {
                var json = File.ReadAllText(LASTLOGINS_FILE);
                lastLogins = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, DateTime>>(json);
            }
            catch(Exception e)
            {
                lastLogins = new ConcurrentDictionary<ulong, DateTime>();
            }
        }

        public void Kill()
        {
            try
            {
                timer.Stop();

                if (!Directory.Exists("Files\\Admin"))
                    Directory.CreateDirectory("Files\\Admin");

                var json = JsonConvert.SerializeObject(lastLogins, Formatting.Indented);
                File.WriteAllText(LASTLOGINS_FILE, json);
            }
            catch(Exception e)
            {
                Log.Error("Failed to save lastLogins.");
                if (Program.cfg.Debug())
                    Log.Error(e.ToString());
            }
        }

        //Removes non existant users, adds new ones
        public void Initialize(IReadOnlyCollection<DiscordMember> members)
        {
            foreach (var m in members)
                if (m.Presence != null && m.Presence.Status != UserStatus.Offline)
                    lastLogins[m.Id] = DateTime.Now;
            
            timer.Start();
        }

        public void Logged(ulong memberID)
        {
            lastLogins[memberID] = DateTime.Now;
        }

        public static async Task<int> Prune()
        {
            double.TryParse(Program.cfg.GetValue("prunelimit"), out double dayLimit);
            int kicked = 0;

            if (dayLimit < 15)
            {
                Log.Warning("Prune limit is less than 15 days! Notify bot owner.");
                return -1;
            }

            List<ulong> offenders = new List<ulong>();

            foreach (var pair in lastLogins)
                if ((DateTime.Now - pair.Value).TotalDays > dayLimit)
                    offenders.Add(pair.Key);

            if (offenders.Count > 0)
            {
                var guild = await Program._discord.GetGuildAsync(ulong.Parse(Program.cfg.GetValue("discord")));
                foreach (var offender in offenders)
                {
                    var member = await guild.GetMemberAsync(offender);
                    if (member.Presence.Status != UserStatus.Offline)
                        lastLogins[offender] = DateTime.Now;
                    else
                    {
                        await guild.RemoveMemberAsync(member, "Automatically pruned for being offline for " + dayLimit + " days.");
                        lastLogins.TryRemove(offender, out var disposable);
                        kicked++;
                    }
                }

            }
            return kicked;
        }

        private static async void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            Log.Info("Scheduled pruning...");

            int kicked = await Prune();

            if (kicked > 0)
                Log.Info("Scheduled pruning removed " + kicked + " member(s).");
        }

    }
}
