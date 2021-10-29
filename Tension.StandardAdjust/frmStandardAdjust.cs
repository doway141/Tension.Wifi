using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Tension.Model;
using Tension.Comm;
using Tension.BLL;
using System.Threading;
using System.IO;
using System.Xml.Serialization;

namespace Tension.StandardAdjust
{
    enum OpcItemId
    {
        TypeNo,KeepAlive,Lock,Status,Check,Current,Finished,LineLen,Alarm
    }

    enum OpcItemSubId
    {
        Left,Right
    }
    public partial class frmStandardAdjust : DevExpress.XtraEditors.XtraForm
    {
        private const int Internal = 55;
        private const int PointCount = 4;
        #region Thread
        private List<Thread> _ltAutoAdjust = new List<Thread>();
        private List<Thread> _ltGetTension = new List<Thread>();
        private List<List<Thread>> _ltPlcScan = new List<List<Thread>>();
        private Thread _thUpdateData = null;
        private bool _bAbort = false;
        #endregion


        #region Plc调整变量

        private DeviceParaBLL _devParBll = new DeviceParaBLL();                //查询Plc参数，包括Plc类型，设备名字，线长名字，所属张力计Id

        private List<List<OpcClientHelper>> _ltPlc = new List<List<OpcClientHelper>>(); //每台PLC一个OpcClient
        private List<List<object>> _ltObjPlc = new List<List<object>>();                //每个PLC 分配一把锁       

        private List<List<OpcItemAdjInfo>> _ltOpcItem = new List<List<OpcItemAdjInfo>>();   //OpcItem信息     每台Plc一个OpcItem

        private List<List<DevicePara>> _ltDevPar = new List<List<DevicePara>>();   //Plc参数，包括Plc类型，设备名字，线长名字，所属张力计Id

        private List<List<int>> _ltStepPlcScanL = new List<List<int>>();    //Plc 扫描步 左边
        private List<List<int>> _ltStepPlcScanR = new List<List<int>>();    //Plc 扫描步 右边

        private List<List<float>> _ltLeftLineLen = new List<List<float>>();    //线长 左边
        private List<List<float>> _ltRightLineLen = new List<List<float>>();    //线长 右边

        private List<List<bool>> _ltLeftStatus = new List<List<bool>>();     //左边调整状态
        private List<List<bool>> _ltRightStatus = new List<List<bool>>();    //右边调整状态

        private List<List<bool>> _ltLeftStatusPre = new List<List<bool>>();     //左边调整上一次状态
        private List<List<bool>> _ltRightStatusPre = new List<List<bool>>();    //右边调整上一次状态

        private List<List<bool>> _ltLeftCheck = new List<List<bool>>();      //左边验证上一次状态
        private List<List<bool>> _ltRightCheck = new List<List<bool>>();     //右边验证上一次状态

        private List<List<bool>> _ltLeftCheckPre = new List<List<bool>>();      //左边验证上一次状态
        private List<List<bool>> _ltRightCheckPre = new List<List<bool>>();     //右边验证上一次状态

        private List<bool> _ltPlcKeepAlive = new List<bool>();                  //Plc心跳包
        #endregion


        #region 张力计调整变量
        private TensionConnectParaBLL _tcpBll = new TensionConnectParaBLL();        //查询张力计连接参数 包括ip port

        private TypeInfoBLL _typBll = new TypeInfoBLL();                            //查询型号相关信息

        private List<string> _ltGroupId = new List<string>();                       //每次调整Id



        private List<ModbusServer> _ltMbsSvr = new List<ModbusServer>();            //张力计的实体 张力系统是Server 张力计是Client 

        private List<int> _ltCntMbsSvr = new List<int>();                           //张力计与张力系统断开计数
        private List<int> _ltCntMbsSvrPre = new List<int>();                           //张力计与张力系统断开计数上一次值

        private List<bool> _ltReadUnit = new List<bool>();                          //读张力计单位的变量 变量为True，启动去读张力计的单位


        private ModbusInfo _mdiManTsn = new ModbusInfo();                              //手拉张力计的Modbus变量
        private ModbusInfo _mdiUnit = new ModbusInfo();                                 //张力计的Modbus变量

        private ModbusInfo _mdiAutTsn = new ModbusInfo();                              //手拉张力计的Modbus变量
        private ModbusInfo _mdiMotorRun = new ModbusInfo();                            //张力计的Modbus变量
        private List<bool> _ltMotorRun = new List<bool>();
        


        private List<TensionConnectPara> _ltTcp = new List<TensionConnectPara>();   //张力计连接参数   包括Ip port

        private List<TensionAdjInfo> _ltTenAdj = new List<TensionAdjInfo>();        //张力调整相关信息，包括张力启动，电流，验证等

        private List<TypeInfo> _ltTi = new List<TypeInfo>();                        //型号信息

        private List<int> _ltStepAdjust = new List<int>();                         //调整步


        private List<float> _ltDValue = new List<float>();                          //张力 目标值与当前值差值

        private List<float> _ltValPre = new List<float>();                          // 上一次张力值

        private List<float> _ltValCurPre = new List<float>();                          // 上一次张力值

        private List<bool> _ltStable = new List<bool>();                           //调整 张力值稳定

        private List<int> _ltCntValNotMatch = new List<int>();
        

        #endregion

        #region 张力调整过程中值记录
        private List<List<float>> _ltActTen = new List<List<float>>();                   //实时张力值列表
        private List<List<float>> _ltActLineLen = new List<List<float>>();

        private List<List<float>> _ltAvrTen = new List<List<float>>();                   //平均张力值列表

        private List<List<TensionAdjInfo>> _ltTenAdjData = new List<List<TensionAdjInfo>>();//张力调整过程信息记录

        

        private List<TensionAdjInfo> _ltTenAdjRest = new List<TensionAdjInfo>();
        #endregion

        #region 张力调整报警

        private List<bool> _ltLineLenErr = new List<bool>();            //线长检测报警
        private List<int> _ltCntLineLenErr = new List<int>();

        private List<bool> _ltTenKeepAliveErr = new List<bool>();      //张力计与系统断开
        private List<int> _ltCntTenKeepAliveErr = new List<int>();                           //张力计与张力系统断开计数


        private List<bool> _ltTypeErr = new List<bool>();               //型号不存在

        private List<bool> _ltUnitErr = new List<bool>();               //单位错误
        private List<int> _ltCntUnitErr = new List<int>();                          //读张力计单位的计数

        private List<bool> _ltValOutOfRange = new List<bool>();                     //调整出来的张力值与经验值相关太太报警

        private List<bool> _ltValNotMatchErr = new List<bool>();               //电流与张力不匹配,张力值无效 
        private List<int> _ltCntValNotMatchErr = new List<int>();

        private List<bool> _ltMotorRunErr = new List<bool>();
        #endregion


        private string _lineName = "";

        private string _date = "", _datePre = "";

