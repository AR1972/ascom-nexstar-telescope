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
    internal partial class GuidePerformance : Form
    {
        object SyncLock = null;
        public GuidePerformance()
        {
            InitializeComponent();
            SyncLock = new Object();
        }

        void ScopePulseGuide_DecJitEvent(object sender, EventArgs<double, double, double> e)
        {
            lock (SyncLock)
            {
                DecJitMin.Text = e.ValueA.ToString();
                DecJitMax.Text = e.ValueB.ToString();
                DecJitAvg.Text = e.ValueC.ToString();
            }
        }

        void ScopePulseGuide_DecIntEvent(object sender, EventArgs<long, long, double> e)
        {
            lock (SyncLock)
            {
                DecIntMin.Text = e.ValueA.ToString();
                DecIntMax.Text = e.ValueB.ToString();
                DecIntAvg.Text = e.ValueC.ToString();
            }
        }

        void ScopePulseGuide_RaJitEvent(object sender, EventArgs<double, double, double> e)
        {
            lock (SyncLock)
            {
                RaJitMin.Text = e.ValueA.ToString();
                RaJitMax.Text = e.ValueB.ToString();
                RaJitAvg.Text = e.ValueC.ToString();
            }
        }

        void ScopePulseGuide_RaIntEvent(object sender, EventArgs<long, long, double> e)
        {
            lock (SyncLock)
            {
                RaIntMin.Text = e.ValueA.ToString();
                RaIntMax.Text = e.ValueB.ToString();
                RaIntAvg.Text = e.ValueC.ToString();
            }
        }

        private void GuidePerformance_FormClosing(object sender, FormClosingEventArgs e)
        {
            Common.ScopePulseGuide.RaIntvlEvent -= ScopePulseGuide_RaIntEvent;
            Common.ScopePulseGuide.RaDiffEvent -= ScopePulseGuide_RaJitEvent;
            Common.ScopePulseGuide.DecIntvlEvent -= ScopePulseGuide_DecIntEvent;
            Common.ScopePulseGuide.DecDiffEvent -= ScopePulseGuide_DecJitEvent;
        }

        private void GuidePerformance_Load(object sender, EventArgs e)
        {
            Common.ScopePulseGuide.RaIntvlEvent += new EventHandler<EventArgs<long, long, double>>(ScopePulseGuide_RaIntEvent);
            Common.ScopePulseGuide.RaDiffEvent += new EventHandler<EventArgs<double, double, double>>(ScopePulseGuide_RaJitEvent);
            Common.ScopePulseGuide.DecIntvlEvent += new EventHandler<EventArgs<long, long, double>>(ScopePulseGuide_DecIntEvent);
            Common.ScopePulseGuide.DecDiffEvent += new EventHandler<EventArgs<double, double, double>>(ScopePulseGuide_DecJitEvent);
        }
    }
}
