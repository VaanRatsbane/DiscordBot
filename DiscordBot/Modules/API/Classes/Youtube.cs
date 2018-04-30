using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Timers;
using System.Text.RegularExpressions;
using System.Net;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace DiscordBot.Modules.API
{
    public class YoutubeFeed : IKillable
    {
        const string YOUTUBEFEED_PATH = "Files/API/youtubefeed.json";
        const string MATCH_EXP = @"(?:https|http)\:\/\/(?:[\w]+\.)?youtube\.com\/(?:c\/|channel\/|user\/)?([a-zA-Z0-9\-]{1,})";
        const string CHANNEL_PREFIX = "https://www.youtube.com/channel/";
        const string YOUTUBE_CHANNEL_API = "https://www.googleapis.com/youtube/v3/search?key=@KEY@&channelId=@CHANNELID@&part=snippet,id&order=date&maxResults=20";
        const string YOUTUBE_LOGO = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/a0/YouTube_social_red_circle_%282017%29.svg/2000px-YouTube_social_red_circle_%282017%29.svg.png";

        private List<string> feeds;
        private Timer feedTimer;
        private DateTime lastCheck;

        public YoutubeFeed()
        {
            if (!Directory.Exists("Files/API"))
                Directory.CreateDirectory("Files/API");

            try
            {
                var json = File.ReadAllText(YOUTUBEFEED_PATH);
                feeds = JsonConvert.DeserializeObject<List<string>>(json);
                Log.Success("Loaded youtube feeds.");
            }
            catch
            {
                Log.Warning("Couldn't load youtube feeds. Initializing...");
                feeds = new List<string>();
            }

            lastCheck = DateTime.Now;
            feedTimer = new Timer();
            feedTimer.AutoReset = false;
            feedTimer.Elapsed += FeedTimer_Elapsed;
            feedTimer.Interval = 300000;
            if (feeds.Count > 0)
            {
                feedTimer.Start();
            }
        }

        //Returns channel ID if validated, null if not
        public string Validate(string channelLink)
        {
            var regex = new Regex(MATCH_EXP);
            var match = regex.Match(channelLink);
            return (match.Success && match.Groups.Count == 2) ?
                match.Groups[1].Value :
                null;
        }

        public bool Add(string channelId)
        {
            if (!feeds.Contains(channelId))
            {
                feeds.Add(channelId);
                Save();
                if (feeds.Count == 1)
                {
                    lastCheck = DateTime.Now;
                    feedTimer.Start();
                }
                return true;
            }
            else
                return false;
        }

        public bool Remove(string channelId)
        {
            if (feeds.Contains(channelId))
            {
                feeds.Remove(channelId);
                Save();
                if (feeds.Count == 0)
                    feedTimer.Stop();
                return true;
            }
            else
                return false;
        }

        public void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(feeds, Formatting.Indented);
                File.WriteAllText(YOUTUBEFEED_PATH, json);
            }
            catch(Exception e)
            {
                Log.Error("Failed to save youtube feed data! Error:\n" + e.ToString());
            }
        }

        public void Kill()
        {
            feedTimer.Stop();
        }

        private async void FeedTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            var guild = await Program._discord.GetGuildAsync(ulong.Parse(Program.cfg.GetValue("guild")));
            var channelId = ulong.Parse(Program.cfg.GetValue("youtubechannel"));
            var channel = guild.GetChannel(channelId);
            if(channel != null)
            {
                var newCheck = DateTime.Now;
                var key = Program.keys.GetKey("youtube");
                var toPost = new List<DiscordEmbed>();
                WebClient client = new WebClient();
                foreach(var feed in feeds)
                {
                    try
                    {
                        var url = YOUTUBE_CHANNEL_API.Replace("@KEY@", key).Replace("@CHANNELID@", feed);
                        var json = client.DownloadString(url);
                        var result = JsonConvert.DeserializeObject<YoutubeChannelVideos.RootObject>(json);
                        if(result != null)
                        {
                            foreach(var item in result.items)
                            {
                                if (item.snippet.publishedAt <= lastCheck)
                                    break;
                                else if(item.id.kind == "youtube#video")
                                {
                                    toPost.Add(new DiscordEmbedBuilder()
                                        .WithAuthor(item.snippet.channelTitle, "https://www.youtube.com/channel/" + item.snippet.channelId, YOUTUBE_LOGO)
                                        .WithColor(DiscordColor.Red)
                                        .WithTitle(item.snippet.title)
                                        .WithUrl("https://www.youtube.com/watch?v=" + item.id.videoId)
                                        .WithTimestamp(item.snippet.publishedAt)
                                        .WithThumbnailUrl(item.snippet.thumbnails.@default.url)
                                        .WithDescription(item.snippet.description)
                                    );
                                }
                            }
                            if(toPost.Count > 0)
                            {
                                toPost.Reverse();
                                foreach (var post in toPost)
                                {
                                    await channel.SendMessageAsync(embed: post);
                                    await Task.Delay(2000);
                                }
                            }
                        }
                    }
                    catch
                    {
                        Log.Warning("Youtube feed:\n" + e.ToString());
                    }
                }
                client.Dispose();
                lastCheck = newCheck;
            }
            else
            {
                Log.Error("Couldn't get youtube channel. Stopping...");
                feedTimer.Stop();
            }
        }
    }

    public class YoutubeChannelVideos
    {
        public class PageInfo
        {
            public int totalResults { get; set; }
            public int resultsPerPage { get; set; }
        }

        public class Id
        {
            public string kind { get; set; }
            public string videoId { get; set; }
        }

        public class Default
        {
            public string url { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }

        public class Medium
        {
            public string url { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }

        public class High
        {
            public string url { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }

        public class Thumbnails
        {
            public Default @default { get; set; }
            public Medium medium { get; set; }
            public High high { get; set; }
        }

        public class Snippet
        {
            public DateTime publishedAt { get; set; }
            public string channelId { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public Thumbnails thumbnails { get; set; }
            public string channelTitle { get; set; }
            public string liveBroadcastContent { get; set; }
        }

        public class Item
        {
            public string kind { get; set; }
            public string etag { get; set; }
            public Id id { get; set; }
            public Snippet snippet { get; set; }
        }

        public class RootObject
        {
            public string kind { get; set; }
            public string etag { get; set; }
            public string nextPageToken { get; set; }
            public string regionCode { get; set; }
            public PageInfo pageInfo { get; set; }
            public List<Item> items { get; set; }
        }
    }
}
