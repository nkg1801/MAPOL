using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAPol.Models
{
    public class ElementInfo
    {
        public string Name { get; init; }
        public string SymbolName { get; init; }
        public string RefId { get; init; }
        public string Value { get; init; }

        public override string ToString()
            => $"{Name}, {SymbolName}, {RefId}, {Value}";
    }
}
