using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using static Dommel.DommelMapper;

namespace Small.Tools.DataBase.Resolver
{
    /// <summary>
    /// “Dommel Dapper” 当数据库列的命名约定与实体属性不同。
    /// 表示一个列属性。
    /// </summary>
    public class CustomColumnNameResolver : IColumnNameResolver
    {
        /// <summary>
        /// 自定义列名与数据库不一样
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public string ResolveColumnName(PropertyInfo propertyInfo)
        {
            /* ColumnAttribute [System.ComponentModel.DataAnnotations.Schema.Column()]
             * DommelMapper.SetColumnNameResolver(new CustomColumnNameResolver());
             * **/
            var columnAttr = propertyInfo.GetCustomAttribute<ColumnAttribute>();
            if (columnAttr != null)
            {
                return columnAttr.Name;
            }

            return propertyInfo.Name;
        }
    }
}
