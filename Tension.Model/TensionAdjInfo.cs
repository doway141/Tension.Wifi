using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tension.Model
{
    public class TensionAdjInfo
    {
        //张力计对应的设备索引
        public int IndexTen { set; get; }               //张力计索引

        public string NameTen { set; get; }             //张力计名字
        public int IndexDev { set; get; }               //设备索引
        
        public string NameDev { set; get; }             //设备名字

        public string TypeNo { set; get; }              //当前型号

        public DateTime TimeCreate { set; get; }        //创建时间

        public bool StsLock { set; get; }               //飞叉锁状态
        //调整状态值
        public bool StsLeftAdj { set; get; }            //左边调整状态

        public bool StsRightAdj { set; get; }           //右边调整状态

        public bool StsLeftChk { set; get; }            //左边整验证状态

        public bool StsRightChk { set; get; }           //右边整验状态

        public int StepAdjust { set; get; }             //调整步指示


        public float ValTargetTen { set; get; }         //目标张力值 
        //张力值
        public float ValActTen { set; get; }            //实际张力值

        public float ValAverTen { set; get; }           //平均张力值

        public float ValResultTen { set; get; }         //结果张力值

        //读取电流值
        public float ValReadCur { set; get; }           //读到的电流值
        
        public float ValWriteCur { set; get; }          //写入的电流值

        public bool WriteCur { set; get; }              //写电流  true:启动写电流 false:停止写电流  

        //启动写电流
        public float ValTempCur { set; get; }

    }
}
