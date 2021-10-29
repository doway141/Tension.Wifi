using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tension.Model;
using Tension.DAL;

namespace Tension.BLL
{
    public class DeviceParaBLL
    {
        private DeviceParaDAL _dal = new DeviceParaDAL();


        public List<DevicePara> GetDevParaByLineName(string LineName)
        {
            return _dal.GetDevParaByLineName(LineName);
        }

        public int UpdateLeftDevPara(string LineName, string DeviceName, float k)
        {
            return _dal.UpdateLeftDevPara(LineName, DeviceName, k);
        }

        public int UpdateRightDevPara(string LineName, string DeviceName, float k)
        {
            return _dal.UpdateRightDevPara(LineName, DeviceName, k);
        }
    }
}
