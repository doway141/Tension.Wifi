using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tension.Model;
using Tension.DAL;

namespace Tension.BLL
{
    public class TypeInfoBLL
    {
        private TypeInfoDAL dal = new TypeInfoDAL();
        public float GetLeftTension(TypeInfo Ti)
        {
            return dal.GetLeftTension(Ti);
        }

        public float GetRightTension(TypeInfo Ti)
        {
            return dal.GetRightTension(Ti);
        }

        public TypeInfo GetTypeInfo(TypeInfo Ti)
        {
            return dal.GetTypeInfo(Ti);
        }

        public float GetCurrent(TypeInfo Ti,int LeftRight)
        {
            return dal.GetCurrent(Ti,LeftRight);
        }

        public long GetModelId(TypeInfo Ti)
        {
            return dal.GetModelId(Ti);
        }

        public int AddListCurrentRocord(TypeInfo Ti,List<TensionAdjInfo> ltTai,string GroupId)
        {
            return dal.AddListCurrentRocord(Ti, ltTai, GroupId);
        }

        public float GetAverKVal(TypeInfo Ti, int LeftRight)
        {
            return dal.GetAverKVal(Ti, LeftRight);
        }
    }
}
