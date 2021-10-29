using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Tension.StandardAdjust.Service
{
    public partial class StandardAdjustService : ServiceBase
    {
        private StandardAdjust _standardAdjust = new StandardAdjust();
        public StandardAdjustService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _standardAdjust.StandardAdjust_Load();
        }

        protected override void OnStop()
        {
            _standardAdjust.ExitApp();
        }
    }
}
