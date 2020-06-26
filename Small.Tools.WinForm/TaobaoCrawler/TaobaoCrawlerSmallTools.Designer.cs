using CefSharp.WinForms;

namespace Small.Tools.WinForm
{
    partial class TaobaoCrawlerSmallTools
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.panel_middle = new System.Windows.Forms.Panel();
            this.panel_right = new System.Windows.Forms.Panel();
            this.panel_left_middle = new System.Windows.Forms.Panel();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.txtAnlyticNumber = new System.Windows.Forms.TextBox();
            this.labAnlytic = new System.Windows.Forms.Label();
            this.butLogin = new System.Windows.Forms.Button();
            this.labTheDivider = new System.Windows.Forms.Label();
            this.labPassWord = new System.Windows.Forms.Label();
            this.labUserName = new System.Windows.Forms.Label();
            this.radioAll = new System.Windows.Forms.RadioButton();
            this.radioTaobao = new System.Windows.Forms.RadioButton();
            this.radioTmall = new System.Windows.Forms.RadioButton();
            this.butExportExcel = new System.Windows.Forms.Button();
            this.butStartParsing = new System.Windows.Forms.Button();
            this.txtSales = new System.Windows.Forms.TextBox();
            this.txtPassWord = new System.Windows.Forms.TextBox();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.txtKeyword = new System.Windows.Forms.TextBox();
            this.labPlatform = new System.Windows.Forms.Label();
            this.labSales = new System.Windows.Forms.Label();
            this.labKeyword = new System.Windows.Forms.Label();
            this.panel_left = new System.Windows.Forms.Panel();
            this.listBox_LogOutput = new System.Windows.Forms.ListBox();
            this.panel_bottom = new System.Windows.Forms.Panel();
            this.panel_middle.SuspendLayout();
            this.panel_left_middle.SuspendLayout();
            this.panel_left.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel_middle
            // 
            this.panel_middle.Controls.Add(this.panel_right);
            this.panel_middle.Controls.Add(this.panel_left_middle);
            this.panel_middle.Controls.Add(this.panel_left);
            this.panel_middle.Controls.Add(this.panel_bottom);
            this.panel_middle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_middle.Location = new System.Drawing.Point(0, 0);
            this.panel_middle.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.panel_middle.Name = "panel_middle";
            this.panel_middle.Size = new System.Drawing.Size(1296, 746);
            this.panel_middle.TabIndex = 0;
            // 
            // panel_right
            // 
            this.panel_right.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_right.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_right.Location = new System.Drawing.Point(987, 0);
            this.panel_right.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.panel_right.Name = "panel_right";
            this.panel_right.Size = new System.Drawing.Size(309, 655);
            this.panel_right.TabIndex = 3;
            // 
            // panel_left_middle
            // 
            this.panel_left_middle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_left_middle.Controls.Add(this.button2);
            this.panel_left_middle.Controls.Add(this.textBox1);
            this.panel_left_middle.Controls.Add(this.button1);
            this.panel_left_middle.Controls.Add(this.txtAnlyticNumber);
            this.panel_left_middle.Controls.Add(this.labAnlytic);
            this.panel_left_middle.Controls.Add(this.butLogin);
            this.panel_left_middle.Controls.Add(this.labTheDivider);
            this.panel_left_middle.Controls.Add(this.labPassWord);
            this.panel_left_middle.Controls.Add(this.labUserName);
            this.panel_left_middle.Controls.Add(this.radioAll);
            this.panel_left_middle.Controls.Add(this.radioTaobao);
            this.panel_left_middle.Controls.Add(this.radioTmall);
            this.panel_left_middle.Controls.Add(this.butExportExcel);
            this.panel_left_middle.Controls.Add(this.butStartParsing);
            this.panel_left_middle.Controls.Add(this.txtSales);
            this.panel_left_middle.Controls.Add(this.txtPassWord);
            this.panel_left_middle.Controls.Add(this.txtUserName);
            this.panel_left_middle.Controls.Add(this.txtKeyword);
            this.panel_left_middle.Controls.Add(this.labPlatform);
            this.panel_left_middle.Controls.Add(this.labSales);
            this.panel_left_middle.Controls.Add(this.labKeyword);
            this.panel_left_middle.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel_left_middle.Location = new System.Drawing.Point(508, 0);
            this.panel_left_middle.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.panel_left_middle.Name = "panel_left_middle";
            this.panel_left_middle.Size = new System.Drawing.Size(479, 655);
            this.panel_left_middle.TabIndex = 2;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(270, 564);
            this.button2.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(138, 40);
            this.button2.TabIndex = 13;
            this.button2.Text = "测试";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(75, 492);
            this.textBox1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(180, 31);
            this.textBox1.TabIndex = 12;
            this.textBox1.Text = "960,480";
            this.textBox1.DoubleClick += new System.EventHandler(this.textBox1_DoubleClick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(270, 488);
            this.button1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(138, 40);
            this.button1.TabIndex = 11;
            this.button1.Text = "button1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtAnlyticNumber
            // 
            this.txtAnlyticNumber.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtAnlyticNumber.Location = new System.Drawing.Point(134, 359);
            this.txtAnlyticNumber.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.txtAnlyticNumber.Name = "txtAnlyticNumber";
            this.txtAnlyticNumber.Size = new System.Drawing.Size(329, 35);
            this.txtAnlyticNumber.TabIndex = 10;
            this.txtAnlyticNumber.Text = "5";
            this.txtAnlyticNumber.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSales_KeyPress);
            // 
            // labAnlytic
            // 
            this.labAnlytic.AutoSize = true;
            this.labAnlytic.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labAnlytic.Location = new System.Drawing.Point(6, 366);
            this.labAnlytic.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.labAnlytic.Name = "labAnlytic";
            this.labAnlytic.Size = new System.Drawing.Size(117, 28);
            this.labAnlytic.TabIndex = 9;
            this.labAnlytic.Text = "解析页数：";
            // 
            // butLogin
            // 
            this.butLogin.Cursor = System.Windows.Forms.Cursors.Hand;
            this.butLogin.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.butLogin.Location = new System.Drawing.Point(326, 114);
            this.butLogin.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.butLogin.Name = "butLogin";
            this.butLogin.Size = new System.Drawing.Size(138, 40);
            this.butLogin.TabIndex = 8;
            this.butLogin.Text = "登 录";
            this.butLogin.UseVisualStyleBackColor = true;
            this.butLogin.Click += new System.EventHandler(this.butLogin_Click);
            // 
            // labTheDivider
            // 
            this.labTheDivider.AutoSize = true;
            this.labTheDivider.Location = new System.Drawing.Point(-7, 156);
            this.labTheDivider.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.labTheDivider.Name = "labTheDivider";
            this.labTheDivider.Size = new System.Drawing.Size(527, 21);
            this.labTheDivider.TabIndex = 7;
            this.labTheDivider.Text = "_______________________________________________";
            // 
            // labPassWord
            // 
            this.labPassWord.AutoSize = true;
            this.labPassWord.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labPassWord.Location = new System.Drawing.Point(48, 70);
            this.labPassWord.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.labPassWord.Name = "labPassWord";
            this.labPassWord.Size = new System.Drawing.Size(75, 28);
            this.labPassWord.TabIndex = 6;
            this.labPassWord.Text = "密码：";
            // 
            // labUserName
            // 
            this.labUserName.AutoSize = true;
            this.labUserName.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labUserName.Location = new System.Drawing.Point(26, 23);
            this.labUserName.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.labUserName.Name = "labUserName";
            this.labUserName.Size = new System.Drawing.Size(96, 28);
            this.labUserName.TabIndex = 5;
            this.labUserName.Text = "用户名：";
            // 
            // radioAll
            // 
            this.radioAll.AutoSize = true;
            this.radioAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.radioAll.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radioAll.Location = new System.Drawing.Point(141, 304);
            this.radioAll.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.radioAll.Name = "radioAll";
            this.radioAll.Size = new System.Drawing.Size(85, 32);
            this.radioAll.TabIndex = 4;
            this.radioAll.Text = "全 部";
            this.radioAll.UseVisualStyleBackColor = true;
            // 
            // radioTaobao
            // 
            this.radioTaobao.AutoSize = true;
            this.radioTaobao.Checked = true;
            this.radioTaobao.Cursor = System.Windows.Forms.Cursors.Hand;
            this.radioTaobao.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.radioTaobao.Location = new System.Drawing.Point(248, 304);
            this.radioTaobao.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.radioTaobao.Name = "radioTaobao";
            this.radioTaobao.Size = new System.Drawing.Size(85, 32);
            this.radioTaobao.TabIndex = 4;
            this.radioTaobao.TabStop = true;
            this.radioTaobao.Text = "淘 宝";
            this.radioTaobao.UseVisualStyleBackColor = true;
            // 
            // radioTmall
            // 
            this.radioTmall.AutoSize = true;
            this.radioTmall.Cursor = System.Windows.Forms.Cursors.Hand;
            this.radioTmall.Location = new System.Drawing.Point(361, 308);
            this.radioTmall.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.radioTmall.Name = "radioTmall";
            this.radioTmall.Size = new System.Drawing.Size(88, 25);
            this.radioTmall.TabIndex = 4;
            this.radioTmall.Text = "天 猫";
            this.radioTmall.UseVisualStyleBackColor = true;
            // 
            // butExportExcel
            // 
            this.butExportExcel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.butExportExcel.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.butExportExcel.Location = new System.Drawing.Point(103, 425);
            this.butExportExcel.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.butExportExcel.Name = "butExportExcel";
            this.butExportExcel.Size = new System.Drawing.Size(211, 40);
            this.butExportExcel.TabIndex = 3;
            this.butExportExcel.Text = "导出“Excel”";
            this.butExportExcel.UseVisualStyleBackColor = true;
            this.butExportExcel.Click += new System.EventHandler(this.butExportExcel_Click);
            // 
            // butStartParsing
            // 
            this.butStartParsing.Cursor = System.Windows.Forms.Cursors.Hand;
            this.butStartParsing.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.butStartParsing.Location = new System.Drawing.Point(325, 425);
            this.butStartParsing.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.butStartParsing.Name = "butStartParsing";
            this.butStartParsing.Size = new System.Drawing.Size(138, 40);
            this.butStartParsing.TabIndex = 3;
            this.butStartParsing.Text = "开始解析";
            this.butStartParsing.UseVisualStyleBackColor = true;
            this.butStartParsing.Click += new System.EventHandler(this.butStartParsing_Click);
            // 
            // txtSales
            // 
            this.txtSales.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtSales.Location = new System.Drawing.Point(136, 254);
            this.txtSales.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.txtSales.Name = "txtSales";
            this.txtSales.Size = new System.Drawing.Size(329, 35);
            this.txtSales.TabIndex = 2;
            this.txtSales.Text = "20";
            this.txtSales.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSales_KeyPress);
            // 
            // txtPassWord
            // 
            this.txtPassWord.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtPassWord.Location = new System.Drawing.Point(136, 65);
            this.txtPassWord.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.txtPassWord.Name = "txtPassWord";
            this.txtPassWord.PasswordChar = '*';
            this.txtPassWord.Size = new System.Drawing.Size(329, 35);
            this.txtPassWord.TabIndex = 2;
            this.txtPassWord.Text = "HUANGsl1995";
            this.txtPassWord.TextChanged += new System.EventHandler(this.txtKeyword_TextChanged);
            // 
            // txtUserName
            // 
            this.txtUserName.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtUserName.Location = new System.Drawing.Point(136, 18);
            this.txtUserName.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(329, 35);
            this.txtUserName.TabIndex = 2;
            this.txtUserName.Text = "木木木子1995";
            this.txtUserName.TextChanged += new System.EventHandler(this.txtKeyword_TextChanged);
            // 
            // txtKeyword
            // 
            this.txtKeyword.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.txtKeyword.Location = new System.Drawing.Point(136, 203);
            this.txtKeyword.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.txtKeyword.Name = "txtKeyword";
            this.txtKeyword.Size = new System.Drawing.Size(329, 35);
            this.txtKeyword.TabIndex = 2;
            this.txtKeyword.TextChanged += new System.EventHandler(this.txtKeyword_TextChanged);
            // 
            // labPlatform
            // 
            this.labPlatform.AutoSize = true;
            this.labPlatform.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labPlatform.Location = new System.Drawing.Point(50, 308);
            this.labPlatform.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.labPlatform.Name = "labPlatform";
            this.labPlatform.Size = new System.Drawing.Size(75, 28);
            this.labPlatform.TabIndex = 1;
            this.labPlatform.Text = "平台：";
            // 
            // labSales
            // 
            this.labSales.AutoSize = true;
            this.labSales.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labSales.Location = new System.Drawing.Point(28, 261);
            this.labSales.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.labSales.Name = "labSales";
            this.labSales.Size = new System.Drawing.Size(96, 28);
            this.labSales.TabIndex = 1;
            this.labSales.Text = "销售量：";
            // 
            // labKeyword
            // 
            this.labKeyword.AutoSize = true;
            this.labKeyword.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.labKeyword.Location = new System.Drawing.Point(28, 210);
            this.labKeyword.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.labKeyword.Name = "labKeyword";
            this.labKeyword.Size = new System.Drawing.Size(96, 28);
            this.labKeyword.TabIndex = 0;
            this.labKeyword.Text = "关键字：";
            // 
            // panel_left
            // 
            this.panel_left.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_left.Controls.Add(this.listBox_LogOutput);
            this.panel_left.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel_left.Location = new System.Drawing.Point(0, 0);
            this.panel_left.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.panel_left.Name = "panel_left";
            this.panel_left.Size = new System.Drawing.Size(508, 655);
            this.panel_left.TabIndex = 1;
            // 
            // listBox_LogOutput
            // 
            this.listBox_LogOutput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(232)))), ((int)(((byte)(207)))));
            this.listBox_LogOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox_LogOutput.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.listBox_LogOutput.ForeColor = System.Drawing.Color.Black;
            this.listBox_LogOutput.FormattingEnabled = true;
            this.listBox_LogOutput.ItemHeight = 28;
            this.listBox_LogOutput.Location = new System.Drawing.Point(0, 0);
            this.listBox_LogOutput.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.listBox_LogOutput.Name = "listBox_LogOutput";
            this.listBox_LogOutput.Size = new System.Drawing.Size(506, 653);
            this.listBox_LogOutput.TabIndex = 0;
            // 
            // panel_bottom
            // 
            this.panel_bottom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel_bottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel_bottom.Location = new System.Drawing.Point(0, 655);
            this.panel_bottom.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.panel_bottom.Name = "panel_bottom";
            this.panel_bottom.Size = new System.Drawing.Size(1296, 91);
            this.panel_bottom.TabIndex = 0;
            // 
            // TaobaoCrawlerSmallTools
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1296, 746);
            this.Controls.Add(this.panel_middle);
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.Name = "TaobaoCrawlerSmallTools";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Taobao Crawler Small Tools";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TaobaoCrawlerSmallTools_FormClosing);
            this.Load += new System.EventHandler(this.TaobaoCrawlerSmallTools_Load);
            this.panel_middle.ResumeLayout(false);
            this.panel_left_middle.ResumeLayout(false);
            this.panel_left_middle.PerformLayout();
            this.panel_left.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel_middle;
        private System.Windows.Forms.Panel panel_bottom;
        private System.Windows.Forms.Panel panel_right;
        private System.Windows.Forms.Panel panel_left_middle;
        private System.Windows.Forms.Panel panel_left;
        private System.Windows.Forms.TextBox txtKeyword;
        private System.Windows.Forms.Label labSales;
        private System.Windows.Forms.Label labKeyword;
        private System.Windows.Forms.TextBox txtSales;
        private System.Windows.Forms.RadioButton radioTaobao;
        private System.Windows.Forms.RadioButton radioTmall;
        private System.Windows.Forms.Button butStartParsing;
        private System.Windows.Forms.Label labPlatform;
        //private System.Windows.Forms.WebBrowser webBrowser;
        //private Small.Tools.WinForm.SmallToolsWebBrowser webBrowser;
        private System.Windows.Forms.Label labTheDivider;
        private System.Windows.Forms.Label labPassWord;
        private System.Windows.Forms.Label labUserName;
        private System.Windows.Forms.TextBox txtPassWord;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.Button butLogin;
        private System.Windows.Forms.RadioButton radioAll;
        private System.Windows.Forms.TextBox txtAnlyticNumber;
        private System.Windows.Forms.Label labAnlytic;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button butExportExcel;
        private System.Windows.Forms.ListBox listBox_LogOutput;
        private System.Windows.Forms.Button button2;
    }
}

