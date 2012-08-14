using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ASCOM.Utilities;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using ASCOM.DeviceInterface;

namespace ASCOM.NexStar
{
    internal static class Common
    {

        #region enums

        public enum eDeviceId
        {
            MAIN = 1,
            HC = 4,
            AZM = 16,
            ALT = 17,
            GPS = 176,
            RTC = 178
        }

        /* lifted from Celestron unified driver */
        public enum eScopeType
        {
            UNKNOWN = 0,
            ULTIMA = 1,       // Ultima 2000
            NEXSTAR58 = 2,    // the original NexStar 5 and 8
            NEXSTAR_GPS = 3,  // the GPS, 5i and 8i, CGE and ASC
            GT = 4
        }

        /* lifted from Celestron unified driver */ 
        public enum eScopeModel
        {
            UNKNOWN = 0,
            GPS = 1,
            GPS_SA = 2,
            ISERIES = 3,
            ISERIES_SE = 4,
            CGE = 5,
            ASC = 6,
            SLT = 7,
            C20 = 8,
            CPC = 9,
            GT = 10,
            SE_4_5 = 11,
            SE_6_8 = 12,
            CGE2 = 13,
            EQ6 = 14
        }

        public enum eTrackingMode
        {
            OFF = 0,
            ALTAZ = 1,
            EQNORTH = 2,
            EQSOUTH = 3
        }

        public enum eFixedRate
        {
            Rate0 = 0,
            Rate1 = 1,
            Rate2 = 2,
            Rate3 = 3,
            Rate4 = 4,
            Rate5 = 5,
            Rate6 = 6,
            Rate7 = 7,
            Rate8 = 8,
            Rate9 = 9,
        }

        public enum eDirection
        {
            Positive = 36,
            Negitve = 37
        }

        public enum eScopeEvent
        {
            Connection,
            PropertyChanged
        }

        private enum eTrackRate
        {
            Sidereal = 0xFFFF,
            Solar = 0xFFFE,
            Lunar = 0xFFFD
        }

        #endregion

        #region struct

        private struct HcLocation
        {
            public byte DegLatitude;
            public byte MinLatitude;
            public byte SecLatitude;
            public byte NS; // 0 = north, 1 = south
            public byte DegLongitude;
            public byte MinLongitude;
            public byte SecLongitude;
            public byte EW; // 0 = east, 1 = west
        }

        private struct HcDateTime
        {
            public byte Hours; // 24h clock
            public byte Minutes;
            public byte Seconds;
            public byte Month;
            public byte Day;
            public byte Year; // year - 2000
            public byte TimeZone; // if negative 256-zone
            public byte Dst; // 1 = daylight savings, 0 = standard 
        }

        private struct DevVersion
        {
            public byte Major;
            public byte Minor;
        }

        /*
        private struct AuxCommand
        {
            public byte Command = (byte)'P';    // aux command always 'P'
            public byte DataLen = 0;            // number of used data bytes
            public byte DevId = 0;              // device to recieve the command
            public byte Data0 = 0;              // first data byte
            public byte Data1 = 0;              // second data byte
            public byte Data2 = 0;              // third data byte
            public byte Data3 = 0;              // fourth data byte
            public byte ReplyLen = 0;           // expected reply len (on error Rx data will be ReplyLen + 1)
        }
        */

        #endregion

        #region version

        /* lifted form Celestron unified driver */

        private const ushort Version_16 = 0x106;            // 1.06 first to use "P" commands
        private const ushort Version_22 = 0x202;            // 2.02 GPS and i series scopes
        private const ushort Version_23 = 0x203;            // 2.03 iSeries SE
        private const ushort Version_30 = 0x300;            // 3.00 CGE scopes
        private const ushort Version_34 = 0x304;            // 3.04 ASC scopes
        private const ushort Version_40 = 0x400;            // 4.00 CPC and SLT scopes
        private const ushort Version_41 = 0x40A;            // 4.10 scopes with sync command
        private const ushort Version_412 = 0x40C;           // 4.12 scopes
        private const ushort Version_413 = 0x40D;           // 4.13 scopes
        private const ushort Version_414 = 0x40E;           // 4.14 scopes
        private const ushort Version_Beta = 0x2800;         // beta versions
        private const ushort Version_GT_LO = 0x6400;
        private const ushort Version_GT_HI = 0x66FF;

        #endregion

        #region profile strings

        public const string PROFILE_SITE_ELEVATION = "SiteElevation";
        public const string PROFILE_SITE_LONGITUDE = "SiteLogitude";
        public const string PROFILE_SITE_LATITUDE = "SiteLatitude";
        public const string PROFILE_COM_PORT = "SerialPort";
        private const string PROFILE_DRIVER_TYPE = "Telescope";
        public const string PROFILE_APERTURE_DIAMETER = "ApertureDiameter";
        public const string PROFILE_APERTURE_AREA = "ApertureArea";
        public const string PROFILE_FOCAL_LENGTH = "FocalLength";
        private const string PROFILE_HC_RATE = "HcRate";
        public const string PROFILE_TRACK_MODE = "TrackMode";

        #endregion

        private const double SPD = 86400; // seconds per day
        private const double SIDRATE = .9972695677;
        private const double SIDEREAL_RATE_DEG_SEC = (360d / SPD) / SIDRATE;

        private static Serial ScopeSerialPort = null;
        private static Profile ScopeProfile = null;
        internal static PulseGuide ScopePulseGuide = null;
        private static readonly byte[] Terminator = { (byte)'#' };
        public const string DriverId = "ASCOM.NexStar.Telescope";
        public const string DriverDescription = "Celestron NexStar Telescope";
        private static Thread HC = null;
        private static Gps ScopeGps = null;
        private static GpsThread ScopeGpsThread = null;
        /* lock the Sending object during serial port transations */
        private static object Sending = null;
        private static Form HCWindow = null;
        private static int FAILSAFEINSTANCE = 0;
        private static Thread GotoWatcher = null;
        internal static TraceLogger Log = null;
        /* our disconnect/cleanup code can be called by many events to */
        /* prevent disconnecting multiple times lock the Disconnecting object */
        private static object Disconnecting = null;
        /* events */
        private delegate void EventHandler(object sender, EventArgs<object> e);
        private static event EventHandler<EventArgs<bool>> ScopeEventConnected;

        static Common()
        {
            ScopeProfile = new Profile();
            ScopeGps = new Gps();
            ScopeGpsThread = new GpsThread();
            Sending = new object();
            Disconnecting = new object();
            Log = new TraceLogger();
            Log.Enabled = true;
            Log.LogStart(DriverId, "Celestron NexStar Driver");
            Log.LogFinish("");
            if (!ScopeProfile.IsRegistered(DriverId))
            {
                ScopeProfile.DeviceType = PROFILE_DRIVER_TYPE;
                ScopeProfile.Register(DriverId, DriverDescription);
            }
            ScopeGpsThread.GpsEventError += new EventHandler<EventArgs<int>>(GpsErrorReciever);
            ScopeGpsThread.GpsEventTimeValid += new EventHandler<EventArgs<bool>>(GpsTimeValidReciever);
            ScopeGpsThread.GpsEventLinkState += new EventHandler<EventArgs<int>>(GpsLinkReciever);
            ScopeEventConnected += new EventHandler<EventArgs<bool>>(ScopeConnectReciever);
            Scope.EventPropertyChanged += new EventHandler<EventArgs<string, string>>(UpdateProfileReciever);
            /* attempt to handle unexpected events */
            AppDomain.CurrentDomain.ProcessExit += new System.EventHandler(ProcessExit);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(ProcessExit);
            AppDomain.CurrentDomain.DomainUnload += new System.EventHandler(ProcessExit);
            Application.ThreadException += new ThreadExceptionEventHandler(ProcessExit);
            Application.ThreadExit += new System.EventHandler(ProcessExit);
            Application.ApplicationExit += new System.EventHandler(ProcessExit);
        }

        public static bool ScopeConnect(bool Connect)
        {
            Log.LogMessage(DriverId, "ScopeConnect(" + Connect.ToString() + ")");
            if (Connect == true)
            {
                /* stop the reconnecting thread */
                Scope.Reconnecting = false;
                /* connect to scope */
                if (Scope.isConnected == false)
                {
                    /* create threads */
                    HC = new Thread(HcThread);
                    //
                    foreach (short port in GetSerialPorts())
                    {
                        if (port > 0 && port <= 256) /* sanity check on port number */
                        {
                            Log.LogMessage(DriverId, "ScopeConnect() : trying COM" + port.ToString());
                            try
                            {
                                ScopeSerialPort = new Serial();
                                lock (Sending)
                                {
                                    ScopeSerialPort.Port = port;
                                    ScopeSerialPort.ReceiveTimeout = 4;
                                    ScopeSerialPort.Speed = SerialSpeed.ps9600;
                                    ScopeSerialPort.StopBits = SerialStopBits.One;
                                    ScopeSerialPort.Handshake = SerialHandshake.None;
                                    ScopeSerialPort.DataBits = 8;
                                    ScopeSerialPort.Connected = true;
                                }
                                if (GetScopeType(out Scope.Type))
                                {
                                    if (Scope.Type == eScopeType.NEXSTAR_GPS)
                                    {
                                        Scope.ConnectedPort = port;
                                        Scope.isConnected = true;
                                        if (ScopeEventConnected != null)
                                        {
                                            ScopeEventConnected(eScopeEvent.Connection, new EventArgs<bool>(true));
                                        }
                                        return true;
                                    }
                                }
                                lock (Sending)
                                {
                                    ScopeSerialPort.Connected = false;
                                    ScopeSerialPort = null;
                                    Scope.isConnected = false;
                                    GC.Collect();
                                }
                            }
                            catch (TimeoutException Ex)
                            {
                                Log.LogMessage(DriverId, "ScopeConnect() : " + Ex.Message);
                            }
                            catch (Exception Ex)
                            {
                                Log.LogMessage(DriverId, "ScopeConnect() : " + Ex.Message);
                            }
                        }
                    }
                    Log.LogMessage(DriverId, "ScopeConnect() : no scope found");
                    throw new ASCOM.NotConnectedException(DriverId + "ScopeConnect() : no scope found");
                }
                else
                {
                    Scope.isConnected = true;
                    return true;
                }
            }
            else
            {
                /* disconnect from scope */
                if (Scope.isConnected == true && ScopeSerialPort != null)
                {
                    lock (Disconnecting)
                    {
                        /* this should stop the reconnect thread if running */
                        /* expect an exception to be thrown */
                        Scope.Reconnecting = false;
                        AbortSlew();
                        ScopeGpsThread.Stop();
                        ScopePulseGuide.Stop();
                        /* SlewFixedRate sets tracking, make sure tracking is off */
                        /* by sending the command just before isConnected is set false */
                        SetTracking(false);
                        /* setting isConnected = false will cause all running threads to quit */
                        Scope.isConnected = false;
                        lock (Sending)
                        {
                            try
                            {
                                ScopeSerialPort.Connected = false;
                            }
                            catch (Exception Ex)
                            {
                                Log.LogMessage(DriverId, "ScopeConnect() : " + Ex.Message);
                            }
                            finally
                            {
                                ScopeSerialPort.Dispose();
                                ScopeSerialPort = null;
                            }
                        }
                        return true;
                    }
                }
                else
                {
                    Scope.isConnected = false;
                    return true;
                }
            }
        }

