using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace DiscordBot
{
    public static class Extensions
    {

        public static int GetNthIndex(this string s, char t, int n, bool startFromEnd)
        {
            int count = 0;
            if(!startFromEnd)
                for (int i = 0; i < s.Length; i++)
                {
                    if (s[i] == t)
                    {
                        count++;
                        if (count == n)
                        {
                            return i;
                        }
                    }
                }
            else
                for (int i = s.Length-1; i >= 0; i--)
                {
                    if (s[i] == t)
                    {
                        count++;
                        if (count == n)
                        {
                            return i;
                        }
                    }
                }
            return -1;
        }

        public static string GetFullIdentifier(this DiscordUser user)
        {
            return $"{user.Username}#{user.Discriminator}";
        }

        public static string Between(this string text, string before, string after)
        {
            int pFrom = text.IndexOf(before) + before.Length;
            int pTo = text.LastIndexOf(after);
            return text.Substring(pFrom, pTo - pFrom);
        }

        public static object XmlDeserializeFromString(this string objectData, Type type)
        {
            var serializer = new XmlSerializer(type);
            object result;

            using (TextReader reader = new StringReader(objectData))
            {
                result = serializer.Deserialize(reader);
            }

            return result;
        }

        public static decimal ToCurrency(this decimal currency)
        {
            return Math.Truncate(100 * currency) / 100;
        }

    }
}
