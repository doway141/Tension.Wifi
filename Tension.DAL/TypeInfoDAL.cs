using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tension.Model;
using Tension.DAL;
using Tension.Comm;

namespace Tension.DAL
{
    public class TypeInfoDAL
    {
        private long GetMillis()
        {
            long currentTicks = DateTime.Now.Ticks;
            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long currentMillis = (currentTicks - dtFrom.Ticks) / 100;

            return currentMillis;
        }

        private TypeInfo RowToTypeInfo(DataRow dr)
        {
            TypeInfo ti = new TypeInfo();

            try
            {
                if (dr["left_tension"] != null)
                    ti.LeftTension = Convert.ToSingle(dr["left_tension"]);

                if (dr["right_tension"] != null)
                    ti.RightTension = Convert.ToSingle(dr["right_tension"]);

                if (dr["model_id"] != null)
                    ti.ModelId = Convert.ToInt64(dr["model_id"]);

                if (dr["max_value"] != null)
                    ti.MaxVal = Convert.ToSingle(dr["max_value"]);
            }
            catch(Exception ex)
            {
                LogHelper.WriteFile(ex.ToString());
            }
            return ti;
        }

        public TypeInfo GetTypeInfo(TypeInfo Ti)
        {
            TypeInfo ti = null;
            try
            {
                string sql = "select * from tension_device_model " +
                           "LEFT JOIN tension_device_manage " +
                           "ON tension_device_model.device_id = tension_device_manage.device_id " +
                            "LEFT JOIN tension_product_line " +
                            "ON tension_device_manage.line_id = tension_product_line.line_id " +
                            "WHERE tension_product_line.line_name = @LineName " +
                            "AND tension_device_manage.device_name = @DeviceName " +
                            "AND tension_device_model.model_name =@TypeNo " +
                            "AND tension_device_model.version_no =@version_no";

                
                MySqlParameter[] p =
                {
                    new MySqlParameter("LineName",Ti.LineName),
                    new MySqlParameter("DeviceName",Ti.DeviceName),
                    new MySqlParameter("TypeNo",Ti.TypeNo),
                    new MySqlParameter("version_no",Ti.CopperWireNo),
                 };

                DataTable dt = SqlHelper.ExecuteTable(sql, p);

                if (dt != null)
                {
                    if (dt.Rows.Count > 0)
                        ti = RowToTypeInfo(dt.Rows[0]);
                }

                if(ti != null)
                {
                    ti.LineName = Ti.LineName;
                    ti.DeviceName = Ti.DeviceName;
                    ti.TypeNo = Ti.TypeNo;
                    ti.CopperWireNo = Ti.CopperWireNo;
                    LogHelper.WriteFile("型号查询OK");
                }
                return ti;
            }
            catch
            {
                return null;
            }
        }

        public float GetLeftTension(TypeInfo Ti)
        {
            string sql = "select left_tension FROM tension_device_model " +
                           "LEFT JOIN tension_device_manage " +
                           "ON tension_device_model.device_id = tension_device_manage.device_id " +
                            "LEFT JOIN tension_product_line " +
                            "ON tension_device_manage.line_id = tension_product_line.line_id " +
                            "WHERE tension_product_line.line_name = @LineName " +
                            "AND tension_device_manage.device_name = @DeviceName " +
                            "AND tension_device_model.model_name =@TypeNo " +
                            "AND tension_device_model.version_no =@version_no";

            MySqlParameter[] p =
            {
                new MySqlParameter("LineName",Ti.LineName),
                new MySqlParameter("DeviceName",Ti.DeviceName),
                new MySqlParameter("TypeNo",Ti.TypeNo),
                new MySqlParameter("version_no",Ti.CopperWireNo),
            };

            DataTable dt = SqlHelper.ExecuteTable(sql, p);

            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                    return Convert.ToSingle(dt.Rows[0][0]);
                else
                    return 0.0f;
            }
            else
                return 0.0f;
        }

