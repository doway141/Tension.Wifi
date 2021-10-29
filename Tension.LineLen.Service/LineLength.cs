using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using Tension.Model;
using Tension.Comm;
using Tension.BLL;
using System.IO;
using System.Xml.Serialization;

namespace Tension.LineLen.Service
{
    enum PlcGroup
    {
        Sample, Actual, AllowDev, ActualDev, Press,TurnNo, Finished,TypeNo
    }

    public partial class LineLength
    {
        private List<Thread> _thPlcScan = new List<Thread>();
        private bool _abort = false;


        private List<OpcClientHelper> _ltPlc = new List<OpcClientHelper>();
        private List<object> _ltObjPlc = new List<object>();
  
        private List<DevicePara> _ltDevPar = new List<DevicePara>();
        private DeviceParaBLL _dpBll = new DeviceParaBLL();

        private List<OpcItemLineLengthInfo> _ltOpcItem = new List<OpcItemLineLengthInfo>();

        private List<List<LineLengthInfo>> _ltLs = new List<List<LineLengthInfo>>();
        private List<List<LineLengthInfo>> _ltLe = new List<List<LineLengthInfo>>();
        private List<List<LineLengthInfo>> _ltRs = new List<List<LineLengthInfo>>();
        private List<List<LineLengthInfo>> _ltRe = new List<List<LineLengthInfo>>();

        private LineLengthBLL _liBll = new LineLengthBLL();

        private List<short> _ltTurnNoPre = new List<short>(), _ltTurnNoAct = new List<short>();
        private List<bool> _ltFinishedAct = new List<bool>(), _ltFinishedPre = new List<bool>();

        private List<TurnNoCnt> _ltTurnCnt = new List<TurnNoCnt>();

        private List<string> _ltGroupId = new List<string>();
        private List<string> _ltTypeNo = new List<string>();

        private List<int> _ltIsFloat = new List<int>();

        private string _lineName = "";

        private string _date = "", _datePre = "";

        private string _rootPath = "";

        public LineLength()
        {

        }

