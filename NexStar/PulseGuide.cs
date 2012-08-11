using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using ASCOM;
using ASCOM.DeviceInterface;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace ASCOM.NexStar
{
    internal class PulseGuide : IDisposable
    {
        private System.Timers.Timer RaTimer = null;
        private System.Timers.Timer DecTimer = null;
        private object SyncRoot = null;
        /* Ra guide interval diff */
        private Stopwatch RaStopWatch = null;
        private List<double> RaDiffList = null;
        private Thread RaDiffStatUpdt = null;
        /* Dec guide interval diff */
        private Stopwatch DecStopWatch = null;
        private List<double> DecDiffList = null;
        private Thread DecDiffStatUpdt = null;
        /* Ra guide interval */
        private List<long> RaIntvlList = null;
        private long RaIntvl = 0;
        private Thread RaIntvlStatUpdt = null;
        /* Dec guide interval */
        private List<long> DecIntvlList = null;
        private long DecIntvl = 0;
        private Thread DecIntvlStatUpdt = null;
        private bool GuideEnabled = false;

        public PulseGuide()
        {
            RaTimer = new System.Timers.Timer();
            DecTimer = new System.Timers.Timer();
            SyncRoot = new object();
            RaStopWatch = new Stopwatch();
            DecStopWatch = new Stopwatch();
            RaTimer.Enabled = false;
            DecTimer.Enabled = false;
            RaTimer.AutoReset = false;
            DecTimer.AutoReset = false;
            RaTimer.Elapsed += new ElapsedEventHandler(RaTimer_Elapsed);
            DecTimer.Elapsed += new ElapsedEventHandler(DecTimer_Elapsed);
            RaStopWatch.Reset();
            DecStopWatch.Reset();
            RaDiffList = new List<double>();
            DecDiffList = new List<double>();
            RaIntvlList = new List<long>();
            DecIntvlList = new List<long>();
            GuideEnabled = true;
        }

        public delegate void EventHandler(object sender, EventArgs<object, object, object> e);
        public event EventHandler<EventArgs<double, double, double>> RaDiffEvent;
        public event EventHandler<EventArgs<double, double, double>> DecDiffEvent;
        public event EventHandler<EventArgs<long, long, double>> RaIntvlEvent;
        public event EventHandler<EventArgs<long, long, double>> DecIntvlEvent;

        public bool Enabled
        {
            set { GuideEnabled = value; }
            get { return GuideEnabled; }
        }

        private void DecTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            DecTimer.Stop();
            Scope.isGuiding = RaTimer.Enabled | DecTimer.Enabled;
            Common.SlewFixedRate(Common.eDeviceId.ALT, Common.eDirection.Positive, Common.eFixedRate.Rate0);
            DecDiffUpdt(DecStopWatch.ElapsedMilliseconds - DecIntvl);
        }

        private void RaTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            RaTimer.Stop();
            Scope.isGuiding = RaTimer.Enabled | DecTimer.Enabled;
            Common.SlewFixedRate(Common.eDeviceId.AZM, Common.eDirection.Positive, Common.eFixedRate.Rate0);
            RaDiffUpdt(RaStopWatch.ElapsedMilliseconds - RaIntvl);
        }

        public void Guide(GuideDirections Direction, long Duration)
        {
            Common.eFixedRate Rate = Common.eFixedRate.Rate1;
            Common.eDeviceId DevId = 0;
            Common.eDirection Dir = 0;
            switch (Direction)
            {
                // Dec
                case GuideDirections.guideNorth:
                    DevId = Common.eDeviceId.ALT;
                    Dir = Common.eDirection.Positive;
                    break;
                case GuideDirections.guideSouth:
                    DevId = Common.eDeviceId.ALT;
                    Dir = Common.eDirection.Negitve;
                    break;
                // Ra
                case GuideDirections.guideEast:
                    DevId = Common.eDeviceId.AZM;
                    switch (Scope.TrackingMode)
                    {
                        case Common.eTrackingMode.EQNORTH:
                            Dir = Common.eDirection.Negitve;
                            break;
                        case Common.eTrackingMode.EQSOUTH:
                            Dir = Common.eDirection.Positive;
                            break;
                    }
                    break;
                case GuideDirections.guideWest:
                    DevId = Common.eDeviceId.AZM;
                    switch (Scope.TrackingMode)
                    {
                        case Common.eTrackingMode.EQNORTH:
                            Dir = Common.eDirection.Positive;
                            break;
                        case Common.eTrackingMode.EQSOUTH:
                            Dir = Common.eDirection.Negitve;
                            break;
                    }
                    break;
                default:
                    Common.Log.LogMessage(Common.DriverId, "Guide() : invalid guide direction " + Direction.ToString());
                    throw new ASCOM.InvalidValueException(Common.DriverId + ": Guide() : invalid guide direction " + Direction.ToString());
            }
            // guide Dec
            switch (DevId)
            {
                case Common.eDeviceId.ALT:
                    if (Duration > 0 && GuideEnabled)
                    {
                        Scope.isGuiding = true;
                        DecTimer.Interval = Duration;
                        Common.SlewFixedRate(DevId, Dir, Rate);
                        DecStopWatch.Reset();
                        DecStopWatch.Start();
                        DecTimer.Start();
                        DecIntvlUpdt(Duration);
                        DecIntvl = Duration;
                    }
                    else
                    {
                        Common.SlewFixedRate(DevId, Dir, Common.eFixedRate.Rate0);
                    }
                    break;
                // guide Ra
                case Common.eDeviceId.AZM:
                    if (Duration > 0 && GuideEnabled)
                    {
                        Scope.isGuiding = true;
                        RaTimer.Interval = Duration;
                        Common.SlewFixedRate(DevId, Dir, Rate);
                        RaStopWatch.Reset();
                        RaStopWatch.Start();
                        RaTimer.Start();
                        RaIntvlUpdt(Duration);
                        RaIntvl = Duration;
                    }
                    else
                    {
                        Common.SlewFixedRate(DevId, Dir, Common.eFixedRate.Rate0);
                    }
                    break;
            }
        }

        public void Stop()
        {
            RaTimer.Stop();
            DecTimer.Stop();
            RaStopWatch.Stop();
            DecStopWatch.Stop();
            Scope.isGuiding = RaTimer.Enabled | DecTimer.Enabled;
            Common.SlewFixedRate(Common.eDeviceId.AZM, Common.eDirection.Positive, Common.eFixedRate.Rate0);
            Common.SlewFixedRate(Common.eDeviceId.ALT, Common.eDirection.Positive, Common.eFixedRate.Rate0);
            GuideEnabled = false;
        }

        public void Dispose()
        {
            Stop();
            RaTimer.Close();
            DecTimer.Close();
        }

        private void RaDiffUpdt(double value)
        /* tracks the difference between requested */
        /* messured guide interval for the Ra axis */
        {
            if (RaDiffEvent != null)
            {
                RaDiffStatUpdt = new Thread(() =>
                    {
                        if (RaDiffList.Count == 100)
                        {
                            RaDiffList.RemoveAt(0);
                        }
                        RaDiffList.Add(value);
                        RaDiffEvent(this, new EventArgs<double, double, double>(RaDiffList.Min(), RaDiffList.Max(), Math.Round(RaDiffList.Average(), 2)));
                    });
                RaDiffStatUpdt.IsBackground = true;
                RaDiffStatUpdt.Priority = ThreadPriority.Lowest;
                RaDiffStatUpdt.Start();
            }
        }

        private void DecDiffUpdt(double value)
        /* tracks the difference between requested */
        /* messured guide interval for the Dec axis */
        {
            if (DecDiffEvent != null)
            {
                DecDiffStatUpdt = new Thread(() =>
                    {
                        if (DecDiffList.Count == 100)
                        {
                            DecDiffList.RemoveAt(0);
                        }
                        DecDiffList.Add(value);
                        DecDiffEvent(this, new EventArgs<double, double, double>(DecDiffList.Min(), DecDiffList.Max(), Math.Round(DecDiffList.Average(), 2)));
                    });
                DecDiffStatUpdt.IsBackground = true;
                DecDiffStatUpdt.Priority = ThreadPriority.Lowest;
                DecDiffStatUpdt.Start();
            }
        }

        private void RaIntvlUpdt(long value)
        /* tracks guide intervals for Ra axis */
        {
            if (RaIntvlEvent != null)
            {
                RaIntvlStatUpdt = new Thread(() =>
                    {
                        if (RaIntvlList.Count == 100)
                        {
                            RaIntvlList.RemoveAt(0);
                        }
                        RaIntvlList.Add(value);
                        RaIntvlEvent(this, new EventArgs<long, long, double>(RaIntvlList.Min(), RaIntvlList.Max(), Math.Round(RaIntvlList.Average(), 2)));
                    });
                RaIntvlStatUpdt.IsBackground = true;
                RaIntvlStatUpdt.Priority = ThreadPriority.Lowest;
                RaIntvlStatUpdt.Start();
            }
        }

        private void DecIntvlUpdt(long value)
        /* tracks guide intervals for Dec axis */
        {
            if (DecIntvlEvent != null)
            {
                DecIntvlStatUpdt = new Thread(() =>
                    {
                        if (DecIntvlList.Count == 100)
                        {
                            DecIntvlList.RemoveAt(0);
                        }
                        DecIntvlList.Add(value);
                        DecIntvlEvent(this, new EventArgs<long, long, double>(DecIntvlList.Min(), DecIntvlList.Max(), Math.Round(DecIntvlList.Average(), 2)));
                    });
                DecIntvlStatUpdt.IsBackground = true;
                DecIntvlStatUpdt.Priority = ThreadPriority.Lowest;
                DecIntvlStatUpdt.Start();
            }
        }
    }
}
