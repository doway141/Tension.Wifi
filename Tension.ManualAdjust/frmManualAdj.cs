using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Tension.Comm;
using System.Threading;
using Tension.Model;
using System.Runtime.InteropServices;
using Tension.BLL;
using OPCAutomation;
using System.IO;
using System.Xml.Serialization;

namespace Tension.ManualAdjust
{
    public partial class frmManualAdj : Form
    {

        private Thread _thAutoAdjustL = null, _thAutoAdjustR = null;
        private Thread _thGetTension = null;
        private Thread _thPlcScan = null;
        private OpcClientHelper _plc = new OpcClientHelper();

        private List<List<OPCItem>> _lstOpcItem = new List<List<OPCItem>>();

        private bool _bAbort = false;

        private string _groupId = "";
        private TensionAdjInfo _tenAdj = new TensionAdjInfo();
        private List<TensionAdjInfo> _ltTenAdj = new List<TensionAdjInfo>();

        private TypeInfo _typInfo = new TypeInfo();

        private TypeInfoBLL _typBll = new TypeInfoBLL();

        private string _date = "", _datePre = "";

        private double _dValueL = 0.00, _dValueR = 0.0;
        private double _valPreL = 0.0, _valPreR = 0.0;

        private int _stepL = 0, _stepR = 0;
        private int _stepPlcScanL = 0, _stepPlcScanR = 0;

        private bool _finishedL = false, _finishedR = false;


        private BettenHelper _bth = new BettenHelper();
        private string _localIp = "";

        private bool _startL = false, _startR = false;

        private List<double> _ltActTenL = new List<double>();
        private List<double> _ltActTenR = new List<double>();

        private List<double> _ltAverTenL = new List<double>();
        private List<double> _ltAverTenR = new List<double>();

        private bool _stableL = false, _stableR = false;

        private string _stsMsgL = "",_stsMsgR = "";

        private object _objMsgL = new object(), _objMsgR = new object();

        private int _cntBtnKeepAlive = 0;


        public frmManualAdj()
        {
            InitializeComponent();
        }
        

        private void IniDevice()
        {
            AddNewLogFile();


            _typInfo.LineName = "XLP-3";
            _typInfo.DeviceName = "4号机";

    
            //_groupId = GetMillis();

            //for (int i = 0; i < 10; i++)
            //{
            //    TensionAdjInfo tai = new TensionAdjInfo()
            //    {

            //        LeftCurrentRead = 20 + i,
            //        LeftValue = 4,
            //        CreateTime = DateTime.Now.ToLocalTime(),
            //    };
            //    _ltTenAdj.Add(tai);
            //    Thread.Sleep(500);
            //}


            //_typBll.AddListCurrentRocord(_typInfo, _ltTenAdj, 0, _groupId);

            //int cur =  _typBll.GetCurrent(_typInfo, 0);

            ReadConfig();

            _bth.InitTcpServer(_localIp, 20001);

            IniPlc();


            _thGetTension = new Thread(GetTension);
            if (_thGetTension.ThreadState == ThreadState.Running)
                _thGetTension.Abort();
            _thGetTension.Start();

            _thPlcScan = new Thread(PlcScan);
            if (_thPlcScan.ThreadState == ThreadState.Running)
                _thPlcScan.Abort();
            _thPlcScan.Start();

            _thAutoAdjustL = new Thread(AutoAdjustL);
            if (_thAutoAdjustL.ThreadState == ThreadState.Running)
                _thAutoAdjustL.Abort();
            _thAutoAdjustL.Start();

            _thAutoAdjustR = new Thread(AutoAdjustR);
            if (_thAutoAdjustR.ThreadState == ThreadState.Running)
                _thAutoAdjustR.Abort();
            _thAutoAdjustR.Start();
        }

