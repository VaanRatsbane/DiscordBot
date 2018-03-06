using DiscordBot.Modules.Classes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
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

        [Command("inviterolelink"), Description("Associates a channel invite to an automatic role attribution. Run in the channel that generated such invite.")]
        public async Task InviteToRoleLink(CommandContext ctx, DiscordRole role)
        {
            if(Program.inviteRoles.HasChannel(ctx.Channel.Id))
            {
                await ctx.RespondAsync("This channel already has automatic role attribution.");
            }
            else
            {
                Program.inviteRoles.AddLink(ctx.Channel.Id, role.Id);
                await ctx.RespondAsync("Link created.");
            }
        }

        [Command("removerolelink"), Description("Removes a channel invite role association. Run in the channel that generated such invite.")]
        public async Task RemoveRoleLink(CommandContext ctx)
        {
            if (Program.inviteRoles.RemoveChannel(ctx.Channel.Id))
                await ctx.RespondAsync("Link erased.");
            else
                await ctx.RespondAsync("This channel has no automatic role assignment associated.");
        }

        [Command("prune"), Description("Prunes the server.")]
        public async Task Prune(CommandContext ctx)
        {
            int kicked = await AutoPrune.Prune();
            if (kicked < 0)
                await ctx.RespondAsync("Error! Check bot log.");
            else if (kicked == 0)
                await ctx.RespondAsync("No one to kick.");
            else
                await ctx.RespondAsync("Kicked " + kicked + " member(s).");
        }

    }
}
