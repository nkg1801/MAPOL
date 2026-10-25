using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAPol.Models
{
    public class PeaInformation
    {
        public string RefID { get; set; }
        public string TagName { get; set; }
        public string TagDescription { get; set; }
        public string AssetId { get; set; }
        public string ComponentName { get; set; }
        public string DeviceClass { get; set; }
        public string DeviceHealth { get; set; }
        public string DeviceRevision { get; set; }
        public string HardwareRevision { get; set; }
        public string Manufacturer { get; set; }
        public string ManufacturerUri { get; set; }
        public string Model { get; set; }
        public string ProductCode { get; set; }
        public string ProductInstanceUri { get; set; }
        public string RevisionCounter { get; set; }
        public string SerialNumber { get; set; }
        public string SoftwareRevision { get; set; }
    }
}
