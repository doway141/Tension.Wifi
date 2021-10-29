using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tension.Model
{
    public class TensionConnectPara
    {
        public long Id { set; get; }
        public string IP { set; get; }
        public ushort Port { set; get; }
        public string Name { set; get; }
    }
}
