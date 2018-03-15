using DSharpPlus.CommandsNext;
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
        public async Task RemindeMe(CommandContext ctx, [Description("When to remind you. Remember to add a timezone if you don't share the bot's.")]string date,
            [RemainingText, Description("The message you want to be reminded with.")] string message)
        {
            try
            {
                DateTime scheduled = DateTime.Parse(date);
                if(scheduled < DateTime.Now)
                    await ctx.RespondAsync("I can't remind you in the past!");
                else
                {
                    Program.scheduler.CreateReminder(ctx.Member.Id, message, scheduled);

                    DiscordEmbed embed = new DiscordEmbedBuilder()
                        .WithAuthor("Reminder created.")
                        .WithDescription("You will be reminded at " + scheduled.ToString("yyyy/MM/dd HH:mm:ss") + " (GMT+1)");
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
        public async Task CancelReminder(CommandContext ctx, int reminderToRemove)
        {
            if (reminderToRemove <= 0 || !Program.scheduler.CancelReminder(ctx.Member.Id, reminderToRemove))
                await ctx.RespondAsync("Use listreminders to get a correct Reminder ID to remove.");
            else
                await ctx.RespondAsync("Reminder cancelled.");
        }

    }
}