        public static bool AbortSlew()
        /* supported from version 1.2 */
        {
            if (Scope.isConnected)
            {
                // stop GOTO slew
                if (isSlewing())
                {
                    byte[] TxBuffer = { (byte)'M' };
                    byte[] RxBuffer = { };
                    if (SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                    {
                        SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                    }
                    if (!isSlewing())
                    {
                        Scope.isSlewing = false;
                        return true;
                    }
                }
                else
                {
                    /* stop other slew */
                    SlewFixedRate(eDeviceId.ALT, eDirection.Positive, eFixedRate.Rate0);
                    SlewFixedRate(eDeviceId.AZM, eDirection.Positive, eFixedRate.Rate0);
                    return true;
                }
            }
            return false;
        }

        private static bool isAligned()
        /* supported since version 1.2 */
        {
            if (Scope.isConnected)
            {
                short RxLength = 2;
                byte[] TxBuffer = { (byte)'J' };
                byte[] RxBuffer = { };
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    return RxBuffer[0] == 1;
                }
            }
            return false;
        }

        public static bool GetTrackingMode(out eTrackingMode TrackingMode)
        /* supported from version 2.3 */
        {
            if (Scope.isConnected && Scope.Version >= Version_23)
            {
                short RxLength = 2;
                byte[] TxBuffer = { (byte)'t' };
                byte[] RxBuffer = { };
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    TrackingMode = (eTrackingMode)RxBuffer[0];
                    return true;
                }
            }
            TrackingMode = eTrackingMode.OFF;
            return false;
        }

        public static bool SetTrackingMode(eTrackingMode Mode)
        /* supported from version 1.6 */
        {
            if (Scope.isConnected && Scope.Version >= Version_16)
            {
                byte[] TxBuffer = { (byte)'T', (byte)Mode };
                byte[] RxBuffer = { };
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                return RxBuffer[0] == (byte)'#';
            }
            return false;
        }

        public static bool SlewFixedRate(eDeviceId Axis, eDirection Direction, eFixedRate Rate)
        /* supported from version 1.6 */
        {
            if (Scope.isConnected && Scope.Version >= Version_16)
            {
                if (Scope.AlignmentMode == AlignmentModes.algAltAz || Rate > eFixedRate.Rate2)
                {
                    SetTracking(false);
                }
                byte[] TxBuffer = { (byte)'P', 2, (byte)Axis, (byte)Direction, (byte)Rate, 0, 0, 0 };
                byte[] RxBuffer = { };
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                if (RxBuffer[0] == (byte)'#')
                {
                    if (Rate == eFixedRate.Rate0)
                    {
                        Scope.isSlewing = false;
                        if (Scope.isTracking == false)
                        {
                            SetTracking(true);
                        }
                    }
                    else
                    {
                        Scope.isSlewing = true;
                    }
                    return true;
                }
            }
            return false;
        }

        private static bool SlewVariableRate(eDeviceId Dev, long Rate)
        /* supported from version 1.6 */
        {
            if (Scope.isConnected && Scope.Version >= Version_16)
            {
                SetTrackingMode(eTrackingMode.OFF);
                byte Direction = 0;
                byte RateHi = 0;
                byte RateLow = 0;
                long r = 0;
                byte[] RxBuffer = { };
                if (Rate > 0)
                {
                    Direction = 6;
                }
                else
                {
                    Direction = 7;
                }
                r = Math.Abs(Rate);
                RateHi = Convert.ToByte(r / 0x10000);
                RateLow = Convert.ToByte(r % 0x1000);
                byte[] TxBuffer = { (byte)'P', 3, (byte)Dev, Direction, RateHi, RateLow, 0, 0 };
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                if (RxBuffer[0] == (byte)'#')
                {
                    if (Rate == 0)
                    {
                        SetTrackingMode(Scope.TrackingMode);
                        Scope.isSlewing = false;
                    }
                    else
                    {
                        Scope.isSlewing = true;
                        FailSafeStop(90);
                    }
                    return true;
                }
            }
            return false;
        }

