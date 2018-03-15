using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DiscordBot
{
    /// <summary>
    /// Contains and manages configurations.
    /// </summary>
    class ConfigLoader : Killable
    {
        
        const string FLAGS_FILE = "Files/Meta/flags.json";
        const string VALUES_FILE = "Files/Meta/values.json";

        private ConcurrentDictionary<string, bool> flags;
        private ConcurrentDictionary<string, string> values;

        public ConfigLoader()
        {
            try
            {
                var json = File.ReadAllText(FLAGS_FILE);
                flags = JsonConvert.DeserializeObject<ConcurrentDictionary<string, bool>>(json);
                Log.Success("Loaded configuration flags.");
            }
            catch (Exception e)
            {
                Log.Warning("Could not load the configuration flags file.");
                Log.Warning(e.ToString());

                flags = new ConcurrentDictionary<string, bool>();
            }
            try
            {
                var json = File.ReadAllText(VALUES_FILE);
                values = JsonConvert.DeserializeObject<ConcurrentDictionary<string, string>>(json);
                Log.Success("Loaded configuration settings.");
            }
            catch (Exception e)
            {
                Log.Warning("Could not load the configuration values file.");
                Log.Warning(e.ToString());

                values = new ConcurrentDictionary<string, string>();
            }

            if (!flags.ContainsKey("debug"))
                flags["debug"] = true;

            if (!flags.ContainsKey("filelog"))
                flags["filelog"] = true;
        }

        public void Kill()
        {
            Save();
        }

        public void Save()
        {
            if (!Directory.Exists("Files/Meta/"))
                Directory.CreateDirectory("Files/Meta/");

            try
            {
                var json = JsonConvert.SerializeObject(flags, Formatting.Indented);
                File.WriteAllText(FLAGS_FILE, json);
            }
            catch (Exception e)
            {
                Log.Error("Failed to save configuration flags!");
                if (Debug())
                    Log.Error(e.ToString());
            }
            try
            {
                var json = JsonConvert.SerializeObject(values, Formatting.Indented);
                File.WriteAllText(VALUES_FILE, json);
            }
            catch (Exception e)
            {
                Log.Error("Failed to save configuration settings!");
                if (Debug())
                    Log.Error(e.ToString());
            }
        }

        /// <summary>
        /// Returns the value of a flag
        /// </summary>
        /// <param name="flag"></param>
        /// <returns>1 if the value is true, -1 if the value is <c>false</c>, 0 if the flag doesn't exist.</returns>
        public int GetFlag(string flag)
        {
            if (flags.ContainsKey(flag))
                return flags[flag] ? 1 : -1;
            return 0;
        }

        /// <summary>
        /// Toggles the value of a flag.
        /// </summary>
        /// <param name="flag">The flag to toggle</param>
        /// <returns>1 if the new value is true, -1 if the new value is <c>false</c>, 0 if the flag doesn't exist.</returns>
        public int ToggleFlag(string flag)
        {
            if(flags.ContainsKey(flag))
            {
                var f = flags[flag];
                flags[flag] = !f;
                return !f ? 1 : -1;
            }
            return 0;
        }

        /// <summary>
        /// Creates a new flag.
        /// </summary>
        /// <param name="flag">The flag name.</param>
        /// <param name="defaultValue">The value to set to the flag.</param>
        /// <returns><c>true</c> if the flag was created, <c>false</c> if otherwise.</returns>
        public bool CreateFlag(string flag, bool defaultValue)
        {
            if(!flags.ContainsKey(flag))
            {
                flags[flag] = defaultValue;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attempts to remove a flag from the configuration.
        /// </summary>
        /// <param name="flag">The flag to remove.</param>
        /// <returns><c>true</c> if the flag was removed, <c>false</c> if otherwise.</returns>
        public bool DeleteFlag(string flag)
        {
            if(flags.ContainsKey(flag))
            {
                flags.TryRemove(flag, out bool o);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets a list of flags.
        /// </summary>
        /// <returns>The list of flags.</returns>
        public IEnumerable<string> ListFlags()
        {
            return flags.Keys;
        }

        /// <summary>
        /// Gets the string associated with a configuration value.
        /// </summary>
        /// <param name="valueType">The value to return.</param>
        /// <returns>The <c>string</c> value if it exists, <c>null</c> if otherwise.</returns>
        public string GetValue(string valueType)
        {
            return values.ContainsKey(valueType) ? values[valueType] : null;
        }

        /// <summary>
        /// Sets the value of a configuration.
        /// </summary>
        /// <param name="valueType">The type to set.</param>
        /// <param name="newValue">The new value.</param>
        /// <returns><c>true</c> if the update was a success, <c>false</c> if not.</returns>
        public bool SetValue(string valueType, string newValue)
        {
            if (values.ContainsKey(valueType))
            {
                values[valueType] = newValue;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Creates a new configuration value.
        /// </summary>
        /// <param name="valueType">The new type to add.</param>
        /// <param name="newValue">The value the type will have.</param>
        /// <returns><c>true</c> if it was added, <c>false</c> if not.</returns>
        public bool CreateValue(string valueType, string newValue)
        {
            if(!values.ContainsKey(valueType))
            {
                values[valueType] = newValue;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attempts to remove a setting from the configuration.
        /// </summary>
        /// <param name="value">The value to remove.</param>
        /// <returns><c>true</c> if the value was removed, <c>false</c> if otherwise.</returns>
        public bool DeleteValue(string value)
        {
            if (values.ContainsKey(value))
            {
                values.TryRemove(value, out string s);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets a list of settings.
        /// </summary>
        /// <returns>The list of settings.</returns>
        public IEnumerable<string> ListValues()
        {
            return values.Keys;
        }

        /// <summary>
        /// Wether or not the bot is in debug mode.
        /// </summary>
        /// <returns><c>true</c> if it is in debug mode, <c>false</c> if not.</returns>
        public bool Debug()
        {
            return flags["debug"];
        }

        public bool FileLogging()
        {
            return flags["filelog"];
        }

    }
}
