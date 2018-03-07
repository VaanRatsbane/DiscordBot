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

        //Not happy with this if else party, should prob do a trycatch and throws
        [Command("softban"), Description("Softbans a member.")]
        public async Task Softban(CommandContext ctx, DiscordMember member,
            [RemainingText, Description("The time limit for the ban. Use 'permanent' or a set of time denotations '#amount# {year/month/day/hour/minute/second}'")]string limit)
        {

            var pieces = limit.Split(' ');
            DateTime lift;
            TimeSpan span = new TimeSpan();
            if(pieces.Length < 12)
            {
                if(pieces.Length == 1 && pieces[0].ToLowerInvariant() == "permanent")
                {
                    lift = default(DateTime);
                }
                else if(pieces.Length % 2 == 0) //check if number is even
                {
                    for(int i = 0; i < pieces.Length; i += 2)
                    {
                        if(int.TryParse(pieces[i], out int value))
                        {
                            switch(pieces[i+1].ToLowerInvariant())
                            {
                                case "year": case "years":
                                    span.Add(new TimeSpan(365 * value, 0, 0, 0));
                                    break;

                                case "month": case "months":
                                    span.Add(new TimeSpan(30 * value, 0, 0, 0));
                                    break;

                                case "day": case "days":
                                    span.Add(new TimeSpan(value, 0, 0, 0));
                                    break;

                                case "hour": case "hours":
                                    span.Add(new TimeSpan(0, value, 0, 0));
                                    break;

                                case "minute":case "minutes":
                                    span.Add(new TimeSpan(0, 0, value, 0));
                                    break;

                                case "second": case "seconds":
                                    span.Add(new TimeSpan(0, 0, 0, value));
                                    break;

                                default:
                                    await ctx.RespondAsync("Wrong input.");
                                    return;
                            }
                        }
                        else
                        {
                            await ctx.RespondAsync("Wrong input.");
                            return;
                        }
                    }
                    lift = DateTime.Now + span;
                }
                else //number is not even therefore wrong input
                {
                    await ctx.RespondAsync("Wrong input.");
                    return;
                }
            }
            else //wrong input
            {
                await ctx.RespondAsync("Wrong input.");
                return;
            }

            var ban = Program.softbans.GetBan(member);
            if(ban != null) //update
            {
                if(ban.GetLimit() != default(DateTime))
                {
                    if(lift > ban.GetLimit())
                    {
                        Program.softbans.Ban(member, lift);
                        await ctx.RespondAsync("The ban was lengthened to " + lift.ToString("yyyy-MM-dd HH:mm:ss") + ".");
                    }
                    else //cant shorten it
                    {
                        await ctx.RespondAsync("You cannot shorten a softban's length.");
                    }
                }
                else //cant update permanent
                {
                    await ctx.RespondAsync("You cannot update a permanent softban.");
                }
            }
            else //create
            {
                if (lift != default(DateTime))
                    await ctx.RespondAsync("Softbanned " + member.Username + "#" + member.Discriminator + " until " + lift.ToString("yyyy-MM-dd HH:mm:ss") + ".");
                else
                    await ctx.RespondAsync("Softbanned " + member.Username + "#" + member.Discriminator + " permanently.");
                Program.softbans.Ban(member, lift);
                await member.ReplaceRolesAsync(new List<DiscordRole>() {
                        member.Guild.GetRole(ulong.Parse(Program.cfg.GetValue("softbanrole")))
                    });
            }
        }

        [Command("pardon"), Description("Pardons a member.")]
        public async Task PardonBan(CommandContext ctx, DiscordMember member)
        {
            if (Program.softbans.Pardon(member))
                await ctx.RespondAsync("Pardoned.");
            else
                await ctx.RespondAsync("That member is not softbanned.");
        }

        [Command("pardonnoroles"), Description("Pardons a member without restoring their roles.")]
        public async Task PardonBanNoRoles(CommandContext ctx, DiscordMember member)
        {
            if (Program.softbans.PardonWithNoRoles(member))
                await ctx.RespondAsync("Pardoned.");
            else
                await ctx.RespondAsync("That member is not softbanned.");
        }

        [Command("listsoftbans"), Description("Prints softbans and their time limits.")]
        public async Task ListBans(CommandContext ctx)
        {
            var list = Program.softbans.Listing();
            if (list.Count == 0)
                await ctx.RespondAsync("There are no softbans to show.");
            else
            {
                string response = "There " + (list.Count > 1 ? "are " : "is ") + list.Count + " softban" + (list.Count > 1 ? "s" : "") + ".\n";
                foreach(var pair in list)
                {
                    var member = await ctx.Guild.GetMemberAsync(pair.Value);
                    response += $"({member.DisplayName}) | {member.Username}#{member.Discriminator} - Expires: " +
                        ((pair.Key == default(DateTime)) ? "NEVER" : pair.Key.ToString("yyyy-MM-dd HH:mm:ss")) + "\n";
                }
                await ctx.RespondAsync(response);
            }
        }

    }
}
