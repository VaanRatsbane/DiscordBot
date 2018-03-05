using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DiscordBot.Modules;
using Newtonsoft.Json;

namespace DiscordBot
{
    class ModuleManager : Killable
    {

        const string MODULES_FILE = "Files\\Meta\\loadedModules.json";

        List<string> modules; //hardcoded for ease of use
        ConcurrentDictionary<string, bool> isLoaded; //saved data

        public ModuleManager()
        {
            modules = new List<string>();
            modules.Add("math");

            try
            {
                var json = File.ReadAllText(MODULES_FILE);
                isLoaded = JsonConvert.DeserializeObject<ConcurrentDictionary<string, bool>>(json);
                Log.Success("Loaded module data.");
            }
            catch(Exception e)
            {
                Log.Warning("Could not load the ModulesLoaded file. Initializing...");
                if (Program.cfg.Debug())
                    Log.Warning(e.ToString());

                isLoaded = new ConcurrentDictionary<string, bool>();
                foreach(var m in modules)
                    isLoaded[m] = true;

            }

        }

        public void Kill()
        {
            try
            {
                var json = JsonConvert.SerializeObject(isLoaded, Formatting.Indented);
                File.WriteAllText(MODULES_FILE, json);
                Log.Success("Saved module data.");
            }
            catch(Exception e)
            {
                Log.Error("Failed to save ModulesLoaded file!");
                if (Program.cfg.Debug())
                    Log.Error(e.ToString());
            }
        }

        public bool HasModule(string moduleName)
        {
            return modules.Contains(moduleName);
        }

        public bool Activate(string moduleName)
        {

            if (!isLoaded[moduleName])
            {
                isLoaded[moduleName] = true;
                reg(moduleName);
                return true;
            }
            else
                return false;
        }

        public bool Deactivate(string moduleName)
        {
            if (isLoaded[moduleName])
            {
                isLoaded[moduleName] = false;
                unreg(moduleName);
                return true;
            }
            else
                return false;
        }

        private void reg(string moduleName)
        {
            switch (moduleName)
            {
                case "math":
                    Program._commands.RegisterCommands<MathModule>();
                    break;

                default: break;
            }
        }

        private void unreg(string moduleName)
        {
            switch(moduleName)
            {
                case "math":
                    Program._commands.UnregisterCommands<MathModule>();
                    break;

                default: break;
            }
        }
    }
}
