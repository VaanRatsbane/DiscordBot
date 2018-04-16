using DSharpPlus.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace DiscordBot.Modules.Classes
{
    public class CookieManager : Killable
    {

        const string COOKIE_PATH = "Files/Chat/cookies.json";
        const ulong SERVER = 0;

        ConcurrentDictionary<ulong, CookieProfile> cookieAlbum;

        public CookieManager()
        {
            try
            {
                var json = File.ReadAllText(COOKIE_PATH);
                cookieAlbum = JsonConvert.DeserializeObject<ConcurrentDictionary<ulong, CookieProfile>>(json);
                if (!cookieAlbum.ContainsKey(SERVER))
                    cookieAlbum[SERVER] = new CookieProfile();
            }
            catch(Exception)
            {
                cookieAlbum = new ConcurrentDictionary<ulong, CookieProfile>();
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
                var json = JsonConvert.SerializeObject(cookieAlbum, Formatting.Indented);
                File.WriteAllText(COOKIE_PATH, json);
            }
            catch(Exception e)
            {
                Log.Error("Failed to save Cookie Profile.");
                if (Program.cfg.Debug())
                    Log.Error(e.ToString());
            }
        }

        public void AddCookie(DiscordMember giver, DiscordMember receiver)
        {
            CookieProfile giverProfile, receiverProfile, serverProfile;

            if (!cookieAlbum.ContainsKey(giver.Id))
                giverProfile = new CookieProfile();
            else if (!cookieAlbum.TryGetValue(giver.Id, out giverProfile))
                return;

            if (!cookieAlbum.ContainsKey(receiver.Id))
                receiverProfile = new CookieProfile();
            else if (!cookieAlbum.TryGetValue(receiver.Id, out receiverProfile))
                return;

            if (!cookieAlbum.TryGetValue(SERVER, out serverProfile))
                return;

            cookieAlbum[giver.Id].Gave();
            receiverProfile.Received();
            serverProfile.Gave();
            serverProfile.Received();
        }

        public void GetCookie(DiscordMember member, out int given, out int received)
        {
            CookieProfile profile;
            if ((member == null && cookieAlbum.TryGetValue(SERVER, out profile))
                || cookieAlbum.TryGetValue(member.Id, out profile))
                profile.Data(out given, out received);
            else
            {
                given = -1;
                received = -1;
            }
        }

        internal class CookieProfile
        {
            int given, received;

            public void Gave()
            {
                given++;
            }

            public void Received()
            {
                received++;
            }

            public void Data(out int givenCookies, out int receivedCookies)
            {
                givenCookies = given;
                receivedCookies = received;
            }

        }

    }
}
