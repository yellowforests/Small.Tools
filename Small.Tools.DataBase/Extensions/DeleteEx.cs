using Dommel;
using System;
using System.Data;
using System.Linq.Expressions;

namespace Small.Tools.DataBase.Extensions
{
    /// <summary>
    /// Delete Extensions
    /// </summary>
    public static partial class DapperExtensions
    {
        /// <summary>
        /// Delte
        /// 根据实体删除。
        /// </summary>
        /// <typeparam name="TEntity">类型参数</typeparam>
        /// <param name="connection">DbConnection</param>
        /// <param name="entity">实体数据信息</param>
        /// <param name="transaction">事务</param>
        /// <returns>bool</returns>
        public static bool Delete<TEntity>(this IDbConnection connection, TEntity entity, IDbTransaction transaction = null)
        {
            try
            {
                return DommelMapper.Delete<TEntity>(connection, entity, transaction);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        /// <summary>
        /// Delete ALL
        /// 删除全部。
        /// </summary>
        /// <typeparam name="TEntity">类型参数</typeparam>
        /// <param name="connection">DbConnection</param>
        /// <param name="transaction">事务</param>
        /// <returns>bool</returns>
        public static bool DeleteAll<TEntity>(this IDbConnection connection, IDbTransaction transaction = null)
        {
            try
            {
                return DommelMapper.DeleteAll<TEntity>(connection, transaction);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }

        /// <summary>
        /// Delete
        /// 根据条件删除。
        /// </summary>
        /// <typeparam name="TEntity">类型参数</typeparam>
        /// <param name="connection">DbConnection</param>
        /// <param name="predicate">表达式目录树</param>
        /// <param name="transaction">事务</param>
        /// <returns>bool</returns>
        public static bool Delete<TEntity>(this IDbConnection connection, Expression<Func<TEntity, bool>> predicate, IDbTransaction transaction = null)
        {
            try
            {
                return DommelMapper.DeleteMultiple<TEntity>(connection, predicate, transaction);
            }
            catch (Exception ex) { throw new Exception(ex.Message); }
        }
    }
}
