using System;
using System.Linq.Expressions;

namespace Small.Tools.DataBase
{
    /// <summary>
    /// SQL Check
    /// </summary>
    public class SqlCheck
    {
        /// <summary>
        /// 校验表达式是否包含方法
        /// </summary>
        /// <returns>bool</returns>
        public static bool Check<TEntity>(Expression<Func<TEntity, bool>> predicate)
        {
            if (predicate.ToString().ToUpper().IndexOf("TOSTRING") > -1
                    || predicate.ToString().ToUpper().IndexOf("TODATETIME") > -1
                    || predicate.ToString().ToUpper().IndexOf("STARTSWITH") > -1
                    || predicate.ToString().ToUpper().IndexOf("ENDSWITH") > -1
                    || predicate.ToString().ToUpper().IndexOf("CONTAINS") > -1
                    || predicate.ToString().ToUpper().IndexOf("TRIM") > -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 校验表达式是否包含方法
        /// </summary>
        /// <returns>bool</returns>
        public static bool Check<TEntity>(string predicate)
        {
            if (predicate.ToString().ToUpper().IndexOf("TOSTRING") > -1
                    || predicate.ToString().ToUpper().IndexOf("TODATETIME") > -1
                    || predicate.ToString().ToUpper().IndexOf("STARTSWITH") > -1
                    || predicate.ToString().ToUpper().IndexOf("ENDSWITH") > -1
                    || predicate.ToString().ToUpper().IndexOf("CONTAINS") > -1
                    || predicate.ToString().ToUpper().IndexOf("TRIM") > -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
