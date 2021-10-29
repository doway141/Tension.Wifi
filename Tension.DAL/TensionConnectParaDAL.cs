using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using MySql.Data.MySqlClient;
using Tension.Model;
using Tension.Comm;

namespace Tension.DAL
{
    public class TensionConnectParaDAL
    {
        private TensionConnectPara RowToTensionConnectPara(DataRow dr)
        {
            TensionConnectPara tcp = new TensionConnectPara();

            try
            {
                if (dr["tension_id"] != null)
                    tcp.Id = Convert.ToInt64(dr["tension_id"]);

                if (dr["ip"] != null)
                    tcp.IP = dr["ip"].ToString();

                if (dr["port"] != null)
                    tcp.Port = Convert.ToUInt16(dr["port"]);

                if (dr["tension_name"] != null)
                    tcp.Name = dr["tension_name"].ToString();
            }
            catch(Exception ex)
            {
                LogHelper.WriteFile(ex.ToString());
            }
            return tcp;
        }

        public int AddTensionConnectPara(TensionConnectPara tcp)
        {
            string sql = "insert into tension_connect_para(tension_id,ip,port,tension_name) value(@tension_id,@ip,@port,@tension_name)";
            MySqlParameter[] p =
            {
                new MySqlParameter("@tension_id",tcp.Id),
                new MySqlParameter("@ip",tcp.IP),
                new MySqlParameter("@port",tcp.Port),
                new MySqlParameter("@tension_name",tcp.Name)
            };
            return SqlHelper.ExecuteNonQuery(sql, p);
        }

        public TensionConnectPara GetTcpByTenId(long TenId)
        {
            List<TensionConnectPara> ltTcp = new List<TensionConnectPara>();
            StringBuilder sb = new StringBuilder();
            sb.Append("select * from tension_connect_para where tension_id=@tension_id");

            MySqlParameter p = new MySqlParameter(string.Format("@tension_id"), TenId);

            DataTable dt = SqlHelper.ExecuteTable(sb.ToString(),p);

            if(dt != null)
            {
                if(dt.Rows.Count > 0)
                {
                    return RowToTensionConnectPara(dt.Rows[0]);
                }
                return null;
            }
            return null;
        }
    }
}
