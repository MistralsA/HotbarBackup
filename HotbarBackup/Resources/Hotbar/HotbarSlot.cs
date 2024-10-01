using FFXIVClientStructs.FFXIV.Client.UI.Misc;

namespace HotbarBackup.Resources.Hotbar
{
    public class HotbarSlot
    {
        public RaptureHotbarModule.HotbarSlotType hotbarType;
        public uint hotbarActionId;

        public HotbarSlot(RaptureHotbarModule.HotbarSlotType type, uint actionId)
        {
            hotbarType = type;
            hotbarActionId = actionId;
        }

        public override string ToString()
        {
            return "" + hotbarType.ToString() + " " + hotbarActionId.ToString();
        }
    }
}
