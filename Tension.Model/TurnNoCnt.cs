using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tension.Model
{
    public class TurnNoCnt
    {
        public string LineName { set; get; }                    //线体名字
        public long LeftCnt { set; get; }

        public long RightCnt { set; get; }
        public string DeviceName { set; get; }                  //设备名字
    }
}
