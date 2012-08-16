namespace ASCOM.NexStar
{
    partial class SetupDialogForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cmdOK = new System.Windows.Forms.Button();
            this.cmdCancel = new System.Windows.Forms.Button();
            this.picASCOM = new System.Windows.Forms.PictureBox();
            this.label_port = new System.Windows.Forms.Label();
            this.text_lat = new System.Windows.Forms.TextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tab_scope = new System.Windows.Forms.TabPage();
            this.label4 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.label_align = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.text_foc_len = new System.Windows.Forms.TextBox();
            this.label_focal_len = new System.Windows.Forms.Label();
            this.text_ap_area = new System.Windows.Forms.TextBox();
            this.text_ap_dia = new System.Windows.Forms.TextBox();
            this.lable_aapature_area = new System.Windows.Forms.Label();
            this.label_apature_dia = new System.Windows.Forms.Label();
            this.tab_site = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.text_lst = new System.Windows.Forms.TextBox();
            this.label_lst = new System.Windows.Forms.Label();
            this.lable_evel = new System.Windows.Forms.Label();
            this.lable_long = new System.Windows.Forms.Label();
            this.lable_lat = new System.Windows.Forms.Label();
            this.text_evel = new System.Windows.Forms.TextBox();
            this.text_long = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tab_scope.SuspendLayout();
            this.tab_site.SuspendLayout();
            this.SuspendLayout();
            // 
            // cmdOK
            // 
            this.cmdOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.cmdOK.Location = new System.Drawing.Point(282, 118);
            this.cmdOK.Name = "cmdOK";
            this.cmdOK.Size = new System.Drawing.Size(59, 24);
            this.cmdOK.TabIndex = 9;
            this.cmdOK.Text = "OK";
            this.cmdOK.UseVisualStyleBackColor = true;
            this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // cmdCancel
            // 
            this.cmdCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cmdCancel.Location = new System.Drawing.Point(282, 151);
            this.cmdCancel.Name = "cmdCancel";
            this.cmdCancel.Size = new System.Drawing.Size(59, 25);
            this.cmdCancel.TabIndex = 10;
            this.cmdCancel.Text = "Cancel";
            this.cmdCancel.UseVisualStyleBackColor = true;
            this.cmdCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // picASCOM
            // 
            this.picASCOM.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picASCOM.Cursor = System.Windows.Forms.Cursors.Hand;
            this.picASCOM.Image = global::ASCOM.NexStar.Properties.Resources.ASCOM;
            this.picASCOM.Location = new System.Drawing.Point(292, 9);
            this.picASCOM.Name = "picASCOM";
            this.picASCOM.Size = new System.Drawing.Size(48, 56);
            this.picASCOM.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picASCOM.TabIndex = 3;
            this.picASCOM.TabStop = false;
            this.picASCOM.Click += new System.EventHandler(this.BrowseToAscom);
            this.picASCOM.DoubleClick += new System.EventHandler(this.BrowseToAscom);
            // 
            // label_port
            // 
            this.label_port.AutoSize = true;
            this.label_port.Location = new System.Drawing.Point(3, 8);
            this.label_port.Name = "label_port";
            this.label_port.Size = new System.Drawing.Size(58, 13);
            this.label_port.TabIndex = 0;
            this.label_port.Text = "Comm Port";
            // 
            // text_lat
            // 
            this.text_lat.Location = new System.Drawing.Point(6, 25);
            this.text_lat.Name = "text_lat";
            this.text_lat.Size = new System.Drawing.Size(120, 20);
            this.text_lat.TabIndex = 1;
            this.text_lat.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.text_lat_KeyPress);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tab_scope);
            this.tabControl1.Controls.Add(this.tab_site);
            this.tabControl1.Location = new System.Drawing.Point(6, 9);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(270, 170);
            this.tabControl1.TabIndex = 0;
            // 
            // tab_scope
            // 
            this.tab_scope.BackColor = System.Drawing.SystemColors.Control;
            this.tab_scope.Controls.Add(this.label4);
            this.tab_scope.Controls.Add(this.checkBox1);
            this.tab_scope.Controls.Add(this.label2);
            this.tab_scope.Controls.Add(this.label1);
            this.tab_scope.Controls.Add(this.comboBox2);
            this.tab_scope.Controls.Add(this.label_align);
            this.tab_scope.Controls.Add(this.comboBox1);
            this.tab_scope.Controls.Add(this.text_foc_len);
            this.tab_scope.Controls.Add(this.label_focal_len);
            this.tab_scope.Controls.Add(this.text_ap_area);
            this.tab_scope.Controls.Add(this.text_ap_dia);
            this.tab_scope.Controls.Add(this.lable_aapature_area);
            this.tab_scope.Controls.Add(this.label_apature_dia);
            this.tab_scope.Controls.Add(this.label_port);
            this.tab_scope.Location = new System.Drawing.Point(4, 22);
            this.tab_scope.Name = "tab_scope";
            this.tab_scope.Padding = new System.Windows.Forms.Padding(3);
            this.tab_scope.Size = new System.Drawing.Size(262, 144);
            this.tab_scope.TabIndex = 1;
            this.tab_scope.Text = "Scope";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(72, 110);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 13);
            this.label4.TabIndex = 9;
            this.label4.Text = " mm²";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(150, 109);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(83, 17);
            this.checkBox1.TabIndex = 8;
            this.checkBox1.Text = "Enable PEC";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(216, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(23, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "mm";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(72, 69);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(23, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "mm";
            // 
            // comboBox2
            // 
            this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(150, 68);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(66, 21);
            this.comboBox2.TabIndex = 5;
            // 
            // label_align
            // 
            this.label_align.AutoSize = true;
            this.label_align.Location = new System.Drawing.Point(148, 51);
            this.label_align.Name = "label_align";
            this.label_align.Size = new System.Drawing.Size(53, 13);
            this.label_align.TabIndex = 0;
            this.label_align.Text = "Alignment";
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(6, 25);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(66, 21);
            this.comboBox1.TabIndex = 1;
            // 
            // text_foc_len
            // 
            this.text_foc_len.Location = new System.Drawing.Point(150, 25);
            this.text_foc_len.Name = "text_foc_len";
            this.text_foc_len.Size = new System.Drawing.Size(66, 20);
            this.text_foc_len.TabIndex = 4;
            this.text_foc_len.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.text_foc_len_KeyPress);
            // 
            // label_focal_len
            // 
            this.label_focal_len.AutoSize = true;
            this.label_focal_len.Location = new System.Drawing.Point(148, 8);
            this.label_focal_len.Name = "label_focal_len";
            this.label_focal_len.Size = new System.Drawing.Size(69, 13);
            this.label_focal_len.TabIndex = 0;
            this.label_focal_len.Text = "Focal Length";
            // 
            // text_ap_area
            // 
            this.text_ap_area.Location = new System.Drawing.Point(6, 109);
            this.text_ap_area.Name = "text_ap_area";
            this.text_ap_area.Size = new System.Drawing.Size(66, 20);
            this.text_ap_area.TabIndex = 3;
            this.text_ap_area.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.text_ap_area_KeyPress);
            // 
            // text_ap_dia
            // 
            this.text_ap_dia.Location = new System.Drawing.Point(6, 68);
            this.text_ap_dia.Name = "text_ap_dia";
            this.text_ap_dia.Size = new System.Drawing.Size(66, 20);
            this.text_ap_dia.TabIndex = 2;
            this.text_ap_dia.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.text_ap_dia_KeyPress);
            this.text_ap_dia.Leave += new System.EventHandler(this.text_ap_dia_Leave);
            // 
            // lable_aapature_area
            // 
            this.lable_aapature_area.AutoSize = true;
            this.lable_aapature_area.Location = new System.Drawing.Point(3, 92);
            this.lable_aapature_area.Name = "lable_aapature_area";
            this.lable_aapature_area.Size = new System.Drawing.Size(72, 13);
            this.lable_aapature_area.TabIndex = 0;
            this.lable_aapature_area.Text = "Aperture Area";
            // 
            // label_apature_dia
            // 
            this.label_apature_dia.AutoSize = true;
            this.label_apature_dia.Location = new System.Drawing.Point(3, 51);
            this.label_apature_dia.Name = "label_apature_dia";
            this.label_apature_dia.Size = new System.Drawing.Size(69, 13);
            this.label_apature_dia.TabIndex = 0;
            this.label_apature_dia.Text = "Aperture Dia.";
            // 
            // tab_site
            // 
            this.tab_site.BackColor = System.Drawing.SystemColors.Control;
            this.tab_site.Controls.Add(this.label3);
            this.tab_site.Controls.Add(this.text_lst);
            this.tab_site.Controls.Add(this.label_lst);
            this.tab_site.Controls.Add(this.lable_evel);
            this.tab_site.Controls.Add(this.lable_long);
            this.tab_site.Controls.Add(this.lable_lat);
            this.tab_site.Controls.Add(this.text_evel);
            this.tab_site.Controls.Add(this.text_long);
            this.tab_site.Controls.Add(this.text_lat);
            this.tab_site.Location = new System.Drawing.Point(4, 22);
            this.tab_site.Name = "tab_site";
            this.tab_site.Padding = new System.Windows.Forms.Padding(3);
            this.tab_site.Size = new System.Drawing.Size(262, 144);
            this.tab_site.TabIndex = 0;
            this.tab_site.Text = "Site";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(72, 110);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(15, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "m";
            // 
            // text_lst
            // 
            this.text_lst.BackColor = System.Drawing.SystemColors.Window;
            this.text_lst.Location = new System.Drawing.Point(150, 25);
            this.text_lst.Name = "text_lst";
            this.text_lst.ReadOnly = true;
            this.text_lst.Size = new System.Drawing.Size(60, 20);
            this.text_lst.TabIndex = 4;
            this.text_lst.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label_lst
            // 
            this.label_lst.AutoSize = true;
            this.label_lst.Location = new System.Drawing.Point(148, 8);
            this.label_lst.Name = "label_lst";
            this.label_lst.Size = new System.Drawing.Size(27, 13);
            this.label_lst.TabIndex = 0;
            this.label_lst.Text = "LST";
            // 
            // lable_evel
            // 
            this.lable_evel.AutoSize = true;
            this.lable_evel.Location = new System.Drawing.Point(3, 92);
            this.lable_evel.Name = "lable_evel";
            this.lable_evel.Size = new System.Drawing.Size(51, 13);
            this.lable_evel.TabIndex = 0;
            this.lable_evel.Text = "Elevation";
            // 
            // lable_long
            // 
            this.lable_long.AutoSize = true;
            this.lable_long.Location = new System.Drawing.Point(3, 51);
            this.lable_long.Name = "lable_long";
            this.lable_long.Size = new System.Drawing.Size(54, 13);
            this.lable_long.TabIndex = 0;
            this.lable_long.Text = "Longitude";
            // 
            // lable_lat
            // 
            this.lable_lat.AutoSize = true;
            this.lable_lat.Location = new System.Drawing.Point(3, 8);
            this.lable_lat.Margin = new System.Windows.Forms.Padding(0);
            this.lable_lat.Name = "lable_lat";
            this.lable_lat.Size = new System.Drawing.Size(45, 13);
            this.lable_lat.TabIndex = 0;
            this.lable_lat.Text = "Latitude";
            // 
            // text_evel
            // 
            this.text_evel.Location = new System.Drawing.Point(6, 109);
            this.text_evel.Name = "text_evel";
            this.text_evel.Size = new System.Drawing.Size(66, 20);
            this.text_evel.TabIndex = 3;
            this.text_evel.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.text_evel_KeyPress);
            // 
            // text_long
            // 
            this.text_long.Location = new System.Drawing.Point(6, 68);
            this.text_long.Name = "text_long";
            this.text_long.Size = new System.Drawing.Size(120, 20);
            this.text_long.TabIndex = 2;
            this.text_long.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.text_long_KeyPress);
            // 
            // SetupDialogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(350, 187);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.picASCOM);
            this.Controls.Add(this.cmdCancel);
            this.Controls.Add(this.cmdOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SetupDialogForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NexStar Setup";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SetupDialogForm_FormClosing);
            this.Load += new System.EventHandler(this.SetupDialogForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picASCOM)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tab_scope.ResumeLayout(false);
            this.tab_scope.PerformLayout();
            this.tab_site.ResumeLayout(false);
            this.tab_site.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdOK;
        private System.Windows.Forms.Button cmdCancel;
        private System.Windows.Forms.PictureBox picASCOM;
        private System.Windows.Forms.Label label_port;
        private System.Windows.Forms.TextBox text_lat;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tab_site;
        private System.Windows.Forms.Label lable_evel;
        private System.Windows.Forms.Label lable_long;
        private System.Windows.Forms.Label lable_lat;
        private System.Windows.Forms.TextBox text_evel;
        private System.Windows.Forms.TextBox text_long;
        private System.Windows.Forms.TabPage tab_scope;
        private System.Windows.Forms.TextBox text_foc_len;
        private System.Windows.Forms.Label label_focal_len;
        private System.Windows.Forms.TextBox text_ap_area;
        private System.Windows.Forms.TextBox text_ap_dia;
        private System.Windows.Forms.Label lable_aapature_area;
        private System.Windows.Forms.Label label_apature_dia;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.TextBox text_lst;
        private System.Windows.Forms.Label label_lst;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.Label label_align;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
    }
}