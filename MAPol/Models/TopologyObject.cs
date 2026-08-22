using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace MAPol.Models
{
    internal class TopologyObject
    {
        public string id {  get; set; }
        public string Name { get; set; }
        public System.Drawing.Rectangle Bounds { get; set; }
        public List<Point> InputPoints { get; set; }
        public List<Point> OutputPoints { get; set; }

        public Label topologyObjectLabel { get; set; }

    }
}