        private void AddNewLogFile()
        {
            _date = DateTime.Now.Date.ToString("yyyy-MM-dd");

            LogHelper.AddNewLogFile(_date, _datePre, _rootPath + @"\Log\" + _date + ".log");
            _datePre = _date;
        }

        public void LineLength_Load()
        {
            IniDevice();
        }



        public void ExitApp()
        {
            _abort = true;
            Thread.Sleep(300);
            int i = 0;
            foreach (var item in _ltDevPar)
            {
                if(item.IsActual == 1)
                {
                    lock(_ltObjPlc[i])
                        _ltPlc[i].Disconnect();
                    i++;
                }
            }
            Thread.Sleep(200);

            System.Threading.Thread.CurrentThread.Abort();
        }

        private void ReadConfigPara()
        {
            try
            {
                List<string> ltPara = new List<string>();
                FileStream fs = new FileStream(_rootPath + @"\Config\Para.XML", FileMode.Open);
                XmlSerializer xml = new XmlSerializer(typeof(List<string>));
                ltPara = xml.Deserialize(fs) as List<string>;
                fs.Close();


                string[] strItem = new string[2];
                foreach (string str in ltPara)
                {
                    strItem = str.Split(':');
                    if (strItem.Length == 2)
                    {
                        if (!string.IsNullOrEmpty(strItem[0]))
                        {
                            if (strItem[0].Contains("LineName"))
                            {
                                _lineName = strItem[1];
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteFile(ex.ToString());
            }
        }

        private void IniDevice()
        {
            _rootPath = AppDomain.CurrentDomain.BaseDirectory;

            AddNewLogFile();

            ReadConfigPara();

            _ltDevPar = _dpBll.GetDevParaByLineName(_lineName);

            int i = 0;
            foreach (var dp in _ltDevPar)
            {
                if(dp.IsActual == 1)
                {
                    _ltLs.Add(new List<LineLengthInfo>());
                    _ltLe.Add(new List<LineLengthInfo>());
                    _ltRs.Add(new List<LineLengthInfo>());
                    _ltRe.Add(new List<LineLengthInfo>());
                    for (int j = 0; j < 64; j++)
                    {
                        _ltLs[i].Add(new LineLengthInfo());
                        _ltLe[i].Add(new LineLengthInfo());
                        _ltRs[i].Add(new LineLengthInfo());
                        _ltRe[i].Add(new LineLengthInfo());
                    }

                    IniOpcItem(i, dp);

                    IniOpcClient(i);

                    _ltTurnCnt.Add(new TurnNoCnt());
                    _ltTurnCnt[i].LineName = _lineName;
                    _ltTurnCnt[i].DeviceName = dp.DeviceName;

                    _ltTurnNoAct.Add(new short());
                    _ltTurnNoPre.Add(new short());

                    _ltFinishedAct.Add(new bool());
                    _ltFinishedPre.Add(new bool());

                    _ltGroupId.Add("");
                    _ltTypeNo.Add("");

                    _ltIsFloat.Add(new int());
                    _ltIsFloat[i] = dp.IsFloat;

                    _thPlcScan.Add(new Thread(PlcScan));
                    if (_thPlcScan[i].ThreadState == ThreadState.Running)
                        _thPlcScan[i].Abort();
                    _thPlcScan[i].Start(i);
                    i++;
                }
                
            }
        }



        private void IniOpcClient(int i)
        {
            _ltPlc.Add(new OpcClientHelper());
            _ltPlc[i].IniClient("KEPware.KEPServerEx.V6", "127.0.0.1");
            _ltObjPlc.Add(new object());


            List<List<string>> ltItemName = new List<List<string>>();

            int idx = 0;
            ltItemName.Add(new List<string>());

            #region Sample
            ltItemName.Add(new List<string>());
            idx = (int)PlcGroup.Sample;
            ltItemName[idx].Add(_ltOpcItem[i].SampleLeft1);
            ltItemName[idx].Add(_ltOpcItem[i].SampleLeft2);
            ltItemName[idx].Add(_ltOpcItem[i].SampleLeft3);


            ltItemName[idx].Add(_ltOpcItem[i].SampleRight1);
            ltItemName[idx].Add(_ltOpcItem[i].SampleRight2);
            ltItemName[idx].Add(_ltOpcItem[i].SampleRight3);

            #endregion

            #region Actual
            ltItemName.Add(new List<string>());
            idx = (int)PlcGroup.Actual;
            ltItemName[idx].Add(_ltOpcItem[i].ActualLeft1);
            ltItemName[idx].Add(_ltOpcItem[i].ActualLeft2);
            ltItemName[idx].Add(_ltOpcItem[i].ActualLeft3);


            ltItemName[idx].Add(_ltOpcItem[i].ActualRight1);
            ltItemName[idx].Add(_ltOpcItem[i].ActualRight2);
            ltItemName[idx].Add(_ltOpcItem[i].ActualRight3);

            #endregion

            #region AllowDev
            ltItemName.Add(new List<string>());
            idx = (int)PlcGroup.AllowDev;
            ltItemName[idx].Add(_ltOpcItem[i].AllowDevLeft1);
            ltItemName[idx].Add(_ltOpcItem[i].AllowDevLeft2);
            ltItemName[idx].Add(_ltOpcItem[i].AllowDevLeft3);


            ltItemName[idx].Add(_ltOpcItem[i].AllowDevRight1);
            ltItemName[idx].Add(_ltOpcItem[i].AllowDevRight2);
            ltItemName[idx].Add(_ltOpcItem[i].AllowDevRight3);

            #endregion

            #region ActualDev
            ltItemName.Add(new List<string>());
            idx = (int)PlcGroup.ActualDev;
            ltItemName[idx].Add(_ltOpcItem[i].ActualDevLeft1);
            ltItemName[idx].Add(_ltOpcItem[i].ActualDevLeft2);
            ltItemName[idx].Add(_ltOpcItem[i].ActualDevLeft3);


            ltItemName[idx].Add(_ltOpcItem[i].ActualDevRight1);
            ltItemName[idx].Add(_ltOpcItem[i].ActualDevRight2);
            ltItemName[idx].Add(_ltOpcItem[i].ActualDevRight3);

            #endregion

            #region Press
            ltItemName.Add(new List<string>());
            idx = (int)PlcGroup.Press;
            ltItemName[idx].Add(_ltOpcItem[i].PressLeft);
            ltItemName[idx].Add(_ltOpcItem[i].PressRight);
            #endregion

            ltItemName.Add(new List<string>());
            idx = (int)PlcGroup.TurnNo;
            ltItemName[idx].Add(_ltOpcItem[i].TurnNoActual);

            ltItemName.Add(new List<string>());
            idx = (int)PlcGroup.Finished;
            ltItemName[idx].Add(_ltOpcItem[i].Finished);

            ltItemName.Add(new List<string>());
            idx = (int)PlcGroup.TypeNo;
            ltItemName[idx].Add(_ltOpcItem[i].TypeNo);

            _ltPlc[i].CreateGroup(ltItemName);
        }

        private void IniOpcItem(int i, DevicePara dp)
        {
            _ltOpcItem.Add(new OpcItemLineLengthInfo());
            string str = string.Format("{0}.{1}.LineLength.", dp.PlcType, dp.DeviceName);


            #region Sample
            _ltOpcItem[i].SampleLeft1 = str + "Sample.Left1";
            _ltOpcItem[i].SampleLeft2 = str + "Sample.Left2";
            _ltOpcItem[i].SampleLeft3 = str + "Sample.Left3";

            _ltOpcItem[i].SampleRight1 = str + "Sample.Right1";
            _ltOpcItem[i].SampleRight2 = str + "Sample.Right2";
            _ltOpcItem[i].SampleRight3 = str + "Sample.Right3";

            #endregion

            #region Actual
            _ltOpcItem[i].ActualLeft1 = str + "Actual.Left1";
            _ltOpcItem[i].ActualLeft2 = str + "Actual.Left2";
            _ltOpcItem[i].ActualLeft3 = str + "Actual.Left3";

            _ltOpcItem[i].ActualRight1 = str + "Actual.Right1";
            _ltOpcItem[i].ActualRight2 = str + "Actual.Right2";
            _ltOpcItem[i].ActualRight3 = str + "Actual.Right3";

            #endregion

            #region ActualDev
            _ltOpcItem[i].ActualDevLeft1 = str + "ActualDev.Left1";
            _ltOpcItem[i].ActualDevLeft2 = str + "ActualDev.Left2";
            _ltOpcItem[i].ActualDevLeft3 = str + "ActualDev.Left3";

            _ltOpcItem[i].ActualDevRight1 = str + "ActualDev.Right1";
            _ltOpcItem[i].ActualDevRight2 = str + "ActualDev.Right2";
            _ltOpcItem[i].ActualDevRight3 = str + "ActualDev.Right3";

            #endregion

            #region AllowDev`
            _ltOpcItem[i].AllowDevLeft1 = str + "AllowDev.Left1";
            _ltOpcItem[i].AllowDevLeft2 = str + "AllowDev.Left2";
            _ltOpcItem[i].AllowDevLeft3 = str + "AllowDev.Left3";

            _ltOpcItem[i].AllowDevRight1 = str + "AllowDev.Right1";
            _ltOpcItem[i].AllowDevRight2 = str + "AllowDev.Right2";
            _ltOpcItem[i].AllowDevRight3 = str + "AllowDev.Right3";

            #endregion

            _ltOpcItem[i].PressLeft = str + "Press.Left";
            _ltOpcItem[i].PressRight = str + "Press.Right";

            _ltOpcItem[i].TurnNoActual = str + "TurnNo.Actual";

            _ltOpcItem[i].Finished = str + "Finished.Finished";

            _ltOpcItem[i].TypeNo = str + "TypeNoCur";
        }

        /// <summary>
        /// 检测数据变化
        /// </summary>
        /// <param name="Start"></param>
        /// <param name="End"></param>
        /// <returns></returns>
        private bool RisingEdge(bool Start, bool End)
        {
            if (!Start)
            {
                if (End)
                    return true;
                return false;
            }
            else
                return false;
        }
        /// <summary>
        /// 检测数据变化
        /// </summary>
        /// <param name="Start"></param>
        /// <param name="End"></param>
        /// <returns></returns>
        private bool RisingEdge(short Start, short End)
        {
            return (End - Start) == 1 ? true : false;
        }

        private bool TrialingEdge(short Start, short End)
        {
            return Start > End ? true : false;
        }

        

        private string GetMillis()
        {
            long currentTicks = DateTime.Now.Ticks;
            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long currentMillis = (currentTicks - dtFrom.Ticks) / 10000;

            return currentMillis.ToString();
        }

        /// <summary>
        /// 扫描圈数变化信号，启动去读线长数据
        /// </summary>
        private void PlcScan(object idx)
        {
            object obj;
            int i = (int)idx;
            while (true)
            {
                if (_abort)
                    break;
                AddNewLogFile();

                Thread.Sleep(10);
                try
                {
                    if(_ltPlc[i].Connected)
                    {
                        lock (_ltObjPlc[i])
                        {
                            obj = _ltPlc[i].ReadItem((short)PlcGroup.TurnNo, 0);

                            if (obj != null)
                            {
                                _ltTurnNoAct[i] = Convert.ToInt16(obj);

                            }


                            obj = _ltPlc[i].ReadItem((short)PlcGroup.Finished, 0);

                            if (obj != null)
                            {
                                _ltFinishedAct[i] = Convert.ToBoolean(obj);
                            }


                            if (RisingEdge(_ltTurnNoPre[i], _ltTurnNoAct[i]))
                            {
                                //         LogHelper.WriteFile(string.Format("Device:{0},TurnNoPre:{1},TurnNoAct:{2}", i, _ltTurnNoPre[i], _ltTurnNoAct[i]));

                                if (_ltTurnNoPre[i] == 0)
                                {
                                    _ltGroupId[i] = GetMillis();
                                    obj = _ltPlc[i].ReadItem((short)PlcGroup.TypeNo, 0);

                                    _ltTypeNo[i] = obj.ToString();

                                }

                                for (int j = _ltTurnNoPre[i]; j < _ltTurnNoAct[i]; j++)
                                {
                                    string t = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    short turnNo = (short)(j + 1);

                                    _ltLs[i][j].DT = t;
                                    _ltLs[i][j].TurnNo = turnNo;

                                    _ltLe[i][j].DT = t;
                                    _ltLe[i][j].TurnNo = turnNo;

                                    _ltRs[i][j].DT = t;
                                    _ltRs[i][j].TurnNo = turnNo;

                                    _ltRe[i][j].DT = t;
                                    _ltRe[i][j].TurnNo = turnNo;
                                }
                            }

                            if (!string.IsNullOrEmpty(_ltGroupId[i]))
                            {
                                if (RisingEdge(_ltFinishedPre[i], _ltFinishedAct[i]))
                                {
                                    ReadLineLengthInfo(i, _ltTurnNoAct[i]);

                                    _liBll.AddListLi(_ltLs[i], _ltLe[i], _ltTurnNoAct[i], 0, _ltGroupId[i], _ltDevPar[i].DeviceId, _lineName, _ltTypeNo[i]);
                                    _liBll.AddListLi(_ltRs[i], _ltRe[i], _ltTurnNoAct[i], 1, _ltGroupId[i], _ltDevPar[i].DeviceId, _lineName, _ltTypeNo[i]);
                                    _ltGroupId[i] = "";

                                    _ltTurnCnt[i].LeftCnt += _ltTurnNoPre[i];
                                    _ltTurnCnt[i].RightCnt += _ltTurnNoPre[i];

                                    LogHelper.WriteFile(String.Format("DeviceName:{0},Left:{1},Right:{2}", _ltTurnCnt[i].DeviceName, _ltTurnCnt[i].LeftCnt, _ltTurnCnt[i].RightCnt));
                                }
                            }

                            _ltTurnNoPre[i] = _ltTurnNoAct[i];
                            _ltFinishedPre[i] = _ltFinishedAct[i];
                        }
                    }
                    else
                    {
                        LogHelper.WriteFile("Plc:{0}与Kepware服务器断开,扫描线程退出");
                    }
                        
                }
                catch (Exception ex)
                {
                    LogHelper.WriteFile(ex.ToString());
                }
            }
        }



        /// <summary>
        /// 读线长详细数据,写到对应的实体中
        /// </summary>
        /// <param name="ItemId"></param>
        private void ReadLineLengthInfo(int idx, int MaxTurnNo)
        {
            object pressLeft = null;
            object pressRight = null;
            pressLeft = _ltPlc[idx].ReadItem((short)PlcGroup.Press, 0);
            pressRight = _ltPlc[idx].ReadItem((short)PlcGroup.Press, 1);

            for (short i = (short)PlcGroup.Sample; i <= (short)PlcGroup.ActualDev; i++)
            {
                object obj1 = null, obj2 = null;

                for (int j = 0; j < MaxTurnNo / 8 + 1; j++)
                {
                    obj1 = _ltPlc[idx].ReadItem(i, (short)(j));
                    obj2 = _ltPlc[idx].ReadItem(i, (short)(j + 3));

                    if (obj1 != null && obj2 != null)
                    {
                        
                        if (_ltIsFloat[idx] == 1)
                        {
            
                            float[] val1 = new float[16];
                            float[] val2 = new float[16];
                            val1 = (float[])obj1;
                            val2 = (float[])obj2;


                            int TurnNoPre = 0;

                            for (int a = 0; a < val1.Length; a++)
                            {
                                TurnNoPre = j * 8 + a / 2;

                                if (TurnNoPre + 1 > MaxTurnNo)
                                    break;

                                _ltLs[idx][TurnNoPre].Press = (float)pressLeft;
                                _ltLe[idx][TurnNoPre].Press = (float)pressLeft;

                                _ltRs[idx][TurnNoPre].Press = (float)pressRight;
                                _ltRe[idx][TurnNoPre].Press = (float)pressRight;

                                switch (i)
                                {
                                    case (short)PlcGroup.Sample:
                                        if (a % 2 == 0)
                                        {

                                            _ltLs[idx][TurnNoPre].Sample = val1[a];
                                            _ltRs[idx][TurnNoPre].Sample = val2[a];
                                        }
                                        else
                                        {
                                            _ltLe[idx][TurnNoPre].Sample = val1[a];
                                            _ltRe[idx][TurnNoPre].Sample = val2[a];
                                        }
                                        break;
                                    case (short)PlcGroup.Actual:
                                        if (a % 2 == 0)
                                        {
                                            _ltLs[idx][TurnNoPre].Actual = val1[a];
                                            _ltRs[idx][TurnNoPre].Actual = val2[a];
                                        }
                                        else
                                        {
                                            _ltLe[idx][TurnNoPre].Actual = val1[a];
                                            _ltRe[idx][TurnNoPre].Actual = val2[a];
                                        }
                                        break;
                                    case (short)PlcGroup.AllowDev:
                                        if (a % 2 == 0)
                                        {
                                            _ltLs[idx][TurnNoPre].AllowDev = val1[a];
                                            _ltRs[idx][TurnNoPre].AllowDev = val2[a];
                                        }
                                        else
                                        {
                                            _ltLe[idx][TurnNoPre].AllowDev = val1[a];
                                            _ltRe[idx][TurnNoPre].AllowDev = val2[a];
                                        }
                                        break;
                                    case (short)PlcGroup.ActualDev:
                                        if (a % 2 == 0)
                                        {
                                            _ltLs[idx][TurnNoPre].ActualDev = val1[a];
                                            _ltRs[idx][TurnNoPre].ActualDev = val2[a];
                                        }
                                        else
                                        {
                                            _ltLe[idx][TurnNoPre].ActualDev = val1[a];
                                            _ltRe[idx][TurnNoPre].ActualDev = val2[a];
                                        }
                                        break;

                                }
                            }
                        }
                        else
                        {
                            int[] val1 = new int[16];
                            int[] val2 = new int[16];
                            val1 = new int[16];
                            val2 = new int[16];
                            val1 = (int[])obj1;
                            val2 = (int[])obj2;

                            int TurnNoPre = 0;

                            for (int a = 0; a < val1.Length; a++)
                            {
                                TurnNoPre = j * 8 + a / 2;

                                if (TurnNoPre + 1 > MaxTurnNo)
                                    break;

                                _ltLs[idx][TurnNoPre].Press = (int)pressLeft;
                                _ltLe[idx][TurnNoPre].Press = (int)pressLeft;

                                _ltRs[idx][TurnNoPre].Press = (int)pressRight;
                                _ltRe[idx][TurnNoPre].Press = (int)pressRight;

                                switch (i)
                                {
                                    case (short)PlcGroup.Sample:
                                        if (a % 2 == 0)
                                        {

                                            _ltLs[idx][TurnNoPre].Sample = val1[a];
                                            _ltRs[idx][TurnNoPre].Sample = val2[a];
                                        }
                                        else
                                        {
                                            _ltLe[idx][TurnNoPre].Sample = val1[a];
                                            _ltRe[idx][TurnNoPre].Sample = val2[a];
                                        }
                                        break;
                                    case (short)PlcGroup.Actual:
                                        if (a % 2 == 0)
                                        {
                                            _ltLs[idx][TurnNoPre].Actual = val1[a];
                                            _ltRs[idx][TurnNoPre].Actual = val2[a];
                                        }
                                        else
                                        {
                                            _ltLe[idx][TurnNoPre].Actual = val1[a];
                                            _ltRe[idx][TurnNoPre].Actual = val2[a];
                                        }
                                        break;
                                    case (short)PlcGroup.AllowDev:
                                        if (a % 2 == 0)
                                        {
                                            _ltLs[idx][TurnNoPre].AllowDev = val1[a];
                                            _ltRs[idx][TurnNoPre].AllowDev = val2[a];
                                        }
                                        else
                                        {
                                            _ltLe[idx][TurnNoPre].AllowDev = val1[a];
                                            _ltRe[idx][TurnNoPre].AllowDev = val2[a];
                                        }
                                        break;
                                    case (short)PlcGroup.ActualDev:
                                        if (a % 2 == 0)
                                        {
                                            _ltLs[idx][TurnNoPre].ActualDev = val1[a];
                                            _ltRs[idx][TurnNoPre].ActualDev = val2[a];
                                        }
                                        else
                                        {
                                            _ltLe[idx][TurnNoPre].ActualDev = val1[a];
                                            _ltRe[idx][TurnNoPre].ActualDev = val2[a];
                                        }
                                        break;

                                }
                            }
                        }  
                    }
                }
            }
        }
    }
}
