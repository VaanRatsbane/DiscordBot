using DSharpPlus.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Timers;
using System.IO;
using Newtonsoft.Json;

namespace DiscordBot.Modules.Classes
{
    class Softbans : Killable
    {

        ConcurrentDictionary<ulong, Softban> softbans;
        ulong nextUnbanID; //if 0 there is no next
        Timer unbanTimer;

        const string SOFTBANS_FILE = "Files/Admin/softbans.json";

        public Softbans()
        {
            try
            {
                var json = File.ReadAllText(SOFTBANS_FILE);
                softbans = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, Softban>>(json);
            }
            catch(Exception e)
            {
                Log.Error("Could not load softbans.");
                if (Program.cfg.Debug())
                    Log.Error(e.ToString());
                softbans = new ConcurrentDictionary<ulong, Softban>();
            }

            unbanTimer = new Timer();
            unbanTimer.Elapsed += OnTimedEvent;
            unbanTimer.AutoReset = false;
        }

        public void Kill()
        {
            unbanTimer.Stop();
            Save();
        }

        public void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(softbans, Formatting.Indented);
                File.WriteAllText(SOFTBANS_FILE, json);
            }
            catch (Exception e)
            {
                Log.Error("Could not save softbans.");
                if (Program.cfg.Debug())
                    Log.Error(e.ToString());
            }
        }

        private async void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            var guildText = Program.cfg.GetValue("guild");
            var guild = ulong.Parse(guildText);
            var guildObj = await Program._discord.GetGuildAsync(guild);
            var member = await guildObj.GetMemberAsync(nextUnbanID);

            if (member != null)
                Pardon(member);
            else //remove softban even if member isn't present
                softbans.Remove(nextUnbanID, out var ban);

            CalculateNextUnban();
            NextTimer();
        }

        /// <summary>
        /// Stops the timer, lifts every ban that has passed, and sets the timer again. For when the bot launches or goes down.
        /// </summary>
        public async void SolvePardons()
        {
            unbanTimer.Stop();
            var guildText = Program.cfg.GetValue("guild");
            var guild = ulong.Parse(guildText);
            var guildObj = await Program._discord.GetGuildAsync(guild);
            foreach(var pair in softbans)
            {
                if(pair.Value.HasExpired())
                {
                    var member = await guildObj.GetMemberAsync(pair.Key);

                    if (member != null)
                        Pardon(member);
                    else //remove softban even if member isn't present
                        softbans.Remove(pair.Key, out var ban);
                }
            }
            CalculateNextUnban();
            NextTimer();
        }

        public Softban GetBan(DiscordMember member)
        {
            return softbans.ContainsKey(member.Id) ? softbans[member.Id] : null;
        }

        /// <summary>
        /// Creates or updates a ban. By policy, bans cannot be shortened. Assumes GetBan check has happened.
        /// </summary>
        /// <param name="member">The member to softban.</param>
        /// <param name="limit">The time limit.</param>
        public void Ban(DiscordMember member, DateTime limit)
        {
            if (softbans.ContainsKey(member.Id))
                softbans[member.Id].SetLimit(limit);
            else
                softbans[member.Id] = new Softban(member.Roles, limit);
            if (nextUnbanID == member.Id)
            {
                CalculateNextUnban();
                NextTimer();
            }
        }

        public bool Pardon(DiscordMember member)
        {
            Softban ban;
            if (softbans.TryRemove(member.Id, out ban))
            {
                List<DiscordRole> rolesToRestore = new List<DiscordRole>();

                foreach (var id in ban.GetRoles())
                {
                    DiscordRole role = member.Guild.GetRole(id);
                    if(role != null)
                        rolesToRestore.Add(role);
                }

                member.ReplaceRolesAsync(rolesToRestore, $"Softban pardoned with roles restored.");
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool PardonWithNoRoles(DiscordMember member)
        {
            Softban ban;
            if(softbans.TryRemove(member.Id, out ban))
            {
                member.ReplaceRolesAsync(new List<DiscordRole>(), "Softban pardoned with no roles restored.");
                return true;
            }
            else
            {
                return false;
            }
        }

        public void CalculateNextUnban()
        {
            ulong id = 0;
            TimeSpan? smallest = null;

            foreach(var pair in softbans)
            {
                var ban = pair.Value;
                var limit = ban.GetLimit();
                if (smallest == null || (limit != default(DateTime) && limit - DateTime.Now < smallest))
                {
                    smallest = limit - DateTime.Now;
                    id = pair.Key;
                }
            }

            nextUnbanID = id;
        }

        public void NextTimer()
        {
            if (nextUnbanID != 0)
            {
                unbanTimer.Interval = (softbans[nextUnbanID].GetLimit() - DateTime.Now).Milliseconds;
                unbanTimer.Start();
            }
        }

        public SortedList<DateTime, ulong> Listing()
        {
            var list = new SortedList<DateTime, ulong>();
            foreach (var pair in softbans)
                list.Add(pair.Value.GetLimit(), pair.Key);
            return list;
        }

        internal class Softban
        {
            List<ulong> previousRoles;
            DateTime limit;

            public Softban(IEnumerable<DiscordRole> roles, DateTime limit)
            {
                previousRoles = new List<ulong>();
                foreach(var role in roles)
                    previousRoles.Add(role.Id);
                this.limit = limit;
            }

            public List<ulong> GetRoles()
            {
                return previousRoles;
            }

            public DateTime GetLimit()
            {
                return limit;
            }

            public void SetLimit(DateTime limit)
            {
                this.limit = limit;
            }

            public bool HasExpired()
            {
                if (limit == default(DateTime))
                    return false;
                return DateTime.Now >= limit;
            }
        }
    }
}
