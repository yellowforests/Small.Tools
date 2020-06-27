using System;
using System.Reflection;
using static Dommel.DommelMapper;

namespace Small.Tools.DataBase.Resolver
{
    /// <summary>
    /// “Dommel Dapper” 表名自定义解析。
    /// </summary>
    public class CustomTableNameResolver : ITableNameResolver
    {
        /// <summary>
        /// “Dommel Dapper” 表名自定义解析
        /// 解析表名。
        /// 查找[System.ComponentModel.DataAnnotations.Schema.Table("")] 属性。否则生成类型
        /// </summary>
        /// <param name="type">entity type</param>
        /// <returns>string table name</returns>
        public string ResolveTableName(Type type)
        {
            /* TableAttribute [System.ComponentModel.DataAnnotations.Schema.Table("test_deptno")]
             * DommelMapper.SetTableNameResolver(new CustomTableNameResolver()); 
             * **/
            var typeInfo = type.GetTypeInfo();
            var tableAttribute = typeInfo.GetCustomAttribute<System.ComponentModel.DataAnnotations.Schema.TableAttribute>();
            var tableName = type.Name;
            if (tableAttribute != null)
            {
                if (!string.IsNullOrEmpty(tableAttribute.Schema))
                    return $"{tableAttribute.Schema}.{tableAttribute.Name}";
                tableName = tableAttribute.Name;
            }
            return tableName;
        }
    }
}
