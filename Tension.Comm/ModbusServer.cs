using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HPSocket;
using HPSocket.Tcp;
using Tension.Model;
using System.IO;

namespace Tension.Comm
{
    public class ModbusServer
    {
        
        public bool bConnected { get; set; }

        private readonly AutoResetEvent _eventDisconnected = new AutoResetEvent(false);
        private readonly AutoResetEvent _eventRecv = new AutoResetEvent(false);
        private IntPtr _connId = IntPtr.Zero;
        private List<IntPtr> _ltConnId = new List<IntPtr>();
        private TcpServer _server = new TcpServer();

        private readonly byte[] _readBuff = new byte[1024];

        private ModbusInfo mdi = new ModbusInfo();

        private StreamWriter _sw = null;

        /// <summary>
        ///     初始化TCP通信服务，主要为设置回调函数、包协议等
        /// </summary>
        public bool InitTcpServer(string ip, ushort port)
        {
            _server.Address = ip;
            _server.Port = port;
            //设置回调函数
            _server.OnAccept += OnAccept;
            _server.OnReceive += OnReceive;
            _server.OnClose += OnClose;
            

            if (_sw != null)
                _sw = null;
            _sw = new StreamWriter(string.Format(@"D:\Log\{0}.txt", DateTime.Now.ToString("yyyyMMddHHmmssfff")));
            return _server.Start();
        }

        public bool Connected()
        {
            if (_connId != IntPtr.Zero)
                return true;
            else
                return false;
        }

    

        private HandleResult OnClose(IServer sender, IntPtr connId, SocketOperation socketOperation, int errorCode)
        {
            int idx = _ltConnId.FindIndex(c => c == connId);
            if (idx >= 0)
                _ltConnId.RemoveAt(idx);

            if(_ltConnId.Count == 0)
                _connId = IntPtr.Zero;

            string ip = "";
            ushort port = 0;
            _server.GetRemoteAddress(connId, out ip, out port);
            LogHelper.WriteFile(string.Format("张力计:{0}与张力系统断开,connId:{1}", ip,connId));
            
            return HandleResult.Ok;
        }

        private HandleResult OnReceive(IServer sender, IntPtr connId, byte[] data)
        {
            if(data.Length > 0)
            {
                string strRecv = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "---" + "Recv: ";
                foreach (var item in data)
                {
                    strRecv += item.ToString() + " ";
                }
                _sw.WriteLine(strRecv);
                Array.Copy(data, _readBuff, data.Length);

                Array.Clear(data, 0, data.Length);
                _eventRecv.Set();
            }
            
            return HandleResult.Ok;
        }

        private HandleResult OnAccept(IServer sender, IntPtr connId, IntPtr client)
        {
            _ltConnId.Add(connId);
            _connId = connId;
            string ip = "";
            ushort port = 0;
            _server.GetRemoteAddress(connId, out ip, out port);
            LogHelper.WriteFile(string.Format("张力计:{0}连接到张力系统,connId:{1}", ip,connId));
            return HandleResult.Ok;
        }

        public void CloseServer()
        {
            if (_server != null)
            {
                if (_server.State != ServiceState.Stopped)
                    _server.Stop();
                _server.Dispose();
            }
        }




        public bool ReadData(ref ModbusInfo mdi)
        {
            bool ret = false;
            List<byte> ltRet = new List<byte>();
            byte[] dataSend = GetCmd(mdi);

            lock(this)
            {
                Array.Clear(_readBuff, 0, _readBuff.Length);                        //清除_readBuffer上一次返回值
                _server.Send(_connId, dataSend, dataSend.Length);

                string strSend = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "---" + "Send: ";
                foreach (var item in dataSend)
                {
                    strSend += item.ToString("X2") + " ";
                }
                LogHelper.WriteFile(strSend);
             //   _sw.WriteLine(strSend);

                _eventRecv.Reset();                                                     //复位上一次超时过后收到的信号

                if (_eventRecv.WaitOne(1000))
                {
                    if (_readBuff[0] == mdi.AddrStation)
                    {

                        if (_readBuff[1] == (byte)mdi.FunCode)
                        {
                            int len = _readBuff[2];
                            byte[] buffer = new byte[len];
                            Array.ConstrainedCopy(_readBuff, 3, buffer, 0, len);        //从readbuff中第3位开始取readbuff[2]长度到buffer数组中  
                                                                                        //                    float f = BitConverter.ToSingle(buffer, 0);
                            ResolutionData(ref mdi, buffer);

                            ret = true;
                        }
                        else
                            _sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "---" + "功能码错误" + _readBuff[1].ToString());
                    }
                    else
                        _sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff") + "---" + "站号错误:" + _readBuff[0].ToString());
                }
                return ret;
            }
            
        }
        

