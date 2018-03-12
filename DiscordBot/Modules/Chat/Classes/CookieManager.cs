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
            if (!cookieAlbum.ContainsKey(giver.Id))
                cookieAlbum[giver.Id] = new CookieProfile();
            if (!cookieAlbum.ContainsKey(receiver.Id))
                cookieAlbum[receiver.Id] = new CookieProfile();

            cookieAlbum[giver.Id].Gave();
            cookieAlbum[receiver.Id].Received();
            cookieAlbum[SERVER].Gave();
            cookieAlbum[SERVER].Received();
        }

        public void GetCookie(DiscordMember member, out int given, out int received)
        {
            if (member == null)
                cookieAlbum[SERVER].Data(out given, out received);
            else if (cookieAlbum.ContainsKey(member.Id))
                cookieAlbum[member.Id].Data(out given, out received);
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
