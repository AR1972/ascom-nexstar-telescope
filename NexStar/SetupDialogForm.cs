using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Timers;
using System.Diagnostics;

namespace ASCOM.NexStar
{
    [ComVisible(false)]					// Form not registered for COM!
    public partial class SetupDialogForm : Form
    {
        System.Timers.Timer tmr = new System.Timers.Timer();
        DateTime dt;

        public SetupDialogForm()
        {
            InitializeComponent();
            foreach (string port in System.IO.Ports.SerialPort.GetPortNames())
            {
                comboBox1.Items.Add(port);
                comboBox1.Sorted = true;
                comboBox1.SelectedIndex = 0;
                int p = Common.GetSerialPort();
                if (p > 0 && p <= 256)
                {
                    comboBox1.Text = "COM" + p;
                }
            }
            text_lat.Text = Scope.Latitude.ToString();
            text_long.Text = Scope.Longitude.ToString();
            text_evel.Text = Scope.Elevation.ToString();
            text_foc_len.Text = (Scope.FocalLength * 1000).ToString();
            text_ap_area.Text = (Scope.ApertureArea * 1000000).ToString();
            text_ap_dia.Text = (Scope.ApertureDiameter * 1000).ToString();
            dt = Common.GetLstDateTime();
            tmr.Interval = 1000;
            tmr.AutoReset = true;
            tmr.Elapsed += new ElapsedEventHandler(tmr_Elapsed);
            tmr.Start();
            text_lst.Text = dt.ToString("HH:mm:ss");
            comboBox2.Items.Add("Off");
            comboBox2.Items.Add("Alt-Azm");
            comboBox2.Items.Add("Eq North");
            comboBox2.Items.Add("Eq South");
            comboBox2.SelectedIndex = 0;
            comboBox2.SelectedIndex = ((int)Scope.TrackingMode);
            checkBox1.Checked = Scope.PecEnabled;
        }

        public void updatelst()
        {
            text_lst.Text = dt.ToString("HH:mm:ss");
        }

        void tmr_Elapsed(object sender, ElapsedEventArgs e)
        {
            /* add one sidereal second for every "normal" second */
            dt = dt.AddMilliseconds(1002.737916);
            text_lst.Invoke(new updatetextboxCallback(updatelst));
        }

        public delegate void updatetextboxCallback();

        private void cmdOK_Click(object sender, EventArgs e)
        {
            double value = 0;
            short pn = 0;
            string p = "0";
            if (!string.IsNullOrEmpty(comboBox1.Text))
            {
                p = comboBox1.Text;
            }
            Regex rgx = new Regex("[^0-9]");
             p = rgx.Replace(p, "");
             short.TryParse(p, out pn);
             if (pn > 0 && pn <= 256)
             {
                 Scope.ConnectedPort = pn;
             }
            double.TryParse(text_lat.Text, out value);
            Scope.Latitude = value;
            double.TryParse(text_long.Text, out value);
            Scope.Longitude = value;
            double.TryParse(text_evel.Text, out value);
            Scope.Elevation = value;
            double.TryParse(text_foc_len.Text, out value);
            Scope.FocalLength = value / 1000;
            double.TryParse(text_ap_area.Text, out value);
            Scope.ApertureArea = value / 1000000;
            double.TryParse(text_ap_dia.Text, out value);
            Scope.ApertureDiameter = value / 1000;
            Scope.TrackingMode = (Common.eTrackingMode)comboBox2.SelectedIndex;
            Scope.PecEnabled = checkBox1.Checked;
            tmr.Stop();
            tmr.Dispose();
            Close();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            tmr.Stop();
            tmr.Dispose();
            Close();
        }

        private void BrowseToAscom(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://ascom-standards.org/");
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        private void SetupDialogForm_Load(object sender, EventArgs e)
        {
        }

        /* allow only numbers in the text boxes */

        private void text_ap_dia_Leave(object sender, EventArgs e)
        {
            if (text_ap_dia.Text == null ||
                text_ap_dia.Text == String.Empty)
            {
                text_ap_dia.Text = "0";
            }
            if (text_ap_dia.Text != null ||
                text_ap_dia.Text != String.Empty)
            {
                double r = 0;
                double.TryParse(text_ap_dia.Text.ToString(), out r);
                r /= 2;
                text_ap_area.Text = Math.Round((Math.PI * (r * r)), 1).ToString();
            }
        }

        private void text_ap_dia_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(!char.IsNumber(e.KeyChar) &&
                !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void text_ap_area_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsNumber(e.KeyChar) &&
                !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void text_foc_len_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsNumber(e.KeyChar) &&
                !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void text_evel_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsNumber(e.KeyChar) &&
                !char.IsControl(e.KeyChar) &&
                e.KeyChar != '-' &&
                e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        private void text_lat_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsNumber(e.KeyChar) &&
                !char.IsControl(e.KeyChar) &&
                e.KeyChar != '-' &&
                e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        private void text_long_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsNumber(e.KeyChar) &&
                !char.IsControl(e.KeyChar) &&
                e.KeyChar != '-' &&
                e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        private void SetupDialogForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            tmr.Stop();
            tmr.Dispose();
        }
    }
}