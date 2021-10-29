using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tension.Model;
using Tension.DAL;

namespace Tension.BLL
{
    public class TensionConnectParaBLL
    {
        private TensionConnectParaDAL _dal = new TensionConnectParaDAL();

        public int AddTensionConnectPara(TensionConnectPara tcp)
        {
            return _dal.AddTensionConnectPara(tcp);
        }

        public TensionConnectPara GetTcpByTenId(long TenId)
        {
            return _dal.GetTcpByTenId(TenId);
        }
    }
}
