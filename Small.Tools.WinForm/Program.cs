using Small.Tools.DataBase;
using Small.Tools.DataBase.Extensions;
using Small.Tools.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Small.Tools.WinForm
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                //判断是否有权限使用该程序，“ select * from [dbo].[ip_agency_data] where [ip_sourcename] ='登陆权限验证' ”
                var entityList = new List<ip_agency_data>();
                using (var dbConnection = BaseConfig.GetSqlConnection())
                {
                    entityList = dbConnection.Query<ip_agency_data>(c => c.ip_sourcename == "登陆权限验证").ToList();
                }
                if (entityList.Count() > 0) { Application.Run(new TaobaoCrawlerSmallTools()); }
                else { MessageBox.Show("您已无权使用该插件。", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
