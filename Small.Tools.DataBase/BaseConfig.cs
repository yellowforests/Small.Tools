using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Small.Tools.DataBase
{
    /// <summary>
    /// 初始化 “IDbConnection”
    /// </summary>
    public static class BaseConfig
    {
        /// <summary>
        /// 数据库链接字符串
        /// </summary>
        private static string DefaultSqlConnectionString = @"";

        /// <summary>
        /// 静态构造函数 
        /// </summary>
        static BaseConfig()
        {
            if (string.IsNullOrWhiteSpace(DefaultSqlConnectionString))
                DefaultSqlConnectionString = ConfigurationManager.ConnectionStrings["SqlConnectionString"].ToString();
        }

        /// <summary>
        /// 创建“IDbConnection”对象
        /// </summary>
        /// <param name="sqlConnectionString">数据库链接字符串,可空</param>
        /// <returns>IDbConnection</returns>
        public static IDbConnection GetSqlConnection(string sqlConnectionString = null)
        {
            if (string.IsNullOrWhiteSpace(sqlConnectionString))
            {
                sqlConnectionString = DefaultSqlConnectionString;
            }
            IDbConnection conn = new SqlConnection(sqlConnectionString);
            conn.Open();
            return conn;
        }
    }
}
