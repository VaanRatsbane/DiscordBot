using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace DiscordBot
{
    /// <summary>
    /// Contains and manages API keys
    /// </summary>
    class Keys : Killable
    {

        private const string KEYS_FILE = "Files/Meta/keys.json";

        private ConcurrentDictionary<string, string> keys;

        public Keys()
        {
            try
            {
                var json = File.ReadAllText(KEYS_FILE);
                keys = JsonConvert.DeserializeObject<ConcurrentDictionary<string, string>>(json);
                Log.Success("Loaded API keys.");
            }
            catch(Exception e)
            {
                Log.Warning("Could not load the keys file.");
                if (Program.cfg.Debug())
                    Log.Warning(e.ToString());

                Log.Input("Please insert the discord bot key (empty to exit):");
                string dbotkey = Console.ReadLine();

                keys = new ConcurrentDictionary<string, string>();
                keys["discord"] = dbotkey;
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
                var json = JsonConvert.SerializeObject(keys, Formatting.Indented);
                File.WriteAllText(KEYS_FILE, json);
            }
            catch (Exception e)
            {
                Log.Error("Failed to save API Keys!");
                if (Program.cfg.Debug())
                    Log.Error(e.ToString());
            }
        }

        /// <summary>
        /// Returns the value associated with a key type.
        /// </summary>
        /// <param name="keytype">The key type to search for.</param>
        /// <returns>The key associated with the type, or <c>null</c> in case it does not exist.</returns>
        public string GetKey(string keytype)
        {
            return keys[keytype];
        }

        /// <summary>
        /// Sets a key with a new value. Doesn't allow insertion of new keys.
        /// </summary>
        /// <param name="keytype">The key type</param>
        /// <param name="keyvalue">The new value</param>
        /// <returns><c>true</c> if the key was updated, <c>false</c> if otherwise.</returns>
        public bool SetKey(string keytype, string keyvalue)
        {
            if (keys.ContainsKey(keytype))
            {
                keys[keytype] = keyvalue;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Creates a key with a set value. Doesn't allow updating of existing keys.
        /// </summary>
        /// <param name="keytype">The key type</param>
        /// <param name="keyvalue">The key value</param>
        /// <returns><c>true</c> if the key was created, <c>false</c> if otherwise.</returns>
        public bool CreateKey(string keytype, string keyvalue)
        {
            if(!keys.ContainsKey(keytype))
            {
                keys[keytype] = keyvalue;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removed an API key.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>The value the key used to hold.</returns>
        public string DeleteKey(string key)
        {
            string value = null;
            if (keys.ContainsKey(key))
                keys.TryRemove(key, out value);
            return value;
        }

        /// <summary>
        /// Gets a list of all API Key names.
        /// </summary>
        /// <returns>A list of api key names.</returns>
        public IEnumerable<string> ListKeys()
        {
            return keys.Keys;
        }

    }
}
