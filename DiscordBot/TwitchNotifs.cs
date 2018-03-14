using DSharpPlus.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot
{
    public static class TwitchNotifs
    {
        public static async System.Threading.Tasks.Task AddNotificationAsync(DSharpPlus.EventArgs.PresenceUpdateEventArgs e)
        {
            //check eligibility to create notification
            if(e.Member != null && e.Member.Presence != null && e.Member.Presence.Game != null && e.Member.Presence.Game.StreamType == GameStreamType.Twitch)
            {
                //check if it is a new state
                if(e.PresenceBefore != null && ((e.PresenceBefore.Game != null && e.PresenceBefore.Game.StreamType != GameStreamType.Twitch) || e.PresenceBefore.Game == null))
                {
                    var channel = e.Guild.GetChannel(ulong.Parse(Program.cfg.GetValue("twitchchannel")));
                    if (channel != null)
                        await channel.SendMessageAsync(e.Member.DisplayName + " is streaming at " + e.Member.Presence.Game.Url);
                }
            }
        }

    }
}
