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

        const string LASTLOGINS_FILE = "Files/Admin/lastlogins.json";

        public AutoPrune()
        {
            if(Program.cfg.GetValue("prunelimit") == null)
            {
                Program.cfg.SetValue("prunelimit", 30.ToString());
            }

            try
            {
                var json = File.ReadAllText(LASTLOGINS_FILE);
                lastLogins = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, DateTime>>(json);
            }
            catch(Exception e)
            {
                lastLogins = new ConcurrentDictionary<ulong, DateTime>();
                Log.Warning("Couldn't load lastLogins.");
                if (Program.cfg.Debug())
                    Log.Warning(e.ToString());
            }
        }

        public void Kill()
        {
            Save();
        }

        public void Save()
        {
            try
            {
                if (!Directory.Exists("Files/Admin"))
                    Directory.CreateDirectory("Files/Admin");

                var json = JsonConvert.SerializeObject(lastLogins, Formatting.Indented);
                File.WriteAllText(LASTLOGINS_FILE, json);
            }
            catch (Exception e)
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
        }

        public void Logged(ulong memberID)
        {
            lastLogins[memberID] = DateTime.Now;
        }

        public static async Task<string[]> Report()
        {
            double.TryParse(Program.cfg.GetValue("prunelimit"), out double dayLimit);

            List<ulong> offenders = new List<ulong>();
            if (lastLogins.Count > 0)
            {
                var enumerator = lastLogins.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var pair = enumerator.Current;
                    if ((DateTime.Now - pair.Value).TotalDays > dayLimit)
                        offenders.Add(pair.Key);
                }
            }

            if (offenders.Count > 0)
            {
                var guild = await Program._discord.GetGuildAsync(ulong.Parse(Program.cfg.GetValue("discord")));
                var regularsId = ulong.Parse(Program.cfg.GetValue("regulars"));
                var result = new List<string>();
                result.Add($"People who haven't reported online activity in {dayLimit} days:\n");
                foreach(var offender in offenders)
                {
                    var member = await guild.GetMemberAsync(offender);
                    bool isRegular = false;
                    foreach (var role in member.Roles)
                    {
                        if (role.Id == regularsId) //Regulars dont get kicked
                        {
                            lastLogins.TryRemove(offender, out var disp);
                            isRegular = true;
                            break;
                        }
                    }
                    result.Add($"{(isRegular ? "[REGULAR]" : "")}{member.GetFullIdentifier()}");
                }
                return result.ToArray();
            }
            else
            {
                return new string[]{ $"No one has been offline for at least {dayLimit} days."};
            }

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

            if (lastLogins.Count > 0)
            {
                var enumerator = lastLogins.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var pair = enumerator.Current;
                    if ((DateTime.Now - pair.Value).TotalDays > dayLimit)
                        offenders.Add(pair.Key);
                }
            }

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
                        var regularsId = ulong.Parse(Program.cfg.GetValue("regulars"));
                        bool isRegular = false;
                        foreach(var role in member.Roles)
                        {
                            if(role.Id == regularsId) //Regulars dont get kicked
                            {
                                lastLogins.TryRemove(offender, out var disp);
                                Log.Warning($"Regular {member.Username}#{member.Discriminator} hasn't been online for 30 days!");
                                isRegular = true;
                                break;
                            }
                        }

                        if (!isRegular)
                        {
                            await guild.RemoveMemberAsync(member, "Automatically pruned for being offline for " + dayLimit + " days.");
                            lastLogins.TryRemove(offender, out var disposable);
                            kicked++;
                        }
                    }
                }

            }
            if (kicked > 0)
                Log.Info("Scheduled pruning removed " + kicked + " member(s).");
            return kicked;
        }

    }
}
