using Small.Tools.DataBase;
using Small.Tools.DataBase.Extensions;
using Small.Tools.Entity;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Small.Tools.Console
{
    public class Program
    {
        static void Main(string[] args)
        {
            //获取代理IP
            Task.Run(() => HtmlRule.Liunian());
            Task.Run(() => HtmlRule.Xici());
            Task.Run(() => HtmlRule.XiciGaoni());
            Task.Run(() => HtmlRule.XiciPutong());


            System.Console.ReadLine();
        }

        /// <summary>
        /// 插入到数据库
        /// </summary>
        /// <param name="info">info</param>
        public static void Add_IPAddress(ip_agency_data info)
        {
            using (var dbConnection = BaseConfig.GetSqlConnection())
            {
                var result = dbConnection.Insert<ip_agency_data>(info);
                System.Console.WriteLine($"{info.ip_address}:{info.ip_port} ， 已加入到数据库。");
            }
        }
    }
}
