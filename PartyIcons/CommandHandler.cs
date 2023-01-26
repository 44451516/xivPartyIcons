using System;
using Dalamud.Game.Command;
using Dalamud.Logging;
using Dalamud.Memory;
using PartyIcons.Configuration;
using PartyIcons.Utils;

namespace PartyIcons;

public class CommandHandler : IDisposable
{
    private const string commandName = "/ppi";
    
    public CommandHandler()
    {
        Service.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
        {
            HelpMessage =
                "opens configuration window; \"reset\" or \"r\" resets all assignments; \"debug\" prints debugging info"
        });
    }
    
    public void Dispose()
    {
        Service.CommandManager.RemoveHandler(commandName);
    }

    private unsafe void OnCommand(string command, string arguments)
    {
        arguments = arguments.Trim().ToLower();
        string[] strings = arguments.Split(" ");
        
        if (arguments == "" || arguments == "config")
        {
            Plugin.SettingsWindow.ToggleSettingsWindow();
        }
        else if (arguments == "reset" || arguments == "r")
        {
            Plugin.RoleTracker.ResetOccupations();
            Plugin.RoleTracker.ResetAssignments();
            Plugin.RoleTracker.CalculateUnassignedPartyRoles();
            Service.ChatGui.Print("Occupations are reset, roles are auto assigned.");
        }
        else if (arguments == "dbg state")
        {
            Service.ChatGui.Print($"Current mode is {Plugin.NameplateView.PartyMode}, party count {Service.PartyList.Length}");
            Service.ChatGui.Print(Plugin.RoleTracker.DebugDescription());
        }
        else if (arguments == "dbg party")
        {
            Service.ChatGui.Print(Plugin.PartyHudView.GetDebugInfo());
        }
        else if (arguments.Contains("set"))
        {
            var argv = arguments.Split(' ');

            if (argv.Length == 2)
            {
                try
                {
                    Plugin.NameplateUpdater.DebugIcon = int.Parse(argv[1]);
                    PluginLog.Verbose($"Set debug icon to {Plugin.NameplateUpdater.DebugIcon}");
                }
                catch (Exception)
                {
                    PluginLog.Verbose("Invalid icon id given for debug icon.");
                    Plugin.NameplateUpdater.DebugIcon = -1;
                }
            }
            else
            {
                Plugin.NameplateUpdater.DebugIcon = -1;
            }
        }
        else if (strings[0] == "name")
        {
            if (strings.Length > 1)
            {
                var localPlayer = Service.ClientState.LocalPlayer;
                if (localPlayer != null)
                {
                    Plugin.Settings.名字伪装me = strings[1].Trim();
                    Plugin.Settings.Save();
                    FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* Struct = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*) localPlayer.Address;
                    MemoryHelper.WriteSeString((IntPtr)Struct->Name, SeStringUtils.Text(Plugin.Settings.名字伪装me));
                }

            }

        }else if (arguments == "xiaoduiname")
        {
            
            var localPlayer = Service.ClientState.LocalPlayer;
            if (localPlayer == null)
            {
                return;
            }
            
            foreach (var member in Service.PartyList)
            {
                if (member.ObjectId == localPlayer.ObjectId)
                {
                    continue;
                }

                if (member.ObjectId > 0)
                {
                    var jobName = member.ClassJob.GameData.Name;
                    if (jobName.ToString() != member.Name.ToString())
                    {
                        FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* playerAddress = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)member.GameObject.Address;
                        MemoryHelper.WriteString((IntPtr)playerAddress->Name, jobName);
                    }
                }
            }
        }
    }
}