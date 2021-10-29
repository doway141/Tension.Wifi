using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tension.Model;
using Tension.Comm;
using Tension.BLL;
using System.Threading;
using OPCAutomation;
using System.IO;
using System.Xml.Serialization;

namespace Tension.AutoAdjust
{
    public partial class frmAutoAdj : Form
    {
        private Thread _thAutoAdjustL = null, _thAutoAdjustR = null;
        private Thread _thGetTension = null;
        private Thread _thPlcScan = null;
        private OpcClientHelper _plc = new OpcClientHelper();

        private List<List<OPCItem>> _lstOpcItem = new List<List<OPCItem>>();

        private bool _bAbort = false;
        private bool _startMotor = false;
        private bool _stopMotor = false;

        private string _groupId = "";
        private TensionAdjInfo _tenAdj = new TensionAdjInfo();
        private List<TensionAdjInfo> _ltTenAdj = new List<TensionAdjInfo>();

        private TypeInfo _typInfo = new TypeInfo();

        private TypeInfoBLL _typBll = new TypeInfoBLL();

        private string _date = "", _datePre = "";



        private double _dValueL = 0.00, _dValueR = 0.0;
        private double _valPreL = 0.0, _valPreR = 0.0;

        private int _stepL = 0, _stepPlcScan = 0;

        private bool _finishedL = false, _finishedR = false;
        private int _cntTmrL = 0, _cntTmrR = 0;

        private Modbus _modbus = new Modbus();
        private ModbusInfo _mdiMotor = new ModbusInfo();
        private ModbusInfo _mdiTension = new ModbusInfo();
        private string _serverIp = "";

        private bool _startL = false;

        private List<double> _ltActTenL = new List<double>();


        private List<double> _ltAverTenL = new List<double>();


        private bool _stableL = false, _stableR = false;

        private string _stsMsgL = "";

        private object _objBtnStart = new object(),_objMsg = new object();
        public frmAutoAdj()
        {
            InitializeComponent();
        }

        private byte[] CalCrc16(byte[] data)
        {
            ushort temp = 0xFFFF;

            foreach (byte item in data)
            {
                temp ^= item;
                for (int i = 0; i < 8; i++)
                {
                    if ((temp & 0x01) != 0)
                    {
                        temp >>= (ushort)1;
                        temp ^= 0xA001;
                    }
                    else
                    {
                        temp >>= (ushort)1;
                    }
                }
            }

            byte[] ret = BitConverter.GetBytes(temp);
            return ret;
        }

        private void frmMain_Load(object sender, EventArgs e)
        {

            IniDevice();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            ExitApp();
        }

        private void btnStartL_Click(object sender, EventArgs e)
        {
            if (tbTargetL.Text == "")
            {
                MessageBox.Show("请输入目标张力");
                return;
            }
            double val = 0;
            if (!double.TryParse(tbTargetL.Text, out val))
            {
                MessageBox.Show("请输入一个合法的数值");
            }


            if (btnStartL.Text.Contains("启动"))
                btnStartL.Text = "停止";
            else
                btnStartL.Text = "启动";
            _startL = !_startL;
        }


        private void ExitApp()
        {
            _finishedL = true;
            _startL = false;
            Thread.Sleep(500);
            _bAbort = true;
            Thread.Sleep(200);
            _modbus.Close();
            _plc.Disconnect();
            Application.ExitThread();
        }

