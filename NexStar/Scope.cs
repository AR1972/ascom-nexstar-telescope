using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ASCOM.DeviceInterface;
using System.Windows.Forms;

namespace ASCOM.NexStar
{
    internal static class Scope
    /* class to hold the global telescope state variables */
    {
        /* private members */
        private static short pConnectedPort = 0;
        private static double pLongitude = -1;
        private static double pLatitude = -1;
        private static double pElevation = 0;
        private static double pApertureArea = 0;
        private static double pApertureDiameter = 0;
        private static double pFocalLength = 0;
        private static Common.eTrackingMode pTrackingMode = Common.eTrackingMode.OFF;
        private static bool pPecEnabled = false;
        private static double pApertureObstruction = 0;
        /* public members */
        public static bool isConnected = false;
        public static string ModelName = "Uknown";
        public static string Name = "Unknown";
        public static bool isGem = false;
        public static bool HasPec = false;
        public static bool HasGps = false;
        public static int GpsState = -1;
        public static bool GpsTimeValid = false;
        public static Common.eScopeModel Model = Common.eScopeModel.UNKNOWN;
        public static Common.eScopeType Type = Common.eScopeType.UNKNOWN;
        public static int Version = 0;
        public static bool isTracking = false;
        public static bool Reconnecting = false;
        public static bool isSlewing = true;
        public static bool isAligned = false;
        public static AlignmentModes AlignmentMode = AlignmentModes.algAltAz;
        public static int AzmVersion = 0;
        public static int AltVersion = 0;
        public static short SettleTime = 0;
        public static double RateDec = 0;
        public static double RateRa = 0;
        public static double GuideRateDec = 0;
        public static double GuideRateRa = 0;
        public static bool isGuiding = false;
        public static double TargetRa = 0;
        public static bool TargetRaSet = false;
        public static double TargetDec = 0;
        public static bool TargetDecSet = false;
        public static TrackingRates TrackingRates = null;
        public static AxisRates[] AxisRates = null;
        public static AxisRates[] GuideRates = null;
        public static DriveRates TrackingRate = DriveRates.driveSidereal;
        /* property changed event */
        public delegate void EventHandler(object sender, EventArgs<object, object> e);
        public static event EventHandler<EventArgs<string, string>> EventPropertyChanged;
        /* properties */
        public static short ConnectedPort
        {
            get { return pConnectedPort; }
            set
            {
                pConnectedPort = value;
                if (EventPropertyChanged != null)
                {
                    EventPropertyChanged(Common.eScopeEvent.PropertyChanged, new EventArgs<string, string>(Common.PROFILE_COM_PORT, pConnectedPort.ToString()));
                }
            }
        }
        public static double Longitude
        {
            get { return pLongitude; }
            set
            {
                pLongitude = value;
                if (EventPropertyChanged != null)
                {
                    EventPropertyChanged(Common.eScopeEvent.PropertyChanged, new EventArgs<string, string>(Common.PROFILE_SITE_LONGITUDE, pLongitude.ToString()));
                }
            }
        }
        public static double Latitude
        {
            get { return pLatitude; }
            set
            {
                pLatitude = value;
                if(EventPropertyChanged != null)
                {
                    EventPropertyChanged(Common.eScopeEvent.PropertyChanged, new EventArgs<string, string>(Common.PROFILE_SITE_LATITUDE, pLatitude.ToString()));
                }
            }
        }
        public static double Elevation
        {
            get { return pElevation; }
            set
            {
                pElevation = value;
                if (EventPropertyChanged != null)
                {
                    EventPropertyChanged(Common.eScopeEvent.PropertyChanged, new EventArgs<string, string>(Common.PROFILE_SITE_ELEVATION, pElevation.ToString()));
                }
            }
        }
        public static double FocalLength
        {
            get { return pFocalLength; }
            set
            {
                pFocalLength = value;
                if (EventPropertyChanged != null)
                {
                    EventPropertyChanged(Common.eScopeEvent.PropertyChanged, new EventArgs<string, string>(Common.PROFILE_FOCAL_LENGTH, pFocalLength.ToString()));
                }
            }
        }
        public static double ApertureArea
        {
            get { return pApertureArea; }
            set
            {
                pApertureArea = value;
                if (EventPropertyChanged != null)
                {
                    EventPropertyChanged(Common.eScopeEvent.PropertyChanged, new EventArgs<string, string>(Common.PROFILE_APERTURE_AREA, pApertureArea.ToString()));
                }
            }
        }
        public static double ApertureObstruction
        {
            get { return pApertureObstruction; }
            set
            {
                pApertureObstruction = value;
                if (EventPropertyChanged != null)
                {
                    EventPropertyChanged(Common.eScopeEvent.PropertyChanged, new EventArgs<string, string>(Common.PROFILE_APERTURE_OBSTRUCTION, pApertureObstruction.ToString()));
                }
            }
        }
        public static double ApertureDiameter
        {
            get { return pApertureDiameter; }
            set
            {
                pApertureDiameter = value;
                if (EventPropertyChanged != null)
                {
                    EventPropertyChanged(Common.eScopeEvent.PropertyChanged, new EventArgs<string, string>(Common.PROFILE_APERTURE_DIAMETER, pApertureDiameter.ToString()));
                }
            }
        }
        public static Common.eTrackingMode TrackingMode
        {
            get { return pTrackingMode; }
            set 
            {
                pTrackingMode = value;
                if (EventPropertyChanged != null)
                {
                    EventPropertyChanged(Common.eScopeEvent.PropertyChanged, new EventArgs<string, string>(Common.PROFILE_TRACK_MODE, ((int)pTrackingMode).ToString()));
                }
            }
        }
        public static bool PecEnabled
        {
            get { return pPecEnabled; }
            set
            {
                pPecEnabled = value;
                if (EventPropertyChanged != null)
                {
                    EventPropertyChanged(Common.eScopeEvent.PropertyChanged, new EventArgs<string, string>(Common.PROFILE_PEC_ENABLE, pPecEnabled.ToString()));
                }
            }
        }

        static Scope()
        {
            TrackingRates = new TrackingRates();
            pElevation = Common.GetElevation();
            pFocalLength = Common.GetFocalLength();
            pApertureDiameter = Common.GetApertureDiameter();
            pApertureArea = Common.GetApertureArea();
            pConnectedPort = Common.GetSerialPort();
            pLatitude = Common.GetLatitude();
            pLongitude = Common.GetLongitude();
            pTrackingMode = Common.GetTrackMode();
            pPecEnabled = Common.GetPecEnabled();
            pApertureObstruction = Common.GetApertureObstruction();
        }
    }
}
