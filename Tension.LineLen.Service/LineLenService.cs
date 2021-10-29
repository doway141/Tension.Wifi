using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Tension.LineLen.Service
{
    public partial class LineLenService : ServiceBase
    {
        private LineLength _lineLength = new LineLength();
        public LineLenService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _lineLength.LineLength_Load();
        }

        protected override void OnStop()
        {
            _lineLength.ExitApp();
        }
    }
}
