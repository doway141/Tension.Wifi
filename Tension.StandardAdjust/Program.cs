using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.UserSkins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Tension.StandardAdjust
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            bool ret = false;
            Mutex m = new Mutex(true, "Adjust", out ret);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (ret)
                Application.Run(new frmStandardAdjust());
        }
    }
}