        public frmStandardAdjust()
        {
            InitializeComponent();
        }
        private void AddNewLogFile()
        {
            _date = DateTime.Now.Date.ToString("yyyy-MM-dd");

            LogHelper.AddNewLogFile(_date, _datePre, Application.StartupPath + @"\Log\" + _date + ".log");

            _datePre = _date;
        }
        private void Test()
        {
            ModbusInfo mdi = new ModbusInfo();
            mdi.AddrStation = 1;
            mdi.FunCode = FuncCode.WriteData;
            mdi.AddrData = 7;
            mdi.CntData = 1;
            mdi.TypData = TypeData.ushortData;
           mdi.Data = new object[] { 500 };
            ModbusServer msr = new ModbusServer();
            //          msr.InitTcpServer("192.168.3.45", 20010);
      //     msr.ReadData(ref mdi);
            msr.WriteData(mdi);
            while (true)
            {
                Thread.Sleep(100);
                if (msr.Connected())
                {
                    if (msr.WriteData(mdi))
                        break;

                    msr.ReadData(ref mdi);

                    //if (mdi.Data != null)
                    //{
                    //    float ten = (float)Math.Round((float)mdi.Data[0], 2);                   //保留两位小数

                    //    LogHelper.WriteFile(ten.ToString());
                    //}
                }
            }
            
        }

        private void frmStandardAdjust_Load(object sender, EventArgs e)
        {
            AddNewLogFile();
            Test();
            IniDevice();
        }

        private void ExitApp()
        {
            _bAbort = true;
            Thread.Sleep(1000);
            LogHelper.WriteFile("应用程序启动退出");
            try
            {
                int i = 0, j = 0;
                foreach (var plc in _ltPlc)
                {      
                    foreach (var item in plc)
                    {   
                        lock(_ltObjPlc[i][j])
                            item.Disconnect();
                        j++;
                    }
                    i++;           
                }
            }
            catch(Exception ex)
            {
                LogHelper.WriteFile(ex.ToString());
            }

            foreach (ModbusServer svr in _ltMbsSvr)
            {
                svr.CloseServer();
            }
            LogHelper.WriteFile("应用程序正常结束");
       
            Application.ExitThread();
        }

        private void frmStandardAdjust_FormClosing(object sender, FormClosingEventArgs e)
        {
            ExitApp();
        }

        #region Ini
        private void ReadConfigPara()
        {
            try
            {
                List<string> ltPara = new List<string>();
                FileStream fs = new FileStream(Application.StartupPath + @"\Config\Para.XML", FileMode.Open);
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
                MessageBox.Show(ex.ToString());
            }
        }

        private void IniMdiReadManTsn()
        {
            _mdiManTsn.AddrStation = 1;
            _mdiManTsn.FunCode = FuncCode.ReadHoldingData;
            _mdiManTsn.AddrData = 0;
            _mdiManTsn.CntData = 0x02;
            _mdiManTsn.TypData = TypeData.floatData;
        }

        private void IniMdiReadAutTsn()
        {
            _mdiAutTsn.AddrStation = 1;
            _mdiAutTsn.FunCode = FuncCode.ReadHoldingData;
            _mdiAutTsn.AddrData = 0;
            _mdiAutTsn.CntData = 0x02;
            _mdiAutTsn.TypData = TypeData.floatData;
        }

        private void IniMdiMotorRun()
        {
            _mdiMotorRun.AddrStation = 1;
            _mdiMotorRun.FunCode = FuncCode.WriteMultipleData;
            _mdiMotorRun.AddrData = 0x0a;
            _mdiMotorRun.CntData = 0x02;
            _mdiMotorRun.TypData = TypeData.ushortData;
            _mdiMotorRun.Data = new object[] { 1,1};
        }

        private void ClrMdiTsnData()
        {
            _mdiManTsn.Data = null;
            _mdiAutTsn.Data = null;
        }

        private void IniMdiReadUnit()
        {
            _mdiUnit.AddrStation = 1;
            _mdiUnit.FunCode = FuncCode.ReadHoldingData;
            _mdiUnit.AddrData = 0x0b;
            _mdiUnit.CntData = 0x02;
            _mdiUnit.TypData = TypeData.ushortData;
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

      

        private void IniDevice()
        {
            AddNewLogFile();

            ReadConfigPara();

            IniMdiReadManTsn();

            IniMdiReadUnit();

            IniMdiReadAutTsn();

            IniMdiMotorRun();

            List<DevicePara> ltPar = _devParBll.GetDevParaByLineName(_lineName);


            if (ltPar != null)
            {
                var igDp = ltPar.GroupBy(dp => new { dp.TensionId });

                int i = 0;

                foreach (var item in igDp)
                {
                    _ltTcp.Add(new TensionConnectPara());
                    _ltTcp[i] = _tcpBll.GetTcpByTenId(item.Key.TensionId);


                    if(_ltTcp[i] != null)
                    {
                        #region Plc调整变量
                        _ltDevPar.Add(new List<DevicePara>());
                        _ltDevPar[i] = item.ToList<DevicePara>();

                        _ltPlc.Add(new List<OpcClientHelper>());
                        _ltObjPlc.Add(new List<object>());

                        _ltOpcItem.Add(new List<OpcItemAdjInfo>());

                        _ltStepPlcScanL.Add(new List<int>());
                        _ltStepPlcScanR.Add(new List<int>());

                        _ltLeftLineLen.Add(new List<float>());
                        _ltRightLineLen.Add(new List<float>());

                        _ltLeftStatusPre.Add(new List<bool>());
                        _ltRightStatusPre.Add(new List<bool>());

                        _ltLeftStatus.Add(new List<bool>());
                        _ltRightStatus.Add(new List<bool>());

                        _ltLeftCheckPre.Add(new List<bool>());
                        _ltRightCheckPre.Add(new List<bool>());

                        _ltLeftCheck.Add(new List<bool>());
                        _ltRightCheck.Add(new List<bool>());

                        _ltPlcKeepAlive.Add(new bool());

                        #endregion

                        #region 张力计调整变量
                        _ltGroupId.Add("");

                        _ltTi.Add(new TypeInfo());

                        _ltMbsSvr.Add(new ModbusServer());

                        if (!string.IsNullOrEmpty(_ltTcp[i].IP))
                            _ltMbsSvr[i].InitTcpServer(_ltTcp[i].IP, _ltTcp[i].Port);
                        else
                            LogHelper.WriteFile(string.Format("张力计IP为空:{0}", i));
                        _ltCntMbsSvr.Add(new int());
                        _ltCntMbsSvrPre.Add(new int());

                        _ltReadUnit.Add(new bool());
                        _ltMotorRun.Add(new bool());


                        _ltDValue.Add(new float());

                        _ltValPre.Add(new float());
                        _ltValCurPre.Add(new float());

                        _ltStable.Add(new bool());

                        


                        _ltStepAdjust.Add(new int());

                        _ltTenAdj.Add(new TensionAdjInfo());
                        #endregion


                        #region 张力调整过程中值记录
                        _ltActTen.Add(new List<float>());

                        _ltAvrTen.Add(new List<float>());

                        _ltTenAdjData.Add(new List<TensionAdjInfo>());
                        _ltActLineLen.Add(new List<float>());

                        for (int a = 0; a < 1; a++)
                        {
                            _ltTenAdjRest.Add(new TensionAdjInfo());
                        }
                        #endregion

                        #region 张力调整报警
                        _ltLineLenErr.Add(new bool());
                        _ltCntLineLenErr.Add(new int());

                        _ltTenKeepAliveErr.Add(new bool());

                        _ltCntTenKeepAliveErr.Add(new int());

                        _ltValOutOfRange.Add(new bool());
                        _ltCntValNotMatchErr.Add(new int());

                        _ltTypeErr.Add(new bool());

                        _ltUnitErr.Add(new bool());
                        _ltCntUnitErr.Add(new int());

                        _ltValNotMatchErr.Add(new bool());
                        _ltMotorRunErr.Add(new bool());
                        #endregion

                        _ltPlcScan.Add(new List<Thread>());

                        _ltGetTension.Add(new Thread(GetTension));
                        if (_ltGetTension[i].ThreadState == ThreadState.Running)
                            _ltGetTension[i].Abort();
                        _ltGetTension[i].Start(i);


                        _ltAutoAdjust.Add(new Thread(AutoAdjust));
                        if (_ltAutoAdjust[i].ThreadState == ThreadState.Running)
                            _ltAutoAdjust[i].Abort();
                        _ltAutoAdjust[i].Start(i);

                        int j = 0;
                        foreach (var dp in _ltDevPar[i])
                        {
                            dp.LineName = _lineName;

                            LogHelper.WriteFile(dp.DeviceName);

                            IniLineLen(i);

                            IniStepPlcScan(i);

                            IniStatus(i);

                            IniStatusPre(i);

                            IniCheck(i);

                            IniCheckPre(i);

                            IniOpcItem(i, j, dp);

                            IniOpcClient(i, j);

                            _ltTenAdjData[i].Add(new TensionAdjInfo());

                            _ltObjPlc[i].Add(new object());

                            _ltPlcScan[i].Add(new Thread(PlcScan));
                            if (_ltPlcScan[i][j].ThreadState == ThreadState.Running)
                                _ltPlcScan[i][j].Abort();
                            PlcInfo pi = new PlcInfo()
                            {
                                Index = i,
                                SubIndex = j,
                            };
                            _ltPlcScan[i][j].Start(pi);

                            j++;
                        }
                        i++;
                    }
                    else
                    {
                        LogHelper.WriteFile("张力系统中无张力计,清添加");
                    }
                }
            }
            else
            {
                LogHelper.WriteFile("张力系统中无设备,请添加");
            }

            

            

            Text = _lineName + "张力自动调整系统";

            _thUpdateData = new Thread(UpdateData);
            if (_thUpdateData.ThreadState == ThreadState.Running)
                _thUpdateData.Abort();
            _thUpdateData.Start();

            //        DeleteOldDataFile();
        }

        private void IniStatus(int i)
        {
            _ltLeftStatus[i].Add(new bool());
            _ltRightStatus[i].Add(new bool());
        }

        private void IniStatusPre(int i)
        {
            _ltLeftStatusPre[i].Add(new bool());
            _ltRightStatusPre[i].Add(new bool());
        }

        private void IniCheckPre(int i)
        {
            _ltLeftCheckPre[i].Add(new bool());
            _ltRightCheckPre[i].Add(new bool());
        }

        private void IniCheck(int i)
        {
            _ltLeftCheck[i].Add(new bool());
            _ltRightCheck[i].Add(new bool());
        }

        private void IniOpcClient(int i, int j)
        {
            _ltPlc[i].Add(new OpcClientHelper());
            _ltPlc[i][j].IniClient("KEPware.KEPServerEx.V6", "127.0.0.1");

            List<List<string>> ltItemName = new List<List<string>>();

            ltItemName.Add(new List<string>());
            ltItemName[(int)OpcItemId.TypeNo].Add(_ltOpcItem[i][j].TypeNo);
            ltItemName[(int)OpcItemId.TypeNo].Add(_ltOpcItem[i][j].CopperWireNo);

            ltItemName.Add(new List<string>());
            ltItemName[(int)OpcItemId.KeepAlive].Add(_ltOpcItem[i][j].KeepAlive);

            ltItemName.Add(new List<string>());
            ltItemName[(int)OpcItemId.Lock].Add(_ltOpcItem[i][j].LockLeft);
            ltItemName[(int)OpcItemId.Lock].Add(_ltOpcItem[i][j].LockRight);

            ltItemName.Add(new List<string>());
            ltItemName[(int)OpcItemId.Status].Add(_ltOpcItem[i][j].StatusLeft);
            ltItemName[(int)OpcItemId.Status].Add(_ltOpcItem[i][j].StatusRight);

            ltItemName.Add(new List<string>());
            ltItemName[(int)OpcItemId.Check].Add(_ltOpcItem[i][j].CheckLeft);
            ltItemName[(int)OpcItemId.Check].Add(_ltOpcItem[i][j].CheckRight);

            ltItemName.Add(new List<string>());
            ltItemName[(int)OpcItemId.Current].Add(_ltOpcItem[i][j].CurrentLeft);
            ltItemName[(int)OpcItemId.Current].Add(_ltOpcItem[i][j].CurrentRight);

            ltItemName.Add(new List<string>());
            ltItemName[(int)OpcItemId.Finished].Add(_ltOpcItem[i][j].FinishedLeft);
            ltItemName[(int)OpcItemId.Finished].Add(_ltOpcItem[i][j].FinishedRight);

            ltItemName.Add(new List<string>());
            ltItemName[(int)OpcItemId.LineLen].Add(_ltOpcItem[i][j].LineLenLeft);
            ltItemName[(int)OpcItemId.LineLen].Add(_ltOpcItem[i][j].LineLenRight);

            ltItemName.Add(new List<string>());
            ltItemName[(int)OpcItemId.Alarm].Add(_ltOpcItem[i][j].AlarmTenOffLine);
            ltItemName[(int)OpcItemId.Alarm].Add(_ltOpcItem[i][j].AlarmLineLen);
            ltItemName[(int)OpcItemId.Alarm].Add(_ltOpcItem[i][j].AlarmValOutOfRange);
            ltItemName[(int)OpcItemId.Alarm].Add(_ltOpcItem[i][j].AlarmTypeErr);
            ltItemName[(int)OpcItemId.Alarm].Add(_ltOpcItem[i][j].AlarmUnitErr);
            ltItemName[(int)OpcItemId.Alarm].Add(_ltOpcItem[i][j].AlarmValNotMatch);

            _ltPlc[i][j].CreateGroup(ltItemName);
        }

        private void IniOpcItem(int i, int j, DevicePara dp)
        {
            _ltOpcItem[i].Add(new OpcItemAdjInfo());
            string str = string.Format("{0}.{1}.Adjust.", dp.PlcType, dp.DeviceName);
            _ltOpcItem[i][j].AlarmTenOffLine = str + "Alarm.TenOffLine";
            _ltOpcItem[i][j].AlarmLineLen = str + "Alarm.LineLen";
            _ltOpcItem[i][j].AlarmValOutOfRange = str + "Alarm.ValOutOfRange";
            _ltOpcItem[i][j].AlarmTypeErr = str + "Alarm.TypeErr";
            _ltOpcItem[i][j].AlarmUnitErr = str + "Alarm.UnitErr";
            _ltOpcItem[i][j].AlarmValNotMatch = str + "Alarm.ValNotMatch";

            _ltOpcItem[i][j].CheckLeft = str + "Check.Left";
            _ltOpcItem[i][j].CheckRight = str + "Check.Right";

            _ltOpcItem[i][j].CurrentLeft = str + "Current.Left";
            _ltOpcItem[i][j].CurrentRight = str + "Current.Right";

            _ltOpcItem[i][j].KeepAlive = str + "KeepAlive";

            _ltOpcItem[i][j].LockLeft = str + "Lock.Left";
            _ltOpcItem[i][j].LockRight = str + "Lock.Right";

            _ltOpcItem[i][j].FinishedLeft = str + "Finished.Left";
            _ltOpcItem[i][j].FinishedRight = str + "Finished.Right";

            _ltOpcItem[i][j].StatusLeft = str + "Status.Left";
            _ltOpcItem[i][j].StatusRight = str + "Status.Right";

            _ltOpcItem[i][j].LineLenLeft = str + "LineLen.Left";
            _ltOpcItem[i][j].LineLenRight = str + "LineLen.Right";

            _ltOpcItem[i][j].TypeNo = str + "TypeData.TypeNoCur";
            _ltOpcItem[i][j].CopperWireNo = str + "TypeData.CopperWireNo";
        }

        private void IniStepPlcScan(int i)
        {
            _ltStepPlcScanL[i].Add(new int());
            _ltStepPlcScanR[i].Add(new int());
        }

        private void IniLineLen(int i)
        {
            _ltLeftLineLen[i].Add(new int());
            _ltRightLineLen[i].Add(new int());
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
            catch (Exception ex)
            {
                LogHelper.WriteFile(ex.ToString());
            }

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

        private bool IsInvalidDateTime(DateTime dt)
        {
            if (DateTime.Now.Subtract(dt) > new TimeSpan(30, 0, 0, 0))
                return true;
            else
                return false;
        }
        #endregion


        #region Tension



        private void GetTension(object idx)
        {
            int i = (int)idx;
            int deviceId = 0;
            while (true)
            {
                if (_bAbort)
                    break;
                Thread.Sleep(Internal);
                float aver, max;
                List<float> ltTemTen = new List<float>();

                try
                {
                    bool update1 = false, update2 = false;

                    float ten = 0.0f;
                    ushort unit = 6;

                    //张力计连接状态及单位判断
                    if (_ltTenAdj[i].StepAdjust >= 10)
                    {
                        deviceId = _ltTenAdj[i].IndexDev;

                        if (_ltCntTenKeepAliveErr[i] <= 4)
                        {
                            if (_ltMbsSvr[i].Connected())
                            {
                                if (_ltStepAdjust[i] == 100)
                                {
                                    if (_ltReadUnit[i])
                                    {
                                        if (_ltMbsSvr[i].ReadData(ref _mdiUnit))
                                        {
                                            _ltCntTenKeepAliveErr[i] = 0;
                                            if (_mdiUnit.Data != null)
                                            {
                                                _ltCntTenKeepAliveErr[i] = 0;
                                                unit = (ushort)_mdiUnit.Data[0];

                                                LogHelper.WriteFile("Unit:" + unit);

                                                if (unit == 0)
                                                {
                                                    _ltCntUnitErr[i] = 0;
                                                    _ltReadUnit[i] = false;

                                                }
                                                else
                                                {
                                                    _ltCntUnitErr[i]++;
                                                    if (_ltCntUnitErr[i] > 5)
                                                    {
                                                        _ltCntUnitErr[i] = 0;
                                                        _ltUnitErr[i] = true;
                                                    }

                                                }
                                            }
                                            else
                                            {
                                                LogHelper.WriteFile("张力计读单位值为空");
                                                _ltCntTenKeepAliveErr[i]++;
                                            }

                                        }
                                        else
                                        {
                                            LogHelper.WriteFile("张力计读单位值不成功");
                                            _ltCntTenKeepAliveErr[i]++;
                                        }

                                    }

                                }
                                else if(_ltStepAdjust[i] == 350)
                                {
                                    if(_ltMotorRun[i])
                                    {
                                        _mdiMotorRun.Data = new object[] { 1,1};
                                        _mdiMotorRun.FunCode = FuncCode.WriteMultipleData;

                                        if (_ltMbsSvr[i].WriteData(_mdiMotorRun))
                                        {
                                            Thread.Sleep(2000);
                                            _mdiMotorRun.FunCode = FuncCode.ReadHoldingData;
                                            if (_ltMbsSvr[i].ReadData(ref _mdiMotorRun))
                                            {
                                                if(_mdiMotorRun.Data != null)
                                                {
                                                    if(_mdiMotorRun.Data.Length >= 2)
                                                    {
                                                        if((ushort)_mdiMotorRun.Data[1] == 3)
                                                        {
                                                            _ltMotorRun[i] = false;
                                                        }
                                                        else
                                                        {
                                                            _ltMotorRunErr[i] = true;
                                                            LogHelper.WriteFile("自动张力计电机速度还未达到目标状态");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _ltMotorRunErr[i] = true;
                                                        LogHelper.WriteFile("自动张力计读取电机状态值个数错误");
                                                    }
                                                }
                                                else
                                                {
                                                    _ltMotorRunErr[i] = true;
                                                    LogHelper.WriteFile("自动张力计读取电机状态值为空");
                                                }
                                            }
                                            else
                                            {
                                                _ltMotorRunErr[i] = true;
                                                LogHelper.WriteFile("自动张力计读取电机状态错误");
                                            }
                                        }
                                        else
                                        {
                                            _ltMotorRunErr[i] = true;
                                            LogHelper.WriteFile("自动张力计发送电机启动指令错误");
                                        }
                                    }                                 
                                }
                                else if (_ltStepAdjust[i] == 500 || _ltStepAdjust[i] == 1100 || _ltStepAdjust[i] == 4000)
                                {
                                    ClrMdiTsnData();

                                    if(!_ltTcp[i].Name.Contains("Auto"))
                                    {
                                        if (_ltMbsSvr[i].ReadData(ref _mdiManTsn))
                                        {
                                            _ltCntTenKeepAliveErr[i] = 0;
                                            if (_mdiManTsn.Data != null)
                                            {
                                                _ltCntTenKeepAliveErr[i] = 0;
                                                ten = (float)Math.Round((float)_mdiManTsn.Data[0], 2);                   //保留两位小数
                                                                                                                         //    ten = float.Parse(((float)_mdi.Data[0]).ToString("0.00"));
                                            }
                                            else
                                            {
                                                LogHelper.WriteFile("手持张力计读张力值为空");
                                                _ltCntTenKeepAliveErr[i]++;
                                            }

                                        }
                                        else
                                        {
                                            LogHelper.WriteFile("手持张力计读张力值不成功");
                                            _ltCntTenKeepAliveErr[i]++;
                                        }
                                    }
                                    else
                                    {
                                        if (_ltMbsSvr[i].ReadData(ref _mdiAutTsn))
                                        {
                                            _ltCntTenKeepAliveErr[i] = 0;
                                            if (_mdiAutTsn.Data != null)
                                            {
                                                _ltCntTenKeepAliveErr[i] = 0;
                                                ten = (float)Math.Round((float)_mdiAutTsn.Data[0], 2);                   //保留两位小数
                                                                                                                         //    ten = float.Parse(((float)_mdi.Data[0]).ToString("0.00"));
                                            }
                                            else
                                            {
                                                LogHelper.WriteFile("自动张力计读张力值为空");
                                                _ltCntTenKeepAliveErr[i]++;
                                            }

                                        }
                                        else
                                        {
                                            LogHelper.WriteFile("自动张力计读张力值不成功");
                                            _ltCntTenKeepAliveErr[i]++;
                                        }
                                    }
                                }
                                else if(_ltStepAdjust[i] == 650)
                                {
                                    if (_ltMotorRun[i])
                                    {
                                        _mdiMotorRun.FunCode = FuncCode.WriteMultipleData;
                                        _mdiMotorRun.Data = new object[] { 1,4 };

                                        if (_ltMbsSvr[i].WriteData(_mdiMotorRun))
                                        {
                                            Thread.Sleep(2000);
                                            if (_ltMbsSvr[i].ReadData(ref _mdiMotorRun))
                                            {
                                                if (_mdiMotorRun.Data != null)
                                                {
                                                    if (_mdiMotorRun.Data.Length >= 2)
                                                    {
                                                        if ((ushort)_mdiMotorRun.Data[1] == 0)
                                                        {
                                                            _ltMotorRun[i] = false;
                                                        }
                                                        else
                                                        {
                                                            _ltMotorRunErr[i] = true;
                                                            LogHelper.WriteFile("自动张力计电机还未达到空闲状态");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        _ltMotorRunErr[i] = true;
                                                        LogHelper.WriteFile("自动张力计读取电机状态值个数错误");
                                                    }
                                                }
                                                else
                                                {
                                                    _ltMotorRunErr[i] = true;
                                                    LogHelper.WriteFile("自动张力计读取电机状态值为空");
                                                }
                                            }
                                            else
                                            {
                                                LogHelper.WriteFile("自动张力计读取电机状态错误");
                                            }
                                        }
                                        else
                                        {
                                            _ltMotorRunErr[i] = true;
                                            LogHelper.WriteFile("自动张力计发送电机停止指令错误");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                LogHelper.WriteFile("张力计未连接");
                                _ltCntTenKeepAliveErr[i]++;
                            }
                        }

                    }
                    else
                        _ltCntTenKeepAliveErr[i] = 0;

                    if (_ltCntTenKeepAliveErr[i] > 4)
                    {
                        _ltTenKeepAliveErr[i] = true;
                    }


                    //调整中,采集张力值,并判断是否稳定
                    if ((_ltStepAdjust[i] == 500 || _ltStepAdjust[i] == 4000) &&
                        _ltTenAdj[i].StepAdjust == 30 &&
                        !_ltTenAdj[i].WriteCur &&
                        !_ltStable[i]
                        && ten > 0.0)
                    {
                        _ltTenAdj[i].ValActTen = ten;

                        TensionAdjInfo tai = new TensionAdjInfo()
                        {
                            NameDev = _ltTenAdj[i].NameDev,
                            NameTen = _ltTenAdj[i].NameTen,
                            StsLeftAdj = _ltTenAdj[i].StsLeftAdj,
                            StsRightAdj = _ltTenAdj[i].StsRightAdj,
                            ValActTen = _ltTenAdj[i].ValActTen,
                            ValReadCur = _ltTenAdj[i].ValReadCur,
                            TimeCreate = DateTime.Now.ToLocalTime()
                        };
                        _ltTenAdjData[i].Add(tai);                 //添加张力实时记录到表

                        if (_ltTenAdj[i].StsLeftAdj)
                            _ltActLineLen[i].Add(_ltLeftLineLen[i][deviceId]);

                        if (_ltTenAdj[i].StsRightAdj)
                            _ltActLineLen[i].Add(_ltRightLineLen[i][deviceId]);

                        _ltActTen[i].Add(ten);

                        if (_ltActTen[i].Count >= PointCount)
                        {
                            ltTemTen = _ltActTen[i].GetRange(_ltActTen[i].Count - PointCount, PointCount);

                            ltTemTen.Sort();

                            aver = (float)Math.Round(ltTemTen.Average(), 2);

                            max = ltTemTen[PointCount - 1];


                            if (aver > _ltTenAdj[i].ValTargetTen * 0.5)
                            {
                                if (max - ltTemTen[0] <= _ltDevPar[i][deviceId].WaveRange)
                                {
                                    update1 = true;
                                    LogHelper.WriteFile(string.Format("C:{0},aver1:{1},max1:{2},min1:{3}", _ltTenAdj[i].ValReadCur, aver, max, ltTemTen[0]));
                                }
                                else
                                {
                                    if (max - ltTemTen[1] < _ltDevPar[i][deviceId].WaveRange * 0.9)
                                    {
                                        update1 = true;

                                        ltTemTen.RemoveAt(0);
                                        aver = (float)Math.Round(ltTemTen.Average(), 2);
                                        LogHelper.WriteFile(string.Format("C:{0},aver1:{1},max1:{2},min1_:{3}", _ltTenAdj[i].ValReadCur, aver, max, ltTemTen[1]));
                                    }
                                    else if (ltTemTen[PointCount - 2] - ltTemTen[0] < _ltDevPar[i][deviceId].WaveRange * 0.9)
                                    {
                                        update1 = true;

                                        ltTemTen.RemoveAt(PointCount - 1);
                                        aver = (float)Math.Round(ltTemTen.Average(), 2);
                                        LogHelper.WriteFile(string.Format("C:{0},aver1:{1},max1_:{2},min1:{3}", _ltTenAdj[i].ValReadCur, aver, ltTemTen[PointCount - 2], ltTemTen[0]));
                                    }
                                }

                                if (update1)
                                {
                                    _ltAvrTen[i].Add(aver);

                                    ltTemTen.Clear();
                                    if (_ltAvrTen[i].Count >= PointCount)
                                    {
                                        ltTemTen = _ltAvrTen[i].GetRange(_ltAvrTen[i].Count - PointCount, PointCount);

                                        ltTemTen.Sort();
                                        aver = (float)Math.Round(ltTemTen.Average(), 2);

                                        max = ltTemTen[PointCount - 1];

                                        if (max - ltTemTen[0] <= _ltDevPar[i][deviceId].WaveRange)
                                        {
                                            update2 = true;
                                            LogHelper.WriteFile(string.Format("C:{0},aver2:{1},max2:{2},min2:{3}", _ltTenAdj[i].ValReadCur, aver, max, ltTemTen[0]));
                                        }
                                        else
                                        {

                                            if (max - ltTemTen[1] < _ltDevPar[i][deviceId].WaveRange * 0.9)
                                            {
                                                update2 = true;
                                                ltTemTen.RemoveAt(0);
                                                aver = (float)Math.Round(ltTemTen.Average(), 2);
                                                LogHelper.WriteFile(string.Format("C:{0},aver2:{1},max2:{2},min2_:{3}", _ltTenAdj[i].ValReadCur, aver, max, ltTemTen[1]));
                                            }
                                            else if (ltTemTen[PointCount - 2] - ltTemTen[0] < _ltDevPar[i][deviceId].WaveRange * 0.9)
                                            {
                                                update2 = true;
                                                ltTemTen.RemoveAt(PointCount - 1);
                                                aver = (float)Math.Round(ltTemTen.Average(), 2);
                                                LogHelper.WriteFile(string.Format("C:{0},aver2:{1},max2_:{2},min2:{3}", _ltTenAdj[i].ValReadCur, aver, ltTemTen[PointCount - 2], ltTemTen[0]));
                                            }
                                        }

                                        if (update2)
                                        {
                                            int cnt = _ltActLineLen[i].Count;
                                            LogHelper.WriteFile("线长List长度" + cnt.ToString());
                                            int dv = 0;
                                            if (cnt >= PointCount * 2 - 1)
                                            {
                                                LogHelper.WriteFile(string.Format("开始线长:{0},稳定线长:{1}", _ltActLineLen[i][0], _ltActLineLen[i][cnt - 1]));
                                                dv = (int)Math.Abs(_ltActLineLen[i][0] - _ltActLineLen[i][cnt - 1]);

                                                LogHelper.WriteFile(string.Format("张力开始到稳定后轮子转动{0}Pcs", dv));

                                                if (dv >= _ltDevPar[i][deviceId].LineLenMin)
                                                {
                                                    _ltCntLineLenErr[i] = 0;
                                                    _ltLineLenErr[i] = false;
                                                    _ltTenAdj[i].ValAverTen = aver;
                                                    LogHelper.WriteFile("张力稳定并且线长合格:" + aver);
                                                    _ltStable[i] = true;
                                                }
                                                else
                                                {
                                                    _ltLineLenErr[i] = true;
                                                    _ltCntLineLenErr[i]++;
                                                    LogHelper.WriteFile("线长不合格");
                                                    if (_ltCntLineLenErr[i] > 8)
                                                    {
                                                        _ltCntLineLenErr[i] = 0;
                                                        _ltLineLenErr[i] = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }


                    //验证中，只采集张力值
                    if (_ltTenAdj[i].StepAdjust == 30 && _ltStepAdjust[i] == 1100 && ten > 0.0)
                    {
                        _ltTenAdj[i].ValActTen = ten;
                        TensionAdjInfo tai = new TensionAdjInfo()
                        {
                            NameDev = _ltTenAdj[i].NameDev,
                            NameTen = _ltTenAdj[i].NameTen,
                            StsLeftChk = _ltTenAdj[i].StsLeftChk,
                            StsRightChk = _ltTenAdj[i].StsRightChk,
                            ValActTen = ten,
                            ValReadCur = _ltTenAdj[i].ValReadCur,
                            TimeCreate = DateTime.Now.ToLocalTime()
                        };
                        _ltTenAdjData[i].Add(tai);                 //添加张力实时记录到表

                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteFile(ex.ToString());
                }

            }
        }


        #endregion

        #region Plc

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

        private bool RisingEdge(int Start, int End)
        {
            if (End > Start)
                return true;
            else
                return false;
        }

        private bool FallingEdge(bool Start, bool End)
        {
            if (Start)
            {
                if (!End)
                    return true;
                return false;
            }
            return false;
        }

        private void PlcScan(object plc)
        {
            PlcInfo pi = (PlcInfo)plc;
            int i = pi.Index;
            int j = pi.SubIndex;

            bool leftStatus = false, rightStatus = false;

            bool leftCheck = false, rightCheck = false;

            bool val = false;

            float leftCurrent = 0, rightCurrent = 0;
            float leftLineLen = 0, rightLineLen = 0;
            string typeNo = "", copperWireNo = "";
            int deviceIndex = 0;

            while (true)
            {
                if (_bAbort)
                    break;

                Thread.Sleep(50);

                /*
                 _ltTenAdj[i].StepAdjust
                    0：没有调整
                    10：代表已经读到型号，启动去读单位，目标张力，ModelId,经验电流
                    20：锁飞叉
                    30：张力调整中，不断采集张力值，调整电流
                    40：张力调整完成
                    50:清除调整完成信号OK

                    100:调整信号消失,异常终止调整
                    110：异常终止
                 */

                try
                {
                    if (_ltPlc[i][j].Connected)
                    {
                        lock (_ltObjPlc[i][j])
                        {
                            #region Left

                            if (ReadLeftCheck(i, j, out leftCheck))
                            {
                                _ltLeftCheck[i][j] = leftCheck;
                            }

                            if (ReadLeftStatus(i, j, out leftStatus))
                            {
                                _ltLeftStatus[i][j] = leftStatus;
                            }

                            switch (_ltStepPlcScanL[i][j])
                            {
                                case 0:
                                    if (RisingEdge(_ltLeftStatusPre[i][j], _ltLeftStatus[i][j]))
                                    {
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},左边,扫到调整信号", i, j));
                                        _ltStepPlcScanL[i][j] = 50;
                                    }

                                    if (RisingEdge(_ltLeftCheckPre[i][j], _ltLeftCheck[i][j]))
                                    {
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},左边,扫到验证信号", i, j));
                                        _ltStepPlcScanL[i][j] = 50;
                                    }
                                    break;
                                case 50:
                                    if (_ltTenAdj[i].StepAdjust > 0)
                                    {
                                        lock (_ltObjPlc[i][_ltTenAdj[i].IndexDev])                      //此处的_ltTenAdj[i].IndexDev 为上一台未调整完成的索引
                                        {

                                            if (_ltTenAdj[i].IndexDev != j)
                                            {
                                                if (_ltTenAdj[i].StsLeftAdj)
                                                {
                                                    WriteLeftStatus(i, _ltTenAdj[i].IndexDev, false);
                                                }
                                            }
                                            if (_ltTenAdj[i].StsRightAdj)
                                            {
                                                WriteRightStatus(i, _ltTenAdj[i].IndexDev, false);
                                            }



                                            if (_ltTenAdj[i].IndexDev != j)
                                            {
                                                if (_ltTenAdj[i].StsLeftChk)
                                                {
                                                    WriteLeftCheck(i, _ltTenAdj[i].IndexDev, false);
                                                }

                                            }
                                            if (_ltTenAdj[i].StsRightChk)
                                            {
                                                WriteRightCheck(i, _ltTenAdj[i].IndexDev, false);
                                            }
                                        }

                                        _ltStepPlcScanL[i][j] += 50;
                                    }
                                    else
                                        _ltStepPlcScanL[i][j] += 50;
                                    break;
                                case 100:
                                    if (_ltTenAdj[i].StepAdjust == 0)
                                    {
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},左边，上一次调整/验证完成", i, _ltTenAdj[i].IndexDev));
                                        _ltStepPlcScanL[i][j] += 50;
                                    }
                                    break;
                                case 150:
                                    _ltTenAdj[i].IndexDev = j;                                                      //切换IndexDev
                                    _ltTenAdj[i].IndexTen = i;

                                    _ltTenAdj[i].NameDev = _ltDevPar[i][j].DeviceName;
                                    _ltTenAdj[i].NameTen = _ltTcp[i].Name;
                                    deviceIndex = j;


                                    if (ReadType(i, j, out typeNo))
                                    {
                                        if (ReadCopperWireNo(i, j, out copperWireNo))
                                        {
                                            LogHelper.WriteFile(i.ToString());
                                            if (_ltTi[i] == null)
                                                LogHelper.WriteFile("型号list未初始化");
                                            if (!string.IsNullOrEmpty(copperWireNo))
                                                _ltTi[i].CopperWireNo = copperWireNo;
                                            else
                                                _ltTi[i].CopperWireNo = "";

                                            if (!string.IsNullOrEmpty(typeNo))
                                                _ltTi[i].TypeNo = typeNo;
                                            else
                                                _ltTi[i].TypeNo = typeNo;
                                            _ltTi[i].DeviceName = _ltDevPar[i][deviceIndex].DeviceName;
                                            _ltTi[i].LineName = _ltDevPar[i][deviceIndex].LineName;

                                            _ltTenAdj[i].TypeNo = typeNo;


                                            LogHelper.WriteFile(string.Format("TypeNo:{0}", _ltTi[i].TypeNo));
                                            LogHelper.WriteFile(string.Format("调整{0},左边,重新启动一台设备调整/验证", i));

                                            _ltTenAdj[i].StepAdjust = 10;
                                            LogHelper.WriteFile(string.Format("StepAdjust:{0}", _ltTenAdj[i].StepAdjust));

                                            _ltStepPlcScanL[i][j] += 50;
                                        }
                                    }

                                    break;
                                case 200:
                                    if (_ltTenAdj[i].StepAdjust == 20)
                                    {
                                        WriteLeftLock(i, j, true);
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},左边，锁飞叉", i, j));
                                        _ltTenAdj[i].StsLock = true;

                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},左边,调整启动", i, j));
                                        _ltTenAdj[i].StepAdjust = 30;                                           //张力计开始拉扯有效
                                        LogHelper.WriteFile(string.Format("StepAdjust:{0}", _ltTenAdj[i].StepAdjust));
                                        _ltStepPlcScanL[i][j] += 50;
                                    }
                                    break;
                                case 250:
                                    if (_ltTenAdj[i].StepAdjust == 40)
                                    {
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},左边,调整完成", i, j));
                                        _ltStepPlcScanL[i][j] += 50;
                                    }
                                    break;
                                case 300:
                                    WriteLeftLock(i, j, false);
                                    WriteLeftFinished(i, j, true);
                                    _ltTenAdj[i].StsLock = false;
                                    LogHelper.WriteFile(string.Format("调整{0},设备{1},左边,松飞叉", i, j));
                                    LogHelper.WriteFile(string.Format("调整{0},设备{1},左边,清除调整信号", i, j));

                                    _ltStepPlcScanL[i][j] += 50;
                                    break;
                                case 350:
                                    if (!_ltLeftStatus[i][j])
                                    {
                                        _ltTenAdj[i].TypeNo = "";

                                        WriteLeftFinished(i, j, false);

                                        _ltTenAdj[i].StepAdjust = 50;
                                        LogHelper.WriteFile(string.Format("StepAdjust:{0}", _ltTenAdj[i].StepAdjust));

                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},左边,清除调整完成信号", i, j));

                                        _ltStepPlcScanL[i][j] = 0;
                                    }
                                    break;

                                //异常终止调整处理流程
                                case 1000:
                                    if (_ltTenAdj[i].StepAdjust == 120)
                                    {
                                        WriteLeftLock(i, j, false);
                                        WriteLeftFinished(i, j, true);
                                        _ltTenAdj[i].StsLock = false;
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},左边,异常终止,松飞叉", i, j));
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},左边,异常终止,清除调整信号", i, j));

                                        _ltStepPlcScanL[i][j] += 50;
                                    }
                                    break;
                                case 1050:
                                    if (!_ltLeftStatus[i][j])
                                    {
                                        _ltTenAdj[i].TypeNo = "";
                                        WriteLeftFinished(i, j, false);
                                        _ltTenAdj[i].StepAdjust = 130;

                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},左边,异常终止,清除调整完成信号", i, j));
                                        _ltStepPlcScanL[i][j] = 0;
                                    }
                                    break;
                                //终止验证处理流程
                                case 2000:
                                    if (_ltTenAdj[i].StepAdjust == 210)
                                    {
                                        WriteLeftLock(i, j, false);
                                        _ltTenAdj[i].StsLock = false;

                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},左边,终止验证,松飞叉", i, j));
                                        _ltTenAdj[i].StepAdjust = 220;
                                        _ltStepPlcScanL[i][j] = 0;
                                    }
                                    break;
                            }


                            if (j == _ltTenAdj[i].IndexDev)
                            {
                                if (_ltTenAdj[i].StepAdjust >= 10)                                                                              //以等于10为界限，代表重新切换了设备
                                {
                                    _ltTenAdj[i].StsLeftAdj = _ltLeftStatus[i][j];
                                    _ltTenAdj[i].StsLeftChk = _ltLeftCheck[i][j];
                                }



                                if (FallingEdge(_ltLeftStatusPre[i][j], _ltLeftStatus[i][j]))
                                {
                                    if (_ltTenAdj[i].StepAdjust < 40)                                                                            //强制终止调整
                                    {
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},左边,强制终止调整", i, j));

                                        _ltTenAdj[i].StepAdjust = 100;
                                        LogHelper.WriteFile(string.Format("StepAdjust:{0}", _ltTenAdj[i].StepAdjust));
                                        _ltStepPlcScanL[i][j] = 1000;
                                    }

                                }

                                if (FallingEdge(_ltLeftCheckPre[i][j], _ltLeftCheck[i][j]))
                                {
                                    if (_ltTenAdj[i].StepAdjust < 40)                                                                            //强制终止验证
                                    {
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},左边,强制终止验证", i, j));

                                        _ltTenAdj[i].StepAdjust = 200;
                                        LogHelper.WriteFile(string.Format("StepAdjust:{0}", _ltTenAdj[i].StepAdjust));
                                        _ltStepPlcScanL[i][j] = 2000;
                                    }

                                }

                                if (_ltTenAdj[i].StsLeftAdj || _ltTenAdj[i].StsLeftChk)
                                {
                                    if (_ltTenAdj[i].StepAdjust >= 10 && _ltTenAdj[i].StepAdjust != 160)
                                    {
                                        if (ReadLeftLineLen(i, j, out leftLineLen))
                                            _ltLeftLineLen[i][j] = leftLineLen;

                                        if (ReadLeftCurrent(i, j, out leftCurrent))
                                        {
                                            _ltTenAdj[i].ValReadCur = leftCurrent;
                                        }

                                        if (_ltTenAdj[i].WriteCur)
                                        {
                                            WriteLeftCurrent(i, j, _ltTenAdj[i].ValWriteCur);
                                            _ltTenAdj[i].WriteCur = false;
                                        }
                                    }
                                }
                                else if (_ltTenAdj[i].StepAdjust == 110)
                                {
                                    if (ReadLeftCurrent(i, j, out leftCurrent))
                                    {
                                        _ltTenAdj[i].ValReadCur = leftCurrent;
                                    }

                                    if (_ltTenAdj[i].WriteCur)
                                    {
                                        WriteLeftCurrent(i, j, _ltTenAdj[i].ValWriteCur);
                                        _ltTenAdj[i].WriteCur = false;
                                        LogHelper.WriteFile("写左边电流成功");
                                    }
                                }
                            }

                            _ltLeftStatusPre[i][j] = _ltLeftStatus[i][j];
                            _ltLeftCheckPre[i][j] = _ltLeftCheck[i][j];
                            #endregion

                            #region Right

                            if (ReadRightCheck(i, j, out rightCheck))
                            {
                                _ltRightCheck[i][j] = rightCheck;
                            }

                            if (ReadRightStatus(i, j, out rightStatus))
                            {
                                _ltRightStatus[i][j] = rightStatus;
                            }

                            switch (_ltStepPlcScanR[i][j])
                            {
                                case 0:
                                    if (RisingEdge(_ltRightStatusPre[i][j], _ltRightStatus[i][j]))
                                    {
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},右边,扫到调整信号", i, j));
                                        _ltStepPlcScanR[i][j] = 50;
                                    }
                                    if (RisingEdge(_ltRightCheckPre[i][j], _ltRightCheck[i][j]))
                                    {
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},右边,扫到验证信号", i, j));
                                        _ltStepPlcScanR[i][j] = 50;
                                    }
                                    break;
                                case 50:
                                    if (_ltTenAdj[i].StepAdjust > 0)
                                    {
                                        lock (_ltObjPlc[i][_ltTenAdj[i].IndexDev])                      //此处的_ltTenAdj[i].IndexDev 为上一台未调整完成的索引
                                        {
                                            if (_ltTenAdj[i].StsLeftAdj)
                                            {
                                                WriteLeftStatus(i, _ltTenAdj[i].IndexDev, false);
                                            }
                                            if (_ltTenAdj[i].IndexDev != j)                                  //启动的机器跟之前调整的不一致
                                            {
                                                if (_ltTenAdj[i].StsRightAdj)
                                                {
                                                    WriteRightStatus(i, _ltTenAdj[i].IndexDev, false);
                                                }
                                            }

                                            if (_ltTenAdj[i].StsLeftChk)
                                            {
                                                WriteLeftCheck(i, _ltTenAdj[i].IndexDev, false);
                                            }

                                            if (_ltTenAdj[i].IndexDev != j)
                                            {
                                                if (_ltTenAdj[i].StsRightChk)
                                                {
                                                    WriteRightCheck(i, _ltTenAdj[i].IndexDev, false);
                                                }
                                            }
                                        }

                                        _ltStepPlcScanR[i][j] += 50;
                                    }
                                    else
                                        _ltStepPlcScanR[i][j] += 50;

                                    break;
                                case 100:
                                    if (_ltTenAdj[i].StepAdjust == 0)
                                    {
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},右边，上一次调整/验证完成", i, _ltTenAdj[i].IndexDev));
                                        _ltStepPlcScanR[i][j] += 50;
                                    }
                                    break;
                                case 150:
                                    _ltTenAdj[i].IndexDev = j;                                                      //切换IndexDev
                                    _ltTenAdj[i].IndexTen = i;

                                    _ltTenAdj[i].NameDev = _ltDevPar[i][j].DeviceName;
                                    _ltTenAdj[i].NameTen = _ltTcp[i].Name;
                                    deviceIndex = j;

                                    if (ReadType(i, j, out typeNo))
                                    {
                                        if (ReadCopperWireNo(i, j, out copperWireNo))
                                        {
                                            LogHelper.WriteFile(i.ToString());
                                            if (_ltTi[i] == null)
                                                LogHelper.WriteFile("型号list未初始化");
                                            if (!string.IsNullOrEmpty(copperWireNo))
                                                _ltTi[i].CopperWireNo = copperWireNo;
                                            else
                                                _ltTi[i].CopperWireNo = "";

                                            if (!string.IsNullOrEmpty(typeNo))
                                                _ltTi[i].TypeNo = typeNo;
                                            else
                                                _ltTi[i].TypeNo = typeNo;
                                            _ltTi[i].DeviceName = _ltDevPar[i][deviceIndex].DeviceName;
                                            _ltTi[i].LineName = _ltDevPar[i][deviceIndex].LineName;

                                            _ltTenAdj[i].TypeNo = typeNo;

                                            LogHelper.WriteFile(string.Format("TypeNo:{0}", _ltTi[i].TypeNo));
                                            LogHelper.WriteFile(string.Format("调整{0},右边,重新启动一台设备调整/验证", i));

                                            _ltTenAdj[i].StepAdjust = 10;
                                            LogHelper.WriteFile(string.Format("StepAdjust:{0}", _ltTenAdj[i].StepAdjust));

                                            _ltStepPlcScanR[i][j] += 50;
                                        }
                                    }

                                    break;
                                case 200:
                                    if (_ltTenAdj[i].StepAdjust == 20)
                                    {
                                        WriteRightLock(i, j, true);
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},右边，锁飞叉", i, j));
                                        _ltTenAdj[i].StsLock = true;

                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},右边,调整启动", i, j));

                                        _ltTenAdj[i].StepAdjust = 30;                               //张力计开始拉扯有效
                                        LogHelper.WriteFile(string.Format("StepAdjust:{0}", _ltTenAdj[i].StepAdjust));
                                        _ltStepPlcScanR[i][j] += 50;
                                    }
                                    break;
                                case 250:
                                    if (_ltTenAdj[i].StepAdjust == 40)
                                    {
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},右边,调整完成", i, j));
                                        _ltStepPlcScanR[i][j] += 50;
                                    }
                                    break;
                                case 300:
                                    WriteRightLock(i, j, false);
                                    WriteRightFinished(i, j, true);
                                    _ltTenAdj[i].StsLock = false;
                                    LogHelper.WriteFile(string.Format("调整{0},设备{1},右边,松飞叉", i, j));
                                    LogHelper.WriteFile(string.Format("调整{0},设备{1},右边,清除调整信号", i, j));

                                    _ltStepPlcScanR[i][j] += 50;
                                    break;
                                case 350:
                                    if (!_ltRightStatus[i][j])
                                    {
                                        _ltTenAdj[i].TypeNo = "";

                                        WriteRightFinished(i, j, false);

                                        _ltTenAdj[i].StepAdjust = 50;
                                        LogHelper.WriteFile(string.Format("StepAdjust:{0}", _ltTenAdj[i].StepAdjust));
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},右边,清除调整完成信号", i, j));

                                        _ltStepPlcScanR[i][j] = 0;
                                    }
                                    break;

                                //异常终止处理流程
                                case 1000:
                                    if (_ltTenAdj[i].StepAdjust == 120)
                                    {
                                        WriteRightLock(i, j, false);
                                        WriteRightFinished(i, j, true);
                                        _ltTenAdj[i].StsLock = false;
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},右边,异常终止,松飞叉", i, j));
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},右边,异常终止,清除调整信号", i, j));

                                        _ltStepPlcScanR[i][j] += 50;
                                    }
                                    break;
                                case 1050:
                                    if (!_ltRightStatus[i][j])
                                    {
                                        _ltTenAdj[i].TypeNo = "";

                                        WriteRightFinished(i, j, false);

                                        _ltTenAdj[i].StepAdjust = 130;
                                        LogHelper.WriteFile(string.Format("StepAdjust:{0}", _ltTenAdj[i].StepAdjust));
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},右边,清除调整完成信号", i, j));

                                        _ltStepPlcScanR[i][j] = 0;
                                    }
                                    break;
                                //终止验证处理流程
                                case 2000:
                                    if (_ltTenAdj[i].StepAdjust == 210)
                                    {
                                        WriteRightLock(i, j, false);
                                        _ltTenAdj[i].StsLock = false;

                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},右边,终止验证,松飞叉", i, j));
                                        _ltTenAdj[i].StepAdjust = 220;
                                        _ltStepPlcScanR[i][j] = 0;
                                    }
                                    break;
                            }


                            if (j == _ltTenAdj[i].IndexDev)
                            {
                                if (_ltTenAdj[i].StepAdjust >= 10)                                                                              //以等于10为界限，代表重新切换了设备
                                {
                                    _ltTenAdj[i].StsRightAdj = _ltRightStatus[i][j];
                                    _ltTenAdj[i].StsRightChk = _ltRightCheck[i][j];
                                }



                                if (FallingEdge(_ltRightStatusPre[i][j], _ltRightStatus[i][j]))
                                {
                                    if (_ltTenAdj[i].StepAdjust < 40)                                                                            //强制终止
                                    {
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},右边,强制终止启动", i, j));

                                        _ltTenAdj[i].StepAdjust = 150;
                                        LogHelper.WriteFile(string.Format("StepAdjust:{0}", _ltTenAdj[i].StepAdjust));
                                        _ltStepPlcScanR[i][j] = 1000;
                                    }

                                }

                                if (FallingEdge(_ltRightCheckPre[i][j], _ltRightCheck[i][j]))
                                {
                                    if (_ltTenAdj[i].StepAdjust < 40)                                                                            //强制终止
                                    {
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},右边,强制终止启动", i, j));

                                        _ltTenAdj[i].StepAdjust = 200;
                                        LogHelper.WriteFile(string.Format("StepAdjust:{0}", _ltTenAdj[i].StepAdjust));
                                        _ltStepPlcScanR[i][j] = 2000;
                                    }

                                }

                                if (_ltTenAdj[i].StsRightAdj || _ltTenAdj[i].StsRightChk)
                                {
                                    if (_ltTenAdj[i].StepAdjust >= 10 && _ltTenAdj[i].StepAdjust != 110)
                                    {
                                        if (ReadRightLineLen(i, j, out rightLineLen))
                                            _ltRightLineLen[i][j] = rightLineLen;

                                        if (ReadRightCurrent(i, j, out rightCurrent))
                                        {
                                            _ltTenAdj[i].ValReadCur = rightCurrent;
                                        }

                                        if (_ltTenAdj[i].WriteCur)
                                        {
                                            WriteRightCurrent(i, j, _ltTenAdj[i].ValWriteCur);
                                            _ltTenAdj[i].WriteCur = false;
                                        }
                                    }
                                }
                                else if (_ltTenAdj[i].StepAdjust == 160)
                                {
                                    if (ReadRightCurrent(i, j, out rightCurrent))
                                    {
                                        _ltTenAdj[i].ValReadCur = rightCurrent;
                                    }
                                    if (_ltTenAdj[i].WriteCur)
                                    {
                                        WriteRightCurrent(i, j, _ltTenAdj[i].ValWriteCur);
                                        _ltTenAdj[i].WriteCur = false;
                                        LogHelper.WriteFile("写右边电流成功");
                                    }
                                }
                            }

                            _ltRightStatusPre[i][j] = _ltRightStatus[i][j];
                            _ltRightCheckPre[i][j] = _ltRightCheck[i][j];
                            #endregion


                            if (_ltTenAdj[i].IndexDev == j)
                            {
                                if (_ltTenAdj[i].StsLeftAdj ||
                                    _ltTenAdj[i].StsRightAdj ||
                                    _ltTenAdj[i].StsLeftChk ||
                                    _ltTenAdj[i].StsRightChk)
                                    _ltPlcKeepAlive[i] = !_ltPlcKeepAlive[i];
                                WriteKeepAlive(i, j, _ltPlcKeepAlive[i]);


                                //线未拉动报警
                                if (_ltLineLenErr[i])
                                {
                                    WriteLineLenAlarm(i, j, true);
                                    LogHelper.WriteFile("线未拉动");
                                }


                                if (ReadLineLenAlarm(i, j, out val))
                                {
                                    if (val)
                                        _ltLineLenErr[i] = false;
                                }

                                //张力计与调整系统断开报警


                                if (_ltTenKeepAliveErr[i])
                                {
                                    WriteTenOffLineAlarm(i, j, true);
                                    LogHelper.WriteFile("张力计与张力系统断开");
                                }


                                if (ReadTenOffLineAlarm(i, j, out val))
                                {
                                    if (val)
                                    {
                                        _ltTenKeepAliveErr[i] = false;

                                    }
                                }

                                if (_ltValOutOfRange[i])
                                {
                                    WriteValErrAlarm(i, j, true);
                                    LogHelper.WriteFile("磁粉制动器值超范围");
                                }


                                if (ReadValErrAlarm(i, j, out val))
                                {
                                    if (val)
                                        _ltValOutOfRange[i] = false;
                                }

                                //调整系统中无此型号报警
                                if (_ltTypeErr[i])
                                {
                                    WriteTypeErrAlarm(i, j, true);
                                    LogHelper.WriteFile("张力系统中无此型号");
                                }


                                if (ReadTypeErrAlarm(i, j, out val))
                                {
                                    if (val)
                                        _ltTypeErr[i] = false;
                                }

                                //张力计单位不为N报警
                                if (_ltUnitErr[i])
                                {
                                    WriteUnitErrAlarm(i, j, true);
                                    LogHelper.WriteFile("张力计单位不为N");
                                }


                                if (ReadUnitErrAlarm(i, j, out val))
                                {
                                    if (val)
                                        _ltUnitErr[i] = false;
                                }

                                if (_ltValNotMatchErr[i])
                                {
                                    WriteValNotMatchAlarm(i, j, true);
                                    LogHelper.WriteFile("电流与张力值不匹配");
                                }


                                if (ReadValNotMatchAlarm(i, j, out val))
                                {
                                    if (val)
                                        _ltValNotMatchErr[i] = false;
                                }
                            }
                        }
                    }
                    else
                    {
                        LogHelper.WriteFile(string.Format("Plc:{0},{1}与Kepware服务器断开,线程扫描退出", i, j));
                        break;
                    }
                }

                catch (Exception ex)
                {
                    LogHelper.WriteFile(ex.ToString());
                }
            }
        }

        private bool PlcReadValue(int i, int j, short ItemId, short SubItemId, out Object value)
        {
            object ret = _ltPlc[i][j].ReadItem(ItemId, SubItemId);
            value = ret;
            if (ret != null)
            {
                return true;
            }
            else
                return false;
        }

        private bool PlcWriteValue(int i, int j, short ItemId, short SubItemId, Object value)
        {
            return _ltPlc[i][j].WriteItem(ItemId, SubItemId, value);
        }

        private bool ReadType(int i, int j, out string value)
        {
            object ret;
            if (PlcReadValue(i, j, (short)OpcItemId.TypeNo, 0, out ret))
            {
                value = ret.ToString();
                return true;
            }
            else
            {
                value = "";
                return false;
            }
        }

        private bool ReadCopperWireNo(int i, int j, out string value)
        {
            object ret;
            if (PlcReadValue(i, j, (short)OpcItemId.TypeNo, 1, out ret))
            {
                value = ret.ToString();
                return true;
            }
            else
            {
                value = "";
                return false;
            }
        }

        private bool WriteKeepAlive(int i, int j, bool value)
        {
            return PlcWriteValue(i, j, (short)OpcItemId.KeepAlive, 0, value);
        }


        private bool WriteLeftLock(int i, int j, bool value)
        {
            return PlcWriteValue(i, j, (short)OpcItemId.Lock, 0, value);
        }

        private bool WriteRightLock(int i, int j, bool value)
        {
            return PlcWriteValue(i, j, (short)OpcItemId.Lock, 1, value);
        }

        private bool ReadLeftStatus(int i, int j, out bool value)
        {
            object ret;
            if (PlcReadValue(i, j, (short)OpcItemId.Status, 0, out ret))
            {
                value = Convert.ToBoolean(ret);
                return true;
            }
            else
            {
                value = false;
                return false;
            }
        }

        private bool WriteLeftStatus(int i, int j, bool value)
        {
            LogHelper.WriteFile("清除左边调整信号");
            return PlcWriteValue(i, j, (short)OpcItemId.Status, 0, value);
        }

        private bool ReadRightStatus(int i, int j, out bool value)
        {
            object ret;
            if (PlcReadValue(i, j, (short)OpcItemId.Status, 1, out ret))
            {
                value = Convert.ToBoolean(ret);
                return true;
            }
            else
            {
                value = false;
                return false;
            }
        }

        private bool WriteRightStatus(int i, int j, bool value)
        {
            LogHelper.WriteFile("清除右边调整信号");
            return PlcWriteValue(i, j, (short)OpcItemId.Status, 1, value);
        }

        private bool ReadLeftCheck(int i, int j, out bool value)
        {
            object ret;
            if (PlcReadValue(i, j, (short)OpcItemId.Check, 0, out ret))
            {
                value = Convert.ToBoolean(ret);
                return true;
            }
            else
            {
                value = false;
                return false;
            }
        }

        private bool WriteLeftCheck(int i, int j, bool value)
        {
            return PlcWriteValue(i, j, (short)OpcItemId.Check, 0, value);
        }

        private bool ReadRightCheck(int i, int j, out bool value)
        {
            object ret;
            if (PlcReadValue(i, j, (short)OpcItemId.Check, 1, out ret))
            {
                value = Convert.ToBoolean(ret);
                return true;
            }
            else
            {
                value = false;
                return false;
            }
        }

        private bool WriteRightCheck(int i, int j, bool value)
        {
            return PlcWriteValue(i, j, (short)OpcItemId.Check, 1, value);
        }


        private bool ReadLeftCurrent(int i, int j, out float value)
        {
            object ret;
            if (PlcReadValue(i, j, (short)OpcItemId.Current, 0, out ret))
            {
                value = Convert.ToSingle(ret);
                return true;
            }
            else
            {
                value = 0;
                return false;
            }
        }

        private bool WriteLeftCurrent(int i, int j, float value)
        {
            return PlcWriteValue(i, j, (short)OpcItemId.Current, 0, value);
        }

        private bool ReadRightCurrent(int i, int j, out float value)
        {
            object ret;
            if (PlcReadValue(i, j, (short)OpcItemId.Current, 1, out ret))
            {
                value = Convert.ToSingle(ret);
                return true;
            }
            else
            {
                value = 0;
                return false;
            }
        }

        private bool WriteRightCurrent(int i, int j, float value)
        {
            return PlcWriteValue(i, j, (short)OpcItemId.Current, 1, value);
        }

        private bool WriteLeftFinished(int i, int j, bool value)
        {
            return PlcWriteValue(i, j, (short)OpcItemId.Finished, 0, value);
        }

        private bool WriteRightFinished(int i, int j, bool value)
        {
            return PlcWriteValue(i, j, (short)OpcItemId.Finished, 1, value);
        }

        private bool ReadLeftLineLen(int i, int j, out float value)
        {
            object ret;
            if (PlcReadValue(i, j, (short)OpcItemId.LineLen, 0, out ret))
            {
                value = Convert.ToSingle(ret);
                return true;
            }
            else
            {
                value = 0;
                return false;
            }
        }

        private bool ReadRightLineLen(int i, int j, out float value)
        {
            object ret;
            if (PlcReadValue(i, j, (short)OpcItemId.LineLen, 1, out ret))
            {
                value = Convert.ToSingle(ret);
                return true;
            }
            else
            {
                value = 0;
                return false;
            }
        }

        private bool ReadTenOffLineAlarm(int i, int j, out bool value)
        {
            object ret;
            if (PlcReadValue(i, j, (short)OpcItemId.Alarm, 0, out ret))
            {
                value = Convert.ToBoolean(ret);
                return true;
            }

            else
            {
                value = false;
                return false;
            }
        }
        private bool WriteTenOffLineAlarm(int i, int j, bool value)
        {
            return PlcWriteValue(i, j, (short)OpcItemId.Alarm, 0, value);
        }

        private bool ReadLineLenAlarm(int i, int j, out bool value)
        {
            object ret;
            if (PlcReadValue(i, j, (short)OpcItemId.Alarm, 1, out ret))
            {
                value = Convert.ToBoolean(ret);
                return true;
            }
            else
            {
                value = false;
                return false;
            }
        }

        private bool WriteLineLenAlarm(int i, int j, bool value)
        {
            return PlcWriteValue(i, j, (short)OpcItemId.Alarm, 1, value);
        }

        private bool ReadValErrAlarm(int i, int j, out bool value)
        {
            object ret;
            if (PlcReadValue(i, j, (short)OpcItemId.Alarm, 2, out ret))
            {
                value = Convert.ToBoolean(ret);
                return true;
            }
            else
            {
                value = false;
                return false;
            }
        }

        private bool WriteValErrAlarm(int i, int j, bool value)
        {
            return PlcWriteValue(i, j, (short)OpcItemId.Alarm, 2, value);
        }

        private bool ReadTypeErrAlarm(int i, int j, out bool value)
        {
            object ret;
            if (PlcReadValue(i, j, (short)OpcItemId.Alarm, 3, out ret))
            {
                value = Convert.ToBoolean(ret);
                return true;
            }
            else
            {
                value = false;
                return false;
            }
        }

        private bool WriteTypeErrAlarm(int i, int j, bool value)
        {
            return PlcWriteValue(i, j, (short)OpcItemId.Alarm, 3, value);
        }

        private bool ReadUnitErrAlarm(int i, int j, out bool value)
        {
            object ret;
            if (PlcReadValue(i, j, (short)OpcItemId.Alarm, 4, out ret))
            {
                value = Convert.ToBoolean(ret);
                return true;
            }
            else
            {
                value = false;
                return false;
            }
        }

        private bool WriteValNotMatchAlarm(int i, int j, bool value)
        {
            return PlcWriteValue(i, j, (short)OpcItemId.Alarm, 5, value);
        }

        private bool ReadValNotMatchAlarm(int i, int j, out bool value)
        {
            object ret;
            if (PlcReadValue(i, j, (short)OpcItemId.Alarm, 5, out ret))
            {
                value = Convert.ToBoolean(ret);
                return true;
            }
            else
            {
                value = false;
                return false;
            }
        }

        private bool WriteUnitErrAlarm(int i, int j, bool value)
        {
            return PlcWriteValue(i, j, (short)OpcItemId.Alarm, 4, value);
        }

        #endregion

        #region Adjust
        private void AutoAdjust(object idx)
        {
            int i = (int)idx;
            int deviceId = 0;
            while (true)
            {
                if (_bAbort)
                    break;
                Thread.Sleep(100);


                try
                {


                    if (_ltTenAdj[i].StepAdjust == 100 || _ltTenAdj[i].StepAdjust == 150)
                    {
                        _ltStepAdjust[i] = 2000;
                    }

                    if (_ltTenAdj[i].StepAdjust == 200)
                    {
                        _ltStepAdjust[i] = 3000;
                    }

                    bool lr = false;
                    if (_ltTenAdj[i].StsLeftAdj || _ltTenAdj[i].StsLeftChk)
                        lr = false;

                    if (_ltTenAdj[i].StsRightAdj)
                        lr = true;

                    switch (_ltStepAdjust[i])
                    {
                        case 0:
                            if (_ltTenAdj[i].StepAdjust == 10)
                            {
                                deviceId = _ltTenAdj[i].IndexDev;

                                _ltDValue[i] = 0.0f;
                                _ltValPre[i] = 0.0f;
                                _ltTenAdj[i].ValResultTen = 0;
                                _ltActTen[i].Clear();
                                _ltAvrTen[i].Clear();
                                _ltActLineLen[i].Clear();
                                _ltTenAdjData[i].Clear();

                                _ltTenAdj[i].ValAverTen = 0;
                                _ltTenAdj[i].ValTargetTen = 0;
                                _ltTenAdj[i].ValActTen = 0;

                                _ltTenAdj[i].ValTempCur = 0;

                                _ltValOutOfRange[i] = false;

                                _ltTenAdjRest[i].NameDev = _ltTenAdj[i].NameDev;
                                _ltTenAdjRest[i].NameTen = _ltTenAdj[i].NameTen;

                                _ltTenAdjRest[i].ValResultTen = 0;
                                _ltTenAdjRest[i].ValReadCur = 0;


                                LogHelper.WriteFile(string.Format("LeftAdj:{0},RightAdj:{1}", _ltTenAdj[i].StsLeftAdj, _ltTenAdj[i].StsRightAdj));

                                LogHelper.WriteFile(string.Format("调整{0}变量清零", i));


                                _ltCntUnitErr[i] = 0;

                                _ltCntLineLenErr[i] = 0;

                                _ltCntTenKeepAliveErr[i] = 0;

                                _ltStepAdjust[i] += 50;
                            }
                            break;
                        case 50:
                            if(!_ltTcp[i].Name.Contains("Auto"))
                            {
                                _ltReadUnit[i] = true;
                                _ltStepAdjust[i] += 50;
                            }
                            else
                                _ltStepAdjust[i] += 100;
                            break;
                        case 100:
                            if (_ltUnitErr[i])
                            {
                                LogHelper.WriteFile("张力计单位错误");
                                _ltStepAdjust[i] = 1000;
                            }
                            else if (!_ltReadUnit[i])
                            {
                                if (_ltTenAdj[i].StsLeftAdj || _ltTenAdj[i].StsRightAdj)
                                    _ltStepAdjust[i] += 50;
                                else
                                    _ltStepAdjust[i] = 200;
                            }
                            break;
                        case 150:
                            //查询需要调整的目标张力
                            TypeInfo ti = new TypeInfo();
                            ti = _typBll.GetTypeInfo(_ltTi[i]);
                            float target = 0.0f;

                            if (ti != null)
                            {
                                if (_ltTenAdj[i].StsLeftAdj)
                                {
                                    target = ti.LeftTension;
                                    LogHelper.WriteFile(string.Format("调整{0},左边,查询到目标张力{1}", i, target));
                                }
                                else
                                {
                                    target = ti.RightTension;
                                    LogHelper.WriteFile(string.Format("调整{0},右边,查询到目标张力{1}", i, target));
                                }
                            }
                            else
                            {
                                LogHelper.WriteFile(string.Format("系统中无此设备:{0},型号:{1}", _ltTi[i].DeviceName, _ltTenAdj[i].TypeNo));
                                _ltTypeErr[i] = true;
                                _ltStepAdjust[i] = 1000;
                            }


                            if (target > 0)
                            {
                                _ltTenAdj[i].ValTargetTen = target;
                                _ltTi[i].ModelId = ti.ModelId;
                                _ltTi[i].MaxVal = ti.MaxVal;
                                _ltGroupId[i] = GetMillis();
                                _ltStepAdjust[i] += 100;
                            }

                            break;

                        case 200:
                            _ltTi[i].ModelId = _typBll.GetModelId(_ltTi[i]);
                            _ltGroupId[i] = GetMillis();
                            LogHelper.WriteFile(string.Format("调整{0},获取ModelId GroupId", i));

                            if (_ltTenAdj[i].StsLeftAdj || _ltTenAdj[i].StsRightAdj)
                                _ltStepAdjust[i] += 50;
                            else
                            {
                                _ltTenAdj[i].StepAdjust = 20;                                       //plc端锁飞叉
                                _ltStepAdjust[i] = 1100;
                            }
                            break;
                        case 250:
                            if (_ltTenAdj[i].StsLeftAdj)
                            {
                                if (_ltDevPar[i][deviceId].LeftK > 0)
                                {
                                    //查询记录表获得经验电流
                                    float cur = 0.0f;

                                    LogHelper.WriteFile(string.Format("TypeNo;{0}", _ltTi[i].TypeNo));
                                    cur = _typBll.GetCurrent(_ltTi[i], 0);

                                    LogHelper.WriteFile(string.Format("调整{0},左边,查询到经验电流{1}", i, cur));

                                    if (cur == 0.0f)
                                    {
                                        _ltTenAdj[i].ValTempCur = _ltTenAdj[i].ValReadCur;                                  //预存当前电流
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},当前电流:{2}", i, deviceId, _ltTenAdj[i].ValTempCur));
                             
                                        _ltTenAdj[i].ValWriteCur = _ltTenAdj[i].ValReadCur;
                             
                                    }
                                    else
                                        _ltTenAdj[i].ValWriteCur = cur;

                                    if (_ltTenAdj[i].ValWriteCur > _ltTi[i].MaxVal)
                                    {
                                        _ltValOutOfRange[i] = true;
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},磁粉制动器值超范围", i, deviceId));
                                        _ltStepAdjust[i] = 1000;
                                    }
                                    else
                                    {
                                        _ltTenAdj[i].WriteCur = true;

                                        if (_ltTenAdj[i].StepAdjust == 10)
                                            _ltTenAdj[i].StepAdjust = 20;                                                                   //PLC端锁飞叉
                                        LogHelper.WriteFile(string.Format("调整{0},启动写经验电流{1}", i, _ltTenAdj[i].ValWriteCur));
                                        LogHelper.WriteFile(string.Format("StepAdjust:{0}", _ltTenAdj[i].StepAdjust));

                                        _ltStepAdjust[i] += 50;
                                    }
                                }
                                else
                                {
                                    _ltTenAdj[i].StepAdjust = 20;                                                                   //PLC端锁飞叉
                                    _ltStepAdjust[i] = 4000;                                                                            //计算k和a
                                }
                            }

                            if (_ltTenAdj[i].StsRightAdj)
                            {
                                if (_ltDevPar[i][deviceId].RightK > 0)
                                {
                                    float cur = 0.0f;
                                    LogHelper.WriteFile(string.Format("TypeNo;{0}", _ltTi[i].TypeNo));
                                    cur = _typBll.GetCurrent(_ltTi[i], 1);
                                    LogHelper.WriteFile(string.Format("调整{0},右边,查询到经验电流{1}", i, cur));

                                    if (cur == 0.0f)
                                    {
                                        _ltTenAdj[i].ValTempCur = _ltTenAdj[i].ValReadCur;                                  //预存当前电流
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},当前电流:{2}", i, deviceId, _ltTenAdj[i].ValTempCur));
                                        _ltTenAdj[i].ValWriteCur = _ltTenAdj[i].ValReadCur;
                                        //_ltTenAdj[i].ValWriteCur = _ltDevPar[i][deviceId].RightK * _ltTenAdj[i].ValTargetTen + _ltDevPar[i][deviceId].RightA;
                                        //LogHelper.WriteFile(string.Format("调整{0},根据目标张力:{1},计算出经验电流:{2}", i, _ltTenAdj[i].ValTargetTen, _ltTenAdj[i].ValWriteCur));
                                    }
                                    else
                                        _ltTenAdj[i].ValWriteCur = cur;

                                    if (_ltTenAdj[i].ValWriteCur > _ltTi[i].MaxVal)
                                    {
                                        _ltValOutOfRange[i] = true;
                                        LogHelper.WriteFile(string.Format("调整{0},设备{1},磁粉制动器值超范围", i, deviceId));
                                        _ltStepAdjust[i] = 1000;
                                    }
                                    else
                                    {
                                        _ltTenAdj[i].WriteCur = true;

                                        if (_ltTenAdj[i].StepAdjust == 10)
                                            _ltTenAdj[i].StepAdjust = 20;                                                                   //PLC端锁飞叉
                                        LogHelper.WriteFile(string.Format("调整{0},启动写经验电流{1}", i, _ltTenAdj[i].ValWriteCur));
                                        LogHelper.WriteFile(string.Format("StepAdjust:{0}", _ltTenAdj[i].StepAdjust));

                                        _ltStepAdjust[i] += 50;
                                    }
                                }
                                else
                                {
                                    _ltTenAdj[i].StepAdjust = 20;                                                                   //PLC端锁飞叉
                                    _ltStepAdjust[i] = 4000;                                                                            //计算k和a
                                }

                            }

                            break;
                        case 300:
                            if(_ltTenAdj[i].StepAdjust == 30)                                                                   //自动张力计电机开始转动
                            {
                                if(_ltTcp[i].Name.Contains("Auto"))
                                {
                                    _ltMotorRun[i] = true;
                                    _ltStepAdjust[i] += 50;
                                }
                                else
                                    _ltStepAdjust[i] += 100;
                            }
                            break;
                        case 350:
                            if (_ltMotorRunErr[i])
                            {
                                LogHelper.WriteFile("自动张力计电机启动错误");
                                _ltStepAdjust[i] = 1000;
                            }
                            else if (!_ltMotorRun[i])
                                _ltStepAdjust[i] = 500;
                            break;
                        case 400:
                            //目标张力与上一次稳定后张力的差值乘以一个系数再加上之前的电流值获得新电流值写入

                            if (_ltTenAdj[i].StsLeftAdj)
                                _ltTenAdj[i].ValWriteCur = _ltTenAdj[i].ValReadCur + (_ltDevPar[i][deviceId].LeftK * _ltDValue[i]);

                            if (_ltTenAdj[i].StsRightAdj)
                                _ltTenAdj[i].ValWriteCur = _ltTenAdj[i].ValReadCur + (_ltDevPar[i][deviceId].RightK * _ltDValue[i]);

                            LogHelper.WriteFile(string.Format("调整{0},根据张力差值{1},计算出目标电流{2}", i, _ltDValue[i], _ltTenAdj[i].ValWriteCur));

                            if (_ltTenAdj[i].ValWriteCur > _ltTi[i].MaxVal)
                            {
                                _ltValOutOfRange[i] = true;
                                LogHelper.WriteFile(string.Format("磁粉制动器值超范围,将被写入的值:{0},最大值:{1}", _ltTenAdj[i].ValWriteCur, _ltTi[i].MaxVal));
                                _ltStepAdjust[i] = 1000;
                            }
                            else
                            {
                                _ltTenAdj[i].WriteCur = true;
                                _ltStepAdjust[i] += 50;
                            }

                            break;
                        case 450:
                            if (!_ltTenAdj[i].WriteCur)     //等待电流写完成
                            {
                                LogHelper.WriteFile(string.Format("调整{0},写经验电流完成", i));

                                _ltActLineLen[i].Clear();

                                _ltStepAdjust[i] += 20;
                            }
                            break;
                        case 470:
                            Thread.Sleep(1000);
                            _ltStepAdjust[i] += 30;
                            break;
                        case 500:
                            if (_ltTenAdj[i].StepAdjust == 30)
                            {
                                if (_ltStable[i])
                                {
                                    _ltActTen[i].Clear();
                                    _ltAvrTen[i].Clear();
                                    double k = _ltTenAdj[i].StsLeftAdj ? _ltDevPar[i][deviceId].LeftK : _ltDevPar[i][deviceId].RightK;
                                    double a = _ltTenAdj[i].StsLeftAdj ? _ltDevPar[i][deviceId].LeftA : _ltDevPar[i][deviceId].RightA;

                                    double maxTen = _ltTenAdj[i].ValReadCur / (k * (1 - a));
                                    double minTen = _ltTenAdj[i].ValReadCur / (k * (1 + a));

                                    //多次调整后，稳定张力达到目标合格值
                                    if (Math.Abs(_ltTenAdj[i].ValTargetTen - _ltTenAdj[i].ValAverTen) < _ltDevPar[i][deviceId].ValRange)
                                    {
                                        _ltDValue[i] = _ltTenAdj[i].ValTargetTen - _ltTenAdj[i].ValAverTen;
                                        _ltValPre[i] = _ltTenAdj[i].ValAverTen;
                                        _ltStable[i] = false;
                                        LogHelper.WriteFile(string.Format("张力计:{0},设备:{1},左/右:{2},Aver:{3},DValue:{4}", _ltTenAdj[i].NameTen, _ltTenAdj[i].NameDev, lr, _ltTenAdj[i].ValAverTen.ToString(), _ltDValue[i]));

                                        _ltTenAdj[i].ValResultTen = _ltTenAdj[i].ValAverTen;
                                        _ltTenAdjRest[i].StsLeftAdj = _ltTenAdj[i].StsLeftAdj;
                                        _ltTenAdjRest[i].StsRightAdj = _ltTenAdj[i].StsRightAdj;
                                        _ltTenAdjRest[i].ValResultTen = _ltTenAdj[i].ValResultTen;
                                        _ltTenAdjRest[i].ValReadCur = _ltTenAdj[i].ValReadCur;
                                        _ltTenAdjRest[i].TimeCreate = DateTime.Now.ToLocalTime();

                                        LogHelper.WriteFile(string.Format("张力计:{0},设备:{1},左/右:{2},结果合格2,C:{3},T:{4}", _ltTenAdj[i].NameTen, _ltTenAdj[i].NameDev, lr, _ltTenAdj[i].ValReadCur.ToString(), _ltTenAdj[i].ValAverTen));

                                        TensionAdjInfo tai = new TensionAdjInfo()
                                        {
                                            NameDev = _ltTenAdj[i].NameDev,
                                            NameTen = _ltTenAdj[i].NameTen,
                                            StsLeftAdj = _ltTenAdj[i].StsLeftAdj,
                                            StsRightAdj = _ltTenAdj[i].StsRightAdj,
                                            ValReadCur = _ltTenAdj[i].ValReadCur,
                                            ValResultTen = _ltTenAdj[i].ValResultTen,
                                            ValActTen = _ltTenAdj[i].ValAverTen,
                                            TimeCreate = DateTime.Now.ToLocalTime(),
                                        };
                                        LogHelper.WriteFile(string.Format("张力计:{0},设备:{1},左/右:{2},张力调整完成2", _ltTenAdj[i].NameTen, _ltTenAdj[i].NameDev, lr));
                                        _ltTenAdjData[i].Add(tai);

                                        if (_ltTenAdj[i].ValAverTen < minTen || _ltTenAdj[i].ValAverTen > maxTen)
                                            _ltValNotMatchErr[i] = true;

                                        _ltStepAdjust[i] += 50;
                                    }
                                    else
                                    {
                                        _ltCntValNotMatchErr[i] = 0;
                                        _ltDValue[i] = _ltTenAdj[i].ValTargetTen - _ltTenAdj[i].ValAverTen;
                                        _ltValPre[i] = _ltTenAdj[i].ValAverTen;
                                        _ltStable[i] = false;
                                        LogHelper.WriteFile(string.Format("张力计:{0},设备:{1},左/右:{2},Aver:{3},DValue:{4}", _ltTenAdj[i].NameTen, _ltTenAdj[i].NameDev, lr, _ltTenAdj[i].ValAverTen.ToString(), _ltDValue[i]));
                                        _ltStepAdjust[i] = 300;
                                    }
                                        
                                }
                            }
                            break;
                        case 550:
                            if (_ltTenAdjData[i].Count > 0 && _ltGroupId[i] != "")
                            {
                                WriteAdjDataToCsv(_ltTi[i], _ltTenAdjData[i], _ltGroupId[i]);
                                _typBll.AddListCurrentRocord(_ltTi[i], _ltTenAdjData[i], _ltGroupId[i]);

                                _ltGroupId[i] = "";
                                _ltTenAdjData[i].Clear();
                            }

                            if (_ltTenAdj[i].StsLeftAdj)
                            {
                                float k = _typBll.GetAverKVal(_ltTi[i], 0);

                                if (k > 0)
                                {
                                    _ltDevPar[i][deviceId].LeftK = k;

                                    LogHelper.WriteFile(string.Format("查询出来左边k的平均值:{0}", _ltDevPar[i][deviceId].LeftK));

                                    _devParBll.UpdateLeftDevPara(_ltDevPar[i][deviceId].LineName, _ltDevPar[i][deviceId].DeviceName, _ltDevPar[i][deviceId].LeftK);
                                }

                            }

                            if (_ltTenAdj[i].StsRightAdj)
                            {
                                float k = _typBll.GetAverKVal(_ltTi[i], 1);

                                if (k > 0)
                                {
                                    _ltDevPar[i][deviceId].RightK = k;

                                    LogHelper.WriteFile(string.Format("查询出来右边k的平均值:{0}", _ltDevPar[i][deviceId].RightK));

                                    _devParBll.UpdateRightDevPara(_ltDevPar[i][deviceId].LineName, _ltDevPar[i][deviceId].DeviceName, _ltDevPar[i][deviceId].RightK);
                                }

                            }

                            _ltDValue[i] = 0.0f;
                            _ltValPre[i] = 0.0f;
                            _ltActTen[i].Clear();
                            _ltAvrTen[i].Clear();

                            LogHelper.WriteFile(string.Format("张力计:{0},设备:{1},左/右:{2},张力写数据库完成", _ltTenAdj[i].NameTen, _ltTenAdj[i].NameDev, lr));

                            _ltTenAdj[i].StepAdjust = 40;                                           //启动结束工作
                            LogHelper.WriteFile(string.Format("StepAdjust:{0}", _ltTenAdj[i].StepAdjust));
                            _ltStepAdjust[i] += 50;
                            break;
                        case 600:
                            if (_ltTenAdj[i].StepAdjust == 50)
                            {
                                if(!_ltTcp[i].Name.Contains("Auto"))
                                {
                                    _ltTenAdj[i].StepAdjust = 0;
                                    _ltStepAdjust[i] = 0;
                                }
                                else
                                {
                                    _ltMotorRun[i] = true;
                                    _ltStepAdjust[i] += 50;
                                }
                            }
                            break;
                        case 650:
                            if (_ltMotorRunErr[i])
                            {
                                LogHelper.WriteFile("自动张力计电机停止错误");
                                _ltStepAdjust[i] = 1000;
                            }
                            else if (!_ltMotorRun[i])
                            {
                                _ltTenAdj[i].StepAdjust = 0;
                                _ltStepAdjust[i] = 0;
                            }
                            break;


                        ///报警，流程停止
                        case 1000:
                            break;

                        case 1100:
                            //      LogHelper.WriteFile("验证中");
                            break;

                        //异常终止调整流程
                        case 2000:
                            if (_ltTenAdj[i].StepAdjust == 100)
                            {
                                if (_ltTenAdjData[i].Count > 0 && _ltGroupId[i] != "")
                                {
                                    WriteAdjDataToCsv(_ltTi[i], _ltTenAdjData[i], _ltGroupId[i]);
                                    _typBll.AddListCurrentRocord(_ltTi[i], _ltTenAdjData[i], _ltGroupId[i]);

                                    _ltGroupId[i] = "";
                                    _ltTenAdjData[i].Clear();
                                }

                                _ltDValue[i] = 0.0f;
                                _ltValPre[i] = 0.0f;
                                _ltActTen[i].Clear();
                                _ltAvrTen[i].Clear();

                                LogHelper.WriteFile(string.Format("张力计:{0},设备:{1},左/右:{2},异常终止,张力写数据库完成", _ltTenAdj[i].NameTen, _ltTenAdj[i].NameDev, false));



                                _ltTenAdj[i].StepAdjust = 110;
                                LogHelper.WriteFile(string.Format("StepAdjust:{0}", _ltTenAdj[i].StepAdjust));
                                _ltStepAdjust[i] += 50;
                            }

                            if (_ltTenAdj[i].StepAdjust == 150)
                            {
                                if (_ltTenAdjData[i].Count > 0 && _ltGroupId[i] != "")
                                {
                                    WriteAdjDataToCsv(_ltTi[i], _ltTenAdjData[i], _ltGroupId[i]);
                                    _typBll.AddListCurrentRocord(_ltTi[i], _ltTenAdjData[i], _ltGroupId[i]);

                                    _ltGroupId[i] = "";
                                    _ltTenAdjData[i].Clear();
                                }

                                _ltDValue[i] = 0.0f;
                                _ltValPre[i] = 0.0f;
                                _ltActTen[i].Clear();
                                _ltAvrTen[i].Clear();

                                LogHelper.WriteFile(string.Format("张力计:{0},设备:{1},左/右:{2},异常终止,张力写数据库完成", _ltTenAdj[i].NameTen, _ltTenAdj[i].NameDev, true));

                                _ltTenAdj[i].StepAdjust = 160;
                                LogHelper.WriteFile(string.Format("StepAdjust:{0}", _ltTenAdj[i].StepAdjust));
                                _ltStepAdjust[i] += 50;
                            }
                            break;
                        case 2050:
                            if (_ltTenAdj[i].ValTempCur > 0)                                         //如果未调整完成，并且最开始的电流是计算出来的，则会把之前的电流写回去
                            {
                                _ltTenAdj[i].ValWriteCur = _ltTenAdj[i].ValTempCur;

                                _ltTenAdj[i].WriteCur = true;
                                LogHelper.WriteFile(string.Format("张力计:{0},设备:{1},左/右:{2},异常终止,启动写初始电流", _ltTenAdj[i].NameTen, _ltTenAdj[i].NameDev, _ltTenAdj[i].StepAdjust == 110 ? false : true));
                            }

                            _ltStepAdjust[i] += 50;

                            break;
                        case 2100:
                            if (_ltTenAdj[i].ValTempCur > 0)
                            {
                                if (!_ltTenAdj[i].WriteCur)
                                {
                                    LogHelper.WriteFile(string.Format("张力计:{0},设备:{1},左/右:{2},异常终止,写初始电流值:{3}完成", _ltTenAdj[i].NameTen, _ltTenAdj[i].NameDev, _ltTenAdj[i].StepAdjust == 110 ? false : true, _ltTenAdj[i].ValTempCur));


                                    _ltTenAdj[i].StepAdjust = 120;                                                      //启动结束工作
                                    LogHelper.WriteFile(string.Format("StepAdjust:{0}", _ltTenAdj[i].StepAdjust));
                                    _ltStepAdjust[i] += 50;
                                }
                            }
                            else
                            {
                                _ltTenAdj[i].StepAdjust = 120;                                                     //启动结束工作
                                LogHelper.WriteFile(string.Format("StepAdjust:{0}", _ltTenAdj[i].StepAdjust));
                                _ltStepAdjust[i] += 50;
                            }
                            break;
                        case 2150:
                            if (_ltTenAdj[i].StepAdjust == 130)
                            {
                                _ltTenAdj[i].StepAdjust = 0;
                                _ltStepAdjust[i] = 0;
                                LogHelper.WriteFile("异常终止结束");
                            }
                            break;

                        //终止验证流程
                        case 3000:
                            if (_ltTenAdj[i].StepAdjust == 200)
                            {
                                if (_ltTenAdjData[i].Count > 0 && _ltGroupId[i] != "")
                                {
                                    WriteChkDataToCsv(_ltTi[i], _ltTenAdjData[i], _ltGroupId[i]);

                                    _ltGroupId[i] = "";
                                    _ltTenAdjData[i].Clear();
                                }

                                _ltDValue[i] = 0.0f;
                                _ltValPre[i] = 0.0f;
                                _ltActTen[i].Clear();
                                _ltAvrTen[i].Clear();

                                LogHelper.WriteFile(string.Format("张力计:{0},设备:{1},左/右:{2},验证终止,张力写文件完成", _ltTenAdj[i].NameTen, _ltTenAdj[i].NameDev, lr));

                                _ltTenAdj[i].StepAdjust = 210;                                           //启动结束验证工作
                                LogHelper.WriteFile(string.Format("StepAdjust:{0}", _ltTenAdj[i].StepAdjust));
                                _ltStepAdjust[i] += 50;
                            }
                            break;
                        case 3050:
                            if (_ltTenAdj[i].StepAdjust == 220)
                            {
                                _ltTenAdj[i].StepAdjust = 0;
                                _ltStepAdjust[i] = 0;
                            }
                            break;
                        //自动计算k和a
                        case 4000:
                            if (_ltTenAdj[i].StepAdjust == 30)
                            {
                                if (_ltStable[i])
                                {
                                    if (_ltValPre[i] == 0.0)
                                    {
                                        _ltValPre[i] = _ltTenAdj[i].ValAverTen;
                                        _ltValCurPre[i] = _ltTenAdj[i].ValReadCur;

                                        LogHelper.WriteFile(string.Format("计算k和a,张力第一次稳定张力:{0},电流:{1}", _ltValPre[i], _ltValCurPre[i]));
                                        _ltStable[i] = false;
                                        _ltStepAdjust[i] += 50;
                                    }
                                    else
                                    {
                                        if (_ltTenAdj[i].StsLeftAdj)
                                        {
                                            float k1 = _ltValCurPre[i] / _ltValPre[i];
                                            float k2 = _ltTenAdj[i].ValReadCur / _ltTenAdj[i].ValAverTen;
                                            float k = (k1 + k2) / 2;

                                            _ltDevPar[i][deviceId].LeftK = k;



                                            LogHelper.WriteFile(string.Format("根据第二次稳定张力及电流计算出k:{0}", _ltDevPar[i][deviceId].LeftK));

                                            _devParBll.UpdateLeftDevPara(_ltDevPar[i][deviceId].LineName, _ltDevPar[i][deviceId].DeviceName, _ltDevPar[i][deviceId].LeftK);

                                            LogHelper.WriteFile(string.Format("设备:{0},左边参数更新数据库完成", _ltDevPar[i][deviceId].DeviceName));
                                        }

                                        if (_ltTenAdj[i].StsRightAdj)
                                        {
                                            float k1 = _ltValCurPre[i] / _ltValPre[i];
                                            float k2 = _ltTenAdj[i].ValReadCur / _ltTenAdj[i].ValAverTen;
                                            float k = (k1 + k2) / 2;

                                            float a1 = k1 / k;
                                            float a2 = k2 / k;
                                            float a = (a1 + a2) / 2;

                                            _ltDevPar[i][deviceId].RightK = k;

                                            _ltDevPar[i][deviceId].RightA = a;

                                            LogHelper.WriteFile(string.Format("根据第二次稳定张力及电流计算出k:{0}", _ltDevPar[i][deviceId].RightK));

                                            _devParBll.UpdateRightDevPara(_ltDevPar[i][deviceId].LineName, _ltDevPar[i][deviceId].DeviceName, _ltDevPar[i][deviceId].RightK);

                                            LogHelper.WriteFile(string.Format("设备:{0},右边参数更新数据库完成", _ltDevPar[i][deviceId].DeviceName));
                                        }

                                        _ltValPre[i] = 0;
                                        _ltStable[i] = false;
                                        _ltStepAdjust[i] = 250;                                             //跳转到调整流程
                                        Thread.Sleep(2000);
                                    }
                                }
                            }
                            break;
                        case 4050:
                            _ltTenAdj[i].ValWriteCur = _ltTenAdj[i].ValReadCur * 1.1f;
                            _ltTenAdj[i].WriteCur = true;
                            LogHelper.WriteFile("计算k和a,电流放大到1.1倍,启动写入");
                            _ltStepAdjust[i] += 50;
                            break;
                        case 4100:
                            if (!_ltTenAdj[i].WriteCur)
                            {
                                LogHelper.WriteFile("计算k和a,电流放大到1.1倍,启动写入完成");
                                _ltActTen[i].Clear();
                                _ltAvrTen[i].Clear();
                                _ltActLineLen[i].Clear();
                                Thread.Sleep(2000);                                                     //保证新写入的电流起作用
                                _ltStepAdjust[i] = 4000;
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteFile(ex.ToString());
                }
            }
        }

        private string GetMillis()
        {
            long currentTicks = DateTime.Now.Ticks;
            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long currentMillis = (currentTicks - dtFrom.Ticks) / 100;

            return currentMillis.ToString();
        }


        private void WriteAdjDataToCsv(TypeInfo Ti, List<TensionAdjInfo> ltTai, string GroupId)
        {
            int i = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append("DeviceName,TensionName, ModelId,TypeNo,VersionNo,Tension,Current,LeftRight,CreateTime,result\r\n");
            foreach (TensionAdjInfo tai in ltTai)
            {
                string tension, current, result, lr;

                tension = tai.ValActTen.ToString("0.00");
                current = tai.ValReadCur.ToString();

                if (tai.ValResultTen > 0.0)
                    result = "1";
                else
                    result = "0";

                if (tai.StsLeftAdj)
                    lr = "0";
                else
                    lr = "1";

                sb.Append(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}\r\n", tai.NameDev, tai.NameTen, Ti.ModelId, Ti.TypeNo, Ti.CopperWireNo, tension, current, lr, tai.TimeCreate, result));
                i++;
            }

            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());

            FileStream fs = new FileStream(@"\Adjust\" + GroupId + ".csv", FileMode.CreateNew);
            fs.Write(buffer, 0, buffer.Length);
            fs.Close();
        }

        private void WriteChkDataToCsv(TypeInfo Ti, List<TensionAdjInfo> ltTai, string GroupId)
        {
            int i = 0;
            StringBuilder sb = new StringBuilder();
            sb.Append("DeviceName,TensionName, ModelId,TypeNo,VersionNo,Tension,Current,LeftRight,CreateTime\r\n");
            foreach (TensionAdjInfo tai in ltTai)
            {
                string tension, current, lr;

                tension = tai.ValActTen.ToString("0.00");
                current = tai.ValReadCur.ToString();



                if (tai.StsLeftChk)
                    lr = "0";
                else
                    lr = "1";

                sb.Append(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}\r\n", tai.NameDev, tai.NameTen, Ti.ModelId, Ti.TypeNo, Ti.CopperWireNo, tension, current, lr, tai.TimeCreate));
                i++;
            }

            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());

            FileStream fs = new FileStream(@"\Check\" + GroupId + ".csv", FileMode.CreateNew);
            fs.Write(buffer, 0, buffer.Length);
            fs.Close();
        }


        #endregion

        #region UpdateData
        private void UpdateData()
        {
            while(true)
            {
                if (_bAbort)
                    break;
                Thread.Sleep(100);

                AddNewLogFile();

                

                try
                {
                    Invoke(new Action(() =>
                    {
                        gridTenAdj.DataSource = _ltTenAdj;
                        gridTenAdj.RefreshDataSource();

                        gridRiTenAdj.DataSource = _ltTenAdjRest;
                        gridRiTenAdj.RefreshDataSource();

                    }));
                }
                catch
                {

                }
            }
        }
        #endregion
    }
}
 