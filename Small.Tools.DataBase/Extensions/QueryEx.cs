using Dapper;
using Dommel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using Small.Tools.DataBase.Builder;
using static Dommel.DommelMapper;

namespace Small.Tools.DataBase.Extensions
{
    /// <summary>
    /// Query Extensions
    /// </summary>
    public static partial class DapperExtensions
    {
        /// <summary>
        /// 存储生成的“Query SQL”
        /// </summary>
        private static readonly ConcurrentDictionary<Type, string> getAllQueryCache = new ConcurrentDictionary<Type, string>();

        /// <summary>
        /// Query
        /// 根据主键“ID”
        /// </summary>
        /// <typeparam name="TEntity">类型参数</typeparam>
        /// <param name="connection">DbConnection</param>
        /// <param name="id">主键ID值</param>
        /// <returns>TEntity</returns>
        public static TEntity Query<TEntity>(this IDbConnection connection, object id) where TEntity : class
        {
            try
            {
                return DommelMapper.Get<TEntity>(connection, id);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        /// <summary>
        /// Query
        /// 根据条件查询。
        /// </summary>
        /// <typeparam name="TEntity">类型参数</typeparam>
        /// <param name="connection">DbConnection</param>
        /// <param name="predicate">条件表达式目录树</param>
        /// <returns>TEntity 集合</returns>
        public static IEnumerable<TEntity> Query<TEntity>(this IDbConnection connection, Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                if (SqlCheck.Check<TEntity>(predicate))
                {
                    DynamicParameters parameters;
                    var querySQL = BuildSelectSql(predicate, out parameters);
                    return SqlMapper.Query<TEntity>(connection, querySQL, parameters);
                }
                else
                {
                    return DommelMapper.Select<TEntity>(connection, predicate);
                }
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        /// <summary>
        /// Query ALL
        /// 查询全部。
        /// </summary>
        /// <typeparam name="TEntity">类型参数</typeparam>
        /// <param name="connection">DbConnection</param>
        /// <returns>TEntity 集合</returns>
        public static IEnumerable<TEntity> QueryALL<TEntity>(this IDbConnection connection) where TEntity : class
        {
            try
            {
                return DommelMapper.GetAll<TEntity>(connection);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        /// <summary>
        /// Query FirstOrDefault
        /// 匹配的第一个实体，如果没有匹配的实体，则选择默认值。
        /// </summary>
        /// <typeparam name="TEntity">类型参数</typeparam>
        /// <param name="connection">DbConnection</param>
        /// <param name="predicate">条件表达式目录树</param>
        /// <returns>TEntity 集合</returns>
        public static TEntity FirstOrDefault<TEntity>(this IDbConnection connection, Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                return DommelMapper.FirstOrDefault(connection, predicate);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        /// <summary>
        /// Query Paging
        /// 根据条件分页查询。
        /// </summary>
        /// <typeparam name="TEntity">类型参数</typeparam>
        /// <param name="connection">DbConnection</param>
        /// <param name="predicate">条件表达式目录树</param>
        /// <param name="pageNumber">当前页</param>
        /// <param name="pageSize">每页显示数</param>
        /// <param name="transaction">事务</param>
        /// <returns>TEntity 集合</returns>
        public static IEnumerable<TEntity> QueryPaged<TEntity>(this IDbConnection connection, Expression<Func<TEntity, bool>> predicate, int pageNumber = 1, int pageSize = 10, IDbTransaction transaction = null)
        {
            DynamicParameters parameters;
            var queryPaged = BuildSelectPagedQuery(connection, predicate, pageNumber, pageSize, out parameters);
            return SqlMapper.Query<TEntity>(connection, queryPaged, parameters, transaction);
        }

        /// <summary>
        /// Query Count
        /// 查询条数。
        /// </summary>
        /// <typeparam name="TEntity">类型参数</typeparam>
        /// <param name="connection">DbConnection</param>
        /// <param name="predicate">条件表达式目录树</param>
        /// <returns>long</returns>
        public static long Count<TEntity>(this IDbConnection connection, Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                return DommelMapper.Count<TEntity>(connection, predicate);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        #region > private methods

        /// <summary>
        /// 生成“Select Paged Query SQL”
        /// </summary>
        /// <typeparam name="TEntity">类型参数</typeparam>
        /// <param name="connection">DbConnection</param>
        /// <param name="predicate">条件表达式目录树</param>
        /// <param name="pageNumber">当前页</param>
        /// <param name="pageSize">每页显示数</param>
        /// <returns>string</returns>
        private static string BuildSelectPagedQuery<TEntity>(IDbConnection connection, Expression<Func<TEntity, bool>> predicate, int pageNumber, int pageSize, out DynamicParameters parameters)
        {
            string sql = BuildSelectSql<TEntity>(predicate, out parameters);
            var keyColumns = "";
            try { keyColumns = (Resolvers.KeyProperty(typeof(TEntity))).Name; } catch (Exception ex) { throw new Exception("不可存在多个键属性“[Key()]”\r\n" + ex.Message); }

            //排序
            var orderBy = "order by " + string.Join(", ", keyColumns);
            sql += new CustomSqlServerSqlBuilder().BuildPaging(orderBy, pageNumber, pageSize);
            return sql;
        }

        /// <summary>
        /// 生成“Query SQL”
        /// </summary>
        /// <typeparam name="TEntity">类型参数</typeparam>
        /// <param name="predicate">条件表达式目录树</param>
        /// <param name="parameters">参数</param>
        /// <returns>string</returns>
        private static string BuildSelectSql<TEntity>(Expression<Func<TEntity, bool>> predicate, out DynamicParameters parameters)
        {
            Type key = typeof(TEntity);
            string querySentence = "";
            if (!getAllQueryCache.TryGetValue(key, out querySentence))
            {
                string tableName = Resolvers.Table(key);
                querySentence = "select * from " + tableName;
                getAllQueryCache.TryAdd(key, querySentence);
            }
            querySentence = (querySentence + ((SqlCustomExpression<TEntity>)new SqlCustomExpression<TEntity>().Where(predicate)).ToSql(out parameters));

            var keyColumns = "";
            try { keyColumns = (Resolvers.KeyProperty(typeof(TEntity))).Name; } catch (Exception ex) { throw new Exception("不可存在多个键属性“[Key()]”\r\n" + ex.Message); }
            var orderBy = "order by " + string.Join(", ", keyColumns);
            querySentence = querySentence + orderBy;
            return querySentence;
        }

        #endregion
    }
}
