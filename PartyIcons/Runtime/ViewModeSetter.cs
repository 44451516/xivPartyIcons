﻿using System;
using System.Linq;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Memory;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using PartyIcons.Utils;
using PartyIcons.View;

namespace PartyIcons.Runtime;

public sealed class ViewModeSetter
{


    private readonly NameplateView _nameplateView;
    private readonly Configuration _configuration;
    private readonly ChatNameUpdater _chatNameUpdater;
    private readonly PartyListHUDUpdater _partyListHudUpdater;

    private ExcelSheet<ContentFinderCondition> _contentFinderConditionsSheet;

    public ViewModeSetter(NameplateView nameplateView, Configuration configuration, ChatNameUpdater chatNameUpdater,
        PartyListHUDUpdater partyListHudUpdater)
    {
        _nameplateView = nameplateView;
        _configuration = configuration;
        _chatNameUpdater = chatNameUpdater;
        _partyListHudUpdater = partyListHudUpdater;
    }

    public void Enable()
    {
        _contentFinderConditionsSheet = Service.DataManager.GameData.GetExcelSheet<ContentFinderCondition>();

        ForceRefresh();
        Service.ClientState.TerritoryChanged += OnTerritoryChanged;
    }

    public void ForceRefresh()
    {
        _nameplateView.OthersMode = _configuration.NameplateOthers;
        _chatNameUpdater.OthersMode = _configuration.ChatOthers;

        OnTerritoryChanged(null, 0);
    }

    public void Disable()
    {
        Service. ClientState.TerritoryChanged -= OnTerritoryChanged;
    }

    public void Dispose()
    {
        Disable();
    }

    private unsafe  void OnTerritoryChanged(object? sender, ushort e)
    {


        var content =
            _contentFinderConditionsSheet.FirstOrDefault(t => t.TerritoryType.Row == Service.ClientState.TerritoryType);

        if (content == null)
        {
            PluginLog.Information($"Content null {Service.ClientState.TerritoryType}");
            _nameplateView.PartyMode = _configuration.NameplateOverworld;
            _chatNameUpdater.PartyMode = _configuration.ChatOverworld;
        }
        else
        {
            if (_configuration.ChatContentMessage)
            {
                Service.ChatGui.Print($"进入 {content.Name}.");
            }

            var memberType = content.ContentMemberType.Row;

            if (content.RowId == 16 || content.RowId == 15)
            {
                // Praetorium and Castrum Meridianum
                memberType = 2;
            }

            if (content.RowId == 735 || content.RowId == 778)
            {
                // Bozja
                memberType = 127;
            }

            PluginLog.Debug(
                $"Territory changed {content.Name} (id {content.RowId} type {content.ContentType.Row}, terr {Service.ClientState.TerritoryType}, memtype {content.ContentMemberType.Row}, overriden {memberType})");

            switch (memberType)
            {
                case 2:
                    _nameplateView.PartyMode = _configuration.NameplateDungeon;
                    _nameplateView.OthersMode = _configuration.NameplateOthers;
                    _chatNameUpdater.PartyMode = _configuration.ChatDungeon;

                    break;

                case 3:
                    _nameplateView.PartyMode = _configuration.NameplateRaid;
                    _nameplateView.OthersMode = _configuration.NameplateOthers;
                    _chatNameUpdater.PartyMode = _configuration.ChatRaid;

                    break;

                case 4:
                    _nameplateView.PartyMode = _configuration.NameplateAllianceRaid;
                    _nameplateView.OthersMode = _configuration.NameplateOthers;
                    _chatNameUpdater.PartyMode = _configuration.ChatAllianceRaid;

                    break;

                case 127:
                    _nameplateView.PartyMode = _configuration.NameplateBozjaParty;
                    _nameplateView.OthersMode = _configuration.NameplateBozjaOthers;
                    _chatNameUpdater.PartyMode = _configuration.ChatOverworld;

                    break;

                default:
                    _nameplateView.PartyMode = _configuration.NameplateDungeon;
                    _nameplateView.OthersMode = _configuration.NameplateOthers;
                    _chatNameUpdater.PartyMode = _configuration.ChatDungeon;

                    break;
            }
        }

        _partyListHudUpdater.UpdateHUD = _nameplateView.PartyMode == NameplateMode.RoleLetters ||
                                         _nameplateView.PartyMode == NameplateMode.SmallJobName ||
                                         _nameplateView.PartyMode == NameplateMode.SmallJobIcon ||
                                         _nameplateView.PartyMode == NameplateMode.SmallJobIconAndRole;

        PluginLog.Debug(
            $"Setting modes: nameplates party {_nameplateView.PartyMode} others {_nameplateView.OthersMode}, chat {_chatNameUpdater.PartyMode}, update HUD {_partyListHudUpdater.UpdateHUD}");
    }
}
