using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAPol.Models
{
    internal class ProcedureInfo
    {
        public string Name { get; set; } = "";
        public string RefID { get; set; } = "";
        public string Description { get; set; } = "";
        public int? ProcedureID { get; set; }
        public bool? IsSelfCompleting { get; set; }
        public List<ElementRef> ProcessValuesOut { get; set; } = new List<ElementRef>();
        public List<ElementRef> ProcessValuesIn { get; set; } = new List<ElementRef>();
        public List<ElementRef> ReportValues { get; set; } = new List<ElementRef>();
        public List<ElementRef> Parameters { get; set; } = new List<ElementRef>();

        public class ElementRef
        {
            public string Name { get; set; } = "";
            public string RefID { get; set; } = "";
            public string RefBaseSystemUnitPath { get; set; } = "";
        }
    }

    
}
