﻿using DiscordBot.Modules.Classes;
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
    class AdminModule
    {

        [Command("dumplog"), Description("Dumps a specific log file. Leave value at -1 for current."), RequirePermissions(DSharpPlus.Permissions.KickMembers)]
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

        [Command("inviterolelink"), Description("Associates a channel invite to an automatic role attribution. Run in the channel that generated such invite."), RequirePermissions(DSharpPlus.Permissions.KickMembers)]
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

        [Command("removerolelink"), Description("Removes a channel invite role association. Run in the channel that generated such invite."), RequirePermissions(DSharpPlus.Permissions.KickMembers)]
        public async Task RemoveRoleLink(CommandContext ctx)
        {
            if (Program.inviteRoles.RemoveChannel(ctx.Channel.Id))
                await ctx.RespondAsync("Link erased.");
            else
                await ctx.RespondAsync("This channel has no automatic role assignment associated.");
        }

        [Command("prune"), Description("Prunes the server."), RequirePermissions(DSharpPlus.Permissions.KickMembers)]
        public async Task Prune(CommandContext ctx)
        {
            try
            {
                int kicked = await AutoPrune.Prune();
                if (kicked < 0)
                    await ctx.RespondAsync("Error! Check bot log.");
                else if (kicked == 0)
                    await ctx.RespondAsync("No one to kick.");
                else
                    await ctx.RespondAsync("Kicked " + kicked + " member(s).");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        [Command("prunelist"), Description("Returns people to be pruned."), RequirePermissions(DSharpPlus.Permissions.KickMembers)]
        public async Task PruneList(CommandContext ctx)
        {
            var toPrune = AutoPrune.AvailableToPrune();
            if (toPrune == null) return;
            string result = $"You can prune {toPrune.Count} members.\n";
            var dm = await ctx.Member.CreateDmChannelAsync();
            foreach (var t in toPrune)
            {
                var newLine = $"{t}\n";
                if ((result + newLine).Length > 2000)
                {
                    await dm.SendMessageAsync(result);
                    result = "";
                }

                result += newLine;
            }
            if(result != "")
                await dm.SendMessageAsync(result);
            await ctx.RespondAsync("Sent by DM.");
        }

        [Command("prunelist"), Description("Lists prunable people."), RequirePermissions(DSharpPlus.Permissions.KickMembers)]
        public async Task ReportPrune(CommandContext ctx)
        {
            var list = await AutoPrune.Report();
            string toSend = "";
            var dmChannel = await ctx.Member.CreateDmChannelAsync();
            foreach(var line in list)
            {
                if (toSend.Length + line.Length > 2000)
                {
                    await dmChannel.SendMessageAsync(toSend);
                    toSend = "";
                }
                toSend += $"{line}\n";
            }
            if(!String.IsNullOrEmpty(toSend))
                await dmChannel.SendMessageAsync(toSend);
        }

        //Not happy with this if else party, should prob do a trycatch and throws
        [Command("softban"), Description("Softbans a member."), RequirePermissions(DSharpPlus.Permissions.KickMembers)]
        public async Task Softban(CommandContext ctx, DiscordMember member,
            [RemainingText, Description("The time limit for the ban. Use 'permanent' or a set of time denotations '#amount# {year/month/day/hour/minute/second}'")]string limit)
        {

            var pieces = limit.Split(' ');
            DateTime lift;
            TimeSpan span = new TimeSpan();
            if(pieces.Length < 12)
            {
                if(pieces.Length == 1 && pieces[0].ToUpperInvariant() == "PERMANENT")
                {
                    lift = default(DateTime);
                }
                else if(pieces.Length % 2 == 0) //check if number is even
                {
                    for(int i = 0; i < pieces.Length; i += 2)
                    {
                        if(int.TryParse(pieces[i], out int value))
                        {
                            switch(pieces[i+1].ToUpperInvariant())
                            {
                                case "year": case "YEARS":
                                    span.Add(new TimeSpan(365 * value, 0, 0, 0));
                                    break;

                                case "month": case "MONTHS":
                                    span.Add(new TimeSpan(30 * value, 0, 0, 0));
                                    break;

                                case "day": case "DAYS":
                                    span.Add(new TimeSpan(value, 0, 0, 0));
                                    break;

                                case "hour": case "HOURS":
                                    span.Add(new TimeSpan(0, value, 0, 0));
                                    break;

                                case "minute":case "MINUTES":
                                    span.Add(new TimeSpan(0, 0, value, 0));
                                    break;

                                case "second": case "SECONDS":
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

        [Command("pardon"), Description("Pardons a member."), RequirePermissions(DSharpPlus.Permissions.KickMembers)]
        public async Task PardonBan(CommandContext ctx, DiscordMember member)
        {
            if (Program.softbans.Pardon(member))
                await ctx.RespondAsync("Pardoned.");
            else
                await ctx.RespondAsync("That member is not softbanned.");
        }

        [Command("pardonnoroles"), Description("Pardons a member without restoring their roles."), RequirePermissions(DSharpPlus.Permissions.KickMembers)]
        public async Task PardonBanNoRoles(CommandContext ctx, DiscordMember member)
        {
            if (Program.softbans.PardonWithNoRoles(member))
                await ctx.RespondAsync("Pardoned.");
            else
                await ctx.RespondAsync("That member is not softbanned.");
        }

        [Command("listsoftbans"), Description("Prints softbans and their time limits."), RequirePermissions(DSharpPlus.Permissions.KickMembers)]
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
                    string piece = $"({member.DisplayName}) | {member.Username}#{member.Discriminator} - Expires: " +
                        ((pair.Key == default(DateTime)) ? "NEVER" : pair.Key.ToString("yyyy-MM-dd HH:mm:ss")) + "\n";

                    if ((response + piece).Length > 2000)
                    {
                        await ctx.RespondAsync(response);
                        response = piece;
                    }
                    else
                        response += piece;
                }
                await ctx.RespondAsync(response);
            }
        }

        [Command("wipe"), Description("Wipes a number of messages from the channel."), RequirePermissions(DSharpPlus.Permissions.ManageMessages)]
        public async Task Wipe(CommandContext ctx, int quantity = 100)
        {
            await ctx.Message.DeleteAsync();
            var msgs = await ctx.Channel.GetMessagesAsync(quantity);
            if (msgs.Count > 0)
            {
                await ctx.Channel.DeleteMessagesAsync(msgs);
                await ctx.RespondAsync("Wiped " + msgs.Count + " messages.");
            }

        }

    }
}
