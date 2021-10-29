using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tension.DAL;
using Tension.Model;

namespace Tension.BLL
{
    public class LineLengthBLL
    {
        private LineLengthDAL _dal = new LineLengthDAL();

        public int AddListLi(List<LineLengthInfo> ltStart, List<LineLengthInfo> ltEnd, int Count, int LeftRight, string GroupId, long DeviceId,string LineName,string ModelName)
        {
            return _dal.AddListLi(ltStart, ltEnd, Count, LeftRight, GroupId, DeviceId,LineName,ModelName);
        }
    }
}
