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
using TweetSharp;

namespace DiscordBot.Modules.API
{
    public class TwitterFeed : IKillable
    {
        const string TWITTERFEED_PATH = "Files/API/twitterfeed.json";
        const string TWITTER_LOGO = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/a0/YouTube_social_red_circle_%282017%29.svg/2000px-YouTube_social_red_circle_%282017%29.svg.png";

        private List<string> feeds;
        private Timer feedTimer;
        private DateTime lastCheck;

        public TwitterFeed()
        {
            if (!Directory.Exists("Files/API"))
                Directory.CreateDirectory("Files/API");

            try
            {
                var json = File.ReadAllText(TWITTERFEED_PATH);
                feeds = JsonConvert.DeserializeObject<List<string>>(json);
                Log.Success("Loaded twitter feeds.");
            }
            catch
            {
                Log.Warning("Couldn't load twitter feeds. Initializing...");
                feeds = new List<string>();
            }

            lastCheck = DateTime.UtcNow;
            feedTimer = new Timer();
            feedTimer.AutoReset = true;
            feedTimer.Elapsed += FeedTimer_Elapsed;
            feedTimer.Interval = 300000;
            if (feeds.Count > 0)
            {
                feedTimer.Start();
            }
        }

        public bool Validate(string accountName)
        {
            var key = Program.keys.GetKey("twitterkey");
            var secret = Program.keys.GetKey("twittersecret");
            var accesstoken = Program.keys.GetKey("twitteraccesstoken");
            var accesstokensecret = Program.keys.GetKey("twitteraccesstokensecret");
            TwitterService service = new TwitterService(key, secret);
            service.AuthenticateWith(accesstoken, accesstokensecret);
            var user = service.GetUserProfileFor(new GetUserProfileForOptions()
            {
                ScreenName = accountName
            });
            return user != null;
        }

        public bool Add(string accountName)
        {
            if (!feeds.Contains(accountName))
            {
                feeds.Add(accountName);
                Save();
                if (feeds.Count == 1)
                {
                    lastCheck = DateTime.UtcNow;
                    feedTimer.Start();
                }
                return true;
            }
            else
                return false;
        }

        public bool Remove(string accountName)
        {
            if (feeds.Contains(accountName))
            {
                feeds.Remove(accountName);
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
                File.WriteAllText(TWITTERFEED_PATH, json);
            }
            catch(Exception e)
            {
                Log.Error("Failed to save twitter feed data! Error:\n" + e.ToString());
            }
        }

        public void Kill()
        {
            feedTimer.Stop();
        }

        private async void FeedTimer_Elapsed(object sender, ElapsedEventArgs e)
        {

            var guild = await Program._discord.GetGuildAsync(ulong.Parse(Program.cfg.GetValue("guild")));
            var channelId = ulong.Parse(Program.cfg.GetValue("twitterchannel"));
            var channel = guild.GetChannel(channelId);
            if(channel != null)
            {
                var newCheck = DateTime.UtcNow;

                var key = Program.keys.GetKey("twitterkey");
                var secret = Program.keys.GetKey("twittersecret");
                var accesstoken = Program.keys.GetKey("twitteraccesstoken");
                var accesstokensecret = Program.keys.GetKey("twitteraccesstokensecret");
                TwitterService service = new TwitterService(key, secret);
                service.AuthenticateWith(accesstoken, accesstokensecret);

                var toPost = new List<DiscordEmbed>();
                foreach (var feed in feeds)
                {
                    try
                    {
                        var tweets = service.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions
                        {
                            ScreenName = feed,
                            Count = 20,
                            IncludeRts = false,
                            ExcludeReplies = true
                        });

                        if(tweets != null)
                        {
                            foreach(var tweet in tweets)
                            {
                                if (tweet.CreatedDate <= lastCheck)
                                    break;
                                else
                                {
                                    var embed = new DiscordEmbedBuilder()
                                        .WithAuthor(tweet.Author.ScreenName, tweet.User.Url, tweet.Author.ProfileImageUrl)
                                        .WithColor(new DiscordColor(0x1da1f2))
                                        .WithUrl($"https://twitter.com/{tweet.User.Name}/status/{tweet.Id}")
                                        .WithTimestamp(tweet.CreatedDate);
                                    if (!String.IsNullOrEmpty(tweet.Text))
                                        embed = embed.WithDescription(tweet.Text);
                                    if (tweet.Entities.Media.Count > 0 && tweet.Entities.Media[0].MediaType == TwitterMediaType.Photo)
                                        embed = embed.WithImageUrl(tweet.Entities.Media[0].MediaUrl);
                                    toPost.Add(embed.Build());
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
                    catch (Exception ex)
                    {
                        Log.Warning("Tweet feed:\n" + ex.ToString());
                    }
                }
                lastCheck = newCheck;
            }
            else
            {
                Log.Error("Couldn't get youtube channel. Stopping...");
                feedTimer.Stop();
            }
        }
    }
}
