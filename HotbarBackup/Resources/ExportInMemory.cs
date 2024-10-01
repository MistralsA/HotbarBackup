using HotbarBackup.Resources.Hotbar;

namespace HotbarBackup.Resources
{
    public class ImportToMemory
    {
        public int targetClass { get; set; }
        public HotbarSlot[][]? hotbar { get; set; }
    }

    public class ExportInMemoryArguments
    {
        public int targetClass;
        public int totalHotbars;

        public ExportInMemoryArguments() { }
    }

    public class ExportInMemory : ImportToMemory
    {
        public ExportInMemory(ExportInMemoryArguments args)
        {
            targetClass = args.targetClass;
            if (args.totalHotbars > 0)
            {
                hotbar = new HotbarSlot[args.totalHotbars][];
            }
        }
    }
}
