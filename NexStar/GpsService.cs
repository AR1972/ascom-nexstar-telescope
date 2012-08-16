using System;
using System.Threading;
using System.Windows.Forms;

/* this class monitors the state of the GPS and uses event's to notify recievers of the GPS state/updates */
/* communication with the GPS device is pretty slow seems to take about 1 sec for each command to complete */
/* so we should communicate the GPS device as little as possible */

namespace ASCOM.NexStar
{
    internal class GpsService
    {
        Thread t = null;
        private bool Run = false;
        private Gps GpsTools = null;
        private DateTime LastValidTime;
        private DateTime LastLinkedTime;
        private bool Linked = false;
        private bool TimeValid = false;
        private bool Present = false;

        public enum GpsEvent
        {
            Link,
            TimeValid,
            Error
        }

        public delegate void EventHandler(object sender, EventArgs<object> e);
        public event EventHandler<EventArgs<int>> GpsEventLinkState;
        public event EventHandler<EventArgs<bool>> GpsEventTimeValid;
        public event EventHandler<EventArgs<int>> GpsEventError;

        public GpsService()
        {
            GpsTools = new Gps();
        }

        public bool Start()
        {
            Stop();
            Run = true;
            t = new Thread(MainThread);
            t.Name = "GPS Thread";
            t.IsBackground = true;
            t.Priority = ThreadPriority.Lowest;
            t.Start();
            return t.IsAlive;
        }

        public bool Stop()
        {
            if (t != null && t.IsAlive)
            {
                Run = false;
                while (t.IsAlive)
                {
                    Application.DoEvents();
                }
                Run = true;
            }
            return true;
        }

        public bool isRunning
        {
            get { return t.IsAlive; }
        }

        public bool GpsLinked
        {
            get { return Linked; }
        }

        public bool GpsTimeValid
        {
            get { return TimeValid; }
        }

        public bool GpsPresent
        {
            get { return Present; }
        }

        private void MainThread()
        {
            Linked = false;
            TimeValid = false;
            Present = false;
            bool LastTimeValid = false;
            int GpsState = -1;
            int LastGpsState = -1;
            int SleepLength = (1000 * 60) * 60;
            Thread t;
            while (Scope.isConnected && Run)
            {
                GpsState = GpsTools.isLinked();
                if (GpsState != LastGpsState)
                {
                    LastGpsState = GpsState;
                    if (GpsEventLinkState != null)
                    {
                        GpsEventLinkState(GpsEvent.Link, new EventArgs<int>(GpsState));
                    }
                }
                switch (GpsState)
                {
                    case 1:
                        /* GPS linked, check if time is valid send the event*/
                        Linked = true;
                        LastLinkedTime = DateTime.UtcNow;
                        Present = true;
                        TimeValid = GpsTools.isTimeValid();
                        if (TimeValid != LastTimeValid)
                        {
                            LastTimeValid = TimeValid;
                            if (GpsEventTimeValid != null)
                            {
                                GpsEventTimeValid(GpsEvent.TimeValid, new EventArgs<bool>(TimeValid));
                            }
                        }
                        if (TimeValid)
                        {
                            LastValidTime = DateTime.UtcNow;
                            SleepLength = (1000 * 60) * 60;
                        }
                        else
                        {
                            SleepLength = (1000 * 120);
                        }
                        break;
                    case 0:
                        /* not yet linked, try again in 2 min */
                        Linked = false;
                        TimeValid = false;
                        Present = true;
                        SleepLength = 1000 * 120;
                        break;
                    default:
                        /* comunication with GPS device failed assume
                         * device does not exist and exit thread */
                        Linked = false;
                        TimeValid = false;
                        Present = false;
                        if (GpsEventError != null)
                        {
                            GpsEventError(GpsEvent.Error, new EventArgs<int>(GpsState));
                        }
                        return;
                };
                t = new Thread(() =>
                {
                    Thread.Sleep(SleepLength);
                });
                t.IsBackground = true;
                t.Priority = ThreadPriority.Lowest;
                t.Start();
                while (t.IsAlive && Run)
                {
                    Application.DoEvents();
                    Thread.Sleep(500);
                }
                if (t.IsAlive)
                {
                    t.Abort();
                }
            }
        }
    }
}
