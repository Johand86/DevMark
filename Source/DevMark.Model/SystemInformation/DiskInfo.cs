using System.Collections.Generic;

namespace DevMark.Model.SystemInformation
{
    public class DiskInfo
    {
        public DiskInfo()
        {
            DrivePaths = new List<string>();
        }

        public List<string> DrivePaths { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string BusType { get; set; }
        public string MediaType { get; set; }
        public int Size { get; set; }
        public bool? ContainsWorkDirectory { get; set; }
    }
}
