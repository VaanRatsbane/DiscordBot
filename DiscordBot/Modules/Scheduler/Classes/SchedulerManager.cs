using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace DiscordBot.Modules.Classes
{
    class SchedulerManager : Killable
    {

        const string REMINDERS_PATH = "Files/Scheduler/reminders.json";
        const string REMINDERS_PERUSER_PATH = "Files/Scheduler/remindersperuser.json";

        private SortedList<DateTimeOffset, List<Reminder>> reminders;
        private ConcurrentDictionary<ulong, List<Reminder>> remindersPerUser;
        private Timer reminderTimer;

        public SchedulerManager()
        {
            if (!Directory.Exists("Files/Scheduler"))
                Directory.CreateDirectory("Files/Scheduler");

            try
            {
                var json = File.ReadAllText(REMINDERS_PATH);
                reminders = JsonConvert.DeserializeObject<SortedList<DateTimeOffset, List<Reminder>>>(json);
                json = File.ReadAllText(REMINDERS_PERUSER_PATH);
                remindersPerUser = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, List<Reminder>>>(json);

                Log.Success("Loaded reminders.");
            }
            catch
            {
                Log.Warning("Couldn't load schedules. Initializing...");
                reminders = new SortedList<DateTimeOffset, List<Reminder>>();
                remindersPerUser = new ConcurrentDictionary<ulong, List<Reminder>>();
            }

            reminderTimer = new Timer();
            reminderTimer.AutoReset = false;
            reminderTimer.Elapsed += ReminderTimer_Elapsed;

        }

        public void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(reminders, Formatting.Indented);
                File.WriteAllText(REMINDERS_PATH, json);
                json = JsonConvert.SerializeObject(remindersPerUser, Formatting.Indented);
                File.WriteAllText(REMINDERS_PERUSER_PATH, json);
            }
            catch(Exception e)
            {
                Log.Error("Couldn't save reminders!");
                if (Program.cfg.Debug())
                    Log.Error(e.ToString());
            }
        }

        public void Kill()
        {
            reminderTimer.Stop();
            Save();
        }

        private async void ReminderTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await SolveReminders();
            SetReminderTimer();
        }

        public void CreateReminder(ulong memberId, string message, DateTimeOffset scheduled)
        {
            var localDate = scheduled.ToLocalTime().Date;
            List<Reminder> rems, userRems;

            if (reminders.TryGetValue(localDate, out rems)) {
                SetReminderTimer();
            }
            else
            {
                rems = new List<Reminder>();
                reminders.TryAdd(localDate, rems);
            }

            if (!remindersPerUser.TryGetValue(memberId, out userRems))
            {
                userRems = new List<Reminder>();
                remindersPerUser.TryAdd(memberId, userRems); //does nothing if it already exists
            }

            var reminder = new Reminder()
            {
                created = DateTime.Now,
                scheduled = localDate,
                message = message,
                userId = memberId
            };

            rems.Add(reminder);
            userRems.Add(reminder);
        }

        public string[] ListReminders(ulong memberId)
        {
            List<string> rems = new List<string>();
            if (!remindersPerUser.ContainsKey(memberId) || remindersPerUser.Count == 0)
                return rems.ToArray();

            var offset = DateTimeOffset.Now;
            if (remindersPerUser.TryGetValue(memberId, out var userReminders))
            {
                rems.Add($"Dates are in UTC{(offset.Offset.Hours < 0 ? "-" : "+")}{offset.Offset.TotalHours.ToString("0.00")}");
                for (int i = 0; i < userReminders.Count; i++)
                {
                    var reminder = userReminders[i];
                    rems.Add($"({i + 1}) [{reminder.scheduled.ToString()}] {reminder.message}");
                }
            }
            return rems.ToArray();
        }

        public bool CancelReminder(ulong memberId, int reminderPos)
        {
            if (remindersPerUser.TryGetValue(memberId, out var userReminders))
            {
                if (userReminders.Count > reminderPos - 1)
                {
                    Reminder reminder = userReminders[reminderPos - 1];
                    if (userReminders.Remove(reminder))
                    {
                        if (userReminders.Count == 0)
                            remindersPerUser.TryRemove(memberId, out var b);

                        if (reminders.TryGetValue(reminder.scheduled, out var value))
                        {
                            value.Remove(reminder);
                            if (value.Count == 0)
                                reminders.Remove(reminder.scheduled);

                            SetReminderTimer();
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public async Task SolveReminders()
        {
            if (reminders.Count > 0)
            {
                var toSend = new List<List<Reminder>>();
                var toSendLate = new List<List<Reminder>>();
                var toRemove = new List<DateTimeOffset>();

                var enumerator = reminders.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var pair = enumerator.Current;
                    if (pair.Key <= DateTime.Now)
                    {
                        if (pair.Key + TimeSpan.FromMinutes(5) <= DateTime.Now)
                            toSendLate.Add(pair.Value);
                        else
                            toSend.Add(pair.Value);
                        toRemove.Add(pair.Key);
                    }
                    else
                        break;
                }
                
                if (toSend.Count > 0)
                {
                    var guild = await Program._discord.GetGuildAsync(ulong.Parse(Program.cfg.GetValue("guild")));
                    foreach (var list in toSend)
                        if (list != null && list.Count > 0)
                            await SendReminder(list, guild, false);
                    foreach (var list in toSendLate)
                        if (list != null && list.Count > 0)
                            await SendReminder(list, guild, true);
                }

                foreach (var r in toRemove)
                    if(r != null)
                        reminders.Remove(r);
            }
        }

        private async Task SendReminder(List<Reminder> reminders, DiscordGuild guild, bool isLate = false)
        {
            var offset = DateTimeOffset.Now.Offset;
            DiscordEmbed embed = new DiscordEmbedBuilder()
                .WithAuthor($"{Program._discord.CurrentUser.Username}#{Program._discord.CurrentUser.Discriminator}", icon_url: Program._discord.CurrentUser.AvatarUrl)
                .WithTitle($"[{reminders[0].scheduled.ToString("yyyy-MM-dd HH:mm:ss")}" + $" (UTC{(offset.Hours < 0 ? " - " : " + ")}{offset.TotalHours.ToString("0.00")})]")
                .WithDescription(isLate ? "I apologize for not delivering the message on time, here you go:" : "As scheduled, here is your reminder:")
                .WithFooter("As scheduled on " + reminders[0].created.ToString("yyyy-MM-dd HH:mm:ss") + $" (UTC{(offset.Hours < 0 ? " - " : " + ")}{offset.TotalHours.ToString("0.00")})");

            foreach(var reminder in reminders)
            {
                DiscordEmbed remindEmbed = new DiscordEmbedBuilder(embed)
                    .AddField("Reminder", reminder.message);

                var member = await guild.GetMemberAsync(reminder.userId);
                var dm = await member.CreateDmChannelAsync();
                await dm.SendMessageAsync(embed: remindEmbed);

                if (remindersPerUser.TryGetValue(reminder.userId, out var userReminders))
                {
                    userReminders.Remove(reminder);
                    if (userReminders.Count == 0)
                        remindersPerUser.Remove(reminder.userId, out var trash);
                }
            }

        }

        public async void SetReminderTimer()
        {
            reminderTimer.Stop();
            if (reminders.Count > 0)
            {
                await SolveReminders();
                reminderTimer.Interval = System.Math.Min((reminders.Keys[0] - DateTime.Now).TotalMilliseconds, Int32.MaxValue);
                reminderTimer.Start();
            }
        }

        internal class Reminder
        {
            public ulong userId;
            public string message;
            public DateTime created;
            public DateTimeOffset scheduled;
        }

    }
}
