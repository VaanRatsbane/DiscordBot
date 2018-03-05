using System.Collections.Generic;
using System.Reflection;
using DSharpPlus.CommandsNext;

namespace DiscordBot
{

    public static class CommandsNextExtensionExtensions
    {

        public static void UnregisterCommands<T>(this CommandsNextModule cnext) where T : class
        {
            // Command Dictionary Property
            var cdict = cnext.GetType().GetProperty("TopLevelCommands", BindingFlags.NonPublic | BindingFlags.Instance);
            // Original Dictionary
            var odict = cdict.GetValue(cnext) as Dictionary<string,Command>;
            // Temporary Dictionary
            var tdict = new Dictionary<string, Command>();
            // Register commands into empty dictionary
            cdict.SetValue(cnext, tdict);
            cnext.RegisterCommands<T>();
            // Iterate over temporary dictionary and remove commands from the original dictionary
            foreach (var x in tdict)
                odict.Remove(x.Key);
            // Put original dictionary back
            cdict.SetValue(cnext, odict);
        }
    }
}