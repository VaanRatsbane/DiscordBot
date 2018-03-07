using DSharpPlus.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DiscordBot.Modules.Classes
{
    class Quotes : Killable
    {

        const string QUOTE_FILE = "Files\\Chat\\quotes.json";

        Data data;
        Random random;

        public Quotes()
        {
            random = new Random();

            if (!Directory.Exists("Files\\Chat"))
                Directory.CreateDirectory("Files\\Chat");

            try
            {
                var json = File.ReadAllText(QUOTE_FILE);
                data = JsonConvert.DeserializeObject<Data>(json);
            }
            catch(Exception e)
            {
                Log.Warning("Failed to load quotes.");
                if (Program.cfg.Debug())
                    Log.Warning(e.ToString());
                data = new Data();
            }

        }

        public void Kill()
        {
            try
            {
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(QUOTE_FILE, json);
            }
            catch (Exception e)
            {
                Log.Warning("Failed to load quotes.");
                if (Program.cfg.Debug())
                    Log.Warning(e.ToString());
            }
        }

        public bool Add(DiscordMessage message)
        {
            if (data.ids.Contains(message.Id))
                return false;
            else
            {
                data.ids.Add(message.Id);
                data.quotes.Add(new Quote(message.Author.Id, $"{message.Author.Username}#{message.Author.Discriminator}",
                    message.Id, message.Content, message.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")));
                return true;
            }
        }

        public Quote RandomQuote()
        {
            return data.ids.Count == 0 ? null : data.quotes[random.Next(data.ids.Count)];
        }

        public bool RemoveQuote(ulong id)
        {
            var i = data.ids.IndexOf(id);
            if (i != -1)
            {
                data.ids.RemoveAt(i);
                data.quotes.RemoveAt(i);
                return true;
            }
            else
                return false;
        }

        internal class Data
        {
            public List<Quote> quotes;
            public List<ulong> ids;

            public Data()
            {
                quotes = new List<Quote>();
                ids = new List<ulong>();
            }
        }

    }

    class Quote
    {
        public ulong memberId;
        public string username;
        public ulong messageId;
        public string message;
        public string date;

        public Quote(ulong memberId, string username, ulong msgId, string message, string date)
        {
            this.memberId = memberId;
            this.username = username;
            this.messageId = msgId;
            this.message = message;
            this.date = date;
        }

    }
}
