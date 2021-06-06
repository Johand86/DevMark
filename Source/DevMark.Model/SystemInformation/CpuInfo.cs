namespace DevMark.Model.SystemInformation
{
    public class CpuInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Architecture { get; set; }
        public int MaxClockSpeed { get; set; }
        public int CurrentClockSpeed { get; set; }
        public int Cores { get; set; }
        public int LogicalProcessors { get; set; }
        public string Socket { get; set; }
    }
}
