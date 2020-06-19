using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Small.Tools.Common.TaobaoCrawler
{
    /// <summary>
    /// URLCommon
    /// </summary>
    public class URLCommon
    {
        /// <summary>
        /// URL Common
        /// </summary>
        /// <param name="address">address</param>
        /// <returns>string</returns>
        public static string UrlEncode(string address)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = Encoding.UTF8.GetBytes(address);
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(@"%" + Convert.ToString(byStr[i], 16));
            }
            return (sb.ToString());
        }
    }
}
