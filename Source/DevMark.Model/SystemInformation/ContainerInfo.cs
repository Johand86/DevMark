using Newtonsoft.Json;

namespace DevMark.Model.SystemInformation
{
    public class ContainerInfo
    {
        public string ConfigIdentifier { get; set; }

        [JsonProperty("OS")]
        public OsInfo OS { get; set; }
        public int LogicalProcessors { get; set; }
        public long Memory { get; set; }
        public long MemoryLimit { get; set; }
        public int LogicalProcessorsLimit { get; set; }
    }
}
