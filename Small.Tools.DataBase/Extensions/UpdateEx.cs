using Dapper;
using Dommel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using static Dommel.DommelMapper;

namespace Small.Tools.DataBase.Extensions
{
    /// <summary>
    /// Update Extensions
    /// </summary>
    public static partial class DapperExtensions
    {
        /// <summary>
        /// 存储生成“Update SQL”
        /// </summary>
        private static readonly ConcurrentDictionary<Type, string> updateQueryCache = new ConcurrentDictionary<Type, string>();

        /// <summary>
        /// Update
        /// 根据传入实体修改。
        /// </summary>
        /// <typeparam name="TEntity">类型参数</typeparam>
        /// <param name="connection">DbConnection</param>
        /// <param name="entity">实体数据信息</param>
        /// <param name="transaction">事务</param>
        /// <returns>bool</returns>
        public static bool Update<TEntity>(this IDbConnection connection, TEntity entity, IDbTransaction transaction = null)
        {
            try
            {
                return DommelMapper.Update<TEntity>(connection, entity, transaction);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        /// <summary>
        /// Update Batch
        /// 根据传入实体集合批量修改。</summary>
        /// <typeparam name="TEntity">类型参数</typeparam>
        /// <param name="connection">DbConnection</param>
        /// <param name="entitys">实体数据集合</param>
        /// <param name="transaction">事务</param>
        /// <returns>bool</returns>
        public static bool UpdateBatch<TEntity>(this IDbConnection connection, IList<TEntity> entitys, IDbTransaction transaction = null)
        {
            try
            {
                string updateSQL = BuildUpdateQuery(typeof(TEntity));
                return SqlMapper.Execute(connection, updateSQL, entitys, transaction) > 0;
            }
            catch (Exception ex) { throw new Exception(ex.Message); }

        }

        #region > private methods

        /// <summary>
        /// 生成“Update SQL”
        /// </summary>
        /// <param name="type">entity type</param>
        /// <returns>string</returns>
        private static string BuildUpdateQuery(Type type)
        {
            string updateSQLSentence = "";
            if (!updateQueryCache.TryGetValue(type, out updateSQLSentence))
            {
                string tableName = Resolvers.Table(type);
                PropertyInfo keyProperty = Resolvers.KeyProperty(type);
                string[] strArray = (from p in (from p in Resolvers.Properties(type)
                                                where p != keyProperty
                                                where p.GetSetMethod() != null
                                                select p).ToArray<PropertyInfo>()
                                     select Resolvers.Column(p) + " = @" + p.Name).ToArray<string>();
                string[] updateSQLArray = new string[] { "update ", tableName, " set ", string.Join(", ", strArray), " where ", Resolvers.Column(keyProperty), " = @", keyProperty.Name };
                updateSQLSentence = string.Concat(updateSQLArray);
                updateQueryCache.TryAdd(type, updateSQLSentence);
            }
            return updateSQLSentence;
        }

        #endregion
    }
}
