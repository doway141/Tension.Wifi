using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OPCAutomation;
using System.Diagnostics;

namespace Tension.Comm
{
    public class OpcClientHelper
    {
        private OPCServer ser = new OPCServer();
        
        private OPCItems items;
        public bool Connected { set; get; }

        public List<List<OPCItem>> ltGroupItem = new List<List<OPCItem>>();

        public OpcClientHelper()
        {

        }

        public bool IniClient(string ServerName,string ServerIp)
        {
            try
            {
                ser.Connect(ServerName, ServerIp);

                if(ser.ServerState == (int)OPCServerState.OPCRunning)
                {
                    Connected = true;
                    LogHelper.WriteFile("已连接到:" + ser.ServerName);
                }
                else
                {
                    LogHelper.WriteFile("未连接");
                }
            }
            catch(Exception ex)
            {
                LogHelper.WriteFile(ex.ToString());
                return false;
            }
            return true;
        }

        public bool CreateGroup(List<List<string>> lttName)
        {           
            OPCGroups groups;
            OPCGroup group;
            int i = 0, j = 0;

            try
            {                
                foreach (List<string> ltName in lttName)
                {
                    groups = ser.OPCGroups;                
                    groups.DefaultGroupIsActive = true;
                    groups.DefaultGroupDeadband = 0;
                    groups.DefaultGroupUpdateRate = 100;

                    group = groups.Add();
                    group.IsActive = true;
                    group.DeadBand = 0.0f;
                    group.IsSubscribed = true; //是否为订阅
                    group.UpdateRate = 100;


                    ltGroupItem.Add(new List<OPCItem>());

                    items = group.OPCItems;
                    j = 0;
                    foreach (var item in ltName)
                    {
                        ltGroupItem[i].Add(items.AddItem(item, j));
                        j++;
                    }
                    i++;
                }              
            }
            catch (Exception ex)
            {
                LogHelper.WriteFile(ex.ToString());
                LogHelper.WriteFile(string.Format("DeviceName:{0},{1},{2}", lttName[i][j], i, j));
                return false;
            }
            return true;
        }

        public object ReadItem(short GroupId,short ItemId)
        {
            object value = null;
            try
            {
                        value = ltGroupItem[GroupId][ItemId].Value;
         //       ltGroupItem[GroupId][ItemId].Read(1, out value, out qualty, out time);
            }
            catch
            {
                LogHelper.WriteFile(string.Format("GroupId:{0},ItemId:{1},ReadErr", GroupId, ItemId));
     //           LogHelper.WriteFile(ex.ToString());
            }
            return value;
        }

        public bool WriteItem(short GroupId,short ItemId, object value)
        {
            try
            {
                ltGroupItem[GroupId][ItemId].Write(value);
            }
            catch
            {
                LogHelper.WriteFile(string.Format("GroupId:{0},ItemId:{1},WriteErr", GroupId, ItemId));
                return false;
            }
            return true;
        }

        public void Disconnect()
        {
            if (ser != null)
            {
                ser.Disconnect();
                Connected = false;
                foreach (List<OPCItem> item in ltGroupItem)
                {
                    item.Clear();
                }
                ltGroupItem.Clear();
            }
        }
    }
}
