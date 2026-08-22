using Aml.Engine.Resources.Catalogue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAPol.Models
{
    internal class OpcUaItem
    {
        public string ServerEndPoint { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public int Access { get; set; }

        public string Identifier { get; set; }
        public string OpcUaNamespace { get; set; }

        public string Value { get; set; }
    }
}