        public bool WriteData(ModbusInfo mdi)
        {
            bool ret = false;
            byte[] dataSend = GetCmd(mdi);
            string strSend = "Send: ";
            foreach (var item in dataSend)
            {
                strSend += item.ToString("X2") + " ";
            }
            LogHelper.WriteFile(strSend);
            _server.Send(_connId,dataSend, dataSend.Length);
            _eventRecv.Reset();                                                     //复位上一次超时过后收到的信号

            
            if (_eventRecv.WaitOne(500))
            {
                if (_readBuff[0] == mdi.AddrStation)
                {
                    if (_readBuff[1] == (byte)mdi.FunCode)
                    {
                        ret = true;         
                    }
                }
            }
            return ret;
        }

        private void ResolutionData(ref ModbusInfo mdi, byte[] ResultData)
        {
            int len = ResultData.Length;
            int cntData = 0;
            byte[] data = null, data1 = null, data2 = null;
            switch (mdi.TypData)
            {
                case TypeData.byteData:
                    data = new byte[1];
                    cntData = len / 1;
                    mdi.Data = new object[cntData];
                    for (int i = 0; i < cntData; i++)
                    {
                        Array.ConstrainedCopy(ResultData, 1 * i, data, 0, 1);
                        mdi.Data[i] = data[0];
                    }
                    break;
                case TypeData.ushortData:
                    data = new byte[2];
                    cntData = len / 2;
                    mdi.Data = new object[cntData];
                    for (int i = 0; i < cntData; i++)
                    {
                        Array.ConstrainedCopy(ResultData, 2 * i, data, 0, 2);
                        Array.Reverse(data);
                        mdi.Data[i] = BitConverter.ToUInt16(data, 0);
                    }
                    break;
                case TypeData.shortData:
                    data = new byte[2];
                    cntData = len / 2;
                    mdi.Data = new object[cntData];
                    for (int i = 0; i < cntData; i++)
                    {
                        Array.ConstrainedCopy(ResultData, 2 * i, data, 0, 2);
                        Array.Reverse(data);
                        mdi.Data[i] = BitConverter.ToInt16(data, 0);
                    }
                    break;
                case TypeData.uintData:
                    cntData = len / 4;
                    mdi.Data = new object[cntData];
                    data1 = new byte[2];
                    data2 = new byte[2];
                    data = new byte[4];
                    for (int i = 0; i < cntData; i++)
                    {
                        Array.ConstrainedCopy(ResultData, 4 * i, data1, 0, 2);
                        Array.Reverse(data1);
                        Array.ConstrainedCopy(ResultData, 4 * i + 2, data2, 0, 2);
                        Array.Reverse(data2);
                        data = data1.Concat(data2).ToArray();
                        mdi.Data[i] = BitConverter.ToUInt32(data, 0);
                    }
                    break;
                case TypeData.intData:
                    cntData = len / 4;
                    mdi.Data = new object[cntData];
                    data1 = new byte[2];
                    data2 = new byte[2];
                    data = new byte[4];
                    for (int i = 0; i < cntData; i++)
                    {
                        Array.ConstrainedCopy(ResultData, 4 * i, data1, 0, 2);
                        Array.Reverse(data1);
                        Array.ConstrainedCopy(ResultData, 4 * i + 2, data2, 0, 2);
                        Array.Reverse(data2);
                        data = data1.Concat(data2).ToArray();
                        mdi.Data[i] = BitConverter.ToInt32(data, 0);
                    }
                    break;
                case TypeData.floatData:
                    cntData = len / 4;
                    mdi.Data = new object[cntData];
                    data1 = new byte[2];
                    data2 = new byte[2];
                    data = new byte[4];
                    for (int i = 0; i < cntData; i++)
                    {
                        Array.ConstrainedCopy(ResultData, 4 * i, data1, 0, 2);
                        Array.Reverse(data1);
                        Array.ConstrainedCopy(ResultData, 4 * i + 2, data2, 0, 2);
                        Array.Reverse(data2);
                        //        data = data1.Concat(data2).ToArray();
                        data = data2.Concat(data1).ToArray();
                        mdi.Data[i] = BitConverter.ToSingle(data, 0);
                    }
                    break;
            }
        }

