using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tension.Comm
{
    public static class LogHelper
    {
        public static object _obj = new object();
        public static void AddNewLogFile(string Date,string DataPre, string FileName)
        {
            if (!DataPre.Contains(Date))
            {
                lock(_obj)
                {
                    System.Diagnostics.Trace.Listeners.Clear();

                    System.Diagnostics.TextWriterTraceListener tr = new System.Diagnostics.TextWriterTraceListener(FileName);
                    System.Diagnostics.Trace.Listeners.Add(tr);
                    System.Diagnostics.Trace.AutoFlush = true;
                }              
            }
        }

        public static void WriteFile(string log)
        {
            lock(_obj)
                Debug.WriteLine(log +"---"+ GetSysTime());
        }

        /// <summary>
        /// 获取系统时间
        /// </summary>
        /// <returns></returns>
        private static string GetSysTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff");
        }
    }
}
