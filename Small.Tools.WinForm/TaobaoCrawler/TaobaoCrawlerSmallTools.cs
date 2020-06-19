using CefSharp;
using CefSharp.WinForms;
using HtmlAgilityPack;
using Small.Tools.Common.TaobaoCrawler;
using Small.Tools.Entity.TaobaoCrawlerEntity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Small.Tools.WinForm
{
    public partial class TaobaoCrawlerSmallTools : Form
    {

        #region private global variable

        /// <summary>
        /// 淘宝登录地址
        /// </summary>
        public static string loginAddress = @"https://login.taobao.com/";

        /// <summary>
        /// 搜索地址
        /// </summary>
        public static string searchAddress = @"https://s.taobao.com/search?q={0}&imgfile=&js=1&stats_click=search_radio_all:1&initiative_id=staobaoz_20200617&ie=utf8&sort=sale-desc&bcoffset=0&p4ppushleft=,44&s={1}";

        /// <summary>
        /// 当前搜索商品总页数
        /// </summary>
        public static int totalValue = 0;

        /// <summary>
        /// 每页显示数
        /// </summary>
        public static int pageSize = 44;

        /// <summary>
        /// 最后一页商品跳转数
        /// </summary>
        public static int lastPageCount = 0;

        /// <summary>
        /// 当前商品总条数
        /// </summary>
        public static int count = 0;

        /// <summary>
        /// 解析到的商品数据信息
        /// </summary>
        public static List<TaobaoCrawlerEntity> entitySource = new List<TaobaoCrawlerEntity>();

        /// <summary>
        /// 当前地址
        /// </summary>
        public static string currentAddress = string.Empty;

        /// <summary>
        /// ChromiumWebBrowser
        /// </summary>
        public ChromiumWebBrowser webBrowser;
        /// <summary>
        /// 是否加载页尾数据信息
        /// </summary>
        private static bool IsTotalNodes = true;

        #endregion

        /// <summary>
        /// 无参构造
        /// </summary>
        public TaobaoCrawlerSmallTools()
        {
            CefSettings settings = new CefSettings();
            settings.Locale = "zh-CN";
            Cef.Initialize(settings);
            InitializeComponent();

            CefSharpSettings.LegacyJavascriptBindingEnabled = true;
            webBrowser = new ChromiumWebBrowser("https://ie.icoa.cn/");
            webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            webBrowser.Location = new System.Drawing.Point(0, 0);
            webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            webBrowser.Name = "webBrowser";
            webBrowser.Size = new System.Drawing.Size(554, 552);
            webBrowser.TabIndex = 0;
            this.panel_right.Controls.Add(webBrowser);

            webBrowser.Load(loginAddress);  //进入登录地址
        }

        //窗体加载
        private void TaobaoCrawlerSmallTools_Load(object sender, EventArgs e)
        {

        }

        #region private The event

        //根据账号密码登录
        private void butLogin_Click(object sender, EventArgs e)
        {
            string jsCode = $"document.getElementsByName('fm-login-id')[0].value = '{txtUserName.Text.Trim()}'; document.getElementsByName('fm-login-password')[0].value = '{txtPassWord.Text.Trim()}'";
            jsCode = jsCode + "; document.getElementsByClassName('fm-button fm-submit password-login')[0].click();";
            webBrowser.GetBrowser().MainFrame.ExecuteJavaScriptAsync(jsCode);
        }

        //销量只可输入数据
        private void txtSales_KeyPress(object sender, KeyPressEventArgs e) { if (!(e.KeyChar == '\b' || (e.KeyChar >= '0' && e.KeyChar <= '9'))) e.Handled = true; }

        //输入关键字进行搜索，以及解析总条数和总页数
        private void txtKeyword_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtKeyword.Text.Trim()))
            {
                string new_searchAddress = string.Format(searchAddress, URLCommon.UrlEncode(txtKeyword.Text.Trim()), 0);
                webBrowser.Load(new_searchAddress);
                currentAddress = new_searchAddress; //当前加载页面地址
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// 休眠且避免页面卡死
        /// </summary>
        /// <param name="milliseconds">毫秒</param>
        public static void Delay(int milliseconds)
        {
            DateTime current = DateTime.Now;
            while (current.AddMilliseconds(milliseconds) > DateTime.Now) { Application.DoEvents(); }
            return;
        }

        /// <summary>
        /// 根据“JS”获取HTML内容
        /// </summary>
        /// <returns>string</returns>
        private string GetHtml()
        {
            string htmlValue = string.Empty;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("function getHtmlValue() { return document.getElementsByTagName('html')[0].innerHTML; }");
            stringBuilder.AppendLine("getHtmlValue();");
            var task = webBrowser.GetBrowser().GetFrame(webBrowser.GetBrowser().GetFrameNames()[0]).EvaluateScriptAsync(stringBuilder.ToString());
            task.ContinueWith(t =>
            {
                if (!t.IsFaulted)
                {
                    var response = t.Result;
                    if (response.Success == true)
                    {
                        if (response.Result != null)
                        {
                            htmlValue = response.Result.ToString();
                        }
                    }
                }
            }).Wait();
            return htmlValue;
        }

        #endregion

        //开始解析数据
        private void butStartParsing_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtKeyword.Text.Trim()))
            {
                MessageBox.Show("请输入需要搜索的关键字。", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            butStartParsing.Enabled = true; //禁用
            entitySource = new List<TaobaoCrawlerEntity>(); //存储列表页数据信息

            //平台
            string platformCode = string.Empty;
            string platformName = string.Empty;
            if (radioAll.Checked) { platformCode = string.Empty; platformName = string.Empty; }
            else if (radioTaobao.Checked) { platformCode = "taobao"; platformName = "淘宝"; }
            else if (radioTmall.Checked) { platformCode = "tmall"; platformName = "天猫"; }

            //解析数量页数，数据量
            int analysisPageSize = 0;
            int analysisCount = 0;
            if (string.IsNullOrWhiteSpace(txtAnlyticNumber.Text.Trim())
                || Convert.ToInt32(txtAnlyticNumber.Text.Trim()) == 0)
            {
                analysisPageSize = 0;
                analysisCount = lastPageCount;
            }
            else
            {
                if (analysisPageSize > totalValue)
                {
                    MessageBox.Show($"输入解析数据页大于当前商品分页页数（{totalValue}）。", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                analysisPageSize = Convert.ToInt32(txtAnlyticNumber.Text.Trim());
                analysisCount = (Convert.ToInt32(txtAnlyticNumber.Text.Trim()) - 1) * 44;
            }

            //循环解析列表数据
            for (int i = 0; i <= analysisCount; i = i + 44)
            {
                if (i > 0) IsTotalNodes = false;
                AnalysisListPage(i, IsTotalNodes);
            }

            #region 筛选数据

            //平台
            if (!string.IsNullOrWhiteSpace(platformCode) && !string.IsNullOrWhiteSpace(platformName))
            {
                if (entitySource?.Count() > 0)
                {
                    entitySource = entitySource.Where(c => c.PlatformCode == platformCode && c.PlatformName == platformName).ToList();
                }
            }

            //销售量
            if (!string.IsNullOrWhiteSpace(txtSales.Text.Trim()))
            {
                if (entitySource?.Count() > 0)
                {
                    entitySource = entitySource.Where(c => Convert.ToInt32(c.ProductSellNumber) >= Convert.ToInt32(txtSales.Text.Trim())).ToList();
                }
            }

            #endregion

            Delay(2000);
            AnalysisDetails(entitySource);
        }

        /// <summary>
        /// 解析列表页面“Html”
        /// </summary>
        /// <param name="pageLastNumber">跳转条数</param>
        /// <param name="isTotalNodes">是否加载当前页数以及总行数</param>
        /// <returns>bool</returns>
        private bool AnalysisListPage(int pageLastNumber, bool isTotalNodes = false)
        {
            bool result = true;
            string new_searchAddress = string.Format(searchAddress, URLCommon.UrlEncode(txtKeyword.Text.Trim()), pageLastNumber);
            webBrowser.Load(new_searchAddress);
            currentAddress = new_searchAddress; //当前加载页面地址
            Delay(2000);

            HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
            var bodyHtml = GetHtml();//this.webBrowser.GetBrows;
            if (string.IsNullOrWhiteSpace(bodyHtml))
            {
                MessageBox.Show("暂无解析到相关商品信息，请重试。", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                result = false;
            }
            document.LoadHtml(bodyHtml);

            //解析相关商品列表  //<div class="item J_MouserOnverReq  "
            //HtmlNodeCollection goodsNodes = document.DocumentNode.SelectNodes("//div[@class='item J_MouserOnverReq  '] ");
            HtmlNodeCollection goodsNodes = document.DocumentNode.SelectNodes("//div[starts-with(@class,'item J_MouserOnverReq')]");
            if (isTotalNodes)   //是否加载页数
            {
                //总页数
                HtmlNodeCollection totalNodes = document.DocumentNode.SelectNodes("//div[@class='total'] ");
                if (totalNodes != null)
                    if (totalNodes.Count() > 0)
                        totalValue = Convert.ToInt32(totalNodes[0].InnerText.Replace(@"\n", "").Replace("，", "").Replace("共", "").Replace("页", "").Trim());
                var pageNumber = totalValue - 1;
                //最后一页商品跳转数
                lastPageCount = pageNumber * pageSize;
            }

            if (goodsNodes != null)
            {
                foreach (var node in goodsNodes)
                {
                    TaobaoCrawlerEntity entity = new TaobaoCrawlerEntity();
                    HtmlAgilityPack.HtmlDocument nodeDocument = new HtmlAgilityPack.HtmlDocument();
                    var htmlNode = node.OuterHtml;
                    nodeDocument.LoadHtml(htmlNode);

                    #region //div[@class='pic']
                    HtmlNodeCollection productSearchListNameNodes = nodeDocument.DocumentNode.SelectNodes("//div[@class='pic']");
                    if (productSearchListNameNodes != null)
                    {
                        //a 标签
                        var node_a = productSearchListNameNodes[0].SelectSingleNode("//a");
                        if (node != null)
                        {
                            //商品编号 trace-nid
                            string productId = node_a.GetAttributeValue("trace-nid", "");
                            //商品金额 trace-price
                            string productMoney = node_a.GetAttributeValue("trace-price", "");
                            //商品详细页地址 href
                            string productDetailsAddress = node_a.GetAttributeValue("href", "");
                            productDetailsAddress = "https:" + productDetailsAddress.Replace("amp;", "");

                            //img 标签
                            var node_img = node_a.SelectSingleNode("//img");
                            if (node_img != null)
                            {
                                //商品列表页图片地址 data-src
                                var productSearchListImgAddress = node_img.GetAttributeValue("data-src", "");
                                productSearchListImgAddress = "https:" + productSearchListImgAddress;

                                //商品搜索列表页显示的名称
                                var productSearchListName = node_img.GetAttributeValue("alt", "");
                                entity.ProductSearchListName = productSearchListName;
                                entity.ProductSearchListImgAddress = productSearchListImgAddress;
                                entity.ProductDetailsAddress = productDetailsAddress;
                                entity.ProductMoney = productMoney;
                                entity.ProductId = productId;
                            }
                        }

                        if (string.IsNullOrWhiteSpace(entity.ProductSearchListName)
                            || string.IsNullOrWhiteSpace(entity.ProductSearchListImgAddress)
                            || string.IsNullOrWhiteSpace(entity.ProductSearchListImgAddress)
                            || string.IsNullOrWhiteSpace(entity.ProductDetailsAddress)
                            || string.IsNullOrWhiteSpace(entity.ProductMoney)
                              || string.IsNullOrWhiteSpace(entity.ProductId))
                        {
                            MessageBox.Show("程序解析“Html”错误“//div[@class='pic']”，请联系作者。", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            result = false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("程序解析“Html”错误“//div[@class='pic']”，请联系作者。", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        result = false;
                    }
                    #endregion
                    #region //div[@class='row row-1 g-clearfix']
                    HtmlNodeCollection productMoneyAndSellNodes = nodeDocument.DocumentNode.SelectNodes("//div[@class='row row-1 g-clearfix'] ");
                    if (productMoneyAndSellNodes != null)
                    {
                        //销量 div class="deal-cnt">
                        if (productMoneyAndSellNodes[0].SelectSingleNode("//div[@class='deal-cnt']") != null)
                        {
                            var productSellNumber = productMoneyAndSellNodes[0].SelectSingleNode("//div[@class='deal-cnt']")?.InnerText;
                            productSellNumber = productSellNumber.Replace("人", "").Replace("收货", "").Replace("付款", "");
                            entity.ProductSellNumber = productSellNumber;
                        }

                        //金额
                        if (productMoneyAndSellNodes[0].SelectSingleNode("//strong") != null)
                        {
                            var new_ProductMoney = productMoneyAndSellNodes[0].SelectSingleNode("//strong")?.InnerText;
                            if (Convert.ToDouble(entity.ProductMoney) != Convert.ToDouble(new_ProductMoney))
                                entity.ProductMoney = new_ProductMoney;
                        }

                        if (string.IsNullOrWhiteSpace(entity.ProductMoney)
                            || string.IsNullOrWhiteSpace(entity.ProductSellNumber))
                        {
                            MessageBox.Show("程序解析“Html”错误“//div[@class='row row-1 g-clearfix']”，请联系作者。", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            result = false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("程序解析“Html”错误“//div[@class='row row-1 g-clearfix']”，请联系作者。", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        result = false;
                    }

                    #endregion
                    #region //div[@class='row row-2 title']
                    HtmlNodeCollection productNameNodes = nodeDocument.DocumentNode.SelectNodes("//div[@class='row row-2 title'] ");
                    if (productNameNodes != null)
                    {
                        //商品在详情显示的名称
                        entity.ProductDetailsPageName = productNameNodes[0].InnerText.Replace("\n", "").Trim();
                        if (entity.ProductDetailsPageName != nodeDocument.DocumentNode.SelectNodes("//div[@class='row row-2 title']//a")[0].InnerText.Replace("\n", "").Trim())
                            entity.ProductDetailsPageName = nodeDocument.DocumentNode.SelectNodes("//div[@class='row row-2 title']//a")[0].InnerText.Replace("\n", "").Trim();
                    }
                    else
                    {
                        MessageBox.Show("程序解析“Html”错误“//div[@class='row row-2 title']”，请联系作者。", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        result = false;
                    }
                    #endregion
                    #region //div[@class='row row-3 g-clearfix']
                    HtmlNodeCollection theStoreNodes = nodeDocument.DocumentNode.SelectNodes("//div[@class='row row-3 g-clearfix'] ");
                    if (theStoreNodes != null)
                    {
                        //店铺归属地址
                        if (theStoreNodes[0].SelectSingleNode("//div[@class='location'] ") != null)
                            entity.StoreAddress = theStoreNodes[0].SelectSingleNode("//div[@class='location'] ").InnerText;
                        //店铺地址 URL
                        if (theStoreNodes[0].SelectSingleNode("//div[@class='shop']//a") != null)
                            entity.ShopNameAddress = "https:" + theStoreNodes[0].SelectSingleNode("//div[@class='shop']//a").GetAttributeValue("href", "");
                        //店铺名称（掌柜）
                        if (theStoreNodes[0].SelectNodes("//div[@class='shop']//a//span") != null)
                            if (theStoreNodes[0].SelectNodes("//div[@class='shop']//a//span").Count() > 0)
                                entity.ShopName = theStoreNodes[0].SelectNodes("//div[@class='shop']//a//span")
                                    [theStoreNodes[0].SelectNodes("//div[@class='shop']//a//span").Count() - 1].InnerText;

                        if (string.IsNullOrWhiteSpace(entity.StoreAddress)
                            || string.IsNullOrWhiteSpace(entity.ShopNameAddress)
                            || string.IsNullOrWhiteSpace(entity.ShopName))
                        {
                            MessageBox.Show("程序解析“Html”错误“//div[@class='row row-3 g-clearfix']”，请联系作者。", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            result = false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("程序解析“Html”错误“//div[@class='row row-3 g-clearfix']”，请联系作者。", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        result = false;
                    }
                    #endregion
                    #region //div[@class='row row-4 g-clearfix']
                    //所属平台
                    HtmlNodeCollection platformCodeNodes = nodeDocument.DocumentNode.SelectNodes("//div[@class='row row-4 g-clearfix']//div//ul");
                    if (platformCodeNodes != null)
                    {
                        if (platformCodeNodes[0].SelectNodes("//li//a") != null)
                        {
                            var platformCodeLiNodesHtml = platformCodeNodes[0].SelectNodes("//li//a")[0].OuterHtml;
                            if (platformCodeLiNodesHtml.IndexOf("taobao") != -1)
                            {
                                entity.PlatformCode = "taobao";
                                entity.PlatformName = "淘宝";
                            }
                            else if (platformCodeLiNodesHtml.IndexOf("tmall") != -1)
                            {
                                entity.PlatformCode = "tmall";
                                entity.PlatformName = "天猫";
                            }
                        }
                        else
                        {
                            entity.PlatformCode = "taobao";
                            entity.PlatformName = "淘宝";
                        }

                        if (string.IsNullOrWhiteSpace(entity.PlatformName)
                            || string.IsNullOrWhiteSpace(entity.PlatformCode))
                        {
                            MessageBox.Show("程序解析“Html”错误“//div[@class='row row-4 g-clearfix']”，请联系作者。", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            result = false;
                        }
                    }
                    else
                    {
                        MessageBox.Show("程序解析“Html”错误“//div[@class='row row-4 g-clearfix']”，请联系作者。", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        result = false;
                    }

                    #endregion

                    if (result == false) return result;  //中途有解析错误

                    entitySource.Add(entity);
                }
                result = true;  //解析正确完成
            }
            else
            {
                MessageBox.Show("程序解析“Html”错误“//div[@class='row row-4 g-clearfix']”，请联系作者。", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// 解析详情页面“Html”
        /// </summary>
        /// <param name="entity">数据信息</param>
        /// <returns>bool</returns>
        private bool AnalysisDetails(List<TaobaoCrawlerEntity> entitys)
        {
            bool result = true;
            if (entitys?.Count() > 0)
            {
                foreach (var entity in entitys)
                {
                    webBrowser.Load(entity.ProductDetailsAddress);
                    var details = new TaobaoCrawlerDetails();

                    Delay(2000);
                    //开始解析详情页
                    HtmlAgilityPack.HtmlDocument document = new HtmlAgilityPack.HtmlDocument();
                    string htmlValue = GetHtml();
                    document.LoadHtml(htmlValue);

                    //累计评论 | 交易成功(淘宝) //div[@class='tb-counter-bd']
                    HtmlNodeCollection evaluationOrTradingNodes = document.DocumentNode.SelectNodes("//div[@class='tb-counter-bd']");
                    if (evaluationOrTradingNodes != null)
                    {
                        //累计评论  <strong id="J_RateCounter">
                        var evaluation = evaluationOrTradingNodes[0].SelectSingleNode("//div[@class='tb-rate-counter']//a//strong[@id='J_RateCounter']");
                        if (evaluation != null)
                        {
                            details.CumulativeCommentsNum = evaluation.InnerText;
                        }
                        //交易成功  <strong id="J_SellCounter">
                        var trading = evaluationOrTradingNodes[0].SelectSingleNode("//div[@class='tb-sell-counter']//a//strong[@id='J_SellCounter']");
                        if (trading != null)
                        {
                            details.TransactionSuccessNum = trading.InnerText;
                        }
                    }

                }
            }

            return result;
        }

        private void TaobaoCrawlerSmallTools_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cef.Shutdown();
        }
    }

    /// <summary>
    /// 继承“WebBrowser”，重写方法“AttachInterfaces | DetachInterfaces”
    /// 防止弹窗报错。
    /// </summary>
    public class SmallToolsWebBrowser : WebBrowser
    {
        SHDocVw.WebBrowser iweb;
        protected override void AttachInterfaces(object nativeActiveXObject)
        {
            base.AttachInterfaces(nativeActiveXObject);
            iweb = (SHDocVw.WebBrowser)nativeActiveXObject;
            iweb.Silent = true;
        }

        protected override void DetachInterfaces()
        {
            base.DetachInterfaces();
            iweb = null;
        }
    }
}
