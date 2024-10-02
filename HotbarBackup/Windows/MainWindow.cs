using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using HotbarBackup.Resources;
using ImGuiNET;

namespace HotbarBackup.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin plugin;
    private string importString = "";
    private bool exportNormalHotbar = true;
    private bool exportSharedHotbar = false;
    private bool exportJobGauge = false;
    private IPluginLog pluginLog;
    private Configuration textFlags;
    public string PresetImportString
    {
        get { return importString; }
        set { importString = value; }
    }

    public MainWindow(Plugin plugin, IPluginLog pluginLog, Configuration textFlags):base("Hotbar Export/Import##WhatId", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(400, 400),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
        this.pluginLog = pluginLog;
        this.textFlags = textFlags;
    }

    public void Dispose()
    {
        returnToDefault();
    }

    public override void Draw()
    {
        ImGui.Checkbox("Normal Hotbar", ref exportNormalHotbar);
        ImGui.SameLine();
        //ImGui.Checkbox("Job Gauge position", ref exportJobGauge); One day...
        ImGui.Checkbox("Shared Hotbar", ref exportSharedHotbar);
        if (ImGui.Button("Export current Hotbars to clipboard"))
        {
            this.plugin.ExportCurrentClassHotbarToClipboard(exportNormalHotbar, exportSharedHotbar, exportJobGauge);
        }
        ImGui.Spacing();
        ImGui.Separator();
        drawImportSection();
        ImGui.Spacing();
    }

    public override void OnClose()
    {
        returnToDefault();
    }

    public void Nothing()
    {
        return;
    }

    private void returnToDefault()
    {
        importString = "";
        this.textFlags.importClass = null;
        exportNormalHotbar = true;
        exportJobGauge = false;
        exportSharedHotbar = false;
    }

    private void drawImportSection()
    {
        ImGui.Text("Import will:");
        ImGui.BulletText("Not import empty slots");
        ImGui.BulletText("Import buttons into the class the hotbar was exported from");
        ImGui.TextDisabled("\t\t\t(whether you have it unlocked or not)");
        ImGui.BulletText("Save and persist into your character");
        ImGui.Spacing();
        ImGui.TextWrapped("If you fully understand the consequences, feel free to use the import function.");
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(255f, 0f, 0f, 1f));
        ImGui.TextWrapped("This plugin and its developer is not responsible for any consequences that will happen to your character's GUI from using this tool.");
        ImGui.PopStyleColor();
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(255f, 204f, 0f, 1f));
        ImGui.TextWrapped("!! USE WITH CAUTION !!");
        ImGui.PopStyleColor();
        ImGui.InputTextWithHint("##JSONImportTextBox", "Paste a preset here and click \"Import\".", ref importString, 30000); // A fully 
        ImGui.SameLine();
        ImGui.BeginGroup();
        if (ImGui.Button("Import" + "###Import Button"))
        {
            this.textFlags.importClass = null;
            this.pluginLog.Debug(importString);
            this.textFlags.importClass = this.plugin.ImportHotbar(importString);
            importString = "";
        }
        ImGui.EndGroup();
        if (this.textFlags.importClass == "")
        {
            ImGui.TextWrapped("Import unsuccessful");
        }
        else if (this.textFlags.importClass != null)
        {
            ImGui.TextWrapped("Import successful for job " + this.textFlags.importClass);
        }
    }
}
