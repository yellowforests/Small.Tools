using Dommel;
using System;
using System.Data;
using Small.Tools.DataBase.Resolver;
using Dapper;

namespace Small.Tools.DataBase.Extensions
{
    /// <summary>
    /// Execute Extensions
    /// </summary>
    public static partial class DapperExtensions
    {
        /// <summary>
        /// 自定义相关解析注入
        /// </summary>
        static DapperExtensions()
        {
            DommelMapper.SetTableNameResolver(new CustomTableNameResolver());   //TableName
            DommelMapper.SetKeyPropertyResolver(new CustomKeyPropertyResolver());   //Key
        }

        /// <summary>
        /// Execute Insert Or Update
        /// 执行插入或者修改。
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="sql">sql sentence</param>
        /// <param name="param">参数</param>
        /// <param name="transaction">事务</param>
        /// <returns>int</returns>
        public static int Execute(this IDbConnection connection, string sql, object param = null, IDbTransaction transaction = null)
        {
            try
            {
                return SqlMapper.Execute(connection, sql, param, transaction);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }

        }

        /// <summary>
        /// ExecuteScalar
        /// 返回结果是查询后的第一行的第一列。
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="sql">sql sentence</param>
        /// <param name="param">参数</param>
        /// <param name="transaction">事务</param>
        /// <returns>object</returns>
        public static object ExecuteScalar(this IDbConnection connection, string sql, object param = null, IDbTransaction transaction = null)
        {
            try
            {
                return SqlMapper.ExecuteScalar(connection, sql, param, transaction);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        /// <summary>
        /// ExecuteScalar
        /// 返回结果是查询后的第一行的第一列。
        /// </summary>
        /// <typeparam name="T">类型参数</typeparam>
        /// <param name="connection">DbConnection</param>
        /// <param name="sql">sql sentence</param>
        /// <param name="param">参数</param>
        /// <param name="transaction">事务</param>
        /// <returns>T </returns>
        public static T ExecuteScalar<T>(this IDbConnection connection, string sql, object param = null, IDbTransaction transaction = null)
        {
            try
            {
                return SqlMapper.ExecuteScalar<T>(connection, sql, param, transaction);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        /// <summary>
        /// ExecuteReader
        /// 执行查询“SQL”语句,返回“DataTable”。
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="sql">sql sentence</param>
        /// <param name="param">参数</param>
        /// <param name="transaction">事务</param>
        /// <returns>DataTable</returns>
        public static IDataReader ExecuteReader(this IDbConnection connection, string sql, object param = null, IDbTransaction transaction = null)
        {
            try
            {
                return SqlMapper.ExecuteReader(connection, sql, param, transaction);//.ToDataTable("Table");
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

    }
}
