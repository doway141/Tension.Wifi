using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tension.Model
{
    public class OpcItemAdjInfo
    {
        public string AlarmTenOffLine { set; get; }
        public string AlarmPlcOffLine { set; get; }
        public string AlarmLineLen { set; get; }
        public string AlarmValOutOfRange { set; get; }

        public string AlarmTypeErr { set; get; }

        public string AlarmUnitErr { set; get; }

        public string AlarmValNotMatch { set; get; }
        public string CheckLeft { set; get; }
        public string CheckRight { set; get; }
        public string CurrentLeft { set; get; }
        public string CurrentRight { set; get; }
        public string KeepAlive { set; get; }
        public string LockLeft { set; get; }
        public string LockRight { set; get; }
        public string FinishedLeft { set; get; }
        public string FinishedRight { set; get; }

        public string StatusLeft { set; get; }
        public string StatusRight { set; get; }

        public string LineLenLeft { set; get; }
        public string LineLenRight { set; get; }

        public string TypeNo { set; get; }

        public string CopperWireNo { set; get; }

    }
}
