using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    /// <summary>
    /// If you can kick members, you are an "admin" (rather not give admin role away since it can be superseeded, and keep it on the bot)
    /// </summary>
    [Group("admin"), Aliases("ad"), Description("Administrative commands."), RequirePermissions(DSharpPlus.Permissions.KickMembers)]
    class AdminModule
    {

        [Command("dumplog"), Description("Dumps a specific log file. Leave value at -1 for current.")]
        public async Task DumpLog(CommandContext ctx, int year = -1, int month = -1, int day = -1)
        {
            var now = DateTime.Now;
            FileStream fs = null;

            if((year == -1 && month == -1) || (year != -1 && month != -1)) //day's file
            {
                if (year == -1) year = now.Year;
                if (month == -1) month = now.Month;
                if (day == -1)
                    day = now.Day;
                fs = Log.GetLogFile(year, month, day);
            }
            else if(day == -1) //zip
            {
                if (year == -1) year = now.Year;
                fs = Log.GetLogZip(year, month);
            }


            if(fs == null)
            {
                await ctx.RespondAsync("Log file does not exist.");
            }
            else
            {
                await ctx.RespondWithFileAsync(fs);
                Log.CleanTempZip();
            }
        }

    }
}