        private List<byte> ToListByte(object[] ArrayData,TypeData Typ = TypeData.byteData)
        {
            List<byte> ltData = new List<byte>();
            foreach (object item in ArrayData)
            {
                byte[] data = null;
                switch (Typ)
                {
                    case TypeData.byteData:
                        data = BitConverter.GetBytes((byte)item);
                        break;
                    case TypeData.ushortData:
                        data = BitConverter.GetBytes(Convert.ToUInt16(item));
                        Array.Reverse(data);
                        break;
                    case TypeData.shortData:
                        data = BitConverter.GetBytes(Convert.ToInt16(item));
                        Array.Reverse(data);
                        break;
                    case TypeData.uintData:
                        data = BitConverter.GetBytes(Convert.ToUInt32(item));
                        break;
                    case TypeData.intData:
                        data = BitConverter.GetBytes(Convert.ToInt32(item));
                        break;
                    case TypeData.floatData:
                        data = BitConverter.GetBytes(Convert.ToSingle(item));
                        break;
                    case TypeData.doubleData:
                        data = BitConverter.GetBytes(Convert.ToDouble(item));
                        break;
                }
                
                
                ltData.AddRange(data);
            }
            return ltData;
        }

        private byte[] GetCmd(ModbusInfo mdi)
        {
            List<byte> ltRet = new List<byte>();

            ltRet.Add(mdi.AddrStation);
            ltRet.Add((byte)mdi.FunCode);
            byte[] addrData = BitConverter.GetBytes(mdi.AddrData);
            Array.Reverse(addrData);
            ltRet.AddRange(addrData);
            
            //写单个地址是不需要寄存器长度的
            if(mdi.FunCode != FuncCode.WriteData)
            {
                byte[] cntData = BitConverter.GetBytes(mdi.CntData);
                Array.Reverse(cntData);
                ltRet.AddRange(cntData);
            }

            byte cntByte = 0;
            switch (mdi.TypData)
            {
                case TypeData.byteData:
                    cntByte = (byte)mdi.CntData;
                    break;
                case TypeData.ushortData:
                    cntByte = (byte)(mdi.CntData * 2);
                    break;
                case TypeData.shortData:
                    cntByte = (byte)(mdi.CntData * 2);
                    break;
                case TypeData.uintData:
                    cntByte = (byte)(mdi.CntData * 4);
                    break;
                case TypeData.intData:
                    cntByte = (byte)(mdi.CntData * 4);
                    break;
                case TypeData.floatData:
                    cntByte = (byte)(mdi.CntData * 4);
                    break;
                case TypeData.doubleData:
                    cntByte = (byte)(mdi.CntData * 8);
                    break;

            }

            if(mdi.FunCode == FuncCode.WriteMultipleData)
                ltRet.Add(cntByte);

            List<byte> ltData = new List<byte>();

            if(mdi.Data != null)
            {                
                if (mdi.FunCode == FuncCode.WriteData || mdi.FunCode == FuncCode.WriteMultipleData)
                {
                    ltData = ToListByte(mdi.Data, mdi.TypData);
                    ltRet.AddRange(ltData);
                }
            }
            

            ltRet.AddRange(CalCrc16(ltRet.ToArray()));

            return ltRet.ToArray();
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
    }
}
