using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace Tension.ManualAdjust
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool ret = false;
            Mutex m = new Mutex(true, "Adjust", out ret);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if(ret)
                Application.Run(new frmManualAdj());
        }
    }
}
