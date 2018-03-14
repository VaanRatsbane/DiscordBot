﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    class SchedulerModule
    {

        [Command("remindme"), Description("I'll remind you of something after the given time passes.")]
        public async Task RemindeMe(CommandContext ctx, [Description("When to remind you. Use the following format:" +
            " 1 Jan 2018 8:30 AM +01:00 (adjust values accordingly, +00:00 if GMT).")]string date,
            [RemainingText, Description("The message you want to be reminded with.")] string message)
        {
            try
            {
                DateTime scheduled = DateTime.ParseExact(date, "dd MMM yyyy h:mm tt zzz", CultureInfo.InvariantCulture);
                if(scheduled < DateTime.Now)
                    await ctx.RespondAsync("I can't remind you in the past!");
                else
                {
                    Program.scheduler.CreateReminder(ctx.Member.Id, message, scheduled);

                    DiscordEmbed embed = new DiscordEmbedBuilder()
                        .WithAuthor("Reminder created.")
                        .WithDescription("You will be reminded at " + scheduled.ToString() + " (GMT)");
                    await ctx.RespondAsync(embed: embed);
                }
            }
            catch (FormatException)
            {
                await ctx.RespondAsync("Wrong date format.");
            }
            catch(Exception)
            {
                await ctx.RespondAsync("Failed to schedule reminder.");
            }
        }

        [Command("listreminders"), Description("Lists your reminders.")]
        public async Task ListReminders(CommandContext ctx)
        {
            string[] reminders = Program.scheduler.ListReminders(ctx.Member.Id);
            var dm = await ctx.Member.CreateDmChannelAsync();

            if (reminders.Length == 0)
                await dm.SendMessageAsync("You have no scheduled reminders.");
            else
            {
                string res = "";
                foreach (var reminder in reminders)
                {
                    if ((res + reminder).Length > 2000)
                    {
                        await dm.SendMessageAsync(res);
                        res = "";
                    }
                    res += reminder + "\n";
                }
                if (res != "")
                    await dm.SendMessageAsync(res);
            }
        }

        [Command("cancelreminder"), Description("Cancel one of your reminders.")]
        public async Task CancelReminder(CommandContext ctx, int reminderToRemove = 0)
        {
            if (reminderToRemove <= 0 || !Program.scheduler.CancelReminder(ctx.Member.Id, reminderToRemove))
                await ctx.RespondAsync("Use listreminders to get a correct Reminder ID to remove.");
            else
                await ctx.RespondAsync("Reminder cancelled.");
        }

    }
}
