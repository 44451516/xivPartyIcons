using System;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Memory;
using Dalamud.Plugin;
using PartyIcons.Api;
using PartyIcons.Runtime;
using PartyIcons.Stylesheet;
using PartyIcons.Utils;
using PartyIcons.View;
using SigScanner = Dalamud.Game.SigScanner;

namespace PartyIcons;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "PartyIcons";
    private const string commandName = "/ppi";
    
    public PluginAddressResolver Address { get; }
    private Configuration Configuration { get; }

    private readonly PartyListHUDView _partyHUDView;

    private readonly PartyListHUDUpdater _partyListHudUpdater;

    private readonly PlayerContextMenu _contextMenu;
    private readonly PluginUI _ui;
    private readonly NameplateUpdater _nameplateUpdater;
    private readonly NPCNameplateFixer _npcNameplateFixer;
    private readonly NameplateView _nameplateView;
    private readonly RoleTracker _roleTracker;
    private readonly ViewModeSetter _modeSetter;
    private readonly ChatNameUpdater _chatNameUpdater;
    private readonly PlayerStylesheet _playerStylesheet;

    public Plugin(DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
        
        Configuration = Service.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(Service.PluginInterface);
        Configuration.OnSave += OnConfigurationSave;

        Service.CommandManager.AddHandler(commandName, new CommandInfo(OnCommand)
        {
            HelpMessage =
                "opens configuration window; \"reset\" or \"r\" resets all assignments; \"debug\" prints debugging info"
        });

        Address = new PluginAddressResolver();
        Address.Setup(Service.SigScanner);

        _playerStylesheet = new PlayerStylesheet(Configuration);

        _ui = new PluginUI(Configuration, _playerStylesheet);
        
        
        Service.PluginInterface.Inject(_ui);

        XivApi.Initialize(this, Address);

        SeStringUtils.Initialize();

        _partyHUDView = new PartyListHUDView(Service.GameGui, _playerStylesheet);
        Service.PluginInterface.Inject(_partyHUDView);

        _roleTracker = new RoleTracker(Configuration);
        Service.PluginInterface.Inject(_roleTracker);

        _nameplateView = new NameplateView(_roleTracker, Configuration, _playerStylesheet, _partyHUDView);
        Service.PluginInterface.Inject(_nameplateView);

        _chatNameUpdater = new ChatNameUpdater(_roleTracker, _playerStylesheet);
        Service.PluginInterface.Inject(_chatNameUpdater);

        _partyListHudUpdater = new PartyListHUDUpdater(_partyHUDView, _roleTracker, Configuration);
        Service.PluginInterface.Inject(_partyListHudUpdater);

        _nameplateUpdater = new NameplateUpdater(Address, _nameplateView);
        Service.PluginInterface.Inject(_nameplateUpdater);

        _npcNameplateFixer = new NPCNameplateFixer(_nameplateView);

        _contextMenu = new PlayerContextMenu(_roleTracker, Configuration, _playerStylesheet);
        Service.PluginInterface.Inject(_contextMenu);

        _ui.Initialize();
        Service.PluginInterface.UiBuilder.Draw += _ui.DrawSettingsWindow;
        Service.PluginInterface.UiBuilder.OpenConfigUi += _ui.OpenSettingsWindow;

        _roleTracker.OnAssignedRolesUpdated += OnAssignedRolesUpdated;

        _modeSetter = new ViewModeSetter(_nameplateView, Configuration, _chatNameUpdater, _partyListHudUpdater);
        Service.PluginInterface.Inject(_modeSetter);

        _partyListHudUpdater.Enable();
        _modeSetter.Enable();
        _roleTracker.Enable();
        _nameplateUpdater.Enable();
        _npcNameplateFixer.Enable();
        _chatNameUpdater.Enable();
        _contextMenu.Enable();
    }

    public void Dispose()
    {
        _roleTracker.OnAssignedRolesUpdated -= OnAssignedRolesUpdated;

        _partyHUDView.Dispose();
        _partyListHudUpdater.Dispose();
        _chatNameUpdater.Dispose();
        _contextMenu.Dispose();
        _nameplateUpdater.Dispose();
        _npcNameplateFixer.Dispose();
        _roleTracker.Dispose();
        _modeSetter.Dispose();
        Service.PluginInterface.UiBuilder.Draw -= _ui.DrawSettingsWindow;
        Service.PluginInterface.UiBuilder.OpenConfigUi -= _ui.OpenSettingsWindow;
        _ui.Dispose();

        SeStringUtils.Dispose();
        XivApi.DisposeInstance();

        Service.CommandManager.RemoveHandler(commandName);
        Configuration.OnSave -= OnConfigurationSave;
    }

    private void OnConfigurationSave()
    {
        _modeSetter.ForceRefresh();
    }

    private void OnAssignedRolesUpdated()
    {
    }

    private unsafe  void OnCommand(string command, string arguments)
    {
        arguments = arguments.Trim().ToLower();

        string[] strings = arguments.Split(" ");

        if (arguments == "" || arguments == "config")
        {
            _ui.ToggleSettingsWindow();
        }
        else if (arguments == "reset" || arguments == "r")
        {
            _roleTracker.ResetOccupations();
            _roleTracker.ResetAssignments();
            _roleTracker.CalculateUnassignedPartyRoles();
            Service.ChatGui.Print("Occupations are reset, roles are auto assigned.");
        }
        else if (arguments == "dbg state")
        {
            Service.ChatGui.Print($"Current mode is {_nameplateView.PartyMode}, party count {Service.PartyList.Length}");
            Service.ChatGui.Print(_roleTracker.DebugDescription());
        }
        else if (arguments == "dbg party")
        {
            Service.ChatGui.Print(_partyHUDView.GetDebugInfo());
        }
        
        else if (strings[0] == "name")
        {
            if (strings.Length > 1)
            {
                var localPlayer = Service.ClientState.LocalPlayer;
                if (localPlayer != null)
                {
                    Configuration.名字伪装me = strings[1].Trim();
                    Configuration.Save();
                    FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* Struct = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*) localPlayer.Address;
                    MemoryHelper.WriteSeString((IntPtr)Struct->Name, SeStringUtils.Text(Configuration.名字伪装me));
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