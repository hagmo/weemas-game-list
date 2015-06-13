using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeeMasGameFilter
{
    class GameNameEdge
    {
        public GameNameNode Node1 { get; set; }
        public GameNameNode Node2 { get; set; }
        public int Cost { get; set; }
    }
}
