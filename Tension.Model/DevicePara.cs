using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tension.Model
{
    public class DevicePara
    {
        public string LineName { set; get; }                    //线体名字
        public long DeviceId { set; get; }
        public string DeviceName { set; get; }                  //设备名字
        public long TensionId { set; get; }                      //张力计Id
        public string PlcType { set; get; }                     //Plc类型                    
        public float LeftK { set; get; }                              //张力与电流比例系数
        public float LeftA { set; get; }                              //张力偏移量

        public float RightK { set; get; }                              //张力与电流比例系数
        public float RightA { set; get; }                              //张力偏移量

        public float WaveRange { set; get; }                        //张力值波动范围
        public float ValRange { set; get; }                        //目标值范围

        public int LineLenMin { set; get; }                     //张力值稳定时,最小的线长改变量

        public int IsSample { set; get; }                       //是否采集采样值
        public int IsActual { set; get; }                       //是否采集实际值
        public int IsAllowDev { set; get; }                     //是否采集允许误差
        public int IsActualDev { set; get; }                    //是否采集实际误差

        public int IsFloat { set; get; }                        //线长数据是否为小数
    }
}