        private void IniDevice()
        {
            AddNewLogFile();


            _typInfo.LineName = "XLP-3";
            _typInfo.DeviceName = "4号机";
            _typInfo.TypeNo = "1584010794   ";

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

            IniModbus();

          
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
                    _serverIp = strArray[1];
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

        #region Modbus

        private void IniModbus()
        {
            _mdiMotor.AddrStation = 1;
            _mdiTension.AddrStation = 2;

            _modbus.Connect(_serverIp, 8899,true);
        }

        private bool ReadTension()
        {
            _mdiTension.FunCode = FuncCode.ReadHoldingData;
            _mdiTension.AddrData = 0;
            _mdiTension.CntData = 2;
            _mdiTension.TypData = TypeData.floatData;
            return _modbus.ReadData(ref _mdiTension);
        }

        private bool ReadMotorSts()
        {
            _mdiMotor.FunCode = FuncCode.ReadHoldingData;
            _mdiMotor.AddrData = 1004;
            _mdiMotor.CntData = 1;
            _mdiMotor.Data = new object[1];
            _mdiMotor.TypData = TypeData.ushortData;
            return _modbus.ReadData(ref _mdiMotor);
        }

        private bool ResetMotor()
        {
            _mdiMotor.FunCode = FuncCode.WriteData;
            _mdiMotor.AddrData = 2000;
            _mdiMotor.CntData = 1;
            _mdiMotor.Data = new object[1];
            _mdiMotor.TypData = TypeData.ushortData;
            _mdiMotor.Data[0] = 0;
            return _modbus.WriteData(_mdiMotor);
        }

        private bool StartMotor()
        {
            _mdiMotor.FunCode = FuncCode.WriteMultipleData;
            _mdiMotor.AddrData = 2004;
            _mdiMotor.CntData = 2;
            _mdiMotor.Data = new object[2];
            _mdiMotor.TypData = TypeData.uintData;
            _mdiMotor.Data[0] = 0;
            _mdiMotor.Data[1] = 0;
            return _modbus.WriteData(_mdiMotor);
        }

        private bool StopMotor()
        {
            _mdiMotor.FunCode = FuncCode.WriteData;
            _mdiMotor.AddrData = 2001;
            _mdiMotor.CntData = 1;
            _mdiMotor.Data = new object[1];
            _mdiMotor.TypData = TypeData.ushortData;
            _mdiMotor.Data[0] = 0;
            return _modbus.WriteData(_mdiMotor);
        }

        #endregion

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
            ltItemName[2].Add("XLP3 Tension.Line3.Adjust.Alarm.Alarm");

            ltItemName.Add(new List<string>());
            ltItemName[3].Add("XLP3 Tension.Line3.Adjust.Current.Left");
            ltItemName[3].Add("XLP3 Tension.Line3.Adjust.Current.Right");

            ltItemName.Add(new List<string>());
            ltItemName[4].Add("XLP3 Tension.Line3.Adjust.TypeData.TypeNoCur");

            ltItemName.Add(new List<string>());
            ltItemName[5].Add("XLP3 Tension.Line3.Adjust.LineLength.Left");
            ltItemName[5].Add("XLP3 Tension.Line3.Adjust.LineLength.Right");

            ltItemName.Add(new List<string>());
            ltItemName[6].Add("XLP3 Tension.Line3.Adjust.Lock.Lock");


            _plc.CreateGroup(ltItemName);
        }

        private bool PlcReadLeftStart(out bool value)
        {
            object ret = _plc.ReadItem(0, 0);
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
            object ret = _plc.ReadItem(0, 1);
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
            _plc.WriteItem(1, 0, value);
        }

        private void PlcWriteRightStatus(bool value)
        {
            _plc.WriteItem(1, 1, value);
        }

        private void PlcWriteAlarm(bool value)
        {
            _plc.WriteItem(2, 0, value);
        }

        private bool PlcReadLeftCurrent(out int value)
        {
            object ret = _plc.ReadItem(3, 0);
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
            object ret = _plc.ReadItem(3, 1);
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
            _plc.WriteItem(3, 0, value);
        }

        
        private void PlcWriteRightCurrent(int value)
        {
            _plc.WriteItem(3, 1, value);
        }

      

        private bool PlcReadType(out string value)
        {
            value = "";
            object ret = _plc.ReadItem(4, 0);

            if (ret != null)
            {
                value = ret.ToString();
                return true;
            }
            else
                return false;
        }

        private bool PlcReadLeftLineLength(out long value)
        {
            object ret = _plc.ReadItem(5, 0);
            value = 0;
            if (ret != null)
            {
                value = Convert.ToInt64(ret);
                return true;
            }
            else
                return false;
        }

        private bool PlcReadRightLineLength(out long value)
        {
            object ret = _plc.ReadItem(5, 1);
            value = 0;
            if (ret != null)
            {
                value = Convert.ToInt64(ret);
                return true;
            }
            else
                return false;
        }

        private void PlcWriteLock(bool value)
        {
            _plc.WriteItem(6, 0, value);
        }

