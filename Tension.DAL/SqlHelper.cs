using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Data.SqlClient;
using System.Data;
using Tension.Comm;

namespace Tension.DAL
{
    public static class SqlHelper
    {
        private static readonly string strCon = ConfigurationManager.ConnectionStrings["strCon"].ConnectionString;

        /// <summary>
        /// 增删改
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static int ExecuteNonQuery(string sql,params MySqlParameter[] ps)
        {
            try
            {
                using (MySqlConnection con = new MySqlConnection(strCon))
                {
                    con.Open();
                    using (MySqlCommand cmd = new MySqlCommand(sql, con))
                    {
                        if (ps != null)
                        {
                            cmd.Parameters.AddRange(ps);
                        }
                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                LogHelper.WriteFile(sql);
                return 0;
            }
        }
        /// <summary>
        /// 查询整个表
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static DataTable ExecuteTable(string sql,params MySqlParameter[] ps)
        {
            DataTable dt = new DataTable();
            try
            {
                
                using (MySqlDataAdapter da = new MySqlDataAdapter(sql, strCon))
                {
                    if (ps != null)
                    {
                        da.SelectCommand.Parameters.AddRange(ps);
                    }
                    da.Fill(dt);
                }
            }
            catch
            {
                LogHelper.WriteFile(sql);
            }
            return dt;
        }

        /// <summary>
        ///查询首行首列
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static object ExecuteScalar(string sql,params MySqlParameter[] ps)
        {
            using (MySqlConnection con = new MySqlConnection(strCon))
            {
                using (MySqlCommand cmd = new MySqlCommand(sql,con))
                {
                    con.Open();
                    if (ps != null)
                        cmd.Parameters.AddRange(ps);
                    return cmd.ExecuteScalar();
                }
            }
        }

        /// <summary>
        /// 根据参数查询
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public static MySqlDataReader ExecuteReader(string sql,params MySqlParameter[] ps)
        {
            MySqlConnection con = new MySqlConnection(strCon);
            using (MySqlCommand cmd = new MySqlCommand(sql, con))
            {          
                if (ps != null)
                    cmd.Parameters.AddRange(ps);
                try
                {
                    con.Open();
                    return cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch (Exception ex)
                {
                    con.Close();
                    con.Dispose();
                    throw ex;
                }

            }
        }

    }
}
