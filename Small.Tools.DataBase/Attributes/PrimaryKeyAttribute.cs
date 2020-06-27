using System;

namespace Small.Tools.DataBase.Attributes
{
    /// <summary>
    /// 主键,是否自增长
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class PrimaryKeyAttribute : Attribute
    {
        /// <summary>
        /// 是否为自增长
        /// </summary>
        public bool CheckAutoId { get; set; }

        public PrimaryKeyAttribute() { this.CheckAutoId = false; }
        public PrimaryKeyAttribute(bool checkAutoId) { this.CheckAutoId = checkAutoId; }
    }
}
