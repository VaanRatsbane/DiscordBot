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

        const string MODULES_FILE = "Files/Meta/loadedModules.json";

        List<string> modules; //hardcoded for ease of use
        ConcurrentDictionary<string, bool> isLoaded; //saved data

        public ModuleManager()
        {
            modules = new List<string>();
            modules.Add("math");
            modules.Add("admin");
            modules.Add("chat");
            modules.Add("info");
            modules.Add("api");
            modules.Add("tools");
            modules.Add("scheduler");

            try
            {
                var json = File.ReadAllText(MODULES_FILE);
                isLoaded = JsonConvert.DeserializeObject<ConcurrentDictionary<string, bool>>(json);

                if(modules.Count == isLoaded.Count)
                {
                    lock(isLoaded)
                    foreach (var pair in isLoaded)
                        if (!modules.Contains(pair.Key))
                            throw new Exception("Wrong module flags. Resetting.");
                }
                else //Not all flags loaded...
                {
                    throw new Exception("Not all module loading flags loaded. Resetting.");
                }

                Log.Success("Loaded module data.");

            }
            catch (Exception e)
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
            Save();
        }

        public void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(isLoaded, Formatting.Indented);
                File.WriteAllText(MODULES_FILE, json);
            }
            catch (Exception e)
            {
                Log.Error("Failed to save ModulesLoaded file!");
                if (Program.cfg.Debug())
                    Log.Error(e.ToString());
            }
        }

        public string Print()
        {
            string result = "";
            lock(isLoaded)
            foreach (var pair in isLoaded)
                result += "[" + (pair.Value ? "ON" : "OFF") + "]" + pair.Key + " | ";
            return result;
        }

        public bool HasModule(string moduleName)
        {
            return modules.Contains(moduleName);
        }

        public bool ModuleState(string moduleName)
        {
            return isLoaded[moduleName];
        }

        public bool Activate(string moduleName)
        {

            if (!isLoaded[moduleName])
            {
                isLoaded[moduleName] = true;
                Reg(moduleName);
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
                Unreg(moduleName);
                return true;
            }
            else
                return false;
        }

        private void Reg(string moduleName)
        {
            switch (moduleName)
            {
                case "math":
                    Program._commands.RegisterCommands<MathModule>();
                    break;

                case "admin":
                    Program._commands.RegisterCommands<AdminModule>();
                    break;

                case "chat":
                    Program._commands.RegisterCommands<ChatModule>();
                    break;

                case "info":
                    Program._commands.RegisterCommands<InfoModule>();
                    break;

                case "api":
                    Program._commands.RegisterCommands<APIModule>();
                    break;

                case "tools":
                    Program._commands.RegisterCommands<ToolsModule>();
                    break;

                case "scheduler":
                    Program._commands.RegisterCommands<SchedulerModule>();
                    break;

                default: break;
            }
        }

        private void Unreg(string moduleName)
        {
            switch(moduleName)
            {
                case "math":
                    Program._commands.UnregisterCommands<MathModule>();
                    break;

                case "admin":
                    Program._commands.UnregisterCommands<AdminModule>();
                    break;

                case "chat":
                    Program._commands.UnregisterCommands<ChatModule>();
                    break;

                case "info":
                    Program._commands.UnregisterCommands<InfoModule>();
                    break;

                case "api":
                    Program._commands.UnregisterCommands<APIModule>();
                    break;

                case "tools":
                    Program._commands.UnregisterCommands<ToolsModule>();
                    break;

                case "scheduler":
                    Program._commands.UnregisterCommands<SchedulerModule>();
                    break;

                default: break;
            }
        }
    }
}
