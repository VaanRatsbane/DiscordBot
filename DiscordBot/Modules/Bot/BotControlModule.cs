using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    class BotControlModule
    {

        [Command("setnick"), Description("Changes my nickname."), RequirePermissions(DSharpPlus.Permissions.BanMembers)]
        public async Task SetNick(CommandContext ctx, [Description("The new name I will display."), RemainingText]string nickname)
        {
            foreach(var m in ctx.Guild.Members)
                if(m.Id == ctx.Client.CurrentUser.Id)
                {
                    await m.ModifyAsync(nickname: nickname);
                    break;
                }
        }

        [Command("setstate"), Description("Changes my public state."), RequirePermissions(DSharpPlus.Permissions.BanMembers)]
        public async Task SetState(CommandContext ctx, [Description("The state. Use online, dnd, idle or offline.")]string state)
        {
            switch (state.ToLowerInvariant())
            {
                case "online":
                    await ctx.Client.UpdateStatusAsync(user_status: DSharpPlus.Entities.UserStatus.Online);
                    break;

                case "dnd":
                    await ctx.Client.UpdateStatusAsync(user_status: DSharpPlus.Entities.UserStatus.DoNotDisturb);
                    break;

                case "idle":
                case "away":
                    await ctx.Client.UpdateStatusAsync(user_status: DSharpPlus.Entities.UserStatus.Idle);
                    break;

                case "invisible":
                case "offline":
                    await ctx.Client.UpdateStatusAsync(user_status: DSharpPlus.Entities.UserStatus.Invisible);
                    break;
            }
        }

        [Command("setgame"), Description("Changes my current game."), RequirePermissions(DSharpPlus.Permissions.BanMembers)]
        public async Task SetGame(CommandContext ctx, [Description("The game I will be playing.")]string game)
        {
            await ctx.Client.UpdateStatusAsync(game : new DSharpPlus.Entities.DiscordGame(game));
        }

        [Command("flag"), Description("Check the value of a configuration flag."), RequireOwner]
        public async Task Flag(CommandContext ctx, [Description("The flag to check the value of.")]string flag)
        {
            await ctx.TriggerTypingAsync();
            var flagValue = Program.cfg.GetFlag(flag.ToLowerInvariant());
            if (flagValue == 1)
                await ctx.RespondAsync("True");
            else if (flagValue == -1)
                await ctx.RespondAsync("False");
            else
                await ctx.RespondAsync("The flag does not exist.");
        }

        [Command("toggleflag"), Description("Toggles the value of an existing flag."), RequireOwner]
        public async Task toggleFlag(CommandContext ctx, [Description("The flag whose value to toggle.")]string flag)
        {
            await ctx.TriggerTypingAsync();
            var result = Program.cfg.ToggleFlag(flag.ToLowerInvariant());
            if (result == 1)
                await ctx.RespondAsync("The flag was set to True");
            else if (result == -1)
                await ctx.RespondAsync("The flag was set to False");
            else
                await ctx.RespondAsync("The flag does not exist.");

            if (result != 0)
                Log.Info($"{ctx.Member.Username}#{ctx.Member.Discriminator} toggled flag {flag}.");
        }

        [Command("createflag"), Description("Creates a new configuration flag."), RequireOwner]
        public async Task createFlag(CommandContext ctx, [Description("The flag to create.")]string flag, [Description("The new flag's starting value.")]bool defaultValue)
        {
            await ctx.TriggerTypingAsync();
            if (Program.cfg.CreateFlag(flag.ToLowerInvariant(), defaultValue))
            {
                await ctx.RespondAsync("Flag " + flag.ToLowerInvariant() + " created with value " + defaultValue.ToString() + ".");
                Log.Info($"{ctx.Member.Username}#{ctx.Member.Discriminator} created flag {flag} with value {defaultValue}.");
            }
            else
                await ctx.RespondAsync("Flag already exists.");
        }

        [Command("deleteflag"), Description("Deletes a configuration flag."), RequireOwner]
        public async Task deleteFlag(CommandContext ctx, [Description("The flag to remove.")]string flag)
        {
            await ctx.TriggerTypingAsync();
            var interactivity = ctx.Client.GetInteractivityModule();
            await ctx.RespondAsync("Are you sure you wish to remove this flag? It may break the bot! (yes or no)");
            var msg = await interactivity.WaitForMessageAsync(xm => xm.Author.Id == ctx.User.Id &&
                xm.Content.ToLowerInvariant() == "yes");
            if (msg != null)
            {
                if (Program.cfg.DeleteFlag(flag.ToLowerInvariant()))
                {
                    await ctx.RespondAsync("Flag deleted.");
                    Log.Info($"{ctx.Member.Username}#{ctx.Member.Discriminator} deleted the flag {flag}.");
                }
                else
                    await ctx.RespondAsync("No such flag exists.");
            }
            else
                await ctx.RespondAsync("Cancelled.");
        }

        [Command("listflags"), Description("Lists all configuration flags."), RequireOwner]
        public async Task listFlags(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            string printable = string.Empty;
            foreach(var flag in Program.cfg.ListFlags())
            {
                if ((printable + flag).Length > 2000)
                {
                    await ctx.RespondAsync(printable);
                    printable = flag + " ";
                }
                else
                    printable += flag + " ";
            }
            if (printable.Length > 0)
                await ctx.RespondAsync(printable);
        }

        [Command("setting"), Description("Check the value of a configuration setting."), RequireOwner]
        public async Task Setting(CommandContext ctx, [Description("The setting to check the value of.")]string setting)
        {
            await ctx.TriggerTypingAsync();
            var settingValue = Program.cfg.GetValue(setting.ToLowerInvariant());
            if (settingValue != null)
                await ctx.RespondAsync(settingValue);
            else
                await ctx.RespondAsync("The setting does not exist.");
        }

        [Command("setsetting"), Description("Sets the value of an existing setting."), RequireOwner]
        public async Task setSetting(CommandContext ctx, [Description("The setting whose value to set.")]string setting, string newValue)
        {
            await ctx.TriggerTypingAsync();
            var result = Program.cfg.SetValue(setting.ToLowerInvariant(), newValue);
            if (result)
            {
                await ctx.RespondAsync("The setting was updated.");
                Log.Info($"{ctx.Member.Username}#{ctx.Member.Discriminator} updated the setting {setting}.");
            }
            else
                await ctx.RespondAsync("The setting does not exist.");
        }

        [Command("createsetting"), Description("Creates a new configuration setting."), RequireOwner]
        public async Task createSetting(CommandContext ctx, [Description("The setting to create.")]string setting, [Description("The new setting's starting value.")]string defaultValue)
        {
            await ctx.TriggerTypingAsync();
            if (Program.cfg.CreateValue(setting.ToLowerInvariant(), defaultValue))
            {
                await ctx.RespondAsync("Setting " + setting.ToLowerInvariant() + " created with value " + defaultValue.ToString() + ".");
                Log.Info($"{ctx.Member.Username}#{ctx.Member.Discriminator} created setting {setting} with value {defaultValue}.");
            }
            else
                await ctx.RespondAsync("Flag already exists.");
        }

        [Command("deletesetting"), Description("Deletes a configuration setting."), RequireOwner]
        public async Task deleteSetting(CommandContext ctx, [Description("The setting to remove.")]string setting)
        {
            await ctx.TriggerTypingAsync();
            var interactivity = ctx.Client.GetInteractivityModule();
            await ctx.RespondAsync("Are you sure you wish to remove this setting? It may break the bot! (yes or no)");
            var msg = await interactivity.WaitForMessageAsync(xm => xm.Author.Id == ctx.User.Id &&
                xm.Content.ToLowerInvariant() == "yes");
            if (msg != null)
            {
                if (Program.cfg.DeleteFlag(setting.ToLowerInvariant()))
                {
                    await ctx.RespondAsync("Setting deleted.");
                    Log.Info($"{ctx.Member.Username}#{ctx.Member.Discriminator} deleted the setting {setting}.");
                }
                else
                    await ctx.RespondAsync("No such flag exists.");
            }
            else
                await ctx.RespondAsync("Cancelled.");
        }

        [Command("listsettings"), Description("Lists all configuration settings."), RequireOwner]
        public async Task listSettings(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            string printable = string.Empty;
            foreach (var setting in Program.cfg.ListValues())
            {
                if ((printable + setting).Length > 2000)
                {
                    await ctx.RespondAsync(printable);
                    printable = setting + " ";
                }
                else
                    printable += setting + " ";
            }
            if (printable.Length > 0)
                await ctx.RespondAsync(printable);
        }

        [Command("key"), Description("Check the value of an API key."), RequireOwner]
        public async Task Key(CommandContext ctx, [Description("The key to check the value of.")]string key)
        {
            await ctx.TriggerTypingAsync();
            var keyValue = Program.keys.GetKey(key.ToLowerInvariant());
            if (keyValue != null)
            {
                await (await ctx.Member.CreateDmChannelAsync()).SendMessageAsync(key + ":\n" + keyValue);
                await ctx.RespondAsync("Sent by DM.");
            }
            else
                await ctx.RespondAsync("The setting does not exist.");
        }

        [Command("setkey"), Description("Sets the value of an existing API key."), RequireOwner]
        public async Task setKey(CommandContext ctx, [Description("The key whose value to set.")]string key, string newValue)
        {
            await ctx.TriggerTypingAsync();
            var result = Program.keys.SetKey(key.ToLowerInvariant(), newValue);
            if (result)
            {
                await ctx.RespondAsync("The key " + key + " was updated.");
                Log.Info($"{ctx.Member.Username}#{ctx.Member.Discriminator} updated the API key {key}.");
            }
            else
                await ctx.RespondAsync("The key " + key + " does not exist.");
            await ctx.Message.DeleteAsync();
        }

        [Command("createkey"), Description("Creates a new API key."), RequireOwner]
        public async Task createKey(CommandContext ctx, [Description("The key to create.")]string key, [Description("The new setting's starting value.")]string defaultValue)
        {
            await ctx.TriggerTypingAsync();
            if (Program.keys.CreateKey(key.ToLowerInvariant(), defaultValue))
            {
                await ctx.RespondAsync("API key " + key.ToLowerInvariant() + " created.");
                Log.Info($"{ctx.Member.Username}#{ctx.Member.Discriminator} created the API key {key}.");
            }
            else
                await ctx.RespondAsync("API key " + key.ToLowerInvariant() + " already exists.");
            await ctx.Message.DeleteAsync();
        }

        [Command("deletekey"), Description("Deletes an API key."), RequireOwner]
        public async Task deleteKey(CommandContext ctx, [Description("The API key to remove.")]string key)
        {
            await ctx.TriggerTypingAsync();

            if(key.ToLowerInvariant() == "discord")
            {
                await ctx.RespondAsync("You can't remove the bot token.");
                return;
            }

            var interactivity = ctx.Client.GetInteractivityModule();
            await ctx.RespondAsync("Are you sure you wish to remove this key? It may break the bot! (yes or no)");
            var msg = await interactivity.WaitForMessageAsync(xm => xm.Author.Id == ctx.User.Id &&
                xm.Content.ToLowerInvariant() == "yes");
            if (msg != null)
            {
                var value = Program.keys.DeleteKey(key.ToLowerInvariant());
                if (value != null)
                {
                    await (await ctx.Member.CreateDmChannelAsync()).SendMessageAsync(value);
                    await ctx.RespondAsync("Key deleted. Removed value sent by DM.");
                    Log.Info($"{ctx.Member.Username}#{ctx.Member.Discriminator} deleted the API key {key}.");
                }
                else
                    await ctx.RespondAsync("No such key exists.");
            }
            else
                await ctx.RespondAsync("Cancelled.");
        }

        [Command("listkeys"), Description("Lists all API keys."), RequireOwner]
        public async Task listKeys(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            string printable = string.Empty;
            foreach (var key in Program.keys.ListKeys())
            {
                if ((printable + key).Length > 2000)
                {
                    await ctx.RespondAsync(printable);
                    printable = key + " ";
                }
                else
                    printable += key + " ";
            }
            if (printable.Length > 0)
                await ctx.RespondAsync(printable);
        }

        [Command("quit"), Description("Closes the bot."), RequireOwner]
        public async Task Quit(CommandContext ctx)
        {
            await ctx.RespondAsync("Byebye!");
            Program.quitToken.Cancel();
        }

        [Command("enablemodule"), Description("Enable the commands of a module."), RequireOwner]
        public async Task RegisterModule(CommandContext ctx, string moduleName)
        {
            if (moduleName == "botcontrol")
            {
                await ctx.RespondAsync("The Bot Control module is always active.");
                return;
            }

            if (Program.moduleManager.HasModule(moduleName.ToLowerInvariant()))
            {

                if (Program.moduleManager.Activate(moduleName.ToLowerInvariant()))
                    await ctx.RespondAsync("Module activated.");
                else
                    await ctx.RespondAsync("Module is already activated.");
            }
            else
            {
                await ctx.RespondAsync("Module does not exist.");
            }
        }

        [Command("disablemodule"), Description("Disable the commands of a module."), RequireOwner]
        public async Task UnregisterModule(CommandContext ctx, string moduleName)
        {
            if (moduleName == "botcontrol")
            {
                await ctx.RespondAsync("The Bot Control module cannot be deactivated.");
                return;
            }

            if (Program.moduleManager.HasModule(moduleName.ToLowerInvariant()))
            {
                if (Program.moduleManager.Deactivate(moduleName.ToLowerInvariant()))
                    await ctx.RespondAsync("Module deactivated.");
                else
                    await ctx.RespondAsync("Module is already deactivated.");
            }
            else
            {
                await ctx.RespondAsync("Module does not exist.");
            }
        }

    }
}
