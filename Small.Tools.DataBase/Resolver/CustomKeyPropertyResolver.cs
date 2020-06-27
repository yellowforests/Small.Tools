using System;
using System.Linq;
using System.Reflection;
using static Dommel.DommelMapper;

namespace Small.Tools.DataBase.Resolver
{
    /// <summary>
    /// “Dommel Dapper” 自定义键属性。
    /// </summary>
    public class CustomKeyPropertyResolver : IKeyPropertyResolver
    {
        /// <summary>
        /// 自定义键属性 “PrimaryKeyAttribute”
        /// 查找带有[key]属性的属性来查找键属性。
        /// </summary>
        /// <param name="type">entity type</param>
        /// <returns>PropertyInfo</returns>
        public PropertyInfo ResolveKeyProperty(Type type)
        {
            /* KeyAttribute [System.ComponentModel.DataAnnotations.Key()]
             * DommelMapper.SetKeyPropertyResolver(new CustomKeyPropertyResolver());
             * **/
            var keyProps = Resolvers.Properties(type)
               .Where(p => string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase)
                   || p.GetCustomAttribute<System.ComponentModel.DataAnnotations.KeyAttribute>() != null
                   || p.GetCustomAttribute<Small.Tools.DataBase.Attributes.PrimaryKeyAttribute>() != null).ToArray();

            if (keyProps.Length == 0)
                throw new InvalidOperationException($"Could not find the key properties for type '{type.FullName}'.");

            return keyProps.FirstOrDefault();
        }

        /// <summary>
        /// 自定义键属性 “PrimaryKeyAttribute”
        /// 查找带有[key]属性的属性来查找键属性。</summary>
        /// <param name="type">entity type</param>
        /// <param name="isIdentity">是否自增长</param>
        /// <returns>PropertyInfo</returns>
        public PropertyInfo ResolveKeyProperty(Type type, out bool isIdentity)
        {
            /* KeyAttribute [System.ComponentModel.DataAnnotations.Key()]
             * DommelMapper.SetKeyPropertyResolver(new CustomKeyPropertyResolver());
             * **/
            var keyProps = Resolvers.Properties(type)
                .Where(p => string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase)
                    || p.GetCustomAttribute<System.ComponentModel.DataAnnotations.KeyAttribute>() != null
                    || p.GetCustomAttribute<Small.Tools.DataBase.Attributes.PrimaryKeyAttribute>() != null).ToArray();

            if (keyProps.Length == 0)
                throw new InvalidOperationException($"Could not find the key properties for type '{type.FullName}'.");

            if (keyProps.FirstOrDefault().GetCustomAttribute<Small.Tools.DataBase.Attributes.PrimaryKeyAttribute>() != null
                && keyProps.FirstOrDefault().GetCustomAttribute<Small.Tools.DataBase.Attributes.PrimaryKeyAttribute>().CheckAutoId)
                isIdentity = true;
            else
                isIdentity = keyProps.Length > 0 ? true : false;

            return keyProps.FirstOrDefault();
        }
    }
}
