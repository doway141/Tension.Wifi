using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Tension.Model;
using Tension.Comm;

namespace Tension.DAL
{
    public class DeviceParaDAL
    {
        private DevicePara RowToDevicePara(DataRow dr)
        {
            DevicePara dp = new DevicePara();

            try
            {
                if (dr["tension_id"] != null)
                    dp.TensionId = Convert.ToInt64(dr["tension_id"]);

                if (dr["plc_type"] != null)
                    dp.PlcType = dr["plc_type"].ToString();

                if (dr["device_name"] != null)
                    dp.DeviceName = dr["device_name"].ToString();

                if (dr["device_id"] != null)
                    dp.DeviceId = Convert.ToInt64(dr["device_id"]);

                if (dr["left_k"] != null)
                    dp.LeftK = Convert.ToSingle(dr["left_k"]);

                if (dr["left_a"] != null)
                    dp.LeftA = Convert.ToSingle(dr["left_a"]);

                if (dr["right_k"] != null)
                    dp.RightK = Convert.ToSingle(dr["right_k"]);

                if (dr["right_a"] != null)
                    dp.RightA = Convert.ToSingle(dr["right_a"]);

                if (dr["wave_range"] != null)
                    dp.WaveRange = Convert.ToSingle(dr["wave_range"]);

                if (dr["val_range"] != null)
                    dp.ValRange = Convert.ToSingle(dr["val_range"]);

                if (dr["line_len_min"] != null)
                    dp.LineLenMin = Convert.ToInt32(dr["line_len_min"]);

                if (dr["is_sample"] != null)
                    dp.IsSample = Convert.ToInt32(dr["is_sample"]);

                if (dr["is_actual"] != null)
                    dp.IsActual = Convert.ToInt32(dr["is_actual"]);

                if (dr["is_actual_dev"] != null)
                    dp.IsAllowDev = Convert.ToInt32(dr["is_actual_dev"]);

                if (dr["is_allow_dev"] != null)
                    dp.IsActualDev = Convert.ToInt32(dr["is_allow_dev"]);

                if (dr["is_float"] != null)
                    dp.IsFloat = Convert.ToInt32(dr["is_float"]);
            }
            catch(Exception ex)
            {
                LogHelper.WriteFile(ex.ToString());
            }

            return dp;
        }

        public List<DevicePara> GetDevParaByLineName(string LineName)
        {
            List<DevicePara> ltDp = new List<DevicePara>();
            string sql = "select * from tension_device_manage " +
                "LEFT JOIN tension_product_line " +
                "ON tension_device_manage.line_id = tension_product_line.line_id " +
                "where line_name=@line_name";
            MySqlParameter p = new MySqlParameter("@line_name", LineName);

            DataTable dt = SqlHelper.ExecuteTable(sql, p);

            if(dt != null)
            {
                if(dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        ltDp.Add(RowToDevicePara(dr));
                    }
                }
            }
            return ltDp;
        }


        /*
         UPDATE tension_device_manage set a=20,k=5 where tension_device_manage.device_name='WSM3FL-5#' and tension_device_manage.line_id = (select line_id from tension_product_line where tension_product_line.line_name = 'WSD4-A1')
         */
        public int UpdateLeftDevPara(string LineName,string DeviceName,float k)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("update tension_device_manage set left_k=@left_k ");
            sb.Append(string.Format("where tension_device_manage.device_name = @device_name "));
            sb.Append("and tension_device_manage.line_id = (select line_id from tension_product_line where tension_product_line.line_name = @line_name)");

            MySqlParameter[] p =
            {
                new MySqlParameter("@left_k",k),

                new MySqlParameter("@device_name",DeviceName),
                new MySqlParameter("@line_name",LineName)
            };

            return SqlHelper.ExecuteNonQuery(sb.ToString(), p);
        }

        public int UpdateRightDevPara(string LineName, string DeviceName, float k)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("update tension_device_manage set right_k=@right_k ");
            sb.Append(string.Format("where tension_device_manage.device_name = @device_name "));
            sb.Append("and tension_device_manage.line_id = (select line_id from tension_product_line where tension_product_line.line_name = @line_name)");

            MySqlParameter[] p =
            {
                new MySqlParameter("@right_k",k),

                new MySqlParameter("@device_name",DeviceName),
                new MySqlParameter("@line_name",LineName)
            };

            return SqlHelper.ExecuteNonQuery(sb.ToString(), p);
        }
    }
}
