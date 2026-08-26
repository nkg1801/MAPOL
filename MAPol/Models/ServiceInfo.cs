using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAPol.Models
{
    internal class ServiceInfo
    {
        public string Name { get; set; } = "";
        public string RefID { get; set; } = "";
        public string Description { get; set; } = "";
        public List<ProcedureInfo> Procedures { get; set; } = new List<ProcedureInfo>();
    }
}