        public static bool isSlewing()
        /* supported from version 1.2 */
        {
            if (Scope.isConnected)
            {
                short RxLength = 2;
                byte[] TxBuffer = { (byte)'L' };
                byte[] RxBuffer = { };
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    Scope.isSlewing = RxBuffer[0] == (byte)'1';
                    return RxBuffer[0] == (byte)'1';
                }
            }
            return false;
        }

        private static bool GetScopeType(out eScopeType Type)
        /* lifted form Celestron unified driver */
        /* this method is used to determin if connected to a supported scope */
        {
            lock (Sending)
            {
                byte[] TxBuffer = { (byte)'?', (byte)'E' };
                byte[] RxBuffer = { };
                lock (Sending)
                {
                    ScopeSerialPort.ClearBuffers();
                    ScopeSerialPort.TransmitBinary(TxBuffer);
                    RxBuffer = ScopeSerialPort.ReceiveTerminatedBinary(Terminator);
                }
                switch (RxBuffer[0])
                {
                    case (byte)'#':
                        Type = eScopeType.NEXSTAR58;
                        return false;
                    case (byte)'0':
                    case (byte)'1':
                    case (byte)'2':
                    case (byte)'3':
                    case (byte)'4':
                    case (byte)'5':
                    case (byte)'6':
                    case (byte)'7':
                    case (byte)'8':
                    case (byte)'9':
                    case (byte)'A':
                    case (byte)'B':
                    case (byte)'C':
                    case (byte)'D':
                    case (byte)'E':
                    case (byte)'F':
                        Type = eScopeType.NEXSTAR_GPS;
                        break;
                    case 0x01:
                        Type = eScopeType.GT;
                        return false;
                    default:
                        Type = eScopeType.UNKNOWN;
                        return false;
                }
                return true;
            }
        }

        private static bool GetScopeModel(out eScopeModel Model)
        /* lifted form Celestron unified driver */
        /* supported from version 2.2 */
        {
            if (Scope.isConnected && Scope.Version >= Version_22)
            {
                short RxLength = 2;
                byte[] TxBuffer = { (byte)'m' };
                byte[] RxBuffer = { };
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    Model = (eScopeModel)RxBuffer[0];
                    switch (Model)
                    {
                        case eScopeModel.GPS:
                            Scope.isGem = false;
                            Scope.HasPec = true;
                            Scope.ModelName = "GPS";
                            break;
                        case eScopeModel.GPS_SA:
                            Scope.isGem = false;
                            Scope.HasPec = true;
                            Scope.ModelName = "GPS SA";
                            break;
                        case eScopeModel.ISERIES:
                            Scope.isGem = false;
                            Scope.ModelName = "iSeries";
                            break;
                        case eScopeModel.ISERIES_SE:
                            Scope.isGem = false;
                            Scope.ModelName = "iSeries SE";
                            break;
                        case eScopeModel.CGE:
                            Scope.isGem = true;
                            Scope.HasPec = true;
                            Scope.ModelName = "CGE";
                            break;
                        case eScopeModel.ASC:
                            Scope.isGem = true;
                            Scope.ModelName = "ASC";
                            break;
                        case eScopeModel.SLT:
                            Scope.isGem = false;
                            Scope.ModelName = "SLT";
                            break;
                        case eScopeModel.C20:
                            Scope.isGem = true;
                            Scope.HasPec = true;
                            Scope.ModelName = "C20";
                            break;
                        case eScopeModel.CPC:
                            Scope.isGem = false;
                            Scope.HasPec = true;
                            Scope.ModelName = "CPC";
                            break;
                        case eScopeModel.GT:
                            Scope.isGem = false;
                            Scope.ModelName = "GT";
                            break;
                        case eScopeModel.SE_4_5:
                            Scope.isGem = false;
                            Scope.ModelName = "4/5 SE";
                            break;
                        case eScopeModel.SE_6_8:
                            Scope.isGem = false;
                            Scope.ModelName = "6/8 SE";
                            break;
                        case eScopeModel.CGE2:
                            Scope.isGem = true;
                            Scope.HasPec = true;
                            Scope.ModelName = "CGE Pro";
                            break;
                        case eScopeModel.EQ6:
                            Scope.isGem = true;
                            Scope.HasPec = true;
                            Scope.ModelName = "CGEM";
                            break;
                        default:
                            return false;
                    };
                    return true;
                }
            }
            Model = 0;
            return false;
        }

        private static bool GetScopeVersion(out int Version)
        /* supported from version 1.2 */
        {
            if (Scope.isConnected)
            {
                short RxLength = 3;
                byte[] TxBuffer = { (byte)'V' };
                byte[] RxBuffer = { };
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    Version = RxBuffer[0] * 0x100 + RxBuffer[1];
                    if (Scope.Version >= Version_GT_LO && Scope.Version <= Version_GT_HI)
                    {
                        Version = 0;
                    }
                    return true;
                }
            }
            Version = 0;
            return false;
        }

        private static bool GetDeviceVersion(eDeviceId Dev, out DevVersion Version)
        /* supported from version 1.6 */
        /* queries the version of a telescope device */
        {
            if (Scope.isConnected && Scope.Version >= Version_16)
            {
                short RxLength = 3;
                byte[] TxBuffer = { (byte)'P', 1, (byte)Dev, 254, 0, 0, 0, (byte)(RxLength - 1) };
                byte[] RxBuffer = { };
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    Version.Major = RxBuffer[0];
                    Version.Minor = RxBuffer[1];
                    return true;
                }
            }
            Version.Major = 0;
            Version.Minor = 0;
            return false;
        }

        private static bool Echo()
        /* supported from version 1.2 */
        {
            if (Scope.isConnected)
            {
                short RxLength = 2;
                byte[] TxBuffer = { (byte)'K', (byte)'A' };
                byte[] RxBuffer = { };
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    return RxBuffer[0] == (byte)'A';
                }
            }
            return false;
        }

        private static bool RawEcho()
        /* use this when unsure about scope connection state */
        /* supported from version 1.2 */
        {
            byte[] TxBuffer = { (byte)'K', (byte)'A' };
            byte[] RxBuffer = { 0x00, 0x00 };
            lock (Sending)
            {
                try
                {
                    ScopeSerialPort.ClearBuffers();
                    ScopeSerialPort.TransmitBinary(TxBuffer);
                    RxBuffer = ScopeSerialPort.ReceiveTerminatedBinary(Terminator);
                    return RxBuffer[0] == (byte)'A';
                }
                catch (Exception Ex)
                {
                    /* exception black hole                             */
                    /* if we are tring to sort out the scope connection */
                    /* we want to ignore the exceptions, and signal the */
                    /* caller that the ping failed                      */
                    Log.LogMessage(DriverId, "RawEcho() : " + Ex.Message);
                    return RxBuffer[0] == (byte)'A';
                }
            }
        }

        public static int SendSerialPortCommand(ref byte[] TxBuffer, out byte[] RxBuffer)
        /* return -1 on error, 0 to signal caller to resend, 1 on success */
        {
            if (TxBuffer.Length < 1)
            {
                Log.LogMessage(DriverId, "SendSerialPortCommand() : null buffer");
                throw new System.ArgumentNullException(DriverId + ": SendSerialPortCommand() : null buffer");
            }
            if (!Scope.isConnected || !RawEcho() && !ScopeReconnect())
            {
                RxBuffer = new byte[32];
                return -1;
            }
            try
            {
                lock (Sending)
                {
                    ScopeSerialPort.ClearBuffers();
                    ScopeSerialPort.TransmitBinary(TxBuffer);
                    RxBuffer = ScopeSerialPort.ReceiveTerminatedBinary(Terminator);
                }
            }
            catch (TimeoutException Ex)
            {
                Log.LogMessage(DriverId, "SendSerialPortCommand() : " + Ex.Message);
                RxBuffer = new byte[32];
                return 0;
            }
            catch (Exception Ex)
            {
                Log.LogMessage(DriverId, "SendSerialPortCommand() : " + Ex.Message);
                throw;
            }
            return 1;
        }

        public static int SendSerialPortCommand(ref byte[] TxBuffer, out byte[] RxBuffer, short Count)
        /* return -1 on error, 0 to signal caller to resend, 1 on success */
        {
            if (TxBuffer.Length < 1)
            {
                Log.LogMessage(DriverId, "SendSerialPortCommand() : null buffer");
                throw new System.ArgumentNullException(DriverId + ": SendSerialPortCommand() : null buffer");
            }
            if (!Scope.isConnected || !RawEcho() && !ScopeReconnect())
            {
                RxBuffer = new byte[32];
                return -1;
            }
            try
            {
                lock (Sending)
                {
                    ScopeSerialPort.ClearBuffers();
                    ScopeSerialPort.TransmitBinary(TxBuffer);
                    RxBuffer = ScopeSerialPort.ReceiveCountedBinary(Count);
                }
            }
            catch (TimeoutException Ex)
            {
                Log.LogMessage(DriverId, "SendSerialPortCommand() : " + Ex.Message);
                RxBuffer = new byte[32];
                return 0;
            }
            catch (Exception Ex)
            {
                Log.LogMessage(DriverId, "SendSerialPortCommand() : " + Ex.Message);
                throw;
            }
            return 1;
        }

        private static bool GetMotorVersion(out int Azm, out int Alt)
        {
            DevVersion Version;
            GetDeviceVersion(eDeviceId.ALT, out Version);
            Alt = (Version.Major << 8) + Version.Minor;
            GetDeviceVersion(eDeviceId.AZM, out Version);
            Azm = (Version.Major << 8) + Version.Minor;
            return true;
        }

        private static bool isGuiding()
        /* P command supported from version 1.6 */
        {
            /* TODO: untested debug */
            if (Scope.isConnected && Scope.isAligned)
            {
                byte RxLength = 2; /* expected bytes to recieve including terminator "#" */
                byte[] TxBuffer = { (byte)'P', 1, (byte)eDeviceId.AZM, 39, 0, 0, 0, (byte)(RxLength - 1) };
                byte[] RxBuffer = { };
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    if (RxBuffer[0] == 1)
                    {
                        return RxBuffer[0] == 1;
                    }
                    else
                    {
                        TxBuffer[2] += 1; /* incriment device id */
                        if (SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                        {
                            SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                        }
                        if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                        {
                            return RxBuffer[0] == 1;
                        }
                    }
                }
            }
            return false;
        }

        #region sync

        public static void Sync(double Ra, double Dec)
        {
            PreciseSyncRaDec(Ra, Dec);
        }

        private static bool SyncRaDec(double Ra, double Dec)
        /* supported from version 4.10 */
        {
            if (Scope.isConnected && Scope.Version >= Version_41)
            {
                short RxLength = 1;
                byte[] TxBuffer = { (byte)'S', 0, 0, 0, 0, (byte)',', 0, 0, 0, 0 };
                byte[] RxBuffer = { };
                byte[] x = new byte[4];
                DegToHex(Ra * 15d, ref x);
                x.CopyTo(TxBuffer, 1);
                DegToHex(Dec, ref x);
                x.CopyTo(TxBuffer, 6);
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    return RxBuffer[0] == (byte)'#';
                }
            }
            return false;
        }

        private static bool PreciseSyncRaDec(double Ra, double Dec)
        /* supported from version 4.10 */
        {
            if (Scope.isConnected && Scope.Version >= Version_41)
            {
                short RxLength = 1;
                byte[] TxBuffer = { (byte)'s', 0, 0, 0, 0, 0, 0, 0, 0, (byte)',', 0, 0, 0, 0, 0, 0, 0, 0 };
                byte[] RxBuffer = { };
                byte[] x = new byte[6];
                DegToHex(Ra * 15d, ref x);
                x.CopyTo(TxBuffer, 1);
                DegToHex(Dec, ref x);
                x.CopyTo(TxBuffer, 10);
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    return RxBuffer[0] == (byte)'#';
                }
            }
            return false;
        }

        #endregion

        #region goto

        public static void SlewToRaDec(double Ra, double Dec)
        {
            if (PreciseGotoRaDec(Ra, Dec))
            {
                return;
            }
            else if (GotoRaDec(Ra, Dec))
            {
                return;
            }
            Log.LogMessage(DriverId, "SlewToRaDec() : not implimented");
            throw new ASCOM.NotImplementedException(DriverId + ": SlewToRaDec()");
        }

        public static void SlewToAzmAlt(double Azm, double Alt)
        {
            if (PreciseGotoAzmAlt(Azm, Alt))
            {
                return;
            }
            else if (GotoAzmAlt(Azm, Alt))
            {
                return;
            }
            Log.LogMessage(DriverId, "SlewToAzmAlt() : not implimented");
            throw new ASCOM.NotImplementedException(DriverId + ": SlewToAzmAlt()");
        }

        private static bool GotoRaDec(double Ra, double Dec)
        /* supported from version 1.2 */
        {
            if (Scope.isConnected && Scope.isAligned)
            {
                if (Scope.isSlewing)
                {
                    AbortSlew();
                }
                byte RxLength = 1;
                byte[] TxBuffer = { (byte)'R', 0, 0, 0, 0, (byte)',', 0, 0, 0, 0 };
                byte[] RxBuffer = { };
                byte[] x = new byte[4];
                DegToHex(Ra * 15d, ref x);
                x.CopyTo(TxBuffer, 1);
                DegToHex(Dec, ref x);
                x.CopyTo(TxBuffer, 6);
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[0] == (byte)'#')
                {
                    Scope.isSlewing = true;
                    GotoMonitor();
                    return RxBuffer[0] == (byte)'#';
                }
            }
            return false;
        }

        private static bool GotoAzmAlt(double Azm, double Alt)
        /* supported from version 1.2 */
        {
            if (Scope.isConnected && Scope.isAligned)
            {
                if (Scope.isSlewing)
                {
                    AbortSlew();
                }
                byte RxLength = 1;
                byte[] TxBuffer = { (byte)'B', 0, 0, 0, 0, (byte)',', 0, 0, 0, 0 };
                byte[] RxBuffer = { };
                byte[] x = new byte[4];
                DegToHex(Azm, ref x);
                x.CopyTo(TxBuffer, 1);
                DegToHex(Alt, ref x);
                x.CopyTo(TxBuffer, 6);
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[0] == (byte)'#')
                {
                    Scope.isSlewing = true;
                    GotoMonitor();
                    return RxBuffer[0] == (byte)'#';
                }
            }
            return false;
        }

        private static bool PreciseGotoRaDec(double Ra, double Dec)
        /* supported from version 1.6 */
        {
            if (Scope.isConnected && Scope.Version >= Version_16)
            {
                if (Scope.isSlewing)
                {
                    AbortSlew();
                }
                byte RxLength = 1;
                byte[] TxBuffer = { (byte)'r', 0, 0, 0, 0, 0, 0, 0, 0, (byte)',', 0, 0, 0, 0, 0, 0, 0, 0 };
                byte[] RxBuffer = { };
                byte[] x = new byte[6];
                DegToHex(Ra * 15, ref x);
                x.CopyTo(TxBuffer, 1);
                DegToHex(Dec, ref x);
                x.CopyTo(TxBuffer, 10);
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[0] == (byte)'#')
                {
                    Scope.isSlewing = true;
                    GotoMonitor();
                    return RxBuffer[0] == (byte)'#';
                }
            }
            return false;
        }

        private static bool PreciseGotoAzmAlt(double Azm, double Alt)
        /* supported from version 2.2 */
        {
            if (Scope.isConnected && Scope.Version >= Version_22)
            {
                if (Scope.isSlewing)
                {
                    AbortSlew();
                }
                byte RxLength = 1;
                byte[] TxBuffer = { (byte)'b', 0, 0, 0, 0, 0, 0, 0, 0, (byte)',', 0, 0, 0, 0, 0, 0, 0, 0 };
                byte[] RxBuffer = { };
                byte[] x = new byte[6];
                DegToHex(Azm, ref x);
                x.CopyTo(TxBuffer, 1);
                DegToHex(Alt, ref x);
                x.CopyTo(TxBuffer, 10);
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[0] == (byte)'#')
                {
                    Scope.isSlewing = true;
                    GotoMonitor();
                    return RxBuffer[0] == (byte)'#';
                }
            }
            return false;
        }

        private static void GotoMonitor()
        {
            GotoWatcher = new Thread(() =>
                {
                    while (Scope.isConnected && isSlewing())
                    {
                        Thread.Sleep(500);
                    }
                    Scope.isSlewing = false;
                });
            GotoWatcher.Start();
        }

        #endregion

        #region time, date, location

        private static bool GetScopeLst(out double SiderealTime)
        /* gets the sidereal time from the scope */
        {
            /* CPC returns garbage if not aligned */
            if (Scope.isConnected &&
                Scope.Version >= Version_412 &&
                Scope.isAligned)
            {
                /* get sidereal time from scope */
                short RxLength = 9; /* total expected length including terminator */
                byte[] TxBuffer = { (byte)'l' };
                byte[] RxBuffer = { };
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    /* decode the buffer */
                    double x = 0;
                    UInt32 l = 0;
                    HexToByte(ref RxBuffer);
                    for (int i = 0; i < RxLength - 1; i++)
                    {
                        /* ignore bytes not 0-F */
                        if (RxBuffer[i] >= 0x00 && RxBuffer[i] <= 0x0F)
                        {
                            l = (l << 4);
                            l += RxBuffer[i];
                        }
                    }
                    x = l * (360d / 0x100000000);
                    SiderealTime = DriverMath.DegHr(x);
                    return true;
                }
            }
            SiderealTime = 0;
            return false;
        }

        public static DateTime GetLstDateTime()
        /* get sidereal time as DateTime */
        {
            double st = 0;
            int Hours = 0;
            int Minutes = 0;
            int Seconds = 0;
            st = GetLst();
            Hours = (int)Math.Floor(st);
            Minutes = (int)Math.Floor((st - Hours) * 60);
            Seconds = (int)Math.Floor((((st - Hours) * 60) - Minutes) * 60);
            DateTime SiderealTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, Hours, Minutes, Seconds);
            SiderealTime = DateTime.SpecifyKind(SiderealTime, DateTimeKind.Local);
            return SiderealTime;
        }

        public static double GetLst()
        /* get sidereal time as double*/
        {
            double st = 0;
            if (GetScopeLst(out st))
            {
                return st;
            }
            else
            {
                DateTime dt = new DateTime();
                double JD = 0;
                double J2k = 0;
                double lst = 0;
                if (Scope.Longitude == -1)
                {
                    Scope.Longitude = GetLongitude();
                }
                dt = GetUtcDate();
                JD = DriverMath.ToJulianDate(dt);
                J2k = DriverMath.ToJ2000(JD);
                DriverMath.UtcGst(DriverMath.MjdDay(J2k), DriverMath.MjdHr(J2k), out lst);
                lst += DriverMath.DegHr(Scope.Longitude);
                DriverMath.Range(ref lst, 24.0);
                return lst;
            }
        }

        public static DateTime GetUtcDate()
        {
            DateTime dt = new DateTime();
            if (!Scope.isGuiding && Scope.HasGps && Scope.GpsState == 1 && Scope.GpsTimeValid &&
                ScopeGps.GetDateTime(out dt))
            {
                // nothing to see here move along
            }
            else
            {
                // get UTC from system clock
                dt = DateTime.UtcNow;
            }
            return dt;
        }

        public static void SetUtcDate(DateTime Utc)
        {
            HcDateTime dt;
            TimeZoneInfo TzLocal = TimeZoneInfo.Local;
            dt.TimeZone = (byte)(TzLocal.BaseUtcOffset.Hours & 0xFF);
            dt.Dst = Convert.ToByte(TzLocal.IsDaylightSavingTime(Utc));
            dt.Year = (byte)(Utc.Year - 2000);
            dt.Month = (byte)Utc.Month;
            dt.Day = (byte)Utc.Day;
            dt.Hours = (byte)(Utc.Hour + dt.TimeZone + dt.Dst);
            dt.Minutes = (byte)Utc.Minute;
            dt.Seconds = (byte)Utc.Second;
            SetTime(dt);
        }

        private static bool GetLocation(out double Latitude, out double Longitude)
        {
            HcLocation Hcl;
            if (GetLocation(out Hcl))
            {
                Latitude = (double)Hcl.DegLatitude;
                Latitude += Convert.ToDouble(Hcl.MinLatitude) / 60d;
                Latitude += Convert.ToDouble(Hcl.SecLatitude) / 3600d;
                if (Convert.ToBoolean(Hcl.NS))
                {
                    Latitude = -Math.Abs(Latitude);
                }
                Longitude = Hcl.DegLongitude;
                Longitude += Convert.ToDouble(Hcl.MinLongitude) / 60d;
                Longitude += Convert.ToDouble(Hcl.SecLongitude) / 3600d;
                if (Convert.ToBoolean(Hcl.EW))
                {
                    Longitude = -Math.Abs(Longitude);
                }
                return true;
            }
            Latitude = 0;
            Longitude = 0;
            return false;
        }

        private static bool GetLocation(out HcLocation Location)
        {
            if (Scope.isConnected && Scope.Version >= Version_23)
            {
                short RxLength = 9;
                byte[] TxBuffer = { (byte)'w' };
                byte[] RxBuffer = { };
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    Location.DegLatitude = RxBuffer[0];
                    Location.MinLatitude = RxBuffer[1];
                    Location.SecLatitude = RxBuffer[2];
                    Location.NS = RxBuffer[3];
                    Location.DegLongitude = RxBuffer[4];
                    Location.MinLongitude = RxBuffer[5];
                    Location.SecLongitude = RxBuffer[6];
                    Location.EW = RxBuffer[7];
                    return true;
                }
            }
            Location = new HcLocation();
            return false;
        }

        private static bool GetTime(out DateTime DateTime)
        {
            HcDateTime dt;
            if (GetTime(out dt))
            {
                DateTime = new DateTime(dt.Year + 2000, dt.Month, dt.Day, dt.Hours, dt.Minutes, dt.Seconds, DateTimeKind.Local);
                return true;
            }
            DateTime = new DateTime();
            return false;
        }

        private static bool GetTime(out HcDateTime DateTime)
        {
            if (Scope.isConnected && Scope.Version >= Version_23)
            {
                short RxLength = 9;
                byte[] TxBuffer = { (byte)'h' };
                byte[] RxBuffer = { };
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    DateTime.Hours = RxBuffer[0];
                    DateTime.Minutes = RxBuffer[1];
                    DateTime.Seconds = RxBuffer[2];
                    DateTime.Month = RxBuffer[3];
                    DateTime.Day = RxBuffer[4];
                    DateTime.Year = RxBuffer[5];
                    DateTime.TimeZone = RxBuffer[6];
                    DateTime.Dst = RxBuffer[7];
                    return true;
                }
            }
            DateTime = new HcDateTime();
            return false;
        }

        private static bool SetLocation(double Latitude, double Longitude)
        {
            HcLocation Hcl;
            double d, m, s;
            DriverMath.ConvertCoordinate(Math.Abs(Latitude), out d, out m, out s);
            Hcl.DegLatitude = (byte)d;
            Hcl.MinLatitude = (byte)m;
            Hcl.SecLatitude = (byte)s;
            Hcl.NS = Convert.ToByte(Latitude < 0);
            DriverMath.ConvertCoordinate(Math.Abs(Longitude), out d, out m, out s);
            Hcl.DegLongitude = (byte)d;
            Hcl.MinLongitude = (byte)m;
            Hcl.SecLongitude = (byte)s;
            Hcl.EW = Convert.ToByte(Longitude < 0);
            if (SetLocation(Hcl))
            {
                return true;
            }
            return false;
        }

        public static bool SetLongitude(double Longitude)
        {
            if (Longitude < -180 || Longitude > 180)
            {
                Log.LogMessage(DriverId, "SetLongitude() : invalid value " + Longitude.ToString());
                throw new ASCOM.InvalidValueException(DriverId + ": SetLatitude() : " + Longitude.ToString());
            }
            HcLocation Hcl;
            double d, m, s;
            GetLocation(out Hcl);
            DriverMath.ConvertCoordinate(Math.Abs(Longitude), out d, out m, out s);
            Hcl.DegLongitude = (byte)d;
            Hcl.MinLongitude = (byte)m;
            Hcl.SecLongitude = (byte)s;
            Hcl.EW = Convert.ToByte(Longitude < 0);
            Scope.Longitude = Longitude;
            if (SetLocation(Hcl))
            {
                return true;
            }
            Log.LogMessage(DriverId, "SetLongitude() : not implimented");
            throw new ASCOM.NotImplementedException(DriverId + ": SetLongitude()");
        }

        public static double GetLongitude()
        {
            double Longitude = 0;
            double Latitude = 0;
            if (!Scope.isGuiding && Scope.GpsState == 1 &&
                Scope.GpsTimeValid && ScopeGps.GetLongitude(out Longitude))
            {
                return Longitude;
            }
            else if (GetLocation(out Latitude, out Longitude))
            {
                return Longitude;
            }
            else
            {
                string str;
                str = ScopeProfile.GetValue(DriverId, PROFILE_SITE_LONGITUDE);
                double.TryParse(str, out Longitude);
                return Longitude;
            }
        }

        public static bool SetLatitude(double Latitude)
        {
            if (Latitude < -90 || Latitude > 90)
            {
                Log.LogMessage(DriverId, "SetLatitude() : invalid value " + Latitude.ToString());
                throw new ASCOM.InvalidValueException(DriverId + ": SetLatitude() " + Latitude.ToString());
            }
            HcLocation Hcl;
            double d, m, s;
            GetLocation(out Hcl);
            DriverMath.ConvertCoordinate(Math.Abs(Latitude), out d, out m, out s);
            Hcl.DegLatitude = (byte)d;
            Hcl.MinLatitude = (byte)m;
            Hcl.SecLatitude = (byte)s;
            Hcl.NS = Convert.ToByte(Latitude < 0);
            Scope.Latitude = Latitude;
            if (SetLocation(Hcl))
            {
                return true;
            }
            Log.LogMessage(DriverId, "SetLatitude() : not implimented");
            throw new ASCOM.NotImplementedException(DriverId + ": SetLatitude()");
        }

        public static double GetLatitude()
        {
            double Longitude = 0;
            double Latitude = 0;
            if (!Scope.isGuiding && Scope.GpsState == 1 &&
                Scope.GpsTimeValid && ScopeGps.GetLatitude(out Latitude))
            {
                return Latitude;
            }
            else if (GetLocation(out Latitude, out Longitude))
            {
                return Latitude;
            }
            else
            {
                string str;
                str = ScopeProfile.GetValue(DriverId, PROFILE_SITE_LATITUDE);
                double.TryParse(str, out Latitude);
                return Latitude;
            }
        }

        public static void SetElevation(double Elevation)
        {
            if (Elevation < -300 || Elevation > 10000)
            {
                Log.LogMessage(DriverId, "SetElevation() : invalid value " + Elevation.ToString());
                throw new ASCOM.InvalidValueException(DriverId + ": SetElevation()");
            }
            Scope.Elevation = Elevation;
        }

        private static bool SetLocation(HcLocation Location)
        /* NS: North = 0, South = 1 */
        /* EW: East = 0, West = 1 */
        {
            if (Scope.isConnected && Scope.Version >= Version_23)
            {
                byte[] TxBuffer = { (byte)'W', Location.DegLatitude, Location.MinLatitude, Location.SecLatitude, Location.NS,
                                            Location.DegLongitude, Location.MinLongitude, Location.SecLongitude, Location.EW };
                byte[] RxBuffer = { };
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                return RxBuffer[0] == (byte)'#';
            }
            return false;
        }

        private static bool SetTime(HcDateTime DateTime)
        /* Hours: 24h clock */
        /* TimeZone: -8 UTC = 256-8 */
        /* Dst: 1 = Daylight Savings, 0 = Standard Time */
        {
            if (Scope.isConnected && Scope.Version >= Version_23)
            {
                byte[] TxBuffer = { (byte)'H', DateTime.Hours, DateTime.Minutes, DateTime.Seconds,
                          DateTime.Month, DateTime.Day, DateTime.Year, DateTime.TimeZone, DateTime.Dst };
                byte[] RxBuffer = { };
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                return RxBuffer[0] == (byte)'#';
            }
            return false;
        }

        #endregion

        #region position

        private static bool GetRightAscensionDeclination(out double Ra, out double Dec)
        {
            if (!Scope.isAligned)
            {
                Ra = 0;
                Dec = 0;
                return true;
            }
            if (GetPreciseRaDec(out Ra, out Dec))
            {
                return true;
            }
            else if (GetRaDec(out Ra, out Dec))
            {
                return true;
            }
            Log.LogMessage(DriverId, "GetRightAscensionDeclination() : not implimented");
            throw new ASCOM.NotImplementedException(DriverId + ": GetRightAscensionDeclination()");
        }

        private static bool GetRaDec(out double Ra, out double Dec)
        {
            if (Scope.isConnected && Scope.Version >= Version_16)
            {
                short RxLength = 10; /* total expected buffer length including terminator "#" */
                byte[] TxBuffer = { (byte)'E' };
                byte[] RxBuffer = { };
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength);
                }
                /* sanity check on recieve buffer */
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    UInt32 R = 0, D = 0;
                    int i = 0;
                    HexToByte(ref RxBuffer);
                    while (RxBuffer[i] >= 0 && RxBuffer[i] <= 15)
                    {
                        R = (R << 4) + RxBuffer[i];
                        i++;
                    }
                    i++;
                    while (RxBuffer[i] >= 0 && RxBuffer[i] <= 15)
                    {
                        D = (D << 4) + RxBuffer[i];
                        i++;
                    }
                    Ra = R * (360d / 0x10000);
                    Dec = D * (360d / 0x10000);
                    DegRange(ref Ra);
                    DegRange(ref Dec);
                    Ra /= 15d;
                    if (Dec > 180d)
                    {
                        Dec -= 360d;
                    }
                    return true;
                }
            }
            Ra = 0;
            Dec = 0;
            return false;
        }

        private static bool GetPreciseRaDec(out double Ra, out double Dec)
        {
            if (Scope.isConnected && Scope.Version >= Version_16)
            {
                short RxLength = 18;
                byte[] TxBuffer = { (byte)'e' };
                byte[] RxBuffer = { };
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    UInt32 R = 0, D = 0;
                    int i = 0;
                    HexToByte(ref RxBuffer);
                    while (RxBuffer[i] >= 0 && RxBuffer[i] <= 15)
                    {
                        R = (R << 4) + RxBuffer[i];
                        i++;
                    }
                    i++;
                    while (RxBuffer[i] >= 0 && RxBuffer[i] <= 15)
                    {
                        D = (D << 4) + RxBuffer[i];
                        i++;
                    }
                    Ra = R * (360d / 0x100000000);
                    Dec = D * (360d / 0x100000000);
                    DegRange(ref Ra);
                    DegRange(ref Dec);
                    Ra /= 15d;
                    if (Dec > 180d)
                    {
                        Dec -= 360d;
                    }
                    return true;
                }
            }
            Ra = 0;
            Dec = 0;
            return false;
        }

        private static bool GetAzimuthAltitude(out double Azm, out double Alt)
        {
            if (GetPreciseAzmAlt(out Azm, out Alt))
            {
                return true;
            }
            else if (GetAzmAlt(out Azm, out Alt))
            {
                return true;
            }
            Log.LogMessage(DriverId, "GetAzimuthAltitude() : not implimented");
            throw new ASCOM.NotImplementedException(DriverId + ": GetAzimuthAltitude()");
        }

        private static bool GetAzmAlt(out double Azm, out double Alt)
        {
            if (Scope.isConnected && Scope.Version >= Version_16)
            {
                short RxLength = 10;
                byte[] TxBuffer = { (byte)'Z' };
                byte[] RxBuffer = { };
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    UInt32 R = 0, D = 0;
                    int i = 0;
                    HexToByte(ref RxBuffer);
                    while (RxBuffer[i] >= 0 && RxBuffer[i] <= 15)
                    {
                        R = (R << 4) + RxBuffer[i];
                        i++;
                    }
                    i++;
                    while (RxBuffer[i] >= 0 && RxBuffer[i] <= 15)
                    {
                        D = (D << 4) + RxBuffer[i];
                        i++;
                    }
                    Azm = R * (360d / 0x10000);
                    Alt = D * (360d / 0x10000);
                    if (Azm < 0d)
                    {
                        Azm += 360d;
                    }
                    if (Alt > 180d)
                    {
                        Alt -= 360d;
                    }
                    return true;
                }
            }
            Azm = 0;
            Alt = 0;
            return false;
        }

        private static bool GetPreciseAzmAlt(out double Azm, out double Alt)
        {
            if (Scope.isConnected && Scope.Version >= Version_22)
            {
                short RxLength = 18;
                byte[] TxBuffer = { (byte)'z' };
                byte[] RxBuffer = { };
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    UInt32 R = 0, D = 0;
                    int i = 0;
                    HexToByte(ref RxBuffer);
                    while (RxBuffer[i] >= 0 && RxBuffer[i] <= 15)
                    {
                        R = (R << 4) + RxBuffer[i];
                        i++;
                    }
                    i++;
                    while (RxBuffer[i] >= 0 && RxBuffer[i] <= 15)
                    {
                        D = (D << 4) + RxBuffer[i];
                        i++;
                    }
                    Azm = R * (360d / 0x100000000);
                    Alt = D * (360d / 0x100000000);
                    if (Azm < 0d)
                    {
                        Azm += 360d;
                    }
                    if (Alt > 180d)
                    {
                        Alt -= 360d;
                    }
                    return true;
                }
            }
            Azm = 0;
            Alt = 0;
            return false;
        }

        #endregion

        #region event receivers

        private static void GpsErrorReciever(object sender, EventArgs<int> e)
        /* this event signals an error communicating with the GPS device */
        /* the device may not exist? as a result the GpsThread has quit  */
        {
            if ((GpsThread.GpsEvent)sender == GpsThread.GpsEvent.Error &&
                e.Value == -1)
            {
                Scope.HasGps = false;
                Scope.GpsTimeValid = false;
                Scope.GpsState = e.Value;
            }
        }

        private static void GpsTimeValidReciever(object sender, EventArgs<bool> e)
        /* this event signals that the GPS is linked & the time is valid, the   */
        /* GpsThread should send this event every hour if the GPS is linked and */
        /* the time is valid, refresh the global lat/long fields */
        {
            if ((GpsThread.GpsEvent)sender == GpsThread.GpsEvent.TimeValid)
            {
                double longitude;
                double latitude;
                Scope.HasGps = true;
                Scope.GpsTimeValid = e.Value;
                /* try not to block pulse guiding by using the
                /* serial port to communicate with the GPS device */
                if (!Scope.isGuiding)
                {
                    ScopeGps.GetLongitude(out longitude);
                    ScopeGps.GetLatitude(out latitude);
                    Scope.Longitude = longitude;
                    Scope.Latitude = latitude;
                }
            }
        }

        private static void GpsLinkReciever(object sender, EventArgs<int> e)
        /* this event signals that the GPS device is reporting linked */
        {
            if ((GpsThread.GpsEvent)sender == GpsThread.GpsEvent.Link)
            {
                Scope.HasGps = true;
                Scope.GpsState = e.Value;
            }
        }

        private static void ScopeConnectReciever(object sender, EventArgs<bool> e)
        /* this event signals that the driver has connected to the telescope */
        /* and we can start querying the state of the telescope devices      */
        {
            if ((eScopeEvent)sender == eScopeEvent.Connection && e.Value == true)
            {
                /* get version first due to range checking in most other methods. */
                GetScopeVersion(out Scope.Version);
                GetScopeModel(out Scope.Model);
                SetTracking(true);
                Scope.isAligned = isAligned();
                Scope.AlignmentMode = GetAlignmentMode();
                if (Scope.AlignmentMode == AlignmentModes.algPolar ||
                    Scope.AlignmentMode == AlignmentModes.algGermanPolar)
                {
                    Scope.TrackingRates = new TrackingRates(DriveRates.driveLunar, DriveRates.driveSolar);
                }
                Scope.GuideRates = new AxisRates[2];
                Scope.GuideRates[0] = new AxisRates(TelescopeAxes.axisPrimary, new Rate(0, SIDEREAL_RATE_DEG_SEC));
                Scope.GuideRates[1] = new AxisRates(TelescopeAxes.axisSecondary, new Rate(0, SIDEREAL_RATE_DEG_SEC));
                Scope.AxisRates = new AxisRates[3];
                Scope.AxisRates[0] = new AxisRates(TelescopeAxes.axisPrimary, new Rate(0, 4.5));
                Scope.AxisRates[1] = new AxisRates(TelescopeAxes.axisSecondary, new Rate(0, 4.5));
                Scope.AxisRates[2] = new AxisRates(TelescopeAxes.axisTertiary);
                Scope.TargetDecSet = false;
                Scope.TargetRaSet = false;
                Scope.TargetDec = 0;
                Scope.TargetRa = 0;
                Scope.Name = "Celestron " + Scope.ModelName;
                Scope.isSlewing = isSlewing();
                EnablePec(true);
                GetMotorVersion(out Scope.AzmVersion, out Scope.AltVersion);
                Scope.Latitude = GetLatitude();
                Scope.Longitude = GetLongitude();
                /* speed up connect time by a few seconds by starting GPS after GetLatitude/GetLongitude */
                ScopeGpsThread.Start();
                Log.LogMessage(DriverId, "ScopeConnectReciever() : found " + Scope.Name + " " + (Scope.Version >> 8).ToString() + "." + (Scope.Version % 0x100).ToString() + " on COM" + Scope.ConnectedPort.ToString());
                ScopePulseGuide = new PulseGuide();
                ScopePulseGuide.Enabled = true;
#if DEBUG
                HC.Start();
#endif
            }
        }

        private static void UpdateProfileReciever(object sender, EventArgs<string, string> e)
        {
            ScopeProfile.WriteValue(DriverId, e.ValueA, e.ValueB);
        }

        private static void ProcessExit(object sender, EventArgs e)
        /* this is a work around for unmanaged clients that do not */
        /* set connected property when disconnecting e.g. PHD Guiding */
        /* http://stackoverflow.com/questions/1724694/how-to-dispose-of-a-net-com-interop-object-on-release */
        {
            if (Scope.isConnected)
            {
                ScopeConnect(false);
            }
        }

        #endregion

        #region saved properties

        public static short GetHcRate()
        {
            string str;
            short value;
            str = ScopeProfile.GetValue(DriverId, PROFILE_HC_RATE);
            short.TryParse(str, out value);
            return value;
        }

        public static void SetHcRate(short Rate)
        {
            ScopeProfile.WriteValue(DriverId, PROFILE_HC_RATE, Rate.ToString());
        }

        public static double GetApertureArea()
        {
            string str;
            double value;
            str = ScopeProfile.GetValue(DriverId, PROFILE_APERTURE_AREA);
            double.TryParse(str, out value);
            return value;
        }

        public static double GetApertureDiameter()
        {
            string str;
            double value;
            str = ScopeProfile.GetValue(DriverId, PROFILE_APERTURE_DIAMETER);
            double.TryParse(str, out value);
            return value;
        }

        public static short GetSerialPort()
        {
            string str;
            short value;
            str = ScopeProfile.GetValue(DriverId, PROFILE_COM_PORT);
            short.TryParse(str, out value);
            return value;
        }

        public static double GetFocalLength()
        {
            string str;
            double value;
            str = ScopeProfile.GetValue(DriverId, PROFILE_FOCAL_LENGTH);
            double.TryParse(str, out value);
            return value;
        }

        public static double GetElevation()
        {
            string str;
            double value;
            str = ScopeProfile.GetValue(DriverId, PROFILE_SITE_ELEVATION);
            double.TryParse(str, out value);
            return value;
        }

        public static eTrackingMode GetTrackMode()
        {
            string str;
            int value;
            str = ScopeProfile.GetValue(DriverId, PROFILE_TRACK_MODE);
            int.TryParse(str, out value);
            return (eTrackingMode)value;
        }

        #endregion

        private static short[] GetSerialPorts()
        /* returns an array of short representing the serial      */
        /* ports avialble on the system, if a valid saved setting */
        /* for serial port is found it is positioned first in     */
        /* the array.                                             */
        {
            Regex rgx = new Regex("[^0-9]");
            string[] ports = { };
            short[] numbers = { };
            short[] ret = { };
            ports = System.IO.Ports.SerialPort.GetPortNames();
            bool PortExists = false;
            int i = 0;
            short portint = GetSerialPort();
            string portstr = "COM" + portint.ToString();
            short portnum = 0;

            /* test if port in settings exists on system */
            foreach (string port in ports)
            {
                if (portstr == port)
                {
                    PortExists = true;
                    break;
                }
            }
            /* port does not exist set to 0 */
            if (!PortExists)
            {
                Scope.ConnectedPort = 0;
                portint = 0;
            }
            /* if saved port is valid position it first in the array */
            if ((portint > 0) && (portint <= 256))
            {
                numbers = new short[1 + ports.Length];
                numbers[0] = portint;
                i = 1;
            }
            else
            {
                numbers = new short[ports.Length];
                i = 0;
            }
            /* convert string "COMxxx" to short and add to array */
            foreach (string port in ports)
            {
                if (port.Length >= 4 && port.Length <= 6)
                {
                    portnum = Convert.ToInt16(rgx.Replace(port, ""));
                    /* skip the port added from the saved settings */
                    if (portnum != numbers[0] && portnum > 0 && portnum <= 256)
                    {
                        numbers[i] = portnum;
                        i++;
                    }
                }
            }
            /* some ports (invalid/saved setting) may have been */
            /* filtered so construct the array to return to caller */
            ret = new short[i];
            for (int n = 0; n < ret.Length; n++)
            {
                ret[n] = numbers[n];
            }
            ports = null;
            numbers = null;
            return ret;
        }

        private static void HcThread()
        /* thread for the virtual hand control */
        {
            Application.EnableVisualStyles();
            HCWindow = new HandController();
            HCWindow.Show();
            while (Scope.isConnected)
            {
                Application.DoEvents();
                Thread.Sleep(50);
            }
            HCWindow.Close();
            HCWindow.Dispose();
        }

        private static bool ScopeReconnect()
        /* spawns a new thread to reconnect to the scope */
        {
            Thread t = null;
            /* prevent spawning multiple reconnect threads */
            if (Scope.Reconnecting)
            {
                return false;
            }
            t = new Thread(() =>
            {
                int Count = 0;
                int Attempts = 60;
                byte[] TxBuffer = { (byte)'K', (byte)'A' };
                byte[] RxBuffer = { 0x00, 0x00 };
                lock (Sending)
                {
                    for (Count = 0; Count <= Attempts; Count++)
                    {
                        try
                        {
                            ScopeSerialPort.Connected = false;
                            ScopeSerialPort = null;
                            GC.Collect();
                            ScopeSerialPort = new Serial();
                            ScopeSerialPort.ReceiveTimeout = 4;
                            ScopeSerialPort.Port = Scope.ConnectedPort;
                            ScopeSerialPort.Speed = SerialSpeed.ps9600;
                            ScopeSerialPort.StopBits = SerialStopBits.One;
                            ScopeSerialPort.Handshake = SerialHandshake.None;
                            ScopeSerialPort.DataBits = 8;
                            ScopeSerialPort.Connected = true;
                            /* ping the scope */
                            ScopeSerialPort.ClearBuffers();
                            ScopeSerialPort.TransmitBinary(TxBuffer);
                            RxBuffer = ScopeSerialPort.ReceiveTerminatedBinary(Terminator);
                        }
                        catch (Exception Ex)
                        {
                            /* swallow all the exceptions */
                            Log.LogMessage(DriverId, "ScopeReconnect() : " + Ex.Message);
                            Thread.Sleep(2000);
                        }
                        if (RxBuffer[0] == (byte)'A' || !Scope.Reconnecting)
                        {
                            /* quit the loop if ping reply, or user clicked disconnect */
                            break;
                        }
                    } /* end for loop */
                } /* end lock */
                if (RxBuffer[0] != (byte)'A')
                {
                    /* failure! everything is a mess, try to cleanup */
                    /* cause threads to quit, prevent any further serial port comunication */
                    Scope.isConnected = false;
                    /* log the error and throw an exception */
                    Log.LogMessage(DriverId, "ScopeReconnect() : communication with the telescope failed");
                    throw new ASCOM.NotConnectedException(DriverId + ": ScopeReconnect() : communication with the telescope failed");
                }
            });
            t.Name = "Reconnect Thread";
            t.IsBackground = true;
            t.Priority = ThreadPriority.Lowest;
            t.Start();
            Scope.Reconnecting = true;
            t.Join();
            t = null;
            Scope.Reconnecting = false;
            if (RawEcho())
            {
                /* wait 30 sec for any blocked commands to complete then stop any fixed slew */
                FailSafeStop(30);
                return true;
            }
            /* we tried to reconnect 60 times no luck, hope the scope isn't slewing :-< */
            return false;
        }

        private static void FailSafeStop(int Seconds)
        /* spawns a new thread to ensure the scope is stopped */
        {
            if (FAILSAFEINSTANCE == 0)
            {
                Thread FAILSAFE = new Thread(() =>
                {
                    int Count = 0;
                    List<byte[]> CommandList = new List<byte[]>();
                    byte[] A = { (byte)'P', 2, (byte)eDeviceId.ALT, (byte)eDirection.Positive, 0, 0, 0, 0 };
                    byte[] B = { (byte)'P', 2, (byte)eDeviceId.AZM, (byte)eDirection.Positive, 0, 0, 0, 0 };
                    byte[] RxBuffer = { };
                    CommandList.Add(A);
                    CommandList.Add(B);
                    while (Count < Seconds * 2 && Scope.isConnected && Scope.isSlewing)
                    {
                        Thread.Sleep(500);
                        Count++;
                    }
                    if (Scope.isConnected && Scope.isSlewing)
                    {
                        lock (Sending)
                        {
                            foreach (byte[] Command in CommandList)
                            {
                                ScopeSerialPort.ClearBuffers();
                                ScopeSerialPort.TransmitBinary(Command);
                                RxBuffer = ScopeSerialPort.ReceiveTerminatedBinary(Terminator);
                            }
                            byte[] TxBuffer1 = { (byte)'T', (byte)Scope.TrackingMode };
                            ScopeSerialPort.TransmitBinary(TxBuffer1);
                            RxBuffer = ScopeSerialPort.ReceiveTerminatedBinary(Terminator);
                        }
                    }
                    FAILSAFEINSTANCE--;
                });
                FAILSAFE.Name = "Slew Fail Safe";
                FAILSAFE.IsBackground = true;
                FAILSAFE.Priority = ThreadPriority.Lowest;
                FAILSAFE.Start();
                FAILSAFEINSTANCE++;
            }
        }

        private static void HexToByte(ref byte[] Buffer)
        {
            for (int i = 0; i < Buffer.Length; i++)
            {
                if (Buffer[i] >= (byte)'0' && Buffer[i] <= (byte)'9')
                {
                    Buffer[i] -= (byte)'0';
                }
                else if (Buffer[i] >= (byte)'A' && Buffer[i] <= (byte)'F')
                {
                    Buffer[i] -= (byte)'A';
                    Buffer[i] += 10;
                }
            }
            return;
        }

        private static void ByteToHex(ref byte[] Buffer)
        {
            for (int i = 0; i < Buffer.Length; i++)
            {
                if (Buffer[i] >= 0 && Buffer[i] <= 9)
                {
                    Buffer[i] += (byte)'0';
                }
                else if (Buffer[i] >= 10 && Buffer[i] <= 15)
                {
                    Buffer[i] += (byte)'A';
                }
            }
            return;
        }

        private static void DegToHex(double Degrees, ref byte[] Hex)
        {
            byte a;
            Int64 b;
            if (Degrees >= 0)
            {
                b = (Int64)(Degrees * (0x1000000 / 360d));
            }
            else
            {
                b = (Int64)((360d + Degrees) * (0x1000000 / 360d));
            }
            if (Hex.Length == 4)
            {
                b = b >> 8;
            }
            for (int i = Hex.Length - 1; i >= 0; i--)
            {
                a = (byte)(b & 0x0F);
                if (a >= 0 && a <= 9)
                {
                    Hex[i] = (byte)(a + (byte)'0');
                }
                else if (a >= 10 && a <= 15)
                {
                    Hex[i] = (byte)(a - 10 + (byte)'A');
                }
                b = b >> 4;
            }
        }

        private static void DegRange(ref double x)
        {
            if (x > 360d)
            {
                x = x - 360d;
            }
            if (x < -180d)
            {
                x = x + 360;
            }
        }

        public static bool AtPark()
        /* TODO: unfinished */
        {
            /* TODO: implement park and unpark */
            return false;
        }

        public static void SetTracking(bool Tracking)
        {
            switch (Scope.Type)
            {
                case eScopeType.NEXSTAR_GPS:
                    if (Tracking)
                    {
                        switch (GetAlignmentMode())
                        {
                            case AlignmentModes.algAltAz:

                                if (SetTrackingMode(eTrackingMode.ALTAZ))
                                {
                                    Scope.isTracking = true;
                                }
                                break;
                            case AlignmentModes.algGermanPolar:
                            case AlignmentModes.algPolar:
                                switch (Scope.TrackingMode)
                                {
                                    case eTrackingMode.EQNORTH:
                                        if (SetTrackingMode(eTrackingMode.EQNORTH))
                                        {
                                            Scope.isTracking = true;
                                        }
                                        break;
                                    case eTrackingMode.EQSOUTH:
                                        if (SetTrackingMode(eTrackingMode.EQSOUTH))
                                        {
                                            Scope.isTracking = true;
                                        }
                                        break;
                                }
                                break;
                            default:
                                if (SetTrackingMode(eTrackingMode.ALTAZ))
                                {
                                    Scope.isTracking = true;
                                }
                                break;
                        }
                    }
                    else
                    {
                        if (SetTrackingMode(eTrackingMode.OFF))
                        {
                            Scope.isTracking = false;
                        }
                    }
                    break;
                default:
                    Log.LogMessage(DriverId, "SetTracking() : unsupported scope");
                    throw new ASCOM.NotImplementedException(DriverId + ": SetTracking() : unsupported scope");
            }
        }

        public static bool SetTrackingRate(DriveRates Rate)
        /* lifted from Celestron unified driver */
        /* TODO: check/debug */
        {
            UInt16 tr = 0;
            byte dr = 0;
            if (Scope.isConnected && Scope.Type == eScopeType.NEXSTAR_GPS && Scope.Version >= Version_16)
            {
                if (Scope.AlignmentMode == AlignmentModes.algPolar ||
                    Scope.AlignmentMode == AlignmentModes.algGermanPolar)
                {
                    switch (Rate)
                    {
                        case DriveRates.driveSidereal:
                            tr = (UInt16)eTrackRate.Sidereal;
                            break;
                        case DriveRates.driveSolar:
                            tr = (UInt16)eTrackRate.Solar;
                            break;
                        case DriveRates.driveLunar:
                            tr = (UInt16)eTrackRate.Lunar;
                            break;
                        default:
                            Log.LogMessage(DriverId, "SetTrackingRate() : invalid rate " + Rate.ToString());
                            throw new ASCOM.InvalidValueException(DriverId + "SetTrackingRate() : invalid rate " + Rate.ToString());
                    }
                    switch (Scope.TrackingMode)
                    {
                        case eTrackingMode.EQNORTH:
                            dr = 6;
                            break;
                        case eTrackingMode.EQSOUTH:
                            dr = 7;
                            break;
                    }
                    short RxLength = 1;
                    byte[] TxBuffer = { (byte)'P', 3, (byte)eDeviceId.AZM, dr, (byte)(tr / 0x100), (byte)(tr % 0x100), 0, (byte)(RxLength - 1) };
                    byte[] RxBuffer = { };
                    if (SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                    {
                        SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                    }
                    if (RxBuffer[0] == (byte)'#')
                    {
                        Scope.TrackingRate = Rate;
                    }
                    return RxBuffer[0] == (byte)'#';
                }
                else if (Rate != DriveRates.driveSidereal)
                {
                    throw new ASCOM.InvalidValueException(DriverId + "SetTrackingRate()");
                }
            }
            else if (Rate != DriveRates.driveSidereal)
            {
                throw new ASCOM.InvalidValueException(DriverId + "SetTrackingRate()");
            }
            return false;
        }

        public static AlignmentModes GetAlignmentMode()
        {
            switch (Scope.TrackingMode)
            {
                case eTrackingMode.ALTAZ:
                    return AlignmentModes.algAltAz;
                case eTrackingMode.EQNORTH:
                case eTrackingMode.EQSOUTH:
                    if (Scope.isGem)
                    {
                        return AlignmentModes.algGermanPolar;
                    }
                    else
                    {
                        return AlignmentModes.algPolar;
                    }
                default:
                    return AlignmentModes.algAltAz;
            }
        }

        public static void SetRightAscensionRate(double Rate)
        /* lifted from Celestron unified driver */
        /* TODO: check/debug */
        {
            if (Scope.Version >= Version_16 &&
                Scope.AlignmentMode != AlignmentModes.algAltAz)
            {
                double s = 0;
                s = Rate * SIDRATE * 15;
                if (isValidRate(Scope.AxisRates[0], s / 3600d))
                {
                    double mr = 0;
                    Scope.RateRa = Rate;
                    mr = s * 1024d + 0x3C29;
                    if (Scope.TrackingMode == eTrackingMode.EQSOUTH)
                    {
                        mr = -mr;
                    }
                    if (Scope.RateRa != 0)
                    {
                        SlewVariableRate(eDeviceId.AZM, (long)mr);
                    }
                    else
                    {
                        SetTrackingRate(Scope.TrackingRate);
                    }
                    return;
                }
                Log.LogMessage(DriverId, "SetRightAscensionRate() : invalid rate");
                throw new ASCOM.InvalidValueException(DriverId + ": SetRightAscensionRate() : invalid rate");
            }
            Log.LogMessage(DriverId, "SetRightAscensionRate() : not implimented");
            throw new ASCOM.PropertyNotImplementedException(DriverId + ": SetRightAscensionRate()");
        }

        public static void SetDeclinationRate(double Rate)
        /* lifted from Celestron unified driver */
        /* TODO: check/debug */
        {
            if (Scope.Version >= Version_16 &&
                Scope.AlignmentMode != AlignmentModes.algAltAz)
            {
                if (isValidRate(Scope.AxisRates[1], Rate / 3600d))
                {
                    double mr = 0;
                    Scope.RateDec = Rate;
                    mr = Scope.RateDec * 1024d;
                    if (Scope.TrackingMode == eTrackingMode.EQSOUTH)
                    {
                        mr = -mr;
                    }
                    SlewVariableRate(eDeviceId.ALT, (long)mr);
                    return;
                }
                Log.LogMessage(DriverId, "SetDeclinationRate() : invalid rate");
                throw new ASCOM.InvalidValueException(DriverId + ": SetDeclinationRate() : invalid rate");
            }
            Log.LogMessage(DriverId, "SetDeclinationRate() : not implemented");
            throw new ASCOM.PropertyNotImplementedException(DriverId + ": SetDeclinationRate()");
        }

        public static EquatorialCoordinateType GetEquatorialSystem()
        {
            if (Scope.Version <= Version_413)
            {
                return EquatorialCoordinateType.equJ2000;
            }
            else if (Scope.Version >= Version_GT_LO)
            {
                return EquatorialCoordinateType.equJ2000;
            }
            return EquatorialCoordinateType.equLocalTopocentric;
        }

        public static bool GetGuidRate(eDeviceId DevId, out double Rate)
        /* lifted from Celestron unified driver */
        /* TODO: check/debug */
        {
            if (Scope.isConnected && CanPulseGuide())
            {
                short RxLength = 2;
                byte[] TxBuffer = { (byte)'P', 1, (byte)DevId, 71, 0, 0, 0, (byte)(RxLength - 1) };
                byte[] RxBuffer = { };
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength);
                }
                if (RxBuffer[RxLength - 1] == (byte)'#')
                {
                    Rate = ((double)RxBuffer[0] / 256d) * SIDEREAL_RATE_DEG_SEC;
                    return true;
                }
            }
            Rate = 0d;
            return false;
        }

        public static bool SetGuideRate(eDeviceId DevId, double Rate)
        /* lifted from Celestron unified driver */
        /* TODO: check/debug */
        {
            if ((DevId == eDeviceId.AZM && isValidRate(Scope.GuideRates[0], Rate)) ||
                (DevId == eDeviceId.ALT && isValidRate(Scope.GuideRates[1], Rate)))
            {
                if (Scope.isConnected && CanPulseGuide())
                {
                    short RxLength = 1;
                    byte[] TxBuffer = { (byte)'P', 2, (byte)DevId, 70, (byte)(Rate / SIDEREAL_RATE_DEG_SEC * 256), 0, 0, (byte)(RxLength - 1) };
                    byte[] RxBuffer = { };
                    if (SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                    {
                        SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                    }
                    return RxBuffer[0] == (byte)'#';
                }
                else
                {
                    Log.LogMessage(DriverId, "SetGuideRate() : not implemented");
                    throw new ASCOM.PropertyNotImplementedException(DriverId + "SetGuideRate() : not implemented");
                }
            }
            else
            {
                Log.LogMessage(DriverId, "SetGuideRate() : invalid rate " + Rate.ToString());
                throw new ASCOM.InvalidValueException(DriverId + ": SetGuideRate() : invalid rate " + Rate.ToString());
            }
        }

        public static bool EnablePec(bool Enable)
        /* lifted from Celestron unified driver */
        /* TODO: check/debug */
        {
            if (Scope.isConnected && Scope.AlignmentMode != AlignmentModes.algAltAz && Scope.HasPec)
            {
                short RxLength = 1;
                byte[] TxBuffer = { (byte)'P', 2, (byte)eDeviceId.AZM, 13, (byte)((Enable == true) ? 1 : 0), 0, 0, (byte)(RxLength - 1) };
                byte[] RxBuffer = { };
                if (SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                return RxBuffer[0] == (byte)'#';
            }
            return false;
        }

        public static void MoveAxis(TelescopeAxes Axis, double Rate)
        /* lifted from Celestron unified driver */
        {
            if (Scope.Type == eScopeType.NEXSTAR_GPS && Scope.Version >= Version_16)
            {
                int a = 0;
                eDeviceId dev = 0;
                long b = 0;
                switch (Axis)
                {
                    case TelescopeAxes.axisPrimary:
                        a = 0;
                        dev = eDeviceId.AZM;
                        break;
                    case TelescopeAxes.axisSecondary:
                        a = 1;
                        dev = eDeviceId.ALT;
                        break;
                    case TelescopeAxes.axisTertiary:
                        Log.LogMessage(DriverId, "MoveAxis() : no axisTertiary");
                        throw new ASCOM.InvalidValueException(DriverId + ": MoveAxis() : no axisTertiary");
                }
                if (isValidRate(Scope.AxisRates[a], Rate))
                {
                    b = (long)(Rate * 3600d * 1024d);
                    if (Scope.TrackingMode == eTrackingMode.EQSOUTH)
                    {
                        b = -b;
                    }
                    SlewVariableRate(dev, b);
                }
                else
                {
                    Log.LogMessage(DriverId, "MoveAxis() : invalid rate " + Rate.ToString());
                    throw new ASCOM.InvalidValueException(DriverId + ": MoveAxis() : invalid rate " + Rate.ToString());
                }
            }
            else
            {
                Log.LogMessage(DriverId, "MoveAxis() : not NexStar or version < 1.6");
                throw new ASCOM.MethodNotImplementedException(DriverId + ": MoveAxis() : not NexStar or version < 1.6");
            }
        }

        private static bool isValidRate(AxisRates Rates, double AxisRate)
        /* lifted from Celestron unified driver */
        {
            foreach (Rate r in Rates)
            {
                if (Math.Abs(AxisRate) <= r.Maximum && Math.Abs(AxisRate) >= r.Minimum)
                {
                    return true;
                }
            }
            return false;
        }

        public static void PulseGuide(GuideDirections Direction, int Duration)
        {
            if (Scope.AlignmentMode == AlignmentModes.algAltAz)
            {
                Log.LogMessage(DriverId, "PulseGuide() : pulseguide not supported in Alt/Az");
                throw new ASCOM.NotImplementedException(DriverId + ": PulseGuide() : pulseguide not supported in Alt/Az");
            }
            ScopePulseGuide.Guide(Direction, Duration);
        }

        public static double GetAltitude()
        {
            double Alt;
            double Azm;
            if (GetAzimuthAltitude(out Azm, out Alt))
            {
                return Alt;
            }
            Log.LogMessage(DriverId, "GetAltitude() : not implemented");
            throw new ASCOM.NotImplementedException(DriverId + ": GetAltitude() : not implemented");
        }

        public static double GetAzimuth()
        {
            double Azm;
            double Alt;
            if (GetAzimuthAltitude(out Azm, out Alt))
            {
                return Azm;
            }
            Log.LogMessage(DriverId, "GetAzimuth() : not implemented");
            throw new ASCOM.NotImplementedException(DriverId + ": GetAzimuth() : not implemented");
        }

        public static double GetDeclination()
        {
            double Ra;
            double Dec;
            if (GetRightAscensionDeclination(out Ra, out Dec))
            {
                return Dec;
            }
            Log.LogMessage(DriverId, "GetDeclination() : not implemented");
            throw new ASCOM.NotImplementedException(DriverId + ": GetDeclination() : not implemented");
        }

        public static double GetRightAscention()
        {
            double Ra;
            double Dec;
            if (GetRightAscensionDeclination(out Ra, out Dec))
            {
                return Ra;
            }
            Log.LogMessage(DriverId, "GetRightAscention() : not implemented");
            throw new ASCOM.NotImplementedException(DriverId + ": GetRightAscention() : not implemented");
        }

        public static bool IsPulseGuiding()
        {
            if (CanPulseGuide())
            {
                return Scope.isGuiding;
            }
            Log.LogMessage(DriverId, "IsPulseGuiding() : not implemented in Alt/Azm");
            throw new ASCOM.PropertyNotImplementedException(DriverId + ": IsPulseGuiding() : not implemented in Alt/Azm");
        }

        public static double GetTargetRightAscension()
        {
            if (Scope.TargetRa < 0d ||
                Scope.TargetRa > 24d ||
                !Scope.TargetRaSet)
            {
                Log.LogMessage(DriverId, "GetTargetRightAscention() : value not set");
                throw new ASCOM.ValueNotSetException(DriverId + ": GetTargetRightAscention() : value not set");
            }
            return Scope.TargetRa;
        }

        public static void SetTargetRightAscension(double value)
        {
            if (value < 0d || value > 24d)
            {
                Log.LogMessage(DriverId, "SetTargetRightAscention() : invalid value " + value.ToString());
                throw new ASCOM.InvalidValueException(DriverId + ": SetTargetRightAscention() : invalid value " + value.ToString());
            }
            Scope.TargetRaSet = true;
            Scope.TargetRa = value;
        }

        public static double GetTargetDeclination()
        {
            if (Scope.TargetDec < -90d ||
                Scope.TargetDec > 90d ||
                !Scope.TargetDecSet)
            {
                Log.LogMessage(DriverId, "GetTargetDeclination() : value not set");
                throw new ASCOM.ValueNotSetException(DriverId + ": GetTargetDeclination() : value not set");
            }
            return Scope.TargetDec;
        }

        public static void SetTargetDeclination(double value)
        {
            if (value < -90d || value > 90d)
            {
                Log.LogMessage(DriverId, "SetTargetDeclination() : invalid value " + value.ToString());
                throw new ASCOM.InvalidValueException(DriverId + ": SetTargetDeclination() : invalid value " + value.ToString());
            }
            Scope.TargetDecSet = true;
            Scope.TargetDec = value;
        }

        public static void SetSlewSettleTime(short value)
        {
            if (value < 0 || value > 100)
            {
                Log.LogMessage(DriverId, "SetSlewSettleTime() : invalid value " + value.ToString());
                throw new ASCOM.InvalidValueException(DriverId + ": SetSlewSettleTime() : " + value.ToString());
            }
            Scope.SettleTime = value;
        }

        public static PierSide GetSideOfPier()
        {
            if (Scope.AlignmentMode != AlignmentModes.algGermanPolar)
            {
                Log.LogMessage(DriverId, "GetSideOfPier() : not implemented");
                throw new ASCOM.PropertyNotImplementedException(DriverId + ": GetSideOfPier() : not implemented");
            }
            /* TODO: Put code here */
            return PierSide.pierUnknown;
        }

        public static void SetSideOfPier(PierSide value)
        {
            if (Scope.AlignmentMode != AlignmentModes.algGermanPolar)
            {
                Log.LogMessage(DriverId, "SetSideOfPier() : not implemented");
                throw new ASCOM.PropertyNotImplementedException(DriverId + ": SetSideOfPier() : not inplemented");
            }
            /* TODO: put code here */
        }

        #region can-do's

        public static bool CanSetTracking()
        {
            switch (Scope.Type)
            {
                case eScopeType.NEXSTAR_GPS:
                    return (Scope.Version >= Version_16);
                default:
                    return false;
            }
        }

        public static bool CanSetPierSide()
        {
            if (Scope.Version >= Version_414 &&
                Scope.Model == eScopeModel.CGE2 ||
                Scope.Model == eScopeModel.EQ6)
            {
                return Scope.isGem;
            }
            return false;
        }

        public static bool CanSlewAltAz()
        {
            switch (Scope.Type)
            {
                case eScopeType.NEXSTAR_GPS:
                    return Scope.Version >= Version_16;
                default:
                    return false;

            }
        }

        public static bool CanSlewAsync()
        {
            switch (Scope.Type)
            {
                case eScopeType.NEXSTAR_GPS:
                    return true;
                case eScopeType.ULTIMA:
                    return true;
                default:
                    return false;
            }
        }

        public static bool CanPulseGuide()
        {
            switch (Scope.Type)
            {
                case eScopeType.NEXSTAR_GPS:
                    return ((Scope.Version >= Version_16) &
                        (Scope.AlignmentMode != AlignmentModes.algAltAz));
                default:
                    return false;
            }
        }

        public static bool CanAuxGuide()
        {
            switch (Scope.Model)
            {
                case eScopeModel.CGE2:
                case eScopeModel.EQ6:
                    if (Scope.AzmVersion >= 0x60C &&
                        Scope.AltVersion >= 0x60C)
                    {
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }

        public static bool CanSync()
        {
            if (Scope.Version >= Version_41)
            {
                return true;
            }
            return false;
        }

        #endregion

    } // end class
} // end namespace