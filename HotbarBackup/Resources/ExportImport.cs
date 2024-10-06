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
            ImportToMemory? importedHotbar;
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
                bool isCurrentClass = currentClass == raptureHotbarModule->ActiveHotbarClassJobId;
                RaptureHotbarModule.HotbarSlot* slotDump = null;
                if (!isCurrentClass)
                {
                    RaptureHotbarModule.HotbarSlot thing = new RaptureHotbarModule.HotbarSlot();
                    slotDump = &thing;
                }
                for (uint liveHotbarId = 0; liveHotbarId < importedHotbar.hotbar.Length; liveHotbarId++)
                {
                    HotbarSlot[] hotbarMem = importedHotbar.hotbar[liveHotbarId];
                    if (hotbarMem == null) { continue; }

                    for (uint liveHotbarSlot = 0; liveHotbarSlot < hotbarMem.Length; liveHotbarSlot++)
                    {
                        HotbarSlot importedSlot = hotbarMem[liveHotbarSlot];
                        if (importedSlot == null) { continue; }

                        if (isCurrentClass)
                        {
                            slotDump = raptureHotbarModule->GetSlotById(liveHotbarId, liveHotbarSlot);
                        }

                        pluginLog.Debug("Attempting to slot " + importedSlot.hotbarType + " " + importedSlot.hotbarActionId);
                        if (slotDump != null)
                        {
                            slotDump->Set(importedSlot.hotbarType, importedSlot.hotbarActionId);
                        }
                        raptureHotbarModule->WriteSavedSlot(currentClass, liveHotbarId, liveHotbarSlot, slotDump, false, clientState.IsPvP);
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
                case 1: return "GLD";
                case 2: return "PUG";
                case 3: return "MRD";
                case 4: return "LNC";
                case 5: return "ARC";
                case 6: return "CNJ";
                case 7: return "THM";
                case 8: return "CRP";
                case 9: return "BSM";
                case 10: return "ARM";
                case 11: return "GLD";
                case 12: return "LTH";
                case 13: return "WVR";
                case 14: return "ALC";
                case 15: return "CUL";
                case 16: return "MIN";
                case 17: return "BOT";
                case 18: return "FSH";
                case 19: return "PLD";
                case 20: return "MNK";
                case 21: return "WAR";
                case 22: return "DRG";
                case 23: return "BRD";
                case 24: return "WHM";
                case 25: return "BLM";
                case 26: return "ARC";
                case 27: return "SMN";
                case 28: return "SCH";
                case 29: return "ROG";
                case 30: return "NIN";
                case 31: return "MCH";
                case 32: return "DRK";
                case 33: return "AST";
                case 34: return "SAM";
                case 35: return "RDM";
                case 36: return "BLU";
                case 37: return "GNB";
                case 38: return "DNC";
                case 39: return "RPR";
                case 40: return "SGE";
                case 41: return "VPR";
                case 42: return "PCT";
            }
        }
    }
}
