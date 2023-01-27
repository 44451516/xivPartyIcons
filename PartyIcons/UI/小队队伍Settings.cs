using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Dalamud.Game.ClientState.Party;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI;
using ImGuiNET;
using PartyIcons.Entities;

namespace PartyIcons.UI;

public sealed class 小队队伍Settings
{
    public unsafe void Draw小队队伍Settings()
    {
        var 小队队伍开关 = Plugin.Settings.小队队伍开关;
        
        if (ImGui.Checkbox("##小队队伍开关", ref 小队队伍开关))
        {
            Plugin.Settings.小队队伍开关 = 小队队伍开关;
            Plugin.Settings.Save();
        }

        ImGui.SameLine();
        ImGui.Text("启用");
        ImGui.SameLine();
        if (ImGui.Button("确定"))
        {
            Plugin.Settings.UpdateHUD = true;
        }

        
        // var displayJobNameInPartyList = Plugin.Settings.DisplayJobNameInPartyList;
        //
        // if (ImGui.Checkbox("##DisplayJobNameInPartyList", ref displayJobNameInPartyList))
        // {
        //     Plugin.Settings.DisplayJobNameInPartyList = displayJobNameInPartyList;
        //     Plugin.Settings.Save();
        // }

        // ImGui.SameLine();
        // ImGui.Text("用职业名称替换小队队伍");
        // SettingsWindow.ImGuiHelpTooltip("用职业名称填充小队队伍", true);

        
        for (uint i = 0; i < 8; i++)
        {
            var memberStruct = GetPartyMemberStruct(i);

            if (memberStruct.HasValue)
            {
                
                var nameString = memberStruct.Value.Name->NodeText.ToString();
                string fromName = StripSpecialCharactersFromName(nameString);
                ImGui.Text($"{fromName}");
                ImGui.SameLine();
                ImGui.SetNextItemWidth(200);
                ImGui.SetCursorPosX(100);
                switch (i)
                {
                    case 0:
                    {
                        ImGui.InputText($"##小队伪装_{i}", ref Plugin.Settings.PartyList0, 20);
                        break;
                    }
                    case 1:
                    {
                        ImGui.InputText($"##小队伪装_{i}", ref Plugin.Settings.PartyList1, 20);
                        break;
                    }
                    case 2:
                    {
                        ImGui.InputText($"##小队伪装_{i}", ref Plugin.Settings.PartyList2, 20);
                        break;
                    }
                    case 3:
                    {
                        ImGui.InputText($"##小队伪装_{i}", ref Plugin.Settings.PartyList3, 20);
                        break;
                    }
                    case 4:
                    {
                        ImGui.InputText($"##小队伪装_{i}", ref Plugin.Settings.PartyList4, 20);
                        break;
                    }
                    case 5:
                    {
                        ImGui.InputText($"##小队伪装_{i}", ref Plugin.Settings.PartyList5, 20);
                        break;
                    }
                    case 6:
                    {
                        ImGui.InputText($"##小队伪装_{i}", ref Plugin.Settings.PartyList6, 20);
                        break;
                    }
                    case 7:
                    {
                        ImGui.InputText($"##小队伪装_{i}", ref Plugin.Settings.PartyList7, 20);
                        break;
                    }
      
                }
                
               
            }
        }


        
        {
            var partyMemberStruct = GetPartyMemberStruct(1);
            if (partyMemberStruct.HasValue)
            {
                
                var nameString = partyMemberStruct.Value.Name->NodeText.ToString();
                
                for (var i = 0; i < nameString.Length; i++)
                {
                    ImGui.Text($"{i} -> {nameString[i]} -> {Convert.ToInt32(nameString[i])}");   
                }
                
                // string fromName = StripSpecialCharactersFromName(nameString);
              
    
            }
        }
        
    
    }
    private unsafe AddonPartyList.PartyListMemberStruct? GetPartyMemberStruct(uint idx)
    {
        var partyListAddon = (AddonPartyList*) Service.GameGui.GetAddonByName("_PartyList", 1);

        if (partyListAddon == null)
        {
            PluginLog.Warning("PartyListAddon null!");

            return null;
        }

        return idx switch
        {
            0 => partyListAddon->PartyMember.PartyMember0,
            1 => partyListAddon->PartyMember.PartyMember1,
            2 => partyListAddon->PartyMember.PartyMember2,
            3 => partyListAddon->PartyMember.PartyMember3,
            4 => partyListAddon->PartyMember.PartyMember4,
            5 => partyListAddon->PartyMember.PartyMember5,
            6 => partyListAddon->PartyMember.PartyMember6,
            7 => partyListAddon->PartyMember.PartyMember7,
            _ => throw new ArgumentException($"Invalid index: {idx}")
        };
    }
    
    private string StripSpecialCharactersFromName(string name)
    {
        var result = new StringBuilder();

        var isAppend = false;

        var char32Num = 0;
        
        for (var i = 0; i < name.Length; i++)
        {   
            var ch = name[i];
            if (ch == 32)
            {
                char32Num++;
            }
        }
        
        var char32Point = 0;
        
        for (var i = 0; i < name.Length; i++)
        {
            var ch = name[i];
            if (ch == 32)
            {
                char32Point++;
            }

            if (char32Num == char32Point)
            {
                isAppend = true;
            }


            // result.Append($"{i} -> {ch}");
            if (isAppend)
            {
                if (ch != 1 && ch != 2 && ch != 3 && ch != 18 && ch != 26  )
                {
                    result.Append(name[i]);
                }
            }
        }

        return result.ToString().Trim();
    }

}