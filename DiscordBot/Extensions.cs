using System;
using System.Collections.Generic;
using System.Text;

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

    }
}
