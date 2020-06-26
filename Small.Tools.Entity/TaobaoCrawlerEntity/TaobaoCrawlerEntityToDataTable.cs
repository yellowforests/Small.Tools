using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Small.Tools.Entity.TaobaoCrawlerEntity
{
    public class TaobaoCrawlerEntityToDataTable
    {
        /// <summary>
        /// 商品编号 
        /// </summary>
        public string ProductId { set; get; }

        /// 商品搜索列表页显示的名称
        /// </summary>
        public string ProductSearchListName { set; get; }

        /// <summary>
        /// 商品在详情显示的名称
        /// </summary>
        public string ProductDetailsPageName { set; get; }

        /// <summary>
        /// 商品金额
        /// </summary>
        public string ProductMoney { set; get; }

        /// <summary>
        /// 商品卖出数据（{0}人收货 | {0}人收款）
        /// </summary>
        public string ProductSellNumber { set; get; }

        /// <summary>
        /// 店铺名称（店铺掌柜）
        /// </summary>
        public string ShopName { set; get; }

        /// <summary>
        /// 店铺地址
        /// </summary>
        public string ShopNameAddress { set; get; }

        /// <summary>
        /// 店铺归属地址
        /// </summary>
        public string StoreAddress { set; get; }

        /// <summary>
        /// 商品详情页地址
        /// </summary>
        public string ProductDetailsAddress { set; get; }

        /// <summary>
        /// 商品列表页图片地址
        /// </summary>
        public string ProductSearchListImgAddress { set; get; }

        /// <summary>
        /// 所属平台Code（tmall | taobao）
        /// </summary>
        public string PlatformCode { set; get; }

        /// <summary>
        /// 所属平台名称（天猫 | 淘宝）
        /// </summary>
        public string PlatformName { set; get; }

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
        ///商品收藏人气
        /// </summary>
        public string CollectTheSentiment { set; get; }

        /// <summary>
        /// 优惠券内容，“逗号分隔开”
        /// </summary>
        public string PreferentialContent { set; get; }
    }
}