        private void PlcScan()
        {
            bool leftStatus = false;
     
            int leftCurrent = 0;
 
            int errCnt = 0;

            while (true)
            {
                Thread.Sleep(100);
                if (_bAbort)
                    break;

                try
                {
                    AddNewLogFile();

                    errCnt = 0;

                    #region     PlcScanLeft
                    switch (_stepPlcScan)
                    {
                        case 0:
                            if (_startL)
                            {
                                PlcWriteLeftStatus(true);
                                _stepPlcScan = 50;
                            }
                            break;
                        case 50:
                            if (PlcReadLeftStatus(out leftStatus))
                            {
                                _tenAdj.LeftStatus = leftStatus;
                                if(_tenAdj.LeftStatus)
                                {
                                    if(!_finishedL)
                                    {
                                        Invoke(new Action(() =>
                                        {
                                            if (ltbStsL.Items.Count > 0)
                                                ltbStsL.Items.Clear();
                                        }));
                                        _stepPlcScan = 100;
                                    }
                                        
                                }
                                    
                            }
                            break;
                        case 100:
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
                                lock (_objMsg)
                                {
                                    if (_stsMsgL != "")
                                    {
                                        ltbStsL.Items.Add(_stsMsgL);
                                        _stsMsgL = "";
                                    }
                                }

                                double val;

                                if (tbTargetL.Text != "")
                                {
                                    if (double.TryParse(tbTargetL.Text, out val))
                                        _tenAdj.TargetLeftValue = val;
                                }
                                else
                                {
                                    _startL = false;
                                    _finishedL = true;
                                }

                                lbActL.Text = _tenAdj.LeftValue.ToString();
                                lbAverL.Text = _tenAdj.AverLeftValue.ToString();

                                lbActCurL.Text = _tenAdj.LeftCurrentRead.ToString();

                                lbStsL.BackColor = Color.Green;
                                lbStsL.Text = "调整中";

                                if (!_finishedL)
                                {
                                    lbStsL.BackColor = Color.Green;
                                    lbStsL.Text = "调整中";

                                    lock (_objBtnStart)
                                        btnStartL.Text = "停止";
                                }
                            }));
                            if (_finishedL)
                                _stepPlcScan = 150;
                            break;
                        case 150:
                            PlcWriteLeftStatus(false);
                            Invoke(new Action(() =>
                            {
                                lbStsL.BackColor = Color.White;
                                lbStsL.Text = _tenAdj.ResultLeftValue.ToString();

                                lock (_objBtnStart)
                                    btnStartL.Text = "启动";
                            }));
                            _stepPlcScan = 0;
                            break;
                    }

                    if (PlcReadLeftStatus(out leftStatus))
                        _tenAdj.LeftStatus = leftStatus;

                        if (_tenAdj.LeftStatus)
                        {
                            if (!_startL)
                            {
                                _finishedL = true;              //人为停止
                                _stepPlcScan = 150;
                            }
                        }
                    
                        

                    //if (_startL)
                    //{
                    //    PlcWriteLeftStatus(true);
                    //    _finishedR = true;
                    //}

                    //if (PlcReadLeftStatus(out leftStatus))
                    //{
                    //    _tenAdj.LeftStatus = leftStatus;
                    //}

                    //if (_tenAdj.LeftStatus)
                    //{
                    //    if (PlcReadLeftCurrent(out leftCurrent))
                    //        _tenAdj.LeftCurrentRead = leftCurrent;
                    //    else
                    //        errCnt++;

                    //    if (_tenAdj.LeftCurrentStartWrite)
                    //    {
                    //        PlcWriteLeftCurrent(_tenAdj.LeftCurrentWrite);
                    //        _tenAdj.LeftCurrentStartWrite = false;
                    //    }

                    //    if (_cntTmrL > 200 && _cntTmrL <= 210)        //报警1S
                    //    {
                    //        PlcWriteAlarm(true);
                    //    }
                    //    else if (_cntTmrL > 210)
                    //    {
                    //        _finishedL = true;
                    //        PlcWriteAlarm(false);
                    //    }
                    //}

                    //if (_finishedL)
                    //{
                    //    PlcWriteLeftStatus(false);
                    //}

                    #endregion


                    //if (_tenAdj.LeftStatus)
                    //{
                    //    Invoke(new Action(() =>
                    //    {
                    //        lock (this)
                    //        {
                    //            if (_stsMsgL != "")
                    //            {
                    //                ltbStsL.Items.Add(_stsMsgL);
                    //                _stsMsgL = "";
                    //            }
                    //        }

                    //        double val;