        private void ReadConfig()
        {
            FileStream fs = null;
            try
            {
                List<string> ltRet = new List<string>();
                fs = new FileStream(Application.StartupPath + @"\Para.xml", FileMode.Open);
                System.Xml.Serialization.XmlSerializer xml = new XmlSerializer(typeof(List<string>));
                ltRet = xml.Deserialize(fs) as List<string>;
                fs.Close();

                foreach (string item in ltRet)
                {
                    string[] strArray = item.Split(':');
                    _localIp = strArray[1];
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteFile(ex.ToString());
            }
        }


        private void AddNewLogFile()
        {
            _date = DateTime.Now.Date.ToString("yyyy-MM-dd");

            LogHelper.AddNewLogFile(_date, _datePre, Application.StartupPath + @"\Log\" + _date + ".log");

            _datePre = _date;
        }

        #region Plc

        private void IniPlc()
        {
            _plc.IniClient("KEPware.KEPServerEx.V6", "127.0.0.1");


            List<List<string>> ltItemName = new List<List<string>>();

            ltItemName.Add(new List<string>());
            ltItemName[0].Add("XLP3 Tension.Line3.Adjust.Start.Left");
            ltItemName[0].Add("XLP3 Tension.Line3.Adjust.Start.Right");

            ltItemName.Add(new List<string>());
            ltItemName[1].Add("XLP3 Tension.Line3.Adjust.Status.Left");
            ltItemName[1].Add("XLP3 Tension.Line3.Adjust.Status.Right");

            ltItemName.Add(new List<string>());
            ltItemName[2].Add("XLP3 Tension.Line3.Adjust.Alarm.OffLine");
            ltItemName[2].Add("XLP3 Tension.Line3.Adjust.Alarm.TimeOut");

            ltItemName.Add(new List<string>());
            ltItemName[3].Add("XLP3 Tension.Line3.Adjust.Current.Left");
            ltItemName[3].Add("XLP3 Tension.Line3.Adjust.Current.Right");

            ltItemName.Add(new List<string>());
            ltItemName[4].Add("XLP3 Tension.Line3.Adjust.TypeData.TypeNoCur");

            ltItemName.Add(new List<string>());
            ltItemName[5].Add("XLP3 Tension.Line3.Adjust.Check.Left");
            ltItemName[5].Add("XLP3 Tension.Line3.Adjust.Check.Right");

            ltItemName.Add(new List<string>());
            ltItemName[6].Add("XLP3 Tension.Line3.Adjust.Lock.Lock");

            ltItemName.Add(new List<string>());
            ltItemName[7].Add("XLP3 Tension.Line3.Adjust.KeepAlive");

            ltItemName.Add(new List<string>());
            ltItemName[8].Add("XLP3 Tension.Line3.Adjust.Press.Left");
            ltItemName[8].Add("XLP3 Tension.Line3.Adjust.Press.Right");

            //ltItemName.Add(new List<string>());
            //ltItemName[5].Add("XLP3 Tension.Line3.Adjust.Test.Left");
            //ltItemName[5].Add("XLP3 Tension.Line3.Adjust.Test.Right");

            _plc.CreateGroup(ltItemName);
        }

        private bool PlcReadLeftStart(out bool value)
        {
            object ret = _plc.ReadItem(0,0);
            value = false;
            if (ret != null)
            {
                value = Convert.ToBoolean(ret);
                return true;
            }
            else
                return false;
        }
   
        private bool PlcReadRightStart(out bool value)
        {
            object ret = _plc.ReadItem(0,1);
            value = false;
            if (ret != null)
            {
                value = Convert.ToBoolean(ret);
                return true;
            }
            else
                return false;
        }

        private bool PlcReadLeftStatus(out bool value)
        {
            object ret = _plc.ReadItem(1, 0);
            value = false;
            if (ret != null)
            {
                value = Convert.ToBoolean(ret);
                return true;
            }
            else
                return false;
        }

        private bool PlcReadRightStatus(out bool value)
        {
            object ret = _plc.ReadItem(1, 1);
            value = false;
            if (ret != null)
            {
                value = Convert.ToBoolean(ret);
                return true;
            }
            else
                return false;
        }

        private void PlcWriteLeftStatus(bool value)
        {
            _plc.WriteItem(1,0, value);
        }

        private void PlcWriteRightStatus(bool value)
        {
            _plc.WriteItem(1,1, value);
        }

    
        private void PlcWriteOffLine(bool value)
        {
           _plc.WriteItem(2, 0, value);
        }

        private void PlcWriteTimeOut()
        {
            _plc.WriteItem(2, 1, true);
        }

        private bool PlcReadLeftCurrent(out int value)
        {
            object ret = _plc.ReadItem(3,0);
            value = 0;
            if (ret != null)
            {
                value = Convert.ToUInt16(ret);
                return true;
            }
            else
                return false;
        }

        private bool PlcReadRightCurrent(out int value)
        {
            object ret = _plc.ReadItem(3,1);
            value = 0;
            if (ret != null)
            {
                value = Convert.ToUInt16(ret);
                return true;
            }
            else
                return false;
        }

        private void PlcWriteLeftCurrent(int value)
        {
            _plc.WriteItem(3,0, value);
        }

        private void PlcWriteRightCurrent(int value)
        {
            _plc.WriteItem(3,1, value);
        }

        private bool PlcReadType(out string value)
        {
            value = "";
            object ret = _plc.ReadItem(4,0);

            if (ret != null)
            {
                value = ret.ToString();
                return true;
            }
            else
                return false;
        }

        private bool PlcReadLeftCheck(out bool value)
        {
            object ret = _plc.ReadItem(5, 0);
            value = false;
            if (ret != null)
            {
                value = Convert.ToBoolean(ret);
                return true;
            }
            else
                return false;
        }

        private bool PlcReadRightCheck(out bool value)
        {
            object ret = _plc.ReadItem(5, 1);
            value = false;
            if (ret != null)
            {
                value = Convert.ToBoolean(ret);
                return true;
            }
            else
                return false;
        }

        private void PlcWriteLeftCheck(bool value)
        {
            _plc.WriteItem(5, 0, value);
        }

        private void PlcWriteRightCheck(bool value)
        {
            _plc.WriteItem(5, 1, value);
        }

        private void PlcWriteLock(bool value)
        {
            _plc.WriteItem(6, 0, value);
        }

       

        private void PlcWriteKeepAlive(bool value)
        {
            _plc.WriteItem(7, 0, value);
        }

        private bool PlcReadLeftPress(out int value)
        {
            object ret = _plc.ReadItem(8, 0);
            value = 0;
            if (ret != null)
            {
                value = Convert.ToUInt16(ret);
                return true;
            }
            else
                return false;
        }

        private bool PlcReadRightPress(out int value)
        {
            object ret = _plc.ReadItem(8, 1);
            value = 0;
            if (ret != null)
            {
                value = Convert.ToUInt16(ret);
                return true;
            }
            else
                return false;
        }

        private void PlcWriteLeftPress(int value)
        {
            _plc.WriteItem(8, 0, value);
        }

        private void PlcWriteRightPress(int value)
        {
            _plc.WriteItem(8, 1, value);
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

        private bool FallingEdge(bool Start,bool End)
        {
            if(Start)
            {
                if (!End)
                    return true;
                return false;
            }
            return false;
        }

      

        private void PlcScan()
        {
            bool leftStatus = false, rightStatus = false;
            bool leftStatusOld = false, rightStatusOld = false;

            bool leftFinishedOld = false,rightFinishedOld = false;

            bool leftCheck = false, rightCheck = false;
            bool leftCheckOld = false, rightCheckOld = false;

            int leftCurrent = 0, rightCurrent = 0;
            int leftPress = 0, rightPress = 0;
            string typeNo = "";
            int errCnt = 0;
            bool plcKeepAlive = false;

            while (true)
            {
                Thread.Sleep(100);
                if (_bAbort)
                    break;

                try
                {
                    AddNewLogFile();

                    DeleteOldDataFile();

                    errCnt = 0;

                    plcKeepAlive = !plcKeepAlive;
                    PlcWriteKeepAlive(plcKeepAlive);

                    if (_cntBtnKeepAlive > 20)
                        PlcWriteOffLine(true);
                    else
                        PlcWriteOffLine(false);


                    #region     PlcScanLeft

                    if (PlcReadLeftCheck(out leftCheck))
                    {
                        if (leftCheck)
                        {
                            PlcWriteLock(true);
                        }
                            
                    }

                    if (FallingEdge(leftCheckOld, leftCheck))
                    {
                        PlcWriteLock(false);

                    }

                    leftCheckOld = leftCheck;

                    if (PlcReadLeftStatus(out leftStatus))
                        _tenAdj.LeftStatus = leftStatus;

                    if (RisingEdge(leftStatusOld, _tenAdj.LeftStatus))
                        _stepPlcScanL = 50;

                    if (FallingEdge(leftStatusOld, _tenAdj.LeftStatus))
                    {
                        _finishedL = true;
                        _stepPlcScanL = 300;
                    }

                    leftStatusOld = _tenAdj.LeftStatus;

                    //if (RisingEdge(leftFinishedOld, _finishedL))
                    //    _stepPlcScanL = 250;

                    leftFinishedOld = _finishedL;

                    if (_tenAdj.LeftStatus)
                    {
                        if (PlcReadLeftCurrent(out leftCurrent))
                            _tenAdj.LeftCurrentRead = leftCurrent;
                        else
                            errCnt++;

                        if (_tenAdj.LeftCurrentStartWrite)
                        {
                            PlcWriteLeftCurrent(_tenAdj.LeftCurrentWrite);
                            _tenAdj.LeftCurrentStartWrite = false;
                        }

                        Invoke(new Action(() =>
                        {
                            lock (_objMsgL)
                            {
                                if (_stsMsgL != "")
                                {
                                    ltbStsL.Items.Add(_stsMsgL);
                                    _stsMsgL = "";
                                }
                            }
                            lbTargetL.Text = _tenAdj.TargetLeftValue.ToString();
                            lbActL.Text = _tenAdj.LeftValue.ToString();
                            lbAverL.Text = _tenAdj.AverLeftValue.ToString();

                            lbActCurL.Text = _tenAdj.LeftCurrentRead.ToString();

                            lbStsL.BackColor = Color.Green;
                            lbStsL.Text = "调整中";
                        }));
                    }

                    switch (_stepPlcScanL)
                    {

                        case 50:
                            PlcWriteLock(true);
                            if (PlcReadLeftPress(out leftPress))
                                _tenAdj.LeftPressRead = leftPress;
                            _stepPlcScanL += 50;
                            break;
                        case 100:
                            PlcWriteLeftPress(500);
                            _stepPlcScanL += 50;
                            break;
                        case 150:
                            if (!_finishedL)
                            {
                                Invoke(new Action(() =>
                                {
                                    if (ltbStsL.Items.Count > 0)
                                        ltbStsL.Items.Clear();
                                }));
                                _stepPlcScanL += 50;
                            }
                            break;
                        case 200:
                            if (_finishedL)
                                _stepPlcScanL += 50;
                            break;
                        case 250:
                            Thread.Sleep(500);
                            _stepPlcScanL += 50;
                            break;
                        case 300:
                            if (!_bAbort)
                            {
                                PlcWriteLeftStatus(false);
                                PlcWriteLock(false);
                                PlcWriteLeftPress(_tenAdj.LeftPressRead);
                                Invoke(new Action(() =>
                                {
                                    lbStsL.BackColor = Color.White;
                                    lbStsL.Text = _tenAdj.ResultLeftValue.ToString();

                                }));
                            }

                            _stepPlcScanL = 0;
                            break;
                    }


                    #endregion

                    #region    PlcScanRight

                    if(PlcReadRightCheck(out rightCheck))
                    {
                        if (rightCheck)
                            PlcWriteLock(true);
                    }

                    if(FallingEdge(rightCheckOld,rightCheck))
                    {
                        PlcWriteLock(false);
                    }

                    rightCheckOld = rightCheck;

                    if (PlcReadRightStatus(out rightStatus))
                        _tenAdj.RightStatus = rightStatus;

                    if (RisingEdge(rightStatusOld, _tenAdj.RightStatus))
                        _stepPlcScanR = 50;

                    if (FallingEdge(rightStatusOld, _tenAdj.RightStatus))
                    {
                        _finishedR = true;
                        _stepPlcScanR = 300;
                    }

                    rightStatusOld = _tenAdj.RightStatus;

                    //if (RisingEdge(rightFinishedOld, _finishedR))
                    //    _stepPlcScanR = 250;

                    rightFinishedOld = _finishedR;

                    if (_tenAdj.RightStatus)
                    {
                        if (PlcReadRightCurrent(out rightCurrent))
                            _tenAdj.RightCurrentRead = rightCurrent;
                        else
                            errCnt++;

                        if (_tenAdj.RightCurrentStartWrite)
                        {
                            PlcWriteRightCurrent(_tenAdj.RightCurrentWrite);
                            _tenAdj.RightCurrentStartWrite = false;
                        }

                        Invoke(new Action(() =>
                        {
                            lock (_objMsgR)
                            {
                                if (_stsMsgR != "")
                                {
                                    ltbStsR.Items.Add(_stsMsgR);
                                    _stsMsgR = "";
                                }
                            }
                            lbTargetR.Text = _tenAdj.TargetRightValue.ToString();
                            lbActR.Text = _tenAdj.RightValue.ToString();
                            lbAverR.Text = _tenAdj.AverRightValue.ToString();

                            lbActCurR.Text = _tenAdj.RightCurrentRead.ToString();

                            lbStsR.BackColor = Color.Green;
                            lbStsR.Text = "调整中";
                        }));
                    }

                    switch (_stepPlcScanR)
                    {
                        case 50:
                            PlcWriteLock(true);
                            if (PlcReadRightPress(out rightPress))
                                _tenAdj.RightPressRead = rightPress;
                            _stepPlcScanR += 50;
                            break;
                        case 100:
                            PlcWriteRightPress(500);
                            _stepPlcScanR += 50;
                            break;
                        case 150:
                            if (!_finishedR)
                            {
                                Invoke(new Action(() =>
                                {
                                    if (ltbStsR.Items.Count > 0)
                                        ltbStsR.Items.Clear();
                                }));
                                _stepPlcScanR += 50;
                            }
                            break;
                        case 200:
                            if (_finishedR)
                                _stepPlcScanR += 50;
                            break;
                        case 250:
                            Thread.Sleep(500);
                            _stepPlcScanR += 50;
                            break;
                        case 300:
                            if (!_bAbort)
                            {
                                PlcWriteRightStatus(false);
                                PlcWriteLock(false);
                                PlcWriteRightPress(_tenAdj.RightPressRead);
                                Invoke(new Action(() =>
                                {
                                    lbStsR.BackColor = Color.White;
                                    lbStsR.Text = _tenAdj.ResultRightValue.ToString();

                                }));
                            }

                            _stepPlcScanR = 0;
                            break;
                    }


                    #endregion

                    #region PlcScanTypeNo
                    if (!_bAbort)
                    {
                        if (_tenAdj.LeftStatus || _tenAdj.RightStatus)
                        {
                            if (PlcReadType(out typeNo))
                                _typInfo.TypeNo = typeNo;

                            Invoke(new Action(() =>
                            {
                                lbTypeNo.Text = _typInfo.TypeNo;
                                lbGroupId.Text = _groupId;
                            }));
                        }
                    }

                    #endregion

                }
                catch (Exception ex)
                {
                    LogHelper.WriteFile(ex.ToString());
                }
                
            }         
        }

        private void tmrScan_Tick(object sender, EventArgs e)
        {
            if(_tenAdj.LeftStatus || _tenAdj.RightStatus)
            {
                if (!_bth.KeepAlive())
                    _cntBtnKeepAlive++;
                else
                    _cntBtnKeepAlive = 0;
            }
            
        }

        #endregion

        #region GetTension
        private string GetMillis()
        {
            long currentTicks = DateTime.Now.Ticks;
            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long currentMillis = (currentTicks - dtFrom.Ticks) / 100;

            return currentMillis.ToString();
        }

        private DateTime MillisToDt(long millis)
        {
            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long ticks = millis * 100 + dtFrom.Ticks;

            return new DateTime(ticks);
        }

        private bool IsInvalidDateTime(long millis)
        {
            DateTime dtOld = MillisToDt(millis);
            if (DateTime.Now.Subtract(dtOld) > new TimeSpan(30, 0, 0, 0))
                return true;
            else
                return false;
        }

        private void DeleteOldDataFile()
        {
            string fileLog = Application.StartupPath + @"\Log";
            string fileData = Application.StartupPath + @"\Data";

            try
            {
                DirectoryInfo di = Directory.CreateDirectory(fileLog);

                foreach (FileInfo fi in di.GetFiles())
                {
                    string str = fi.Name;
                    string[] strArr = str.Split('.');
                    DateTime var = Convert.ToDateTime(strArr[0]);
                    
                    if (IsInvalidDateTime(var))
                        File.Delete(fi.FullName);
                }

                di = Directory.CreateDirectory(fileData);

                foreach (FileInfo fi in di.GetFiles())
                {
                    string str = fi.Name;
                    string[] strArr = str.Split('.');
                    long var = Convert.ToInt64(strArr[0]);
                    if (IsInvalidDateTime(var))
                        File.Delete(fi.FullName);
                }
            }
            catch(Exception ex)
            {
                LogHelper.WriteFile(ex.ToString());
            }

        }

        private bool IsInvalidDateTime(DateTime dt)
        {
            if (DateTime.Now.Subtract(dt) > new TimeSpan(30, 0, 0, 0))
                return true;
            else
                return false;
        }

        private void GetTension()
        {
            while (true)
            {
                Thread.Sleep(100);
                if (_bAbort)
                    break;

                double averL, averR;
                List<double> ltTemTenL = new List<double>();
                List<double> ltTemTenR = new List<double>();

                try
                {
                    bool updateL1 = false, updateL2 = false, updateR1 = false, updateR2 = false;

                    double ten = ((double)_bth.ValueTension()) * 0.0098;

                    if (_tenAdj.LeftStatus && !_finishedL && !_tenAdj.LeftCurrentStartWrite && _stepPlcScanL == 200 && !_stableL)
                    {
                        _tenAdj.LeftValue = ten;

                        TensionAdjInfo tai = new TensionAdjInfo()
                        {
                            LeftValue = _tenAdj.LeftValue,
                            LeftCurrentRead = _tenAdj.LeftCurrentRead,
                            CreateTime = DateTime.Now.ToLocalTime()
                        };
                        _ltTenAdj.Add(tai);                 //添加张力实时记录到表

                        _ltActTenL.Add(ten);

                        if (_ltActTenL.Count > 5)
                        {
                            for (int i = _ltActTenL.Count - 6; i < _ltActTenL.Count; i++)
                            {
                                ltTemTenL.Add(_ltActTenL[i]);
                            }
                            averL = ltTemTenL.Average();

                            if (averL > _tenAdj.TargetLeftValue * 0.4)
                            {
                                foreach (double item in ltTemTenL)
                                {
                                    if (Math.Abs(item - averL) < 0.2)
                                        updateL1 = true;
                                    else
                                    {
                                        updateL1 = false;
                                        break;
                                    }
                                }
                                LogHelper.WriteFile(string.Format("C:{0},T1:{1},T2:{2},T3:{3},T4:{4}", _tenAdj.LeftCurrentRead.ToString(), ltTemTenL[0].ToString(), ltTemTenL[1].ToString(), ltTemTenL[2].ToString(), ltTemTenL[3].ToString()));

                                if (updateL1)
                                {
                                    _ltAverTenL.Add(averL);
                                    LogHelper.WriteFile(string.Format("AverL:{0}", averL.ToString()));

                                    ltTemTenL.Clear();
                                    if (_ltAverTenL.Count > 5)
                                    {
                                        for (int i = _ltAverTenL.Count - 6; i < _ltAverTenL.Count; i++)
                                        {
                                            ltTemTenL.Add(_ltAverTenL[i]);
                                        }

                                        averL = ltTemTenL.Average();

                                        foreach (var item in ltTemTenL)
                                        {
                                            if (Math.Abs(item - averL) < 0.2)
                                                updateL2 = true;
                                            else
                                            {
                                                updateL2 = false;
                                                break;
                                            }
                                        }
                                        if (updateL2)
                                        {
                                            _tenAdj.AverLeftValue = averL;
                                            LogHelper.WriteFile("张力稳定L:" + _tenAdj.AverLeftValue.ToString());
                                            _stableL = true;
                                        }
                                    }
                                }
                            }
                        }
                    }


                    if (_tenAdj.RightStatus && !_finishedR && !_tenAdj.RightCurrentStartWrite && _stepPlcScanR == 200 && !_stableR)
                    {
                        _tenAdj.RightValue = ten;

                        TensionAdjInfo tai = new TensionAdjInfo()
                        {
                            RightValue = _tenAdj.RightValue,
                            RightCurrentRead = _tenAdj.RightCurrentRead,
                            CreateTime = DateTime.Now.ToLocalTime(),
                        };

                        _ltTenAdj.Add(tai);                 //添加张力实时记录到表

                        _ltActTenR.Add(ten);

                        if (_ltActTenR.Count > 5)
                        {
                            for (int i = _ltActTenR.Count - 6; i < _ltActTenR.Count; i++)
                            {
                                ltTemTenR.Add(_ltActTenR[i]);
                            }
                            averR = ltTemTenR.Average();

                            if (averR > _tenAdj.TargetRightValue * 0.4)
                            {
                                foreach (double item in ltTemTenR)
                                {
                                    if (Math.Abs(item - averR) < 0.2)
                                        updateR1 = true;
                                    else
                                    {
                                        updateR1 = false;
                                        break;
                                    }
                                }


                                LogHelper.WriteFile(string.Format("C:{0},T1:{1},T2:{2},T3:{3},T4:{4}", _tenAdj.RightCurrentRead.ToString(), ltTemTenR[0].ToString(), ltTemTenR[1].ToString(), ltTemTenR[2].ToString(), ltTemTenR[3].ToString()));

                                if (updateR1)
                                {
                                    _ltAverTenR.Add(averR);
                                    LogHelper.WriteFile(string.Format("AverR:{0}", averR.ToString()));

                                    ltTemTenR.Clear();
                                    if (_ltAverTenR.Count > 5)
                                    {
                                        for (int i = _ltAverTenR.Count - 6; i < _ltAverTenR.Count; i++)
                                        {
                                            ltTemTenR.Add(_ltAverTenR[i]);
                                        }

                                        averR = ltTemTenR.Average();

                                        foreach (var item in ltTemTenR)
                                        {
                                            if (Math.Abs(item - averR) < 0.2)
                                                updateR2 = true;
                                            else
                                            {
                                                updateR2 = false;
                                                break;
                                            }
                                        }
                                        if (updateR2)
                                        {
                                            _tenAdj.AverRightValue = averR;
                                            LogHelper.WriteFile("张力稳定R:" + _tenAdj.AverRightValue.ToString());
                                            _stableR = true;
                                        }
                                    }
                                }
                            }
                        }
                    }


                }
                catch(Exception ex)
                {
                    LogHelper.WriteFile(ex.ToString());
                }
            }
        }
        #endregion

        #region Adjust
        private void AutoAdjustL()
        {
            int cntOk = 0;
            bool finishedOld = false;
            while (true)
            {
                Thread.Sleep(100);
                if (_bAbort)
                    break;

                if(!_tenAdj.LeftStatus)
                {
                    if (RisingEdge(finishedOld, _finishedL))
                    {
                        Thread.Sleep(100);
                        _stepL = 350;
                    }
                }
               
                try
                {
                    switch(_stepL)
                    {
                        case 0:
                            if (_tenAdj.LeftStatus && !string.IsNullOrEmpty(_typInfo.TypeNo))
                            {
                                cntOk = 0;
                                _dValueL = 0.0;
                                _valPreL = 0.0;
                                _tenAdj.ResultLeftValue = 0;
                                _ltActTenL.Clear();
                                _ltAverTenL.Clear();
                                _ltTenAdj.Clear();
                                _finishedL = false;
                                _stepL += 50;
                            }                               
                            break;
                        case 50:
                            if(_tenAdj.LeftStatus)
                            {
                                //查询需要调整的目标张力
                                _tenAdj.TargetLeftValue = _typBll.GetLeftTension(_typInfo);
                                _stepL += 50;
                            }                          
                            break;
                        case 100:
                            _typInfo.ModelId = _typBll.GetModelId(_typInfo);
                            _groupId = GetMillis();                           
                            _stepL += 50;
                            break;
                        case 150:
                            //查询记录表获得经验电流
                            int cur = _typBll.GetCurrent(_typInfo, 0);

                            if (cur == 0)
                                _tenAdj.LeftCurrentWrite = (int)(5 * _tenAdj.TargetLeftValue);
                            else
                                _tenAdj.LeftCurrentWrite = cur;
                            _tenAdj.LeftCurrentStartWrite = true;
                            _stepL += 100;
                            break;
                        case 200:
                            //目标张力与上一次稳定后张力的差值乘以一个系数再加上之前的电流值获得新电流值写入
                            _tenAdj.LeftCurrentWrite = _tenAdj.LeftCurrentRead + (int)(5 * _dValueL);
                            _tenAdj.LeftCurrentStartWrite = true;
                            _stepL += 50;
                            break;
                        case 250:
                            if (!_tenAdj.LeftCurrentStartWrite)     //等待电流写完成
                                _stepL += 50;
                            break;
                        case 300:
                            if (_stepPlcScanL == 200)
                            {
                                if (_stableL)
                                {
                                    if(_valPreL == 0.0)
                                    {
                                        if (_tenAdj.AverLeftValue > _tenAdj.TargetLeftValue * 0.5)
                                        {
                                            _dValueL = _tenAdj.TargetLeftValue - _tenAdj.AverLeftValue;
                                            _valPreL = _tenAdj.AverLeftValue;
                                            _stableL = false;
                                            LogHelper.WriteFile(string.Format("稳定1L,Aver:{0},DValue:{1}", _tenAdj.AverLeftValue.ToString(), _dValueL.ToString()));
                                            _stepL = 200;
                                        }
                                        else
                                        {
                                            _stableL = false;
                                            LogHelper.WriteFile("丢弃张力L:" + _tenAdj.AverLeftValue.ToString());
                                        }
                                    }
                                    else
                                    {
                                        if (Math.Abs(_dValueL) < 0.2)               //如果差值小于0.2,不会启动调整,所以不存在条件判断
                                        {
                                            if (Math.Abs(_tenAdj.TargetLeftValue - _tenAdj.AverLeftValue) < 0.2)
                                            {
                                                _dValueL = _tenAdj.TargetLeftValue - _tenAdj.AverLeftValue;
                                                _valPreL = _tenAdj.AverLeftValue;
                                                _stableL = false;
                                                LogHelper.WriteFile(string.Format("稳定2L,Aver:{0},DValue:{1}", _tenAdj.AverLeftValue.ToString(), _dValueL.ToString()));


                                                _tenAdj.ResultLeftValue = _tenAdj.AverLeftValue;
                                                _stsMsgL = string.Format("结果合格L,C:{0},T:{1}", _tenAdj.LeftCurrentRead.ToString(), _tenAdj.AverLeftValue) + "---" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                LogHelper.WriteFile(string.Format("结果合格L,C:{0},T:{1}", _tenAdj.LeftCurrentRead.ToString(), _tenAdj.AverLeftValue));
                                                cntOk++;

                                                if (cntOk < 3)
                                                    _stepL = 200;
                                                else
                                                {
                                                    TensionAdjInfo tai = new TensionAdjInfo()
                                                    {
                                                        LeftCurrentRead = _tenAdj.LeftCurrentRead,
                                                        ResultLeftValue = _tenAdj.ResultLeftValue,
                                                        CreateTime = DateTime.Now.ToLocalTime(),
                                                    };
                                                    LogHelper.WriteFile("L张力调整停止");
                                                    _ltTenAdj.Add(tai);
                                                    _finishedL = true;
                                                    Thread.Sleep(100);
                                                    _stepL += 50;
                                                }
                                            }                                          
                                        }
                                        else
                                        {
                                            if ((_dValueL > 0 && _tenAdj.AverLeftValue - _valPreL > _dValueL * 0.5)
                                            || (_dValueL < 0 && _valPreL - _tenAdj.AverLeftValue > Math.Abs(_dValueL) * 0.5))
                                            {
                                                _dValueL = _tenAdj.TargetLeftValue - _tenAdj.AverLeftValue;
                                                _valPreL = _tenAdj.AverLeftValue;
                                                _stableL = false;
                                                LogHelper.WriteFile(string.Format("稳定2L,Aver:{0},DValue:{1}", _tenAdj.AverLeftValue.ToString(), _dValueL.ToString()));

                                                if (Math.Abs(_dValueL) < 0.2)
                                                {
                                                    _tenAdj.ResultLeftValue = _tenAdj.AverLeftValue;
                                                    _stsMsgL = string.Format("结果合格L,C:{0},T:{1}", _tenAdj.LeftCurrentRead.ToString(), _tenAdj.AverLeftValue) + "---" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                    LogHelper.WriteFile(string.Format("结果合格L,C:{0},T:{1}", _tenAdj.LeftCurrentRead.ToString(), _tenAdj.AverLeftValue));
                                                    cntOk++;
                                                }

                                                if (cntOk < 3)
                                                    _stepL = 200;
                                                else
                                                {
                                                    TensionAdjInfo tai = new TensionAdjInfo()
                                                    {
                                                        LeftCurrentRead = _tenAdj.LeftCurrentRead,
                                                        ResultLeftValue = _tenAdj.ResultLeftValue,
                                                        CreateTime = DateTime.Now.ToLocalTime(),
                                                    };
                                                    LogHelper.WriteFile("L张力调整停止");
                                                    _ltTenAdj.Add(tai);
                                                    _finishedL = true;
                                                    Thread.Sleep(100);
                                                    _stepL += 50;
                                                }
                                            }
                                            else
                                            {
                                                _stableL = false;
                                                LogHelper.WriteFile("丢弃张力:" + _tenAdj.AverLeftValue.ToString());
                                            }
                                        }                                                                           
                                    }
                                    
                                }
                            }
                            else
                                _stableL = false;
                            break;
                        case 350:
                            cntOk = 0;
                            _dValueL = 0.0;
                            _valPreL = 0.0;
                            _ltActTenL.Clear();
                            _ltAverTenL.Clear();

                            //不管调整是否是正常终止，都启动写数据库，写文件
                            _typBll.AddListCurrentRocord(_typInfo, _ltTenAdj, 0, _groupId);
                            WriteResultToCsv(_typInfo, _ltTenAdj, 0, _groupId);
                            LogHelper.WriteFile("L张力写数据库完成");
                            _groupId = "";
                            _ltTenAdj.Clear();
                            
                            _stepL += 50;
                            break;
                        case 400:
                            if (!_tenAdj.LeftStatus)
                                _stepL = 0;
                            break;
                    }

                    finishedOld = _finishedL;
                    
                }
                catch (Exception ex)
                {
                    LogHelper.WriteFile(ex.ToString());
                }
                
            }
            
        }

        private void AutoAdjustR()
        {
            int cntOk = 0;
            bool finishedOld = false;
            while (true)
            {
                Thread.Sleep(100);
                if (_bAbort)
                    break;

                try
                {
                    if (!_tenAdj.RightStatus)
                    {
                        if (RisingEdge(finishedOld, _finishedR))
                        {
                            Thread.Sleep(100);
                            _stepR = 350;
                        }
                    }

                    switch (_stepR)
                    {
                        case 0:
                            if (_tenAdj.RightStatus && !string.IsNullOrEmpty(_typInfo.TypeNo))
                            {
                                cntOk = 0;
                                _dValueR = 0.0;
                                _valPreR = 0.0;
                                _tenAdj.ResultRightValue = 0;
                                _ltActTenR.Clear();
                                _ltAverTenR.Clear();
                                _ltTenAdj.Clear();
                                _finishedR = false;
                                _stepR += 50;
                            }
                            break;
                        case 50:
                            if(_tenAdj.RightStatus)
                            {                               
                                //查询需要调整的目标张力
                                _tenAdj.TargetRightValue = _typBll.GetRightTension(_typInfo);
                                _stepR += 50;
                            }
                            break;
                        case 100:
                            _typInfo.ModelId = _typBll.GetModelId(_typInfo);
                            _groupId = GetMillis();
                            _stepR += 50;
                            break;
                        case 150:
                            //查询记录表获得经验电流
                            int cur = _typBll.GetCurrent(_typInfo, 1);

                            if (cur == 0)
                                _tenAdj.RightCurrentWrite = (int)(5 * _tenAdj.TargetRightValue);
                            else
                                _tenAdj.RightCurrentWrite = cur;
                            _tenAdj.RightCurrentStartWrite = true;
                            _stepR += 100;
                            break;
                        case 200:
                            //目标张力与上一次稳定后张力的差值乘以一个系数再加上之前的电流值获得新电流值写入
                            _tenAdj.RightCurrentWrite = _tenAdj.RightCurrentRead + (int)(5 * _dValueR);
                            _tenAdj.RightCurrentStartWrite = true;
                            _stepR += 50;
                            break;
                        case 250:
                            if (!_tenAdj.RightCurrentStartWrite)     //等待电流写完成
                                _stepR += 50;
                            break;
                        case 300:
                            if (_stepPlcScanR == 200)
                            {
                                if (_stableR)
                                {
                                    if (_valPreR == 0.0)
                                    {
                                        if (_tenAdj.AverRightValue > _tenAdj.TargetRightValue * 0.5)
                                        {
                                            _dValueR = _tenAdj.TargetRightValue - _tenAdj.AverRightValue;
                                            _valPreR = _tenAdj.AverRightValue;
                                            _stableR = false;
                                            LogHelper.WriteFile(string.Format("稳定1R,Aver:{0},DValue:{1}", _tenAdj.AverRightValue.ToString(), _dValueR.ToString()));
                                            _stepR = 200;
                                        }
                                        else
                                        {
                                            _stableR = false;
                                            LogHelper.WriteFile("丢弃张力R:" + _tenAdj.AverRightValue.ToString());
                                        }
                                    }
                                    else
                                    {
                                        if (Math.Abs(_dValueR) < 0.2)
                                        {
                                            if(Math.Abs(_tenAdj.TargetRightValue - _tenAdj.AverRightValue) < 0.2)
                                            {
                                                _dValueR = _tenAdj.TargetRightValue - _tenAdj.AverRightValue;
                                                _valPreR = _tenAdj.AverRightValue;
                                                _stableR = false;
                                                LogHelper.WriteFile(string.Format("稳定2R,Aver:{0},DValue:{1}", _tenAdj.AverRightValue.ToString(), _dValueR.ToString()));


                                                _tenAdj.ResultRightValue = _tenAdj.AverRightValue;
                                                _stsMsgR = string.Format("结果合格R,C:{0},T:{1}", _tenAdj.RightCurrentRead.ToString(), _tenAdj.AverRightValue) + "---" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                LogHelper.WriteFile(string.Format("结果合格R,C:{0},T:{1}", _tenAdj.RightCurrentRead.ToString(), _tenAdj.AverRightValue));
                                                cntOk++;


                                                if (cntOk < 3)
                                                    _stepR = 200;
                                                else
                                                {
                                                    _tenAdj.RightValue = _tenAdj.ResultRightValue;
                                                    TensionAdjInfo tai = new TensionAdjInfo()
                                                    {
                                                        ResultRightValue = _tenAdj.ResultRightValue,
                                                        RightCurrentRead = _tenAdj.RightCurrentRead,
                                                        CreateTime = DateTime.Now.ToLocalTime(),
                                                    };
                                                    LogHelper.WriteFile("R张力调整停止");
                                                    _ltTenAdj.Add(tai);
                                                    _finishedR = true;
                                                    Thread.Sleep(100);
                                                    _stepR += 50;
                                                }
                                            }                                           
                                        }
                                        else
                                        {
                                            if ((_dValueR > 0 && _tenAdj.AverRightValue - _valPreR > _dValueR * 0.5)
                                            || (_dValueR < 0 && _valPreR - _tenAdj.AverRightValue > Math.Abs(_dValueR) * 0.5))  //正负情况需要分开讨论
                                            {
                                                _dValueR = _tenAdj.TargetRightValue - _tenAdj.AverRightValue;
                                                _valPreR = _tenAdj.AverRightValue;
                                                _stableR = false;
                                                LogHelper.WriteFile(string.Format("稳定2R,Aver:{0},DValue:{1}", _tenAdj.AverRightValue.ToString(), _dValueR.ToString()));

                                                if (Math.Abs(_dValueR) < 0.2)
                                                {
                                                    _tenAdj.ResultRightValue = _tenAdj.AverRightValue;
                                                    _stsMsgR = string.Format("结果合格R,C:{0},T:{1}", _tenAdj.RightCurrentRead.ToString(), _tenAdj.AverRightValue) + "---" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                    LogHelper.WriteFile(string.Format("结果合格R,C:{0},T:{1}", _tenAdj.RightCurrentRead.ToString(), _tenAdj.AverRightValue));
                                                    cntOk++;
                                                }                                                  

                                                if (cntOk < 3)
                                                    _stepR = 200;
                                                else
                                                {
                                                    _tenAdj.RightValue = _tenAdj.ResultRightValue;
                                                    TensionAdjInfo tai = new TensionAdjInfo()
                                                    {
                                                        ResultRightValue = _tenAdj.ResultRightValue,
                                                        RightCurrentRead = _tenAdj.RightCurrentRead,
                                                        CreateTime = DateTime.Now.ToLocalTime(),
                                                    };
                                                    LogHelper.WriteFile("R张力调整停止");
                                                    _ltTenAdj.Add(tai);
                                                    _finishedR = true;
                                                    Thread.Sleep(100);
                                                    _stepR += 50;
                                                }
                                            }
                                            else
                                            {
                                                _stableR = false;
                                                LogHelper.WriteFile("丢弃张力:" + _tenAdj.AverRightValue.ToString());
                                            }
                                        }    
                                    }

                                }
                            }
                            else
                                _stableR = false;
                            break;
                        case 350:
                            cntOk = 0;
                            _dValueR = 0.0;
                            _valPreR = 0.0;
                            _ltActTenR.Clear();
                            _ltAverTenR.Clear();

                            //不管是否是正常终止,都会写数据库，写文件
                            _typBll.AddListCurrentRocord(_typInfo, _ltTenAdj, 1, _groupId);
                            WriteResultToCsv(_typInfo, _ltTenAdj, 1, _groupId);
                            LogHelper.WriteFile("R张力写数据库完成");
                            _groupId = "";
                            _ltTenAdj.Clear();

                            _stepR += 50;
                            break;
                        case 400:
                            if (!_tenAdj.RightStatus)
                                _stepR = 0;
                            break;
                    }

                    finishedOld = _finishedR;

                }
                catch (Exception ex)
                {
                    LogHelper.WriteFile(ex.ToString());
                }

            }        
        }


        private void WriteResultToCsv(TypeInfo Ti, List<TensionAdjInfo> ltTai, int LeftRight, string GroupId)
        {
            int i = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append("ModelId,TypeNo,Tension,Current,LeftRight,CreateTime,result\r\n");
            foreach (TensionAdjInfo tai in ltTai)
            {
                string tension, current, result;
                if (LeftRight == 0)
                {
                    if (tai.ResultLeftValue > 0)
                    {
                        tension = tai.ResultLeftValue.ToString();
                        result = "1";
                    }
                    else
                    {
                        tension = tai.LeftValue.ToString();
                        result = "0";
                    }
                    current = tai.LeftCurrentRead.ToString();
                }
                else
                {
                    if (tai.ResultRightValue > 0)
                    {
                        tension = tai.ResultRightValue.ToString();
                        result = "1";
                    }
                    else
                    {
                        tension = tai.RightValue.ToString();
                        result = "0";
                    }

                    current = tai.RightCurrentRead.ToString();
                }
                sb.Append(string.Format("{0},{1},{2},{3},{4},{5},{6}\r\n", Ti.ModelId, Ti.TypeNo, tension, current, LeftRight, tai.CreateTime, result));
                i++;
            }

            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());

            FileStream fs = new FileStream(Application.StartupPath + @"\Data\" + GroupId + ".csv", FileMode.CreateNew);
            fs.Write(buffer, 0, buffer.Length);
            fs.Close();
        }
        #endregion

        private void frmMain_Load(object sender, EventArgs e)
        {
            
            AddNewLogFile();
           
            IniDevice();
        }

        private void btnStartL_Click(object sender, EventArgs e)
        {
            _startL = !_startL;
            Thread.Sleep(500);
        }

        private void btnStartR_Click(object sender, EventArgs e)
        {
            _startR = !_startR;
            Thread.Sleep(500);
 
        }

        private void ExitApp()
        {
            _bAbort = true;
            Thread.Sleep(300);
            try
            {
                PlcWriteLeftStatus(false);
                PlcWriteRightStatus(false);


                if (_tenAdj.LeftPressRead > 0)
                    PlcWriteLeftPress(_tenAdj.LeftPressRead);

                if (_tenAdj.RightPressRead > 0)
                    PlcWriteLeftPress(_tenAdj.RightPressRead);

                PlcWriteLock(false);
            }
            catch(Exception ex)
            {
                LogHelper.WriteFile(ex.ToString());
            }
            _bth.CloseServer();
            _plc.Disconnect();
            Application.ExitThread();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            ExitApp();
        }

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }
    
    }
}
