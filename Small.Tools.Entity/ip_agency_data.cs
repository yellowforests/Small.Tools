using Small.Tools.DataBase.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Small.Tools.Entity
{
    /// <summary>
    /// ip_agency_data
    /// </summary>
    [Table("dbo.ip_agency_data")]
    public class ip_agency_data
    {
        [PrimaryKey(CheckAutoId = true)]
        [Key()]
        public int ip_id { set; get; }
        public string ip_address { set; get; }
        public string ip_port { set; get; }
        public DateTime ip_createtime { set; get; }
        public string ip_sourcename { set; get; }
        
    }
}
