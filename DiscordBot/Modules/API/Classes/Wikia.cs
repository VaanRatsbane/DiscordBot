using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot.Modules.API.Classes
{
    class WikiaSearch
    {
        public class Item
        {
            public string Snippet { get { return snippet.Replace("&hellip;", "...").Replace("<span class=\"searchmatch\">", "**").Replace("</span>","**"); } }

            public int id { get; set; }
            public string title { get; set; }
            public string url { get; set; }
            public int ns { get; set; }
            public int quality { get; set; }
            public string snippet { get; set; }
        }

        public class RootObject
        {
            public int total { get; set; }
            public int batches { get; set; }
            public int currentBatch { get; set; }
            public int next { get; set; }
            public List<Item> items { get; set; }
        }
    }

    class WikiaArticle
    {
        public class Revision
        {
            public int id { get; set; }
            public string user { get; set; }
            public int user_id { get; set; }
            public string timestamp { get; set; }
        }

        public class OriginalDimensions
        {
            public int width { get; set; }
            public int height { get; set; }
        }

        public class RootObject
        {
            public int id { get; set; }
            public string title { get; set; }
            public int ns { get; set; }
            public string url { get; set; }
            public Revision revision { get; set; }
            public string type { get; set; }
            public string @abstract { get; set; }
            public string thumbnail { get; set; }
            public OriginalDimensions original_dimensions { get; set; }
        }
    }
}
