using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using HotbarBackup.Windows;
using HotbarBackup.Resources;

namespace HotbarBackup;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;

    private const string CommandName = "/hbbu";

    public readonly WindowSystem WindowSystem = new("Hotbar Backup");
    private MainWindow MainWindow { get; init; }

    private Configuration textFlags {  get; init; }

    private ExportImport RaptureHotbar { get; init; }

    public Plugin(IDalamudPluginInterface interf, IPluginLog pluginLog, IChatGui chatGui, IClientState clientState)
    {
        textFlags = new Configuration();
        MainWindow = new MainWindow(this, pluginLog, textFlags);
        RaptureHotbar = new ExportImport(this, pluginLog, clientState);

        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens up the hotbar backup tool."
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        ToggleMainUI();
    }

    public void ExportCurrentClassHotbarToClipboard(bool exportNormalHotbar, bool exportShareHotbar, bool exportJobGauge) =>
        RaptureHotbar.ExportCurrentClassHotbarToClipboard(exportNormalHotbar, exportShareHotbar, exportJobGauge);
    public void ToggleMainUI() => MainWindow.Toggle();
    public void ToggleConfigUI() => MainWindow.Nothing(); // No config!

    public string ImportHotbar(string str) => RaptureHotbar.ImportHotbar(str);

    private void DrawUI() => WindowSystem.Draw();
}
