using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DiscordBot.Modules.Classes
{
    class InviteRoles : Killable
    {

        ConcurrentDictionary<ulong, ulong> channelsToRoles;
        ConcurrentDictionary<ulong, int> channelsLinkUsages;

        const string INVITELINK_FILE = "Files/Admin/invitelinkroles.json";

        public InviteRoles()
        {
            try
            {
                var json = File.ReadAllText(INVITELINK_FILE);
                channelsToRoles = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, ulong>>(json);
                Log.Success("Loaded invitelinkroles.");
            }
            catch(Exception e)
            {
                Log.Warning("Could not load the InviteLinkRoles file. Initializing...");
                if (Program.cfg.Debug())
                    Log.Warning(e.ToString());
                channelsToRoles = new ConcurrentDictionary<ulong, ulong>();
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

                var json = JsonConvert.SerializeObject(channelsToRoles, Formatting.Indented);
                File.WriteAllText(INVITELINK_FILE, json);
            }
            catch (Exception e)
            {
                Log.Error("Failed to save invitelinkroles file!");
                if (Program.cfg.Debug())
                    Log.Error(e.ToString());
            }
        }

        public void Initialize(IReadOnlyList<DiscordInvite> invites)
        {
            channelsLinkUsages = new ConcurrentDictionary<ulong, int>();
            foreach (var i in invites)
                if (!i.IsRevoked)
                {
                    var uses = channelsLinkUsages.ContainsKey(i.Channel.Id) ? channelsLinkUsages[i.Channel.Id] : 0;
                    channelsLinkUsages[i.Channel.Id] = uses + i.Uses;
                }
        }

        public void AddLink(ulong channel, ulong role)
        {
            channelsToRoles[channel] = role;
        }

        public bool HasChannel(ulong id)
        {
            return channelsToRoles.ContainsKey(id);
        }

        public bool RemoveChannel(ulong id)
        {
            var temp = new ConcurrentDictionary<ulong, ulong>();

            if (channelsToRoles.ContainsKey(id))
                return channelsToRoles.TryRemove(id, out ulong value);
            else
                return false;
        }

        public void RemoveRole(ulong id)
        {
            var temp = new List<ulong>();

            lock (channelsToRoles)
            foreach (var pair in channelsToRoles)
                if (pair.Value == id)
                    temp.Add(pair.Key);

            foreach (var key in temp)
                if (channelsToRoles.TryRemove(key, out ulong value))
                    Log.Info("Role with ID " + value + " deleted, thus the association with channel with ID " + key + " was removed.");
        }

        public ulong UpdateUsages(IReadOnlyList<DiscordInvite> invites)
        {
            var temp = new ConcurrentDictionary<ulong, int>();
            foreach (var i in invites)
                if (!i.IsRevoked)
                {
                    var uses = temp.ContainsKey(i.Channel.Id) ? temp[i.Channel.Id] : 0;
                    temp[i.Channel.Id] = uses + i.Uses; //accumulating all invites of a channel
                }

            if(channelsLinkUsages != null)
            {
                lock(channelsLinkUsages)
                foreach(var pair in channelsLinkUsages)
                {
                    if(temp.ContainsKey(pair.Key) && temp[pair.Key] > pair.Value)
                    {
                        if (channelsToRoles.ContainsKey(pair.Key))
                            return channelsToRoles[pair.Key];
                        else
                            return 0;
                    }
                }
            }

            channelsLinkUsages = temp;
            return 0;
        }

    }
}
