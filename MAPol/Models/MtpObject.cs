using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAPol
{
    /// <summary>
    /// MtpObject represents VisualObject in a manifest mtp file with RefBaseSystemUnitPath as MTPHMISUCLib/VisualObject
    /// </summary>
    public class MtpObject
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string RefID { get; set; }
        public string OPCUAItemId {  get; set; } // RefId of the 'V' attribute
        public int Width { get; set; }
        public int Height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int? ZIndex { get; set; }  // nullable because value might be missing
        public int Rotation { get; set; }
        public string EClassVersion { get; set; }
        public string EClassClassificationClass { get; set; }
        public string RefBaseSystemUnitPath {get;set;}
        public string Edgepath { get; set; }

    }
}