        public float GetRightTension(TypeInfo Ti)
        {
            string sql = "select right_tension FROM tension_device_model " +
                           "LEFT JOIN tension_device_manage " +
                           "ON tension_device_model.device_id = tension_device_manage.device_id " +
                            "LEFT JOIN tension_product_line " +
                            "ON tension_device_manage.line_id = tension_product_line.line_id " +
                            "WHERE tension_product_line.line_name = @LineName " +
                            "AND tension_device_manage.device_name = @DeviceName " +
                            "AND tension_device_model.model_name =@TypeNo " +
                            "AND tension_device_model.version_no =@version_no";

            MySqlParameter[] p =
            {
                new MySqlParameter("LineName",Ti.LineName),
                new MySqlParameter("DeviceName",Ti.DeviceName),
                new MySqlParameter("TypeNo",Ti.TypeNo),
                new MySqlParameter("version_no",Ti.CopperWireNo),
            };

            DataTable dt = SqlHelper.ExecuteTable(sql, p);

            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                    return Convert.ToSingle(dt.Rows[0][0]);
                else
                    return 0.0f;
            }
            else
                return 0.0f;
        }

        public float GetCurrent(TypeInfo Ti, int LeftRight)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("SELECT tension_device_debug_record_{0}.current FROM tension_device_debug_record_{0} ",Ti.LineName));
            sb.Append(string.Format("LEFT JOIN tension_device_model ON tension_device_debug_record_{0}.model_id = tension_device_model.model_id ",Ti.LineName));
            sb.Append("LEFT JOIN tension_device_manage ON tension_device_model.device_id = tension_device_manage.device_id ");
            sb.Append("LEFT JOIN tension_product_line ON tension_product_line.line_id = tension_device_manage.line_id ");
            sb.Append("WHERE tension_product_line.line_name = @LineName AND ");
            sb.Append("tension_device_manage.device_name = @DeviceName AND ");
            sb.Append("tension_device_model.model_name = @TypeNo AND ");
            sb.Append(string.Format("tension_device_debug_record_{0}.type = @LeftRight AND ",Ti.LineName));
            sb.Append(string.Format("tension_device_debug_record_{0}.is_result_value=@Result ",Ti.LineName));
            sb.Append(string.Format("ORDER BY tension_device_debug_record_{0}.create_time DESC LIMIT 1",Ti.LineName));

            MySqlParameter[] p =
            {
                new MySqlParameter("@LineName",Ti.LineName),
                new MySqlParameter("@DeviceName",Ti.DeviceName),
                new MySqlParameter("@TypeNo",Ti.TypeNo),
                new MySqlParameter("@LeftRight",LeftRight.ToString()),
                new MySqlParameter("@Result","1"),
            };
            string sql = sb.ToString();

            DataTable dt = SqlHelper.ExecuteTable(sql, p);

            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                    return Convert.ToSingle(dt.Rows[0][0]);
                else
                    return 0;
            }
            else
                return 0;
        }

        public long GetModelId(TypeInfo Ti)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT tension_device_model.model_id FROM tension_device_model ");
            sb.Append("INNER JOIN tension_device_manage ON tension_device_model.device_id = tension_device_manage.device_id ");
            sb.Append("LEFT JOIN tension_product_line ON tension_product_line.line_id = tension_device_manage.line_id ");
            sb.Append("WHERE tension_product_line.line_name = @LineName AND ");
            sb.Append("tension_device_manage.device_name = @DeviceName AND ");
            sb.Append("tension_device_model.model_name = @TypeNo");

            MySqlParameter[] p =
            {
                new MySqlParameter("LineName",Ti.LineName),
                new MySqlParameter("DeviceName",Ti.DeviceName),
                new MySqlParameter("TypeNo",Ti.TypeNo)
            };

            string sql = sb.ToString();

            DataTable dt = SqlHelper.ExecuteTable(sql, p);

            if (dt != null)
            {
                if (dt.Rows.Count > 0)
                    return Convert.ToInt64(dt.Rows[0][0]);
                else
                    return 0;
            }
            else
                return 0;
        }

        public int AddListCurrentRocord(TypeInfo Ti, List<TensionAdjInfo> ltTai, string GroupId)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Insert into tension_device_debug_record_{0}(model_id,model_name,version_no,group_id,tension,current,type,create_time,is_result_value) values", Ti.LineName.ToLower()));

            List<MySqlParameter> ltP = new List<MySqlParameter>();
            int i = 0;
            foreach (TensionAdjInfo tai in ltTai)
            {
                if (i < ltTai.Count - 1)
                    sb.Append(string.Format("(@model_id{0},@model_name{0},@version_no{0},@group_id{0},@tension{0},@current{0},@type{0},@create_time{0},@is_result_value{0}),", i));
                else
                    sb.Append(string.Format("(@model_id{0},@model_name{0},@version_no{0},@group_id{0},@tension{0},@current{0},@type{0},@create_time{0},@is_result_value{0});", i));

                string result, lr;

                if (tai.ValResultTen > 0)
                    result = "1";
                else
                    result = "0";

                if (tai.StsLeftAdj)
                    lr = "0";
                else
                    lr = "1";

                long DebugId = Convert.ToInt64(GroupId) + i;

                MySqlParameter[] p =
                {
            //        new MySqlParameter(string.Format("@debug_id{0}",i),DebugId),
                    new MySqlParameter(string.Format("@model_id{0}",i),Ti.ModelId),
                    new MySqlParameter(string.Format("@model_name{0}",i),Ti.TypeNo),
                    new MySqlParameter(string.Format("@version_no{0}",i),Ti.CopperWireNo),
                    new MySqlParameter(string.Format("@group_id{0}",i),GroupId),

                    new MySqlParameter(string.Format("@tension{0}",i),tai.ValActTen),
                    new MySqlParameter(string.Format("@current{0}",i),tai.ValReadCur),
                    new MySqlParameter(string.Format("@type{0}",i),lr),

                    new MySqlParameter(string.Format("@create_time{0}",i),tai.TimeCreate),
                    new MySqlParameter(string.Format("@is_result_value{0}",i),result),
                };

                ltP.AddRange(p);
                i++;
            }

            return SqlHelper.ExecuteNonQuery(sb.ToString(), ltP.ToArray());
        }


        public float GetAverKVal(TypeInfo Ti, int LeftRight)
        {
            float aver = 0.0f;

            StringBuilder sb = new StringBuilder();

            sb.Append(string.Format("SELECT tension_device_debug_record_{0}.current/tension_device_debug_record_{0}.tension FROM tension_device_debug_record_{0} ", Ti.LineName));
            sb.Append(string.Format("LEFT JOIN tension_device_model ON tension_device_debug_record_{0}.model_id = tension_device_model.model_id ", Ti.LineName));
            sb.Append("LEFT JOIN tension_device_manage ON tension_device_model.device_id = tension_device_manage.device_id ");
            sb.Append("LEFT JOIN tension_product_line ON tension_product_line.line_id = tension_device_manage.line_id ");
            sb.Append("WHERE tension_product_line.line_name = @LineName AND ");
            sb.Append("tension_device_manage.device_name = @DeviceName AND ");
            sb.Append(string.Format("tension_device_debug_record_{0}.type = @LeftRight AND ", Ti.LineName));
            sb.Append(string.Format("tension_device_debug_record_{0}.is_result_value=@Result ", Ti.LineName));


            MySqlParameter[] p =
            {
                new MySqlParameter("@LineName",Ti.LineName),
                new MySqlParameter("@DeviceName",Ti.DeviceName),
                new MySqlParameter("@LeftRight",LeftRight.ToString()),
                new MySqlParameter("@Result","1"),
            };
            string sql = sb.ToString();

            DataTable dt = SqlHelper.ExecuteTable(sql, p);

            if(dt != null)
            {
                if(dt.Rows.Count > 0)
                {
                    List<float> ltRet = new List<float>();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        ltRet.Add(Convert.ToSingle(dt.Rows[i][0]));
                    }

                    ltRet.Sort();
                    if(ltRet.Count > 5)
                    {
                        ltRet.RemoveAt(0);
                        ltRet.RemoveAt(ltRet.Count - 1);
                    }
                    
                    aver = (float)Math.Round(ltRet.Average(), 2);
                }
            }

            return aver;
        }
    }
}