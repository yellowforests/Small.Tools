using CefSharp;
using CefSharp.Internals;
using CefSharp.WinForms;
using HtmlAgilityPack;
using mshtml;
using Small.Tools.Common;
using Small.Tools.Common.TaobaoCrawler;
using Small.Tools.DataBase;
using Small.Tools.DataBase.Extensions;
using Small.Tools.Entity;
using Small.Tools.Entity.TaobaoCrawlerEntity;
using Small.Tools.WinForm.TaobaoCrawler;
using Small.Tools.WinForm.TaobaoCrawler.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
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

        #endregion

        //屏幕分辨率
        private static int resolvingWidth = 0;
        private static int resolvingHeigth = 0;
        public static KeyboardHook k_hook = null;

        /// <summary>
        /// 无参构造
        /// </summary>
        public TaobaoCrawlerSmallTools()
        {
            InitializeComponent();

            #region > CefSharp Settings
            CefSettings settings = new CefSettings();
            settings.Locale = "zh-CN";
            settings.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36";
            Cef.Initialize(settings);

            CefSharpSettings.LegacyJavascriptBindingEnabled = true;
            webBrowser = new ChromiumWebBrowser("https://ie.icoa.cn/")
            {
                MenuHandler = new MenuHandler()
            };
            webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            webBrowser.Location = new System.Drawing.Point(0, 0);
            webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            webBrowser.Name = "webBrowser";
            webBrowser.Size = new System.Drawing.Size(554, 552);
            webBrowser.TabIndex = 0;
            this.panel_right.Controls.Add(webBrowser);

            List<string> usedIPAddress = new List<string>();

            //启动一个线程更新代理“IP”
            var starTime = DateTime.Now;
            try
            {
                Task.Factory.StartNew(() =>
                {
                    bool result = true;
                    while (result)
                    {
                        TimeSpan dateTimeSpan = new TimeSpan(DateTime.Now.Ticks);
                        TimeSpan duration = new TimeSpan(starTime.Ticks).Subtract(dateTimeSpan).Duration();
                        var minutes = duration.TotalMinutes;
                        if (minutes >= 1)
                        {
                            var entityList = new List<ip_agency_data>();
                            using (var dbConnection = BaseConfig.GetSqlConnection())
                            {
                                entityList = dbConnection.QueryALL<ip_agency_data>().ToList()
                                .Where(c => c.ip_sourcename != "登陆权限验证").ToList();
                                foreach (var info in entityList)
                                {
                                    if (WhetherTheAgentIsAvailable(info.ip_address, info.ip_port)
                                    && usedIPAddress.Where(c => c.Equals($"{info.ip_address}:{info.ip_port}"))?.Count() <= 0)
                                    {
                                        //设置代理
                                        string userName = string.Empty;
                                        string passWord = string.Empty;
                                        LoadingProxy(info.ip_address, info.ip_port, userName, passWord);
                                        LogWrite($"设置代理“{info.ip_address}:{info.ip_port}”");
                                        starTime = DateTime.Now;
                                        usedIPAddress.Add($"{info.ip_address}:{info.ip_port}");
                                        break;
                                    }
                                    else
                                    {
                                        dbConnection.Delete(info);
                                    }
                                }
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("设置IP代理失败，3秒之后程序自动关闭，error:" + ex.Message, "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Delay(3000);
                System.Environment.Exit(0);
            }

            //进入登录地址
            webBrowser.Load(loginAddress);
            #endregion

            //屏幕分辨率
            resolvingHeigth = Screen.PrimaryScreen.Bounds.Height;
            resolvingWidth = Screen.PrimaryScreen.Bounds.Width;
        }

        //窗体加载
        private void TaobaoCrawlerSmallTools_Load(object sender, EventArgs e) { LogWrite("程序正在启动，请稍后"); }

        #region 设置代理

        private void LoadingProxy(string address, string port, string userName, string passWord)
        {
            SetProxy(webBrowser, $"{address}:{port}");
            CefSharp.CefSharpSettings.Proxy = new CefSharp.ProxyOptions(address, port, userName, passWord);
        }

        private void SetProxy(ChromiumWebBrowser webBrowser, string Address)
        {
            //Address: "127.0.0.1:1080"
            //只能在WebBrowser UI呈现后获取 Request 上下文
            Cef.UIThreadTaskFactory.StartNew(delegate
            {
                //获取 Request 上下文
                var context = webBrowser.GetBrowser().GetHost().RequestContext;
                var dict = new Dictionary<string, object>();
                dict.Add("mode", "fixed_servers");
                dict.Add("server", Address);
                //设置代理
                string error;
                context.SetPreference("proxy", dict, out error);

                //如果 error 不为空则表示设置失败。
                if (!string.IsNullOrWhiteSpace(error))
                {
                    MessageBox.Show(error, "Tip", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
        }

        #endregion

        #region > private The event

        //搜索
        private async void butSearch_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtKeyword.Text.Trim())) { MessageBox.Show("请输入需要搜索的关键字。", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            if (!string.IsNullOrWhiteSpace(txtKeyword.Text.Trim()))
            {
                string new_searchAddress = string.Format(searchAddress, URLCommon.UrlEncode(txtKeyword.Text.Trim()), 0);
                webBrowser.Load(new_searchAddress);
                currentAddress = new_searchAddress; //当前加载页面地址
                while (GetHtmlValue().Trim().ToLower() != "complete".ToLower()) { Application.DoEvents(); };
                await Task.Delay(3000);

                var source = await webBrowser.GetSourceAsync();
                var document = new HtmlAgilityPack.HtmlDocument();
                document.LoadHtml(source);
                //总页数
                HtmlNodeCollection totalNodes = document.DocumentNode.SelectNodes("//div[@class='total']");
                if (totalNodes != null)
                    if (totalNodes.Count() > 0)
                        totalValue = Convert.ToInt32(totalNodes[0].InnerText.Replace(@"\n", "").Replace("，", "").Replace("共", "").Replace("页", "").Trim());
                var pageNumber = totalValue - 1;
                //最后一页商品跳转数
                lastPageCount = pageNumber * pageSize;

                LogWrite($"当前共解析到“{totalValue}”页数据");
                butStartParsing.Enabled = true;
            }
        }

        //窗体关闭
        private void TaobaoCrawlerSmallTools_FormClosing(object sender, FormClosingEventArgs e) { Cef.Shutdown(); }

        //根据账号密码登录
        private async void butLogin_Click(object sender, EventArgs e)
        {
            string jsCode = $"document.getElementsByName('fm-login-id')[0].value = '{txtUserName.Text.Trim()}'; document.getElementsByName('fm-login-password')[0].value = '{txtPassWord.Text.Trim()}'; ";
            webBrowser.GetBrowser().MainFrame.ExecuteJavaScriptAsync(jsCode);

            //document.getElementsByClassName('fm-button fm-submit password-login')[0].click();
            await ClickElement("document.getElementsByClassName('fm-button fm-submit password-login')[0]");
            LogWrite("登录成功");
        }

        //销量只可输入数据
        private void txtSales_KeyPress(object sender, KeyPressEventArgs e) { if (!(e.KeyChar == '\b' || (e.KeyChar >= '0' && e.KeyChar <= '9'))) e.Handled = true; }

        #endregion

        #region > private methods

        private static bool WhetherTheAgentIsAvailable(string ProxyIP, string ProxyPort)
        {
            try
            {
                using (WebClient web = new WebClient())
                {
                    HttpWebRequest Req;
                    HttpWebResponse Resp;
                    WebProxy proxyObject = new WebProxy(ProxyIP, Convert.ToInt32(ProxyPort));
                    Req = WebRequest.Create("https://www.baidu.com") as HttpWebRequest;
                    Req.Proxy = proxyObject; //设置代理
                    Req.Timeout = 3000;   //超时
                    Resp = (HttpWebResponse)Req.GetResponse();
                    Encoding bin = Encoding.GetEncoding("UTF-8");
                    using (StreamReader sr = new StreamReader(Resp.GetResponseStream(), bin))
                    {
                        string str = sr.ReadToEnd();
                        if (str.Contains("百度"))
                        {
                            return true;
                        }
                    }
                }
            }
            catch { return false; }
            return false;
        }

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

        /// <summary>
        /// 获取当前页面状态，是否加载完成
        /// </summary>
        /// <returns>string</returns>
        private string GetHtmlValue()
        {
            string htmlValue = string.Empty;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("function getHtmlValue() { return document.readyState; }");
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

        /// <summary>
        /// 获取“function”函数返回值
        /// </summary>
        /// <param name="stringBuilder">js脚本</param>
        /// <returns>string</returns>
        private string GetFunctionResultValue(string stringBuilder)
        {
            var htmlValue = string.Empty;
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

        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="message">日志信息</param>
        private void LogWrite(string message)
        {
            this.listBox_LogOutput.BeginInvoke(new Action(() =>
            {
                this.listBox_LogOutput.Items.Add($" >{DateTime.Now.ToString("HH:mm:ss ")} {message}...");
            }));
        }

        #endregion

        #region > private task methods

        /// <summary>
        /// 是否需要登录
        /// </summary>
        /// <returns>bool</returns>
        private async Task<bool> WhetherYouNeedToLogin()
        {
            var source = await webBrowser.GetSourceAsync();
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(source);
            var userName = htmlDoc.DocumentNode.SelectNodes("//input[@id='fm-login-id']");
            var passWord = htmlDoc.DocumentNode.SelectNodes("//input[@id='fm-login-password']");
            if (userName != null || passWord != null)
            {
                //需要登录
                LogWrite("解析需要登录，请先登录");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 模拟点击指定选择符DOM元素
        /// </summary>
        /// <param name="selector">js</param>
        private async Task ClickElement(string selector)
        {
            var sSel = $"{selector}.getBoundingClientRect()";
            var t = await webBrowser.EvaluateScriptAsync(sSel);
            if (t.Result != null)
            {
                var expandoDic = t.Result as IDictionary<string, object>;
                if ((null != expandoDic) && expandoDic.ContainsKey("left") && expandoDic.ContainsKey("top"))
                {
                    var left = Convert.ToInt32(expandoDic["left"]) + 5;
                    var top = Convert.ToInt32(expandoDic["top"]) + 5;

                    webBrowser.GetBrowserHost().SendMouseClickEvent(left, top, MouseButtonType.Left, false, 1, CefEventFlags.None);
                    webBrowser.GetBrowserHost().SendMouseClickEvent(left, top, MouseButtonType.Left, true, 1, CefEventFlags.None);
                }
            }
        }

        /// <summary>
        /// 页面加载异步
        /// </summary>
        /// <param name="browser">CefSharp WebBrowser</param>
        /// <returns>Task</returns>
        public Task LoadPageAsync(IWebBrowser browser)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();

            EventHandler<LoadingStateChangedEventArgs> handler = null;
            handler = (sender, args) =>
            {
                if (!args.IsLoading)
                {
                    browser.LoadingStateChanged -= handler;
                    taskCompletionSource.TrySetResultAsync(true);
                }
            };

            browser.LoadingStateChanged += handler;
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 点击下一页
        /// </summary>
        /// <returns>bool</returns>
        public async Task<bool> NextPage()
        {
            var source = await webBrowser.GetSourceAsync();
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(source);
            await Task.Delay(2000);
            var buttonNext = htmlDoc.DocumentNode.Descendants().FirstOrDefault();
            if (buttonNext != null)
            {
                await ClickElement("document.querySelectorAll('[trace=srp_bottom_pagedown]')[0]");
                await AnalysisListPage();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取“IFrame”的Html
        /// </summary>
        /// <returns>string</returns>
        public async Task<Tuple<string, bool>> GetFrameHtml(string frameName, string url)
        {
            string resultStr = string.Empty;
            IFrame frame = null;
            var identifiers = webBrowser.GetBrowser().GetFrameIdentifiers();
            foreach (var i in identifiers)
            {
                frame = webBrowser.GetBrowser().GetFrame(i);

                var htmlId = string.Empty;
                var task = frame.EvaluateScriptAsync(string.Format("document.getElementById('sufei-dialog-content').attributes['id'].value;"));
                task.ContinueWith(t =>
                {
                    if (!t.IsFaulted)
                    {
                        var response = t.Result;
                        if (response.Success == true)
                        {
                            if (response.Result != null)
                            {
                                htmlId = response.Result.ToString();
                            }
                        }
                    }
                }).Wait();

                if (frame.Name == frameName
                    || frame.Url.IndexOf(url) != -1
                    || (!string.IsNullOrWhiteSpace(htmlId) && htmlId == "sufei-dialog-content"))
                {
                    return new Tuple<string, bool>(await frame.GetSourceAsync(), true);
                }
            }
            return new Tuple<string, bool>("", false); ;
        }

        #endregion

        //开始解析数据
        private async void butStartParsing_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtKeyword.Text.Trim())) { MessageBox.Show("请输入需要搜索的关键字。", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            //跳转页面
            string new_searchAddress = string.Format(searchAddress, URLCommon.UrlEncode(txtKeyword.Text.Trim()), 0);
            webBrowser.Load(new_searchAddress);
            while (GetHtmlValue().Trim().ToLower() != "complete".ToLower()) { Application.DoEvents(); };
            await Task.Delay(1000);

            //判断是否需要登录
            var isLogin = await WhetherYouNeedToLogin();
            if (isLogin) { MessageBox.Show("请先登录，才可进行解析。", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            entitySource = new List<TaobaoCrawlerEntity>(); //存储列表页数据信息
            butStartParsing.Enabled = false; //禁用

            LogWrite("_______________________________________________________________");
            LogWrite("正在解析中，请耐心等待");
            LogWrite("按钮已禁用，等待解析完成释放");

            //平台
            string platformCode = string.Empty;
            string platformName = string.Empty;
            if (radioAll.Checked) { platformCode = string.Empty; platformName = string.Empty; }
            else if (radioTaobao.Checked) { platformCode = "taobao"; platformName = "淘宝"; }
            else if (radioTmall.Checked) { platformCode = "tmall"; platformName = "天猫"; }

            //解析数量页数，数据量
            #region > 解析数量页数，数据量
            int analysisPageSize = 0;
            int analysisCount = 0;
            if (string.IsNullOrWhiteSpace(txtAnlyticNumber.Text.Trim()) || Convert.ToInt32(txtAnlyticNumber.Text.Trim()) == 0)
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
            #endregion

            //解析“Html”数据信息
            LogWrite("正在解析第“1”页数据信息");
            await AnalysisListPage();
            for (int i = 2; i <= Convert.ToInt32(txtAnlyticNumber.Text.Trim()); i++)
            {
                string next_searchAddress = string.Format(searchAddress, URLCommon.UrlEncode(txtKeyword.Text.Trim()), 2 * 44);
                webBrowser.Load(next_searchAddress);
                while (GetHtmlValue().Trim().ToLower() != "complete".ToLower()) { Application.DoEvents(); };
                await Task.Delay(1500);
                await NextPage();
                LogWrite($"正在解析第“{i}”页数据信息");
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

            butStartParsing.Enabled = true; //释放按钮禁用
        }

        /// <summary>
        /// 解析列表页面“Html”
        /// </summary>
        /// <returns>bool</returns>
        private async Task AnalysisListPage()
        {
            try
            {
                await Task.Delay(1200);

                var source = await webBrowser.GetSourceAsync();
                if (string.IsNullOrWhiteSpace(source)) { MessageBox.Show("暂无解析到页面信息，请重试。", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

                var document = new HtmlAgilityPack.HtmlDocument();
                document.LoadHtml(source);
                while (GetHtmlValue().Trim().ToLower() != "complete".ToLower()) { Application.DoEvents(); };

                //滑动验证码
                if (await IsDialog())
                {
                    button1_Click(null, null);
                    await Task.Delay(2000);
                }

                //解析相关商品列表  //<div class="item J_MouserOnverReq"
                HtmlNodeCollection goodsNodes = document.DocumentNode.SelectNodes("//div[starts-with(@class,'item J_MouserOnverReq')]");
                if (goodsNodes == null) goodsNodes = document.DocumentNode.SelectNodes("//div[@class='item J_MouserOnverReq']");
                if (goodsNodes == null) { MessageBox.Show("程序解析“Html”错误“//div[starts-with(@class,'item J_MouserOnverReq')]”，请联系作者或者重试。", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

                foreach (var node in goodsNodes)
                {
                    TaobaoCrawlerEntity productEntity = new TaobaoCrawlerEntity();
                    HtmlAgilityPack.HtmlDocument node_document = new HtmlAgilityPack.HtmlDocument();
                    node_document.LoadHtml(node.OuterHtml);

                    //滑动验证码
                    if (await IsDialog())
                    {
                        button1_Click(null, null);
                        await Task.Delay(2000);
                    }

                    #region 解析“Html”

                    #region 列表图片(商品编号 | 商品金额 | 商品详细页地址 | 商品列表页图片地址 | 商品搜索列表页显示的名称) //div[@class='pic']
                    var product_Pic_Nodes = node_document.DocumentNode.SelectNodes("//div[@class='pic']");
                    if (product_Pic_Nodes != null)
                    {
                        var product_Pic_a_node = product_Pic_Nodes[0].SelectSingleNode("//a");
                        if (product_Pic_a_node != null)
                        {
                            //商品编号 - trace-nid
                            productEntity.ProductId = product_Pic_a_node.GetAttributeValue("trace-nid", "");
                            //商品金额 - trace-price
                            productEntity.ProductMoney = product_Pic_a_node.GetAttributeValue("trace-price", "");
                            //商品详细页地址 href
                            string productDetailsAddress = product_Pic_a_node.GetAttributeValue("href", "");
                            productEntity.ProductDetailsAddress = "https:" + productDetailsAddress.Replace("amp;", "");

                            //img 标签
                            var product_Pic_a_img_node = product_Pic_a_node.SelectSingleNode("//img");
                            if (product_Pic_a_img_node != null)
                            {
                                //商品列表页图片地址 data-src
                                var productSearchListImgAddress = product_Pic_a_img_node.GetAttributeValue("data-src", "");
                                productEntity.ProductSearchListImgAddress = "https:" + productSearchListImgAddress;

                                //商品搜索列表页显示的名称
                                productEntity.ProductSearchListName = product_Pic_a_img_node.GetAttributeValue("alt", "");
                            }
                        }
                    }

                    #endregion
                    #region 价格，销量 //div[@class='row row-1 g-clearfix']
                    var productMoneyAndSell_Nodes = node_document.DocumentNode.SelectNodes("//div[@class='row row-1 g-clearfix'] ");
                    if (productMoneyAndSell_Nodes != null)
                    {
                        //销量 //div[@class='deal-cnt']
                        if (productMoneyAndSell_Nodes[0]?.SelectSingleNode("//div[@class='deal-cnt']") != null)
                        {
                            var productSellNumber = productMoneyAndSell_Nodes[0].SelectSingleNode("//div[@class='deal-cnt']")?.InnerText;
                            productEntity.ProductSellNumber = productSellNumber.Replace("人", "").Replace("收货", "").Replace("付款", "");
                        }

                        //金额(二次验证金额) //strong
                        if (productMoneyAndSell_Nodes[0]?.SelectSingleNode("//strong") != null)
                        {
                            var new_ProductMoney = productMoneyAndSell_Nodes[0].SelectSingleNode("//strong")?.InnerText;
                            if (Convert.ToDouble(productEntity.ProductMoney) != Convert.ToDouble(new_ProductMoney))
                                productEntity.ProductMoney = new_ProductMoney;
                        }
                    }

                    #endregion
                    #region 商品显示的名称 //div[@class='row row-2 title']
                    var productName_Nodes = node_document.DocumentNode.SelectNodes("//div[@class='row row-2 title'] ");
                    if (productName_Nodes != null)
                    {
                        //商品在详情显示的名称
                        productEntity.ProductDetailsPageName = productName_Nodes[0].InnerText.Replace("\n", "").Trim();
                        if (productEntity.ProductDetailsPageName != node_document.DocumentNode.SelectNodes("//div[@class='row row-2 title']//a")[0].InnerText.Replace("\n", "").Trim())
                            productEntity.ProductDetailsPageName = node_document.DocumentNode.SelectNodes("//div[@class='row row-2 title']//a")[0].InnerText.Replace("\n", "").Trim();
                    }
                    #endregion
                    #region 店铺名称，地址，归属地 //div[@class='row row-3 g-clearfix']
                    var storeInformation_Nodes = node_document.DocumentNode.SelectNodes("//div[@class='row row-3 g-clearfix'] ");
                    if (storeInformation_Nodes != null)
                    {
                        //店铺归属地址 //div[@class='location']
                        if (storeInformation_Nodes[0]?.SelectSingleNode("//div[@class='location'] ") != null)
                            productEntity.StoreAddress = storeInformation_Nodes[0].SelectSingleNode("//div[@class='location'] ").InnerText;

                        //店铺地址“Url” //div[@class='shop']//a
                        if (storeInformation_Nodes[0].SelectSingleNode("//div[@class='shop']//a") != null)
                            productEntity.ShopNameAddress = "https:" + storeInformation_Nodes[0].SelectSingleNode("//div[@class='shop']//a").GetAttributeValue("href", "");

                        //店铺名称“掌柜” //div[@class='shop']//a//span
                        if (storeInformation_Nodes[0].SelectNodes("//div[@class='shop']//a//span") != null)
                            if (storeInformation_Nodes[0].SelectNodes("//div[@class='shop']//a//span").Count() > 0)
                                productEntity.ShopName = storeInformation_Nodes[0].SelectNodes("//div[@class='shop']//a//span")
                                    [storeInformation_Nodes[0].SelectNodes("//div[@class='shop']//a//span").Count() - 1].InnerText;
                    }

                    #endregion
                    #region 所属平台 //div[@class='row row-4 g-clearfix']
                    HtmlNodeCollection platformCode_Nodes = node_document.DocumentNode.SelectNodes("//div[@class='row row-4 g-clearfix']//div//ul");
                    if (platformCode_Nodes != null)
                    {
                        if (platformCode_Nodes[0]?.SelectNodes("//li//a") != null)
                        {
                            var platformCodeLiNodesHtml = platformCode_Nodes[0].SelectNodes("//li//a")[0].OuterHtml;
                            if (platformCodeLiNodesHtml.IndexOf("taobao") != -1)
                            {
                                productEntity.PlatformCode = "taobao";
                                productEntity.PlatformName = "淘宝";
                            }
                            else if (platformCodeLiNodesHtml.IndexOf("tmall") != -1)
                            {
                                productEntity.PlatformCode = "tmall";
                                productEntity.PlatformName = "天猫";
                            }
                        }
                        else
                        {
                            productEntity.PlatformCode = "taobao";
                            productEntity.PlatformName = "淘宝";
                        }
                    }

                    #endregion

                    #endregion

                    //当前页面地址
                    var currentAddress = webBrowser.Address;

                    #region 解析详情页
                    if (!string.IsNullOrWhiteSpace(productEntity.ProductDetailsAddress))
                        webBrowser.Load(productEntity.ProductDetailsAddress);

                    while (GetHtmlValue().Trim().ToLower() != "complete".ToLower()) { Application.DoEvents(); };
                    //解析详情页
                    var detailsInfo = await AnalysisDetails(productEntity, currentAddress);
                    if (detailsInfo.Item1 != null && detailsInfo.Item2 == true)
                        productEntity.details = detailsInfo.Item1;
                    #endregion

                    entitySource.Add(productEntity);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("解析错误，error:" + ex.Message, "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return;
            }
        }

        /// <summary>
        /// 解析详情页面“Html”
        /// </summary>
        /// <param name="entity">数据信息</param>
        /// <returns>bool</returns>
        private async Task<Tuple<TaobaoCrawlerDetails, bool>> AnalysisDetails(TaobaoCrawlerEntity entity, string currentAddress)
        {
            await Task.Delay(800);
            var taobaoCrawlerDetails = new TaobaoCrawlerDetails();
            if (entity != null)
            {
                var source = await webBrowser.GetSourceAsync();
                if (string.IsNullOrWhiteSpace(source))
                {
                    LogWrite($"“{entity.ProductDetailsAddress}”未解析到相关详细页信息");
                    return new Tuple<TaobaoCrawlerDetails, bool>(null, false);
                }

                var document = new HtmlAgilityPack.HtmlDocument();
                document.LoadHtml(source);
                while (GetHtmlValue().Trim().ToLower() != "complete".ToLower()) { Application.DoEvents(); };

                //验证码识别滑动
                if (await IsDialog())
                {
                    button1_Click(null, null);
                    await Task.Delay(2000);
                }
                #region 解析“Html”

                #region 累计评论，交易成功(淘宝) //div[@class='tb-counter-bd']
                var evaluationOrTrading_Nodes = document.DocumentNode.SelectNodes("//div[@class='tb-counter-bd']");
                if (evaluationOrTrading_Nodes != null)
                {
                    //累计评论 //div[@class='tb-rate-counter']//a//strong[@id='J_RateCounter']
                    var evaluation = evaluationOrTrading_Nodes[0]?.SelectSingleNode("//div[@class='tb-rate-counter']//a//strong[@id='J_RateCounter']");
                    if (evaluation != null)
                        taobaoCrawlerDetails.CumulativeCommentsNum = evaluation?.InnerText;
                    //交易成功 //div[@class='tb-sell-counter']//a//strong[@id='J_SellCounter']
                    var trading = evaluationOrTrading_Nodes[0]?.SelectSingleNode("//div[@class='tb-sell-counter']//a//strong[@id='J_SellCounter']");
                    if (trading != null)
                        taobaoCrawlerDetails.TransactionSuccessNum = trading?.InnerText;
                }
                #endregion
                #region 价格 //div[@class='tb-promo-item-bd']
                var commodityPrices_Nodes = document.DocumentNode.SelectNodes("//div[@class='tb-promo-item-bd']");
                if (commodityPrices_Nodes == null)
                    commodityPrices_Nodes = document.DocumentNode.SelectNodes("//div[@class='tb-property-cont']");
                if (commodityPrices_Nodes != null)
                {
                    var prices = commodityPrices_Nodes[0]?.SelectSingleNode("//strong[@class='tb-promo-price']//em[@id='J_PromoPriceNum']");
                    if (prices == null)
                        prices = commodityPrices_Nodes[0]?.SelectSingleNode("//strong[@id='J_StrPrice']//em[@class='tb-rmb-num']");
                    if (prices != null)
                        taobaoCrawlerDetails.CommodityPrices = prices?.InnerText;
                }

                #endregion
                #region 收藏人气 //li[@class='tb-social-fav']
                var collectTheSentiment_Nodes = document.DocumentNode.SelectNodes("//li[@class='tb-social-fav']");
                if (collectTheSentiment_Nodes != null)
                {
                    taobaoCrawlerDetails.CollectTheSentiment = collectTheSentiment_Nodes[0]?.SelectSingleNode("//a[@class='J_TDialogTrigger']//em")?.InnerText;
                    taobaoCrawlerDetails.CollectTheSentiment = taobaoCrawlerDetails.CollectTheSentiment.Replace("(", "").Replace("人气)", "");
                }

                #endregion

                #region 优惠信息 //div[@class='tb-other-discount']
                List<PreferentialInfo> preferentialList = new List<PreferentialInfo>();
                //div[@class='tb-other-discount']
                var preferentialInfo_Nodes = document.DocumentNode.SelectNodes("//div[@class='tb-other-discount']");
                if (preferentialInfo_Nodes != null)
                {
                    var preferentialOne = preferentialInfo_Nodes[0]?.SelectNodes("//div[starts-with(@class,'tb-other-discount-content')]");
                    if (preferentialOne != null)
                    {
                        foreach (var preferential in preferentialOne)
                        {
                            //div[@class='tb-coupon']
                            var imgNodes = preferential.SelectNodes("//div[@class='tb-coupon']");
                            if (imgNodes != null)   //是否有优惠券
                            {
                                foreach (var img in imgNodes)
                                {
                                    //优惠券icon | 优惠券内容信息
                                    if (img.SelectSingleNode("//img[@class='tb-coupon-icon']") != null)
                                    {
                                        var preferentialContent = string.Empty;
                                        if (!string.IsNullOrWhiteSpace(img.InnerText.Trim()))
                                        {
                                            preferentialContent = img.InnerText.Replace("\n", "").Replace(" ", "");
                                            if (img.InnerText.Replace("\n", "").Replace(" ", "").LastIndexOf("领取") != -1)
                                                preferentialContent = img.InnerText.Replace("\n", "").Replace(" ", "").Substring(0, img.InnerText.Replace("\n", "").Replace(" ", "").LastIndexOf("领取"));
                                        }
                                        preferentialList.Add(new PreferentialInfo()
                                        {
                                            PreferentialIMGAddress = "https:" + img.SelectSingleNode("//img[@class='tb-coupon-icon']").GetAttributeValue("src", ""),
                                            PreferentialContent = preferentialContent
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion

                var new_preferential_source = preferentialList.GroupBy(x =>
                 new { x.PreferentialIMGAddress, x.PreferentialContent }).Select(c => new
                 {
                     Id = c.Key,
                     ListValue = c.ToList()
                 });
                foreach (var info in new_preferential_source)
                {
                    taobaoCrawlerDetails.PreferentialInfo = info.ListValue.ToList<PreferentialInfo>();
                }
                #endregion
            }

            webBrowser.Load(currentAddress);
            while (GetHtmlValue().Trim().ToLower() != "complete".ToLower()) { Application.DoEvents(); };
            await Task.Delay(1000);
            return new Tuple<TaobaoCrawlerDetails, bool>(taobaoCrawlerDetails, true);
        }

        /// <summary>
        /// 是否有模态框验证码
        /// </summary>
        /// <param name="htmlDoc">HtmlDocument</param>
        /// <returns>bool</returns>
        private async Task<bool> IsDialog()
        {
            var source = await webBrowser.GetSourceAsync();
            var htmlDoc = new HtmlAgilityPack.HtmlDocument();
            htmlDoc.LoadHtml(source);
            while (GetHtmlValue().Trim().ToLower() != "complete".ToLower()) { Application.DoEvents(); };

            //验证模态框
            #region 读取验证模态框
            var is_dialog_nodes = false;
            var dialog_nodes = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='sufei-dialog']");
            if (dialog_nodes == null)
                dialog_nodes = htmlDoc.DocumentNode.SelectSingleNode("//div[@id='nocaptcha']");
            if (dialog_nodes == null)
                dialog_nodes = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='wrapper']");

            //上面没有读取到验证码模态框，所以去“Frame”读写模态框
            if (dialog_nodes == null)
            {
                var htmlBody = await GetFrameHtml("<!--dynamicFrame889C6A0361F526C951EACB19C2BC4D31-->", "https://mdskip.taobao.com//core/initItemDetail.htm");
                if (!string.IsNullOrWhiteSpace(htmlBody.Item1) && htmlBody.Item2)
                {
                    is_dialog_nodes = true;
                    await Task.Delay(1200); //休息一下下
                }
            }

            #endregion

            if (dialog_nodes != null || is_dialog_nodes == true)
            {
                try
                {
                    await GetCaptchaPosition(); //绑定事件获取位置坐标
                    LogWrite("正在解析滑动验证码");
                    return true;
                }
                catch (Exception ex) { throw new Exception(ex.Message); }
            }
            else
            {
                return false;
            }
        }




        private async void button1_Click(object sender, EventArgs e)
        {
            var cc = textBox1.Text;

            int int_X = Convert.ToInt32(cc.Split(',')[0]), int_Y = Convert.ToInt32(cc.Split(',')[1]), move_long = 100;
            //Win32Mouse.mouse_event(Win32Mouse.MOUSEEVENTF_LEFTDOWN | Win32Mouse.MOUSEEVENTF_MOVE,
            //    (int_X + 100) * 65536 / 1920, (int_Y + move_long) * 65536 / 1080, 0, 0);
            Win32Mouse.mouse_event(Win32Mouse.MOUSEEVENTF_ABSOLUTE | Win32Mouse.MOUSEEVENTF_LEFTDOWN | Win32Mouse.MOUSEEVENTF_MOVE,
                (int_X + 100) * 65536 / 1920, (int_Y + move_long) * 65536 / 1080, 0, 0);

            await Task.Delay(2000);
            Win32Mouse.MoveMouseToPoint(new Point()
            {
                X = (int_X + 100) * 65536 / 1920,
                Y = 480
            });

            await Task.Delay(3000);
            Win32Mouse.mouse_event(Win32Mouse.MOUSEEVENTF_LEFTUP, (int_X + 100) * 65536 / 1920, (int_Y + move_long) * 65536 / 1080, 0, 0);
        }

        /// <summary>
        /// 获取验证码的位置
        /// </summary>
        /// <returns></returns>
        private async Task GetCaptchaPosition()
        {
            string jsOnClick = @"
                document.getElementById('nc_1_n1z').onclick = function (event) {
                    event = event || window.event;
                    alert (event.clientX + ',' + event.clientY);
                    return event.clientX + ',' + event.clientY;
                }";
            var a = await webBrowser.GetBrowser().GetFrame(webBrowser.GetBrowser().GetFrameNames()[0]).EvaluateScriptAsync(jsOnClick.ToString());
            await Task.Delay(3000);
            JavascriptResponse a1 = null;
            JavascriptResponse a2 = null;
            JavascriptResponse a3 = null;
            if (a.Result == null)
                a1 = await webBrowser.GetBrowser().GetFrame(webBrowser.GetBrowser().GetFrameNames()[1]).EvaluateScriptAsync(jsOnClick.ToString());
            else return;
            if (a1.Result == null)
                a2 = await webBrowser.GetBrowser().GetFrame(webBrowser.GetBrowser().GetFrameNames()[2]).EvaluateScriptAsync(jsOnClick.ToString());
            else return;
            if (a1.Result == null)
                a3 = await webBrowser.GetBrowser().GetFrame(webBrowser.GetBrowser().GetFrameNames()[3]).EvaluateScriptAsync(jsOnClick.ToString());
            else return;

        }

        //导出“Excel”
        private void butExportExcel_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件保存路径";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}_{txtKeyword.Text}爬虫数据信息.xlsx";
                string foldPath = dialog.SelectedPath + $"\\{fileName}";
                if (entitySource?.Count() > 0)
                {
                    List<TaobaoCrawlerEntityToDataTable> dataTableInfos = new List<TaobaoCrawlerEntityToDataTable>();
                    foreach (var info in entitySource)
                    {
                        var PreferentialContentValue = string.Empty;
                        if (info.details?.PreferentialInfo != null)
                        {
                            var arry = (from p in info.details?.PreferentialInfo
                                        select p.PreferentialContent).ToList().ToArray();

                            PreferentialContentValue = System.String.Join(",", arry);
                        }
                        dataTableInfos.Add(new TaobaoCrawlerEntityToDataTable()
                        {
                            CollectTheSentiment = info.details?.CollectTheSentiment,
                            CommodityPrices = info.details?.CommodityPrices,
                            CumulativeCommentsNum = info.details?.CumulativeCommentsNum,
                            TransactionSuccessNum = info.details?.TransactionSuccessNum,
                            PreferentialContent = PreferentialContentValue,
                            PlatformCode = info.PlatformCode,
                            PlatformName = info.PlatformName,

                            ProductDetailsAddress = info.ProductDetailsAddress,
                            ProductDetailsPageName = info.ProductDetailsPageName,
                            ProductId = info.ProductId,
                            ProductMoney = info.ProductMoney,
                            ProductSearchListImgAddress = info.ProductSearchListImgAddress,
                            ProductSearchListName = info.ProductSearchListName,
                            ProductSellNumber = info.ProductSellNumber,
                            ShopName = info.ShopName,
                            ShopNameAddress = info.ShopNameAddress,
                            StoreAddress = info.StoreAddress
                        });
                    }

                    var dataTable = DataTableCommon.ListToDataTable<TaobaoCrawlerEntityToDataTable>(dataTableInfos);
                    string error = string.Empty;
                    var dic = new Dictionary<string, string>();
                    dic.Add("CollectTheSentiment", "CollectTheSentiment");
                    dic.Add("CommodityPrices", "CommodityPrices");
                    dic.Add("CumulativeCommentsNum", "CumulativeCommentsNum");
                    dic.Add("TransactionSuccessNum", "TransactionSuccessNum");
                    dic.Add("PreferentialContent", "PreferentialContent");
                    dic.Add("PlatformCode", "PlatformCode");
                    dic.Add("PlatformName", "PlatformName");
                    dic.Add("ProductDetailsAddress", "ProductDetailsAddress");
                    dic.Add("ProductDetailsPageName", "ProductDetailsPageName");
                    dic.Add("ProductId", "ProductId");
                    dic.Add("ProductMoney", "ProductMoney");
                    dic.Add("ProductSearchListImgAddress", "ProductSearchListImgAddress");
                    dic.Add("ProductSearchListName", "ProductSearchListName");
                    dic.Add("ProductSellNumber", "ProductSellNumber");
                    dic.Add("ShopName", "ShopName");
                    dic.Add("ShopNameAddress", "ShopNameAddress");
                    dic.Add("StoreAddress", "StoreAddress");
                    ExcelCommon.ToExcel(dataTable, foldPath, dic, ref error);
                }
                else
                {
                    MessageBox.Show("暂无导出数据。", "信息提示", MessageBoxButtons.OK, MessageBoxIcon.Information); return;
                }
            }
        }

        private void textBox1_DoubleClick(object sender, EventArgs e)
        {
            webBrowser.ShowDevTools();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            var cc = textBox1.Text;

            int int_X = Convert.ToInt32(cc.Split(',')[0]), int_Y = Convert.ToInt32(cc.Split(',')[1]), move_long = 100;
            //Win32Mouse.mouse_event(Win32Mouse.MOUSEEVENTF_LEFTDOWN | Win32Mouse.MOUSEEVENTF_MOVE,
            //    (int_X + 100) * 65536 / 1920, (int_Y + move_long) * 65536 / 1080, 0, 0);
            Win32Mouse.mouse_event(Win32Mouse.MOUSEEVENTF_ABSOLUTE | Win32Mouse.MOUSEEVENTF_LEFTDOWN | Win32Mouse.MOUSEEVENTF_MOVE,
                (int_X + 100) * 65536 / 1920, (int_Y + move_long) * 65536 / 1080, 0, 0);

            await Task.Delay(2000);
            Win32Mouse.MoveMouseToPoint(new Point()
            {
                X = (int_X + 100) * 65536 / 1920,
                Y = 480
            });
            await Task.Delay(3000);
            Win32Mouse.mouse_event(Win32Mouse.MOUSEEVENTF_LEFTUP, (int_X + 100) * 65536 / 1920, (int_Y + move_long) * 65536 / 1080, 0, 0);
        }
    }
}
