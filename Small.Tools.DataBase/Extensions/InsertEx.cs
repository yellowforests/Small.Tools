using Dommel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using static Dommel.DommelMapper;
using Dapper;
using Small.Tools.DataBase.Builder;

namespace Small.Tools.DataBase.Extensions
{
    /// <summary>
    /// Insert Extensions
    /// </summary>
    public static partial class DapperExtensions
    {
        /// <summary>
        /// 用于存储生成的“Insert SQL”
        /// </summary>
        private static readonly ConcurrentDictionary<Type, string> insertQueryCache = new ConcurrentDictionary<Type, string>();

        /// <summary>
        /// Insert SQL 
        /// ref @@IDENTITY
        /// </summary>
        /// <typeparam name="TEntity">类型参数</typeparam>
        /// <param name="connection">DbConnection</param>
        /// <param name="entity">entity</param>
        /// <param name="transaction">DbTransaction</param>
        /// <returns>bool</returns>
        public static bool Insert<TEntity>(this IDbConnection connection, TEntity entity, ref object @IDENTITY, IDbTransaction transaction = null) where TEntity : class
        {
            try
            {
                @IDENTITY = DommelMapper.Insert<TEntity>(connection, entity, transaction);
                return true;
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        /// <summary>
        /// Insert SQL 
        /// return bool
        /// </summary>
        /// <typeparam name="TEntity">类型参数</typeparam>
        /// <param name="connection">DbConnection</param>
        /// <param name="entity">entity</param>
        /// <param name="transaction">DbTransaction</param>
        /// <returns>object</returns>
        public static bool Insert<TEntity>(this IDbConnection connection, TEntity entity, IDbTransaction transaction = null) where TEntity : class
        {
            try
            {
                DommelMapper.Insert<TEntity>(connection, entity, transaction);
                return true;
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        /// <summary>
        /// Batch Insert SQL
        /// 批量插入。
        /// </summary>
        /// <typeparam name="TEntity">类型参数</typeparam>
        /// <param name="connection">DbConnection</param>
        /// <param name="entity">entity</param>
        /// <param name="transaction">DbTransaction</param>
        /// <returns>bool</returns>
        public static bool InsertBatch<TEntity>(this IDbConnection connection, IList<TEntity> entitys, IDbTransaction transaction = null) where TEntity : class
        {
            try
            {
                var insertSQL = BuildInsertQuery(connection, typeof(TEntity));
                SqlMapper.Execute(connection, insertSQL, entitys, transaction);
                return true;
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        #region > private  methods

        /// <summary>
        /// 根据“Type”生成Insert SQL
        /// </summary>
        /// <param name="connection">DbConnection</param>
        /// <param name="type">Type</param>
        /// <returns>string</returns>
        private static string BuildInsertQuery(IDbConnection connection, Type type)
        {
            string insertSQlSentence = "";
            if (!insertQueryCache.TryGetValue(type, out insertSQlSentence))
            {
                string tableName = Resolvers.Table(type);
                bool flag = false;
                PropertyInfo keyProperty = Resolvers.KeyProperty(type, out flag);
                List<PropertyInfo> source = new List<PropertyInfo>();
                foreach (PropertyInfo propertyInfo in Resolvers.Properties(type))
                {
                    if (((propertyInfo != keyProperty) || !flag) && (propertyInfo.GetSetMethod() != null))
                    {
                        source.Add(propertyInfo);
                    }
                }
                string[] columnNames = source.Select<PropertyInfo, string>(new Func<PropertyInfo, string>(Resolvers.Column)).ToArray<string>();
                string[] paramNames = (from p in source select "@" + p.Name).ToArray<string>();
                insertSQlSentence = new CustomSqlServerSqlBuilder().BuildInsert(tableName, columnNames, paramNames, keyProperty);
                insertQueryCache.TryAdd(type, insertSQlSentence);
            }
            return insertSQlSentence;
        }

        #endregion
    }
}
