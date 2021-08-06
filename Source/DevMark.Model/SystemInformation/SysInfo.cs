using Newtonsoft.Json;
using System.Collections.Generic;

namespace DevMark.Model.SystemInformation
{
    public class SysInfo
    {
        public SysInfo()
        {
            CPUs = new List<CpuInfo>();
            GPUs = new List<GpuInfo>();
            RAM = new List<RamInfo>();
            Disks = new List<DiskInfo>();
            Containers = new List<ContainerInfo>();
        }

        [JsonProperty("CPUs")]
        public List<CpuInfo> CPUs { get; set; }

        [JsonProperty("GPUs")]
        public List<GpuInfo> GPUs { get; set; }

        [JsonProperty("RAM")]
        public List<RamInfo> RAM { get; set; }

        public List<DiskInfo> Disks { get; set; }

        public MotherboardInfo Motherboard { get; set; }

        [JsonProperty("OS")]
        public OsInfo OS { get; set; }

        public PowerSupplyInfo PowerSupply { get; set; }

        public List<ContainerInfo> Containers { get; set; }
    }
}
