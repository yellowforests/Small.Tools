using Dommel;
using System.Reflection;

namespace Small.Tools.DataBase.Builder
{
    public class CustomSqlServerSqlBuilder : DommelMapper.ISqlBuilder
    {
        /// <summary>
        /// 生成“Insert SQL”
        /// </summary>
        /// <param name="tableName">表名</param>
        /// <param name="columnNames">列名</param>
        /// <param name="paramNames">参数名</param>
        /// <param name="keyProperty">属性</param>
        /// <returns>string</returns>
        public string BuildInsert(string tableName, string[] columnNames, string[] paramNames, PropertyInfo keyProperty)
        {
            string[] insertSQLArry = new string[] { "set nocount on insert into ", tableName, " (", string.Join(", ", columnNames), ") values (", string.Join(", ", paramNames), ") select cast(scope_identity() as int)" };
            return string.Concat(insertSQLArry);
        }

        /// <summary>
        /// 生成分页“Paging SQL”
        /// </summary>
        /// <param name="orderBy">排序</param>
        /// <param name="pageNumber">当前页</param>
        /// <param name="pageSize">每页显示数</param>
        /// <returns>string</returns>
        public string BuildPaging(string orderBy, int pageNumber, int pageSize)
        {
            var start = pageNumber >= 1 ? (pageNumber - 1) * pageSize : 0;
            return $" {orderBy} offset {start} rows fetch next {pageSize} rows only";
        }
    }
}
