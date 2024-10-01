using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System;
using Newtonsoft.Json;
using ImGuiNET;
using HotbarBackup.Resources.Hotbar;

namespace HotbarBackup.Resources
{
    internal unsafe class ExportImport
    {
        private RaptureHotbarModule* raptureHotbarModule = Framework.Instance()->UIModule->GetRaptureHotbarModule();
        private IPluginLog pluginLog;
        private IClientState clientState;

        private const int HOTBAR_SIZE = 16;

        public ExportImport(Plugin Plugin, IPluginLog pluginLog, IClientState clientState)
        {
            this.pluginLog = pluginLog;
            this.clientState = clientState;
        }

        public void ExportCurrentClassHotbarToClipboard(bool exportNormalHotbar, bool exportShareHotbar, bool exportJobGauge)
        {
            bool exportHotbars = exportNormalHotbar || exportShareHotbar;
            ExportInMemoryArguments exportArgs = new ExportInMemoryArguments();
            int totalHotbars = exportHotbars ? raptureHotbarModule->Hotbars.Length + raptureHotbarModule->CrossHotbars.Length : 0;
            int currentClass = raptureHotbarModule->ActiveHotbarClassJobId;
            exportArgs.targetClass = currentClass;
            exportArgs.totalHotbars = totalHotbars;

            ExportInMemory exportData = new ExportInMemory(exportArgs);
            if (totalHotbars > 0 && exportData.hotbar != null)
            {
                for (uint liveHotbarId = 0; liveHotbarId < totalHotbars; liveHotbarId++)
                {
                    bool isSharedHotbar = raptureHotbarModule->IsHotbarShared(liveHotbarId);
                    if (!exportShareHotbar && isSharedHotbar) { continue; }
                    if (!(exportNormalHotbar || isSharedHotbar)) { continue; }

                    HotbarSlot[] hotbarSlot = new HotbarSlot[HOTBAR_SIZE];
                    bool isAllNull = true;
                    for (uint liveHotbarSlot = 0; liveHotbarSlot < HOTBAR_SIZE; liveHotbarSlot++)
                    {
                        RaptureHotbarModule.HotbarSlot* slot = raptureHotbarModule->GetSlotById(liveHotbarId, liveHotbarSlot);
                        if (slot->CommandType == RaptureHotbarModule.HotbarSlotType.Empty) { continue; }
                        HotbarSlot memSlot = new HotbarSlot(slot->CommandType, slot->CommandId);
                        hotbarSlot[liveHotbarSlot] = memSlot;
                        isAllNull = false;
                    }
                    if (!isAllNull)
                    {
                        exportData.hotbar[liveHotbarId] = hotbarSlot;
                    }
                }
            }

            ImGui.SetClipboardText(JsonConvert.SerializeObject(exportData));
        }

        public string ImportHotbar(string hotbarJson)
        {
            if (!clientState.IsLoggedIn) { pluginLog.Warning("Not logged in. Don't even try"); return ""; }
            ImportToMemory importedHotbar;
            try
            {
                importedHotbar = JsonConvert.DeserializeObject<ImportToMemory>(hotbarJson);
            }
            catch (Exception ex)
            {
                pluginLog.Warning("Invalid hotbar to import");
                pluginLog.Debug(ex.ToString());
                return "";
            }
            if (importedHotbar == null) { pluginLog.Warning("Invalid hotbar to import"); return ""; }

            uint currentClass;
            if (!uint.TryParse("" + importedHotbar.targetClass, out currentClass)) { pluginLog.Warning("Invalid class to import to"); return ""; }
            string curClassStr = classIdToJobName(currentClass);
            if (curClassStr == "Unknown") { pluginLog.Warning("Invalid class to import to"); return ""; }

            if (importedHotbar.hotbar != null)
            {
                int totalHotbars = raptureHotbarModule->Hotbars.Length + raptureHotbarModule->CrossHotbars.Length;
                pluginLog.Debug("Attempting to import hotbar to " + curClassStr);
                RaptureHotbarModule.HotbarSlot slotDump = new RaptureHotbarModule.HotbarSlot();
                for (uint liveHotbarId = 0; liveHotbarId < importedHotbar.hotbar.Length; liveHotbarId++)
                {
                    HotbarSlot[] hotbarMem = importedHotbar.hotbar[liveHotbarId];
                    if (hotbarMem == null) { continue; }

                    for (uint liveHotbarSlot = 0; liveHotbarSlot < hotbarMem.Length; liveHotbarSlot++)
                    {
                        HotbarSlot importedSlot = hotbarMem[liveHotbarSlot];
                        if (importedSlot == null) { continue; }

                        pluginLog.Debug("Attempting to slot " + importedSlot.hotbarType + " " + importedSlot.hotbarActionId);
                        slotDump.Set(importedSlot.hotbarType, importedSlot.hotbarActionId);
                        raptureHotbarModule->WriteSavedSlot(currentClass, liveHotbarId, liveHotbarSlot, &slotDump, false, clientState.IsPvP);
                    }
                }
            }
            return curClassStr;
        }

        private string classIdToJobName(uint key)
        {
            switch (key)
            {
                default: return "Unknown";
                case 1: return "Gladiator";
                case 2: return "Pugilist";
                case 3: return "Marauder";
                case 4: return "Lancer";
                case 5: return "Archer";
                case 6: return "Conjurer";
                case 7: return "Thaumaturge";
                case 8: return "Carpenter";
                case 9: return "Blacksmith";
                case 10: return "Armorer";
                case 11: return "Goldsmith";
                case 12: return "Leatherworker";
                case 13: return "Weaver";
                case 14: return "Alchemist";
                case 15: return "Culinarian";
                case 16: return "Miner";
                case 17: return "Botanist";
                case 18: return "Fisher";
                case 19: return "Paladin";
                case 20: return "Monk";
                case 21: return "Warrior";
                case 22: return "Dragoon";
                case 23: return "Bard";
                case 24: return "White Mage";
                case 25: return "Black Mage";
                case 26: return "Arcanist";
                case 27: return "Summoner";
                case 28: return "Scholar";
                case 29: return "Rogue";
                case 30: return "Ninja";
                case 31: return "Machinist";
                case 32: return "Dark Knight";
                case 33: return "Astrologian";
                case 34: return "Samurai";
                case 35: return "Red Mage";
                case 36: return "Blue Mage";
                case 37: return "Gunbreaker";
                case 38: return "Dancer";
                case 39: return "Reaper";
                case 40: return "Sage";
                case 41: return "Viper";
                case 42: return "Pictomancer";
            }
        }
    }
}
