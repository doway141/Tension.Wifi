using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tension.Model
{
    public class TypeInfo
    {
        public string LineName { set; get; }
        public string DeviceName { set; get; }
        public string TypeNo { set; get; }
        public string CopperWireNo { set; get; }
        public long ModelId { set; get; }

        public float LeftTension { set; get; }

        public float RightTension { set; get; }
        public float MaxVal { set; get; }
    }
}
