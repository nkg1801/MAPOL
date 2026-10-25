using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAPol.Models
{
    public class PictureObject
    {
        public PictureObject() { }
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<MtpObject> MtpObjects { get; set; } = new List<MtpObject>();
        public List<MtpConnectionObject> MtpConnectionObjects { get; set; } = new List<MtpConnectionObject>();
        public List<MtpJunctionObject> MtpJunctionObjects { get; set; } = new List<MtpJunctionObject>();
        public List<MtpPortObject> MtpPortObjects { get; set; } = new List<MtpPortObject>();
        public List<MtpSinkObject> MtpSinkObjects { get; set; } = new List<MtpSinkObject>();
        public List<MtpSourceObject> MtpSourceObjects { get; set; } = new List<MtpSourceObject>();
        public List<TopologyObject> TopologyObjects { get; set; } = new List<TopologyObject>();
        public List<MtpServiceControlObject> ServiceControlObjects { get; set; } = new List<MtpServiceControlObject>();

        private readonly List<ElementInfo> IndicatorElements = new();
        private readonly List<ElementInfo> ActiveElements = new();
        private readonly List<ElementInfo> ParameterElements = new();
        private readonly List<ElementInfo> OperationElements = new();
        private readonly List<ElementInfo> InputElements = new();
        private readonly List<ElementInfo> ServiceElements = new();
        private readonly List<ElementInfo> DiagnosticElements = new();
    }
}
