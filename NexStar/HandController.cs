using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ASCOM.NexStar
{
    [ComVisible(false)]					// Form not registered for COM!
    public partial class HandController : Form
    {
        public HandController()
        {
            InitializeComponent();
        }

        private void button_up_MouseDown(object sender, MouseEventArgs e)
        {
            Common.SlewFixedRate(Common.eDeviceId.ALT, Common.eDirection.Positive, (Common.eFixedRate)comboBox_rate.SelectedIndex + 1);
        }

        private void button_up_MouseUp(object sender, MouseEventArgs e)
        {
            button6.Select();
            Common.SlewFixedRate(Common.eDeviceId.ALT, Common.eDirection.Positive, Common.eFixedRate.Rate0);
        }

        private void button_right_MouseDown(object sender, MouseEventArgs e)
        {
            Common.SlewFixedRate(Common.eDeviceId.AZM, Common.eDirection.Negitve, (Common.eFixedRate)comboBox_rate.SelectedIndex + 1);
        }

        private void button_right_MouseUp(object sender, MouseEventArgs e)
        {
            button6.Select();
            Common.SlewFixedRate(Common.eDeviceId.AZM, Common.eDirection.Negitve, Common.eFixedRate.Rate0);
        }

        private void button_down_MouseDown(object sender, MouseEventArgs e)
        {
            Common.SlewFixedRate(Common.eDeviceId.ALT, Common.eDirection.Negitve, (Common.eFixedRate)comboBox_rate.SelectedIndex + 1);
        }

        private void button_down_MouseUp(object sender, MouseEventArgs e)
        {
            button6.Select();
            Common.SlewFixedRate(Common.eDeviceId.ALT, Common.eDirection.Negitve, Common.eFixedRate.Rate0);
        }

        private void button_left_MouseDown(object sender, MouseEventArgs e)
        {
            Common.SlewFixedRate(Common.eDeviceId.AZM, Common.eDirection.Positive, (Common.eFixedRate)comboBox_rate.SelectedIndex + 1);
        }

        private void button_left_MouseUp(object sender, MouseEventArgs e)
        {
            Common.SlewFixedRate(Common.eDeviceId.AZM, Common.eDirection.Positive, Common.eFixedRate.Rate0);
        }

        private void button_center_MouseDown(object sender, MouseEventArgs e)
        {
            Common.AbortSlew();
        }

        private void button_center_MouseUp(object sender, MouseEventArgs e)
        {
            button6.Select();
        }

        private void HandController_Load(object sender, EventArgs e)
        {
            comboBox_rate.SelectedIndex = Common.GetHcRate();
            button6.Select();
        }

        private void comboBox_rate_SelectedIndexChanged(object sender, EventArgs e)
        {
            Common.SetHcRate((short)comboBox_rate.SelectedIndex);
        }

        private void comboBox_rate_DropDownClosed(object sender, EventArgs e)
        {
            button6.Select();
        }

        private void HandController_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
