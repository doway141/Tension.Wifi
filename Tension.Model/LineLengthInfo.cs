using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tension.Model
{
    public class LineLengthInfo
    {
        public string DT { set; get; }        //时间
        public short TurnNo { set; get; }        //圈数
        public float Sample { set; get; }       //采集
        public float Actual { set; get; }          //实际
        public float AllowDev { set; get; }      //允许错误
        public float ActualDev { set; get; }       //实际错误
        public float Press { set; get; }
    } 
}
