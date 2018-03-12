﻿using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot.Modules.API.Classes
{
    class Reddit
    {
        public class Source
        {
            public string url { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }

        public class Resolution
        {
            public string url { get; set; }
            public int width { get; set; }
            public int height { get; set; }
        }

        public class Image
        {
            public Source source { get; set; }
            public List<Resolution> resolutions { get; set; }
            public object variants { get; set; }
            public string id { get; set; }
        }

        public class Preview
        {
            public List<Image> images { get; set; }
            public bool enabled { get; set; }
        }

        public class Data2
        {
            public string subreddit_id { get; set; }
            public object approved_at_utc { get; set; }
            public bool send_replies { get; set; }
            public object mod_reason_by { get; set; }
            public object banned_by { get; set; }
            public object num_reports { get; set; }
            public object removal_reason { get; set; }
            public int thumbnail_width { get; set; }
            public string subreddit { get; set; }
            public object selftext_html { get; set; }
            public string selftext { get; set; }
            public object likes { get; set; }
            public object suggested_sort { get; set; }
            public List<object> user_reports { get; set; }
            public object secure_media { get; set; }
            public bool is_reddit_media_domain { get; set; }
            public bool saved { get; set; }
            public string id { get; set; }
            public object banned_at_utc { get; set; }
            public object mod_reason_title { get; set; }
            public object view_count { get; set; }
            public bool archived { get; set; }
            public bool clicked { get; set; }
            public bool no_follow { get; set; }
            public string title { get; set; }
            public int num_crossposts { get; set; }
            public string link_flair_text { get; set; }
            public List<object> mod_reports { get; set; }
            public bool can_mod_post { get; set; }
            public bool is_crosspostable { get; set; }
            public bool pinned { get; set; }
            public int score { get; set; }
            public object approved_by { get; set; }
            public bool over_18 { get; set; }
            public object report_reasons { get; set; }
            public string domain { get; set; }
            public bool hidden { get; set; }
            public Preview preview { get; set; }
            public string thumbnail { get; set; }
            public bool edited { get; set; }
            public string link_flair_css_class { get; set; }
            public string author_flair_css_class { get; set; }
            public bool contest_mode { get; set; }
            public int gilded { get; set; }
            public int downs { get; set; }
            public bool brand_safe { get; set; }
            public object secure_media_embed { get; set; }
            public object media_embed { get; set; }
            public string post_hint { get; set; }
            public string author_flair_text { get; set; }
            public bool stickied { get; set; }
            public bool can_gild { get; set; }
            public int thumbnail_height { get; set; }
            public string parent_whitelist_status { get; set; }
            public string name { get; set; }
            public bool spoiler { get; set; }
            public string permalink { get; set; }
            public string subreddit_type { get; set; }
            public bool locked { get; set; }
            public bool hide_score { get; set; }
            public double created { get; set; }
            public string url { get; set; }
            public string whitelist_status { get; set; }
            public bool quarantine { get; set; }
            public string author { get; set; }
            public double created_utc { get; set; }
            public string subreddit_name_prefixed { get; set; }
            public int ups { get; set; }
            public object media { get; set; }
            public int num_comments { get; set; }
            public bool is_self { get; set; }
            public bool visited { get; set; }
            public object mod_note { get; set; }
            public bool is_video { get; set; }
            public object distinguished { get; set; }
        }

        public class Child
        {
            public string kind { get; set; }
            public Data2 data { get; set; }
        }

        public class Data
        {
            public string after { get; set; }
            public int dist { get; set; }
            public string modhash { get; set; }
            public string whitelist_status { get; set; }
            public List<Child> children { get; set; }
            public object before { get; set; }
        }

        public class RootObject
        {
            public string kind { get; set; }
            public Data data { get; set; }
        }
    }
}
