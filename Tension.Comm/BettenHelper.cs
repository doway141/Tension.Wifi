
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HPSocket;
using HPSocket.Tcp;

namespace Tension.Comm
{
    public class BettenHelper
    {
        private bool bConnected { get; set; }
        private bool bKeepAlive { get; set; }

        private IntPtr _connId;
        private bool bRecvResult { get; set; }
        private int _valueTension { set; get; }
        
        private object _objValTen = new object();

        private object _objKeepAlive = new object();

        private readonly byte[] _readBuff = new byte[102400];

        private TcpServer _hpServer = new TcpServer();
        private int _readBuffLen;

        private byte _stx = 0x01;
  //      private byte _ack = 0x06;

        private Dictionary<string, byte> _func = new Dictionary<string, byte>();

        public BettenHelper()
        {
            _func["KeepAlive"] = 0x10;
            _func["QueryVer"] = 0x20;
            _func["SetWifiName"] = 0x30;
            _func["SetWifiPwd"] = 0x31;
            _func["SetWifiIp"] = 0x32;
            _func["SetWifiPort"] = 0x33;
            _func["RecvResult"] = 0x40;
        }

        /// <summary>
        ///     初始化TCP通信服务，主要为设置回调函数、包协议等
        /// </summary>
        public bool InitTcpServer(string ip, ushort port)
        {
            _hpServer.Address = ip;
            _hpServer.Port = port;
            //设置回调函数
            _hpServer.OnAccept += OnAccept;
            _hpServer.OnReceive += OnReceive;

            return _hpServer.Start();
        }

        public void CloseServer()
        {
            if(_hpServer != null)
            {
                if (_hpServer.State != ServiceState.Stopped)
                    _hpServer.Stop();
                _hpServer.Dispose();
            }
        }

        public bool RecvResult()
        {
            return bRecvResult;
        }

        public bool Connected()
        {
            return bConnected;
        }

        public bool KeepAlive()
        {
            bool ret = bKeepAlive;
            lock (_objKeepAlive)
                bKeepAlive = false;
            return ret;
        }

        public int ValueTension()
        {
            int ten = _valueTension;
            lock (_objValTen)
                _valueTension = 0;
            return ten;
        }

        public void StartRecv()
        {
            SetRecvResult(_connId, true);
        }

        public void StopRecv()
        {
            SetRecvResult(_connId, false);
        }


        private byte CalCrc8(byte[] buffer)
        {
            byte crc = 0;
            for (int i = 0; i < buffer.Length; i++)
            {
                crc ^= buffer[i];
                for (int j = 0; j < 8; j++)
                {
                    if((crc & 0x01) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0x8c;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            return crc;
        }

        #region TCPServer



        private HandleResult OnReceive(IServer sender, IntPtr connId, byte[] data)
        {
            _readBuffLen = data.Length;
            if (_readBuffLen > 0)
            {
                Array.Copy(data, _readBuff, _readBuffLen);

                if (_readBuff[0] == 0x01)
                {
                    if (_readBuff[1] == _func["KeepAlive"])
                    {
                        lock(_objKeepAlive)
                            bKeepAlive = true;
                        if (_readBuff[4] == 0x00)
                            bRecvResult = false;
                        else
                            bRecvResult = true;
                    }
                    else if (_readBuff[1] == _func["RecvResult"])
                    {
                        lock(_objValTen)
                            _valueTension = _readBuff[4] * 256 + _readBuff[5];
                        LogHelper.WriteFile("ValTen:" + _valueTension.ToString());
                    }
                    else
                        lock (_objKeepAlive)
                            bKeepAlive = false;
                }
            }
            return HandleResult.Ok;
        }

        private HandleResult OnAccept(IServer sender, IntPtr connId, IntPtr client)
        {
            _connId = connId;
         //   SetRecvResult(connId, true);
            return HandleResult.Ok;
        }

        private void SetRecvResult(IntPtr ConnId, bool bEnable)
        {
            List<byte> ltSend = new List<byte>();
            ltSend.Add(_stx);
            ltSend.Add(_func["RecvResult"]);
            ltSend.Add(0x00);
            ltSend.Add(0x01);

            if (bEnable)
                ltSend.Add(0x01);
            else
                ltSend.Add(0x00);

            byte crc = CalCrc8(ltSend.ToArray());

            ltSend.Add(crc);

            _hpServer.Send(ConnId, ltSend.ToArray(), ltSend.Count);
        }

        #endregion
    }
}
