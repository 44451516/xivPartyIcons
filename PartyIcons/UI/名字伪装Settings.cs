using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Memory;
using ImGuiNET;
using PartyIcons.Entities;
using PartyIcons.Utils;

namespace PartyIcons.UI;

public sealed class 名字伪装Settings
{
    public void Draw名字伪装Settings()
    {
        ImGui.Dummy(new Vector2(0, 1f));
        ImGui.TextDisabled("实验功能存在很多问题，会修改act名字\n指令:/ppi name [需要修改的名字] \n示例:/ppi name 木木枭");
        ImGui.Dummy(new Vector2(0, 10f));
        
        var 名字伪装开关 = Plugin.Settings.名字伪装开关;
        if (ImGui.Checkbox("##名字伪装_Checkbox", ref 名字伪装开关))
        {
            Plugin.Settings.名字伪装开关 = 名字伪装开关;
            Plugin.Settings.Save();
        }

        ImGui.SameLine();
        ImGui.Text("启用");
        
        
        
        ImGui.PushStyleColor(0, ImGuiHelpers.DefaultColorPalette()[0]);
        ImGui.Text("自己");
        ImGui.PopStyleColor();
        
        ImGui.Separator();
        if (Service.ClientState.LocalPlayer != null)
        {
            ImGui.SetNextItemWidth(200);
            ImGui.InputText($"##名字伪装_InputText", ref Plugin.Settings.名字伪装me, 20);
        }
        
        ImGui.SameLine();
        
        if (ImGui.Button("确定"))
        {
            var localPlayer = Service.ClientState.LocalPlayer;
            if (localPlayer != null)
            {
                unsafe
                {
                    FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* Struct = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*) localPlayer.Address;
                    MemoryHelper.WriteSeString((IntPtr)Struct->Name, SeStringUtils.Text( Plugin.Settings.名字伪装me.Trim()));
                }
            }
            
           
        }
        


        ImGui.PushStyleColor(0, ImGuiHelpers.DefaultColorPalette()[0]);
        ImGui.Text("队友");
        ImGui.PopStyleColor();
        
        ImGui.Separator();
        
        var 队友名字伪装开关_职业名称 = Plugin.Settings.队友名字伪装开关_职业名称;
        if (ImGui.Checkbox("职业名称", ref 队友名字伪装开关_职业名称))
        {
            Plugin.Settings.队友名字伪装开关_职业名称 = 队友名字伪装开关_职业名称;
            Plugin.Settings.Save();
        }

    }

}