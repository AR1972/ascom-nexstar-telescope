namespace ASCOM.NexStar
{
    partial class HandController
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HandController));
            this.button_up = new System.Windows.Forms.Button();
            this.button_center = new System.Windows.Forms.Button();
            this.button_down = new System.Windows.Forms.Button();
            this.button_left = new System.Windows.Forms.Button();
            this.button_right = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.comboBox_rate = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // button_up
            // 
            this.button_up.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.button_up.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.button_up.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.button_up.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.button_up.ForeColor = System.Drawing.SystemColors.ControlText;
            this.button_up.Image = ((System.Drawing.Image)(resources.GetObject("button_up.Image")));
            this.button_up.Location = new System.Drawing.Point(48, 4);
            this.button_up.Name = "button_up";
            this.button_up.Size = new System.Drawing.Size(30, 30);
            this.button_up.TabIndex = 0;
            this.button_up.UseVisualStyleBackColor = false;
            this.button_up.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_up_MouseDown);
            this.button_up.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_up_MouseUp);
            // 
            // button_center
            // 
            this.button_center.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.button_center.Image = ((System.Drawing.Image)(resources.GetObject("button_center.Image")));
            this.button_center.Location = new System.Drawing.Point(48, 40);
            this.button_center.Name = "button_center";
            this.button_center.Size = new System.Drawing.Size(30, 30);
            this.button_center.TabIndex = 1;
            this.button_center.UseVisualStyleBackColor = false;
            this.button_center.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_center_MouseDown);
            this.button_center.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_center_MouseUp);
            // 
            // button_down
            // 
            this.button_down.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.button_down.Image = ((System.Drawing.Image)(resources.GetObject("button_down.Image")));
            this.button_down.Location = new System.Drawing.Point(48, 76);
            this.button_down.Name = "button_down";
            this.button_down.Size = new System.Drawing.Size(30, 30);
            this.button_down.TabIndex = 2;
            this.button_down.UseVisualStyleBackColor = false;
            this.button_down.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_down_MouseDown);
            this.button_down.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_down_MouseUp);
            // 
            // button_left
            // 
            this.button_left.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.button_left.Image = ((System.Drawing.Image)(resources.GetObject("button_left.Image")));
            this.button_left.Location = new System.Drawing.Point(12, 41);
            this.button_left.Name = "button_left";
            this.button_left.Size = new System.Drawing.Size(30, 30);
            this.button_left.TabIndex = 3;
            this.button_left.UseVisualStyleBackColor = false;
            this.button_left.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_left_MouseDown);
            this.button_left.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_left_MouseUp);
            // 
            // button_right
            // 
            this.button_right.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.button_right.Image = ((System.Drawing.Image)(resources.GetObject("button_right.Image")));
            this.button_right.Location = new System.Drawing.Point(84, 41);
            this.button_right.Name = "button_right";
            this.button_right.Size = new System.Drawing.Size(30, 30);
            this.button_right.TabIndex = 4;
            this.button_right.UseVisualStyleBackColor = false;
            this.button_right.MouseDown += new System.Windows.Forms.MouseEventHandler(this.button_right_MouseDown);
            this.button_right.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_right_MouseUp);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(93, 4);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(21, 23);
            this.button6.TabIndex = 5;
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Visible = false;
            // 
            // comboBox_rate
            // 
            this.comboBox_rate.BackColor = System.Drawing.SystemColors.Window;
            this.comboBox_rate.FormattingEnabled = true;
            this.comboBox_rate.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9"});
            this.comboBox_rate.Location = new System.Drawing.Point(12, 118);
            this.comboBox_rate.Name = "comboBox_rate";
            this.comboBox_rate.Size = new System.Drawing.Size(36, 21);
            this.comboBox_rate.TabIndex = 6;
            this.comboBox_rate.SelectedIndexChanged += new System.EventHandler(this.comboBox_rate_SelectedIndexChanged);
            this.comboBox_rate.DropDownClosed += new System.EventHandler(this.comboBox_rate_DropDownClosed);
            // 
            // HandController
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(130, 152);
            this.ControlBox = false;
            this.Controls.Add(this.comboBox_rate);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.button_right);
            this.Controls.Add(this.button_left);
            this.Controls.Add(this.button_down);
            this.Controls.Add(this.button_center);
            this.Controls.Add(this.button_up);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "HandController";
            this.ShowInTaskbar = false;
            this.Text = "HandController";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HandController_FormClosing);
            this.Load += new System.EventHandler(this.HandController_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_up;
        private System.Windows.Forms.Button button_center;
        private System.Windows.Forms.Button button_down;
        private System.Windows.Forms.Button button_left;
        private System.Windows.Forms.Button button_right;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.ComboBox comboBox_rate;
    }
}