using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tension.Model;

namespace Tension.DAL
{
    public class LineLengthDAL
    {
        private string GetMillis()
        {
            long currentTicks = DateTime.Now.Ticks;
            DateTime dtFrom = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long currentMillis = (currentTicks - dtFrom.Ticks) / 1000;

            return currentMillis.ToString();
        }
        public int AddListLi(List<LineLengthInfo> ltStart, List<LineLengthInfo> ltEnd, int Count, int LeftRight, string GroupId, long DeviceId,string LineName,string ModelName)
        {

            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Insert into tension_winding_data_{0}",LineName.ToLower()));
            sb.Append("(group_id, device_id,model_name,turns_number, is_left_right, start_create_time, ");
            sb.Append("start_sampling, start_actual, start_allow_deviation, start_actual_deviation,start_press, ");
            sb.Append("end_create_time, end_sampling, end_actual, end_allow_deviation, end_actual_deviation,end_press) values");

            List<MySqlParameter> ltP = new List<MySqlParameter>();

            for (int i = 0; i < Count; i++)
            {

                sb.Append(string.Format("(@group_id{0},@device_id{0},@model_name{0}, @turns_number{0}, @is_left_right{0}, @start_create_time{0}, ", i));
                sb.Append(string.Format("@start_sampling{0}, @start_actual{0},@start_allow_deviation{0}, @start_actual_deviation{0},@start_press{0}, ", i));
                if (i < Count - 1)
                    sb.Append(string.Format("@end_create_time{0}, @end_sampling{0}, @end_actual{0}, @end_allow_deviation{0}, @end_actual_deviation{0},@end_press{0}),", i));
                else
                    sb.Append(string.Format("@end_create_time{0}, @end_sampling{0}, @end_actual{0}, @end_allow_deviation{0}, @end_actual_deviation{0},@end_press{0});", i));

                MySqlParameter[] p =
                {
             //       new MySqlParameter(string.Format("@winding_id{0}",i),(debugId + i).ToString()),
                    new MySqlParameter(string.Format("@group_id{0}",i),GroupId),
                    new MySqlParameter(string.Format("@turns_number{0}",i),ltStart[i].TurnNo),
                    new MySqlParameter(string.Format("@device_id{0}",i),DeviceId),
                    new MySqlParameter(string.Format("@model_name{0}",i),ModelName),
                    new MySqlParameter(string.Format("@is_left_right{0}",i),LeftRight.ToString()),
                    new MySqlParameter(string.Format("@start_create_time{0}",i),ltStart[i].DT),
                    new MySqlParameter(string.Format("@start_sampling{0}",i),ltStart[i].Sample),
                    new MySqlParameter(string.Format("@start_actual{0}",i),ltStart[i].Actual),
                    new MySqlParameter(string.Format("@start_allow_deviation{0}",i),ltStart[i].AllowDev),
                    new MySqlParameter(string.Format("@start_actual_deviation{0}",i),ltStart[i].ActualDev),
                    new MySqlParameter(string.Format("@start_press{0}",i),ltStart[i].Press),

                    new MySqlParameter(string.Format("@end_create_time{0}",i),ltEnd[i].DT),
                    new MySqlParameter(string.Format("@end_sampling{0}",i),ltEnd[i].Sample),
                    new MySqlParameter(string.Format("@end_actual{0}",i),ltEnd[i].Actual),
                    new MySqlParameter(string.Format("@end_allow_deviation{0}",i),ltEnd[i].AllowDev),
                    new MySqlParameter(string.Format("@end_actual_deviation{0}",i),ltEnd[i].ActualDev),
                    new MySqlParameter(string.Format("@end_press{0}",i),ltEnd[i].Press),
                };
             
                ltP.AddRange(p.ToList());
            }

            return SqlHelper.ExecuteNonQuery(sb.ToString(), ltP.ToArray());
        }
    }
}
