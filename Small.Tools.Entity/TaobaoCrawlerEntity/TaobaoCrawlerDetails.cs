using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Small.Tools.Entity.TaobaoCrawlerEntity
{
    /// <summary>
    /// 商品详情页数据信息
    /// </summary>
    public class TaobaoCrawlerDetails
    {
        /// <summary>
        /// 累计评价数
        /// </summary>
        public string CumulativeCommentsNum { set; get; }

        /// <summary>
        /// 交易成功数据（天猫：月销量）
        /// </summary>
        public string TransactionSuccessNum { set; get; }

        /// <summary>
        /// 详细页的商品价格
        /// </summary>
        public string CommodityPrices { set; get; }

        /// <summary>
        /// 当前商品优惠信息
        /// （一对多，可能存在多条信息）
        /// </summary>
        public List<PreferentialInfo> PreferentialInfo { set; get; }

        /// <summary>
        ///商品收藏人气
        /// </summary>
        public string CollectTheSentiment { set; get; }
    }

    /// <summary>
    /// 当前商品优惠信息
    /// </summary>
    public class PreferentialInfo
    {
        /// <summary>
        /// 优惠券ico图片地址
        /// </summary>
        public string PreferentialIMGAddress { set; get; }

        /// <summary>
        /// 优惠券内容
        /// </summary>
        public string PreferentialContent { set; get; }
    }
}