                    //        if (tbTargetL.Text != "")
                    //        {
                    //            if (double.TryParse(tbTargetL.Text, out val))
                    //                _tenAdj.TargetLeftValue = val;
                    //        }
                    //        else
                    //        {
                    //            _startL = false;
                    //            _finishedL = true;
                    //        }
                                
                    //        lbActL.Text = _tenAdj.LeftValue.ToString();
                    //        lbAverL.Text = _tenAdj.AverLeftValue.ToString();

                    //        lbActCurL.Text = _tenAdj.LeftCurrentRead.ToString();

                    //        if (_tenAdj.LeftStatus)
                    //        {
                    //            if (!_finishedL)
                    //            {
                    //                lbStsL.BackColor = Color.Green;
                    //                lbStsL.Text = "调整中";
                    //            }
                    //        }
                    //        else
                    //        {
                    //            if (ltbStsL.Items.Count > 0)
                    //                ltbStsL.Items.Clear();

                    //            lbStsL.BackColor = Color.White;
                    //            lbStsL.Text = _tenAdj.ResultLeftValue.ToString();
                    //        }
                    //    }));
                    //}                  
                }
                catch (Exception ex)
                {
                    LogHelper.WriteFile(ex.ToString());
                }
            }
        }

        private void tmrScan_Tick(object sender, EventArgs e)
        {
            if (_typInfo.TypeNo != null)
            {
                if (_tenAdj.LeftStatus)
                {
                    _cntTmrL++;
                }
                else
                {
                    _cntTmrL = 0;
                }

                if (_tenAdj.RightStatus)
                {
                    _cntTmrR++;
                }
                else
                {
                    _cntTmrR = 0;
                }
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

        private void GetTension()
        {
            while (true)
            {
                Thread.Sleep(100);
                if (_bAbort)
                    break;

                double averL;
                List<double> ltTemTenL = new List<double>();


                try
                {
                    bool updateL1 = false, updateL2 = false;

                    float ten = 0.0f;

                    if (_modbus.bConnected)
                    {
                        if (_startMotor)
                        {
                            StartMotor();
                            _startMotor = false;
                        }

                        if (_stopMotor)
                        {
                            StopMotor();
                            _stopMotor = false;
                        }
                            
                        if (ReadTension())
                        {
                            if(_mdiTension.Data[0] != null)
                                ten = (float)_mdiTension.Data[0] * 9.8f;
                        }
                            
                    }


                    lock (this)
                    {
                        if (_tenAdj.LeftStatus && !_finishedL && !_tenAdj.LeftCurrentStartWrite && ten > 0.0)
                        {
                            _tenAdj.LeftValue = ten;

                            TensionAdjInfo tai = new TensionAdjInfo()
                            {
                                LeftValue = _tenAdj.LeftValue,
                                LeftCurrentRead = _tenAdj.LeftCurrentRead,
                                CreateTime = DateTime.Now.ToLocalTime(),
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
                                        if (Math.Abs(item - averL) < 0.5)
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
                                                if (Math.Abs(item - averL) < 0.5)
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
                    }

                }
                catch (Exception ex)
                {
                    LogHelper.WriteFile(ex.ToString());
                }
            }
        }
        #endregion

        #region Adjust
        private void AutoAdjustL()
        {

            while (true)
            {
                Thread.Sleep(100);
                if (_bAbort)
                    break;
                try
                {

                    if (!_tenAdj.LeftStatus || _tenAdj.TargetLeftValue == 0)
                        continue;
                    else
                    {
                        _finishedL = false;
                        //查询需要调整的目标张力
                        //_tenAdj.TargetLeftValue = _typBll.GetLeftTension(_typInfo);
                        //_typInfo.ModelId = _typBll.GetModelId(_typInfo);
                        //_groupId = GetMillis();
                        _tenAdj.ResultLeftValue = 0;
                    }

                    while (_stepL <= 100)
                    {
                        if (_bAbort)
                            break;

                        if (_finishedL)
                            break;

                        lock (this)
                        {
                            switch (_stepL)
                            {
                                case 0:
                                    //查询记录表获得经验张力

                                    //int cur = _typBll.GetCurrent(_typInfo, 0);

                                    //if (cur == 0)
                                    //    _tenAdj.LeftCurrentWrite = (int)(5 * _tenAdj.TargetLeftValue);
                                    //else
                                    //    _tenAdj.LeftCurrentWrite = cur;

                                    _tenAdj.LeftCurrentWrite = 255 - (int)(8 * _tenAdj.TargetLeftValue);
                                    _tenAdj.LeftCurrentStartWrite = true;
                                    _stepL = 50;
                                    break;
                                case 50:
                                    if (!_tenAdj.LeftCurrentStartWrite)     //等待电流写完成
                                    {
                                        Thread.Sleep(500);
                                        _startMotor = true;
                                        _stepL = 100;
                                    }                                      
                                    break;
                                case 100:
                                    if (_stableL)
                                    {
                                        if (_tenAdj.AverLeftValue > _tenAdj.TargetLeftValue * 0.5)
                                        {
                                            _dValueL = _tenAdj.TargetLeftValue - _tenAdj.AverLeftValue;
                                            _valPreL = _tenAdj.AverLeftValue;
                                            _stableL = false;
                                            LogHelper.WriteFile(string.Format("稳定1L,Aver:{0},DValue:{1}", _tenAdj.AverLeftValue.ToString(), _dValueL.ToString()));
                                            _stepL = 150;
                                        }
                                        else
                                        {
                                            _stableL = false;
                                            LogHelper.WriteFile("丢弃张力L:" + _tenAdj.AverLeftValue.ToString());
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                    _stepL = 0;
                    int cntOk = 0;

                    while (!_finishedL)
                    {
                        if (_bAbort)
                            break;


                        lock (this)
                        {
                            switch (_stepL)
                            {
                                case 0:
                                    //目标张力与上一次稳定后张力的差值乘以一个系数再加上之前的电流值获得新电流值写入
                                    _tenAdj.LeftCurrentWrite = _tenAdj.LeftCurrentRead - (int)(8 * _dValueL);
                                    _tenAdj.LeftCurrentStartWrite = true;
                                    _stepL = 50;
                                    break;
                                case 50:
                                    if (!_tenAdj.LeftCurrentStartWrite)     //等待电流写完成
                                        _stepL = 100;
                                    break;
                                case 100:

                                    if (_stableL)
                                    {
                                        if (Math.Abs(_valPreL - _tenAdj.AverLeftValue) > _dValueL * 0.4)
                                        {
                                            _dValueL = _tenAdj.TargetLeftValue - _tenAdj.AverLeftValue;
                                            _valPreL = _tenAdj.AverLeftValue;
                                            _stableL = false;
                                            LogHelper.WriteFile(string.Format("稳定2L,Aver:{0},DValue:{1}", _tenAdj.AverLeftValue.ToString(), _dValueL.ToString()));

                                            if (Math.Abs(_dValueL) < 0.2)
                                            {
                                                _tenAdj.ResultLeftValue = _tenAdj.AverLeftValue;
                                                lock(_objMsg)
                                                    _stsMsgL = string.Format("结果合格L,C:{0},T:{1}", _tenAdj.LeftCurrentRead.ToString(), _tenAdj.AverLeftValue) + "---" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                LogHelper.WriteFile(string.Format("结果合格L,C:{0},T:{1}", _tenAdj.LeftCurrentRead.ToString(), _tenAdj.AverLeftValue));
                                                cntOk++;
                                            }

                                            if (cntOk < 3)
                                                _stepL = 0;
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
                                                _startL = false;
                                            }
                                        }
                                        else
                                        {
                                            _stableL = false;
                                            LogHelper.WriteFile("丢弃张力:" + _tenAdj.AverLeftValue.ToString());
                                        }

                                    }
                                    break;
                            }
                        }
                    }

                    lock (this)
                    {
                        if (_finishedL)
                        {
                            _dValueL = 0.0;
                            _valPreL = 0.0;
                            _ltActTenL.Clear();
                            _ltAverTenL.Clear();
                            //_typBll.AddListCurrentRocord(_typInfo, _ltTenAdj, 0, _groupId);
                            //_ltTenAdj.Clear();
                            //LogHelper.WriteFile("L张力写数据库完成");
                            //_groupId = "";
                            _stepL = 0;

                            _stopMotor = true;
                        }
                    }
                    while (_tenAdj.LeftStatus) ;

                    LogHelper.WriteFile("L调整完成,控件刷新完成");
                }
                catch (Exception ex)
                {
                    LogHelper.WriteFile(ex.ToString());
                }
            }

        }

        #endregion
    }
}
