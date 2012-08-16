//tabs=4
// --------------------------------------------------------------------------------
// TODO fill in this information for your driver, then remove this line!
//
// ASCOM Telescope driver for NexStar
//
// Description:	Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam 
//				nonumy eirmod tempor invidunt ut labore et dolore magna aliquyam 
//				erat, sed diam voluptua. At vero eos et accusam et justo duo 
//				dolores et ea rebum. Stet clita kasd gubergren, no sea takimata 
//				sanctus est Lorem ipsum dolor sit amet.
//
// Implements:	ASCOM Telescope interface version: <To be completed by driver developer>
// Author:		(XXX) Your N. Here <your@email.here>
//
// Edit Log:
//
// Date			Who	Vers	Description
// -----------	---	-----	-------------------------------------------------------
// dd-mmm-yyyy	XXX	6.0.0	Initial edit, created from ASCOM driver template
// --------------------------------------------------------------------------------
//

// This is used to define code in the template that is specific to one class implementation
// unused code canbe deleted and this definition removed.
#define Telescope

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

using ASCOM;
using ASCOM.Utilities;
using ASCOM.DeviceInterface;
using System.Globalization;
using System.Collections;

namespace ASCOM.NexStar
{
    //
    // Your driver's DeviceID is ASCOM.NexStar.Telescope
    //
    // The Guid attribute sets the CLSID for ASCOM.NexStar.Telescope
    // The ClassInterface/None addribute prevents an empty interface called
    // _NexStar from being created and used as the [default] interface
    //
    // TODO right click on ITelescopeV3 and select "Implement Interface" to
    // generate the property amd method definitions for the driver.
    //
    // TODO Replace the not implemented exceptions with code to implement the function or
    // throw the appropriate ASCOM exception.
    //

    /// <summary>
    /// ASCOM Telescope Driver for NexStar.
    /// </summary>
    [Guid("9e18e713-1a5c-4200-a7d3-1aafabce0ce4")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Telescope : ITelescopeV3
    {
        /// <summary>
        /// ASCOM DeviceID (COM ProgID) for this driver.
        /// The DeviceID is used by ASCOM applications to load the driver at runtime.
        /// </summary>
        //private static string driverID = "ASCOM.NexStar.Telescope";
        // TODO Change the descriptive string for your driver then remove this line
        /// <summary>
        /// Driver description that displays in the ASCOM Chooser.
        /// </summary>
        //private static string driverDescription = "NexStar Telescope Driver";

#if Telescope
        //
        // Driver private data (rate collections) for the telescope driver only.
        // This can be removed for other driver types
        //
        //private readonly AxisRates[] _axisRates;
#endif
        /// <summary>
        /// Initializes a new instance of the <see cref="NexStar"/> class.
        /// Must be public for COM registration.
        /// </summary>
        public Telescope()
        {
#if Telescope
            // the rates constructors are only needed for the telescope class
            // This can be removed for other driver types
            //_axisRates = new AxisRates[3];
            //_axisRates[0] = new AxisRates(TelescopeAxes.axisPrimary);
            //_axisRates[1] = new AxisRates(TelescopeAxes.axisSecondary);
            //_axisRates[2] = new AxisRates(TelescopeAxes.axisTertiary);
#endif
            //TODO: Implement your additional construction here
        }

        #region ASCOM Registration
        //
        // Register or unregister driver for ASCOM. This is harmless if already
        // registered or unregistered. 
        //
        /// <summary>
        /// Register or unregister the driver with the ASCOM Platform.
        /// This is harmless if the driver is already registered/unregistered.
        /// </summary>
        /// <param name="bRegister">If <c>true</c>, registers the driver, otherwise unregisters it.</param>
        private static void RegUnregASCOM(bool bRegister)
        {
            using (var P = new ASCOM.Utilities.Profile())
            {
                P.DeviceType = "Telescope";
                if (bRegister)
                {
                    P.Register(Common.DriverId, Common.DriverDescription);
                }
                else
                {
                    P.Unregister(Common.DriverId);
                }
            }
        }

        /// <summary>
        /// This function registers the driver with the ASCOM Chooser and
        /// is called automatically whenever this class is registered for COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is successfully built.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During setup, when the installer registers the assembly for COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually register a driver with ASCOM.
        /// </remarks>
        [ComRegisterFunction]
        public static void RegisterASCOM(Type t)
        {
            RegUnregASCOM(true);
        }

        /// <summary>
        /// This function unregisters the driver from the ASCOM Chooser and
        /// is called automatically whenever this class is unregistered from COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is cleaned or prior to rebuilding.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During uninstall, when the installer unregisters the assembly from COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually unregister a driver from ASCOM.
        /// </remarks>
        [ComUnregisterFunction]
        public static void UnregisterASCOM(Type t)
        {
            RegUnregASCOM(false);
        }
        #endregion

        //
        // PUBLIC COM INTERFACE ITelescopeV3 IMPLEMENTATION
        //

        /// <summary>
        /// Displays the Setup Dialog form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        public void SetupDialog()
        {
            // consider only showing the setup dialog if not connected
            // or call a different dialog if connected
            if (IsConnected)
                System.Windows.Forms.MessageBox.Show("Already connected, just press OK");

            using (SetupDialogForm F = new SetupDialogForm())
            {
                var result = F.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    Properties.Settings.Default.Save();
                    return;
                }
                Properties.Settings.Default.Reload();
            }
        }


        #region common properties and methods. All set to no action

        public System.Collections.ArrayList SupportedActions
        {
            get { return new ArrayList(); }
        }

        public string Action(string actionName, string actionParameters)
        {
            throw new ASCOM.MethodNotImplementedException("Action");
        }

        public void CommandBlind(string command, bool raw)
        {
            CheckConnected("CommandBlind");
            // Call CommandString and return as soon as it finishes
            this.CommandString(command, raw);
            // or
            throw new ASCOM.MethodNotImplementedException("CommandBlind");
        }

        public bool CommandBool(string command, bool raw)
        {
            CheckConnected("CommandBool");
            string ret = CommandString(command, raw);
            // TODO decode the return string and return true or false
            // or
            throw new ASCOM.MethodNotImplementedException("CommandBool");
        }

        public string CommandString(string command, bool raw)
        {
            CheckConnected("CommandString");
            // it's a good idea to put all the low level communication with the device here,
            // then all communication calls this function
            // you need something to ensure that only one command is in progress at a time

            throw new ASCOM.MethodNotImplementedException("CommandString");
        }

        #endregion

        #region public properties and methods
        public void Dispose()
        {
            if (Scope.isConnected)
            {
                Common.ScopeConnect(false);
            }
        }

        public bool Connected
        /* Done AR */
        {
            get { return IsConnected; }
            set
            {
                if (value == IsConnected)
                    return;

                if (value)
                {
                    Common.ScopeConnect(value);
                }
                else
                {
                    Common.ScopeConnect(value);
                }
                return;
            }
        }

        public string Description
        /* done AR */
        {
            get { return Common.DriverDescription; }
        }

        public string DriverInfo
        /* done AR */
        {
            get { return Common.DriverDescription; }
        }

        public string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                return String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
            }
        }

        public short InterfaceVersion
        {
            // set by the driver wizard
            get { return 3; }
        }

        #endregion

        #region private properties and methods
        // here are some useful properties and methods that can be used as required
        // to help with

        /// <summary>
        /// Returns true if there is a valid connection to the driver hardware
        /// </summary>
        private bool IsConnected
        {
            get
            {
                return Scope.isConnected;
            }
        }

        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private void CheckConnected(string message)
        {
            if (!IsConnected)
            {
                throw new ASCOM.NotConnectedException(message);
            }
        }
        #endregion

        public void AbortSlew()
        /*  done AR */
        {
            Common.AbortSlew();
        }

        public AlignmentModes AlignmentMode
        /* done AR */
        {
            get { return Scope.AlignmentMode; }
        }

        public double Altitude
        /* done AR */
        {
            get { return Common.GetAltitude(); }
        }

        public double ApertureArea
        /* done AR */
        {
            get { return Scope.ApertureArea; }
        }

        public double ApertureDiameter
        /* done AR */
        {
            get { return Scope.ApertureDiameter; }
        }

        public bool AtHome
        /* done AR */
        {
            get { return false; }
        }

        public bool AtPark
        /* done AR */
        {
            get { return Common.AtPark(); }
        }

        public IAxisRates AxisRates(TelescopeAxes Axis)
        /* done AR */
        {
            switch (Axis)
            {
                case TelescopeAxes.axisPrimary:
                    return Scope.AxisRates[0];
                case TelescopeAxes.axisSecondary:
                    return Scope.AxisRates[1];
                case TelescopeAxes.axisTertiary:
                    return Scope.AxisRates[2];
                default:
                    return null;
            }
        }

        public double Azimuth
        /* done AR */
        {
            get { return Common.GetAzimuth(); }
        }

        public bool CanFindHome
        /* done AR */
        {
            get { return false; }
        }

        public bool CanMoveAxis(TelescopeAxes Axis)
        /* done AR */
        {
            switch (Axis)
            {
                case TelescopeAxes.axisPrimary:
                    return Scope.AxisRates[0].Count > 0;
                case TelescopeAxes.axisSecondary:
                    return Scope.AxisRates[1].Count > 0;
                case TelescopeAxes.axisTertiary:
                    return Scope.AxisRates[2].Count > 0;
                default:
                    return false;
            }
        }

        public bool CanPark
        /* TODO: implement */
        {
            get { return false; }
        }

        public bool CanPulseGuide
        /* done AR */
        {
            get { return Common.CanPulseGuide(); }
        }

        public bool CanSetDeclinationRate
        /* done AR */
        {
            get { return Common.CanPulseGuide(); }
        }

        public bool CanSetGuideRates
        /* done AR */
        {
            get { return Common.CanPulseGuide(); }
        }

        public bool CanSetPark
        /* done AR */
        {
            get { return Common.CanSetTracking(); }
        }

        public bool CanSetPierSide
        /* done AR */
        {
            get { return Common.CanSetPierSide(); }
        }

        public bool CanSetRightAscensionRate
        /* done AR */
        {
            get { return Common.CanPulseGuide(); }
        }

        public bool CanSetTracking
        /* done AR */
        {
            get { return Common.CanSetTracking(); }
        }

        public bool CanSlew
        /* done AR */
        {
            get { return true; }
        }

        public bool CanSlewAltAz
        /* done AR */
        {
            get { return Common.CanSlewAltAz(); }
        }

        public bool CanSlewAltAzAsync
        /* done AR */
        {
            get { return Common.CanSlewAltAz(); }
        }

        public bool CanSlewAsync
        /* done AR */
        {
            get { return Common.CanSlewAsync(); }
        }

        public bool CanSync
        /* done AR */
        {
            get { return Common.CanSync(); }
        }

        public bool CanSyncAltAz
        /* done AR */
        {
            get { return false; }
        }

        public bool CanUnpark
        /* TODO: implement */
        {
            get { return false; }
        }

        public double Declination
        /* done AR */
        {
            get { return Common.GetDeclination(); }
        }

        public double DeclinationRate
        /* TODO: check/debug */
        {
            get { return Scope.RateDec; }
            set { Common.SetDeclinationRate(value); }
        }

        public PierSide DestinationSideOfPier(double RightAscension, double Declination)
        /* TODO: implement */
        {
            if (Scope.AlignmentMode == AlignmentModes.algGermanPolar)
            {
                /* TODO: code here */
                return PierSide.pierUnknown;
            }
            throw new ASCOM.NotImplementedException(Common.DriverId + ": DestinationSideOfPier() failed");
        }

        public bool DoesRefraction
        /* done AR */
        {
            get { return false; }
            set { }
        }

        public EquatorialCoordinateType EquatorialSystem
        /* done AR */
        {
            get { return Common.GetEquatorialSystem(); }
        }

        public void FindHome()
        {
            throw new ASCOM.NotImplementedException();
        }

        public double FocalLength
        /* done AR */
        {
            get { return Scope.FocalLength; }
        }

        public double GuideRateDeclination
        /* TODO: check/debug */
        {
            get
            {
                double Rate;
                Common.GetGuidRate(Common.eDeviceId.ALT, out Rate);
                Scope.GuideRateDec = Rate;
                return Rate;
            }
            set
            {
                Scope.GuideRateDec = value;
                Common.SetGuideRate(Common.eDeviceId.ALT, value);
            }
        }

        public double GuideRateRightAscension
        /* TODO: check/debug */
        {
            get
            {
                double Rate;
                Common.GetGuidRate(Common.eDeviceId.AZM, out Rate);
                Scope.GuideRateRa = Rate;
                return Rate;
            }
            set
            {
                Scope.GuideRateRa = value;
                Common.SetGuideRate(Common.eDeviceId.AZM, value);
            }
        }

        public bool IsPulseGuiding
        /* done AR */
        {
            get { return Common.isPulseGuiding(); }
        }

        public void MoveAxis(TelescopeAxes Axis, double Rate)
        /* TODO: check/debug */
        {
            Common.MoveAxis(Axis, Rate);
        }

        public string Name
        /* done AR */
        {
            get { return Scope.Name; }
        }

        public void Park()
        /* TODO: implement */
        {
            throw new ASCOM.MethodNotImplementedException();
        }

        public void PulseGuide(GuideDirections Direction, int Duration)
        /* done AR */
        {
            Common.PulseGuide(Direction, Duration);
        }

        public double RightAscension
        /* done AR */
        {
            get { return Common.GetRightAscention(); }
        }

        public double RightAscensionRate
        /* TODO: check/debug */
        {
            get { return Scope.RateRa; }
            set { Common.SetRightAscensionRate(value); }
        }

        public void SetPark()
        /* TODO: implement */
        {
            throw new ASCOM.MethodNotImplementedException(Common.DriverId + ": SetPark() failed");
        }

        public PierSide SideOfPier
        /* TODO: implement */
        {
            get { return Common.GetSideOfPier(); }
            set { Common.SetSideOfPier(value); }
        }

        public double SiderealTime
        /* done AR */
        {
            get { return Common.GetLst(); }
        }

        public double SiteElevation
        /* done AR */
        {
            get { return Scope.Elevation; }
            set { Common.SetElevation(value); }
        }

        public double SiteLatitude
        /* done AR */
        {
            get { return Scope.Latitude; }
            set { Common.SetLatitude(value); }
        }

        public double SiteLongitude
        /* done AR */
        {
            get { return Scope.Longitude; }
            set { Common.SetLongitude(value); }
        }

        public short SlewSettleTime
        /* done AR */
        {
            get { return Scope.SettleTime; }
            set { Common.SetSlewSettleTime(value); }
        }

        public void SlewToAltAz(double Azimuth, double Altitude)
        /* done AR */
        {
            Common.SlewToAzmAlt(Azimuth, Altitude);
            while (Scope.isSlewing)
            {
                Thread.Sleep(500);
                Application.DoEvents();
            }
        }

        public void SlewToAltAzAsync(double Azimuth, double Altitude)
        /* done AR */
        {
            Common.SlewToAzmAlt(Azimuth, Altitude);
        }

        public void SlewToCoordinates(double RightAscension, double Declination)
        /* done AR */
        {
            Scope.TargetRa = RightAscension;
            Scope.TargetDec = Declination;
            Scope.TargetRaSet = true;
            Scope.TargetDecSet = true;
            if (Scope.TargetRaSet && Scope.TargetDecSet)
            {
                Common.SlewToRaDec(Scope.TargetRa, Scope.TargetDec);
                while (Scope.isSlewing)
                {
                    Thread.Sleep(500);
                    Application.DoEvents();
                }
            }
        }

        public void SlewToCoordinatesAsync(double RightAscension, double Declination)
        /* done AR */
        {
            Scope.TargetRa = RightAscension;
            Scope.TargetDec = Declination;
            Scope.TargetRaSet = true;
            Scope.TargetDecSet = true;
            if (Scope.TargetRaSet && Scope.TargetDecSet)
            {
                Common.SlewToRaDec(Scope.TargetRa, Scope.TargetDec);
            }
        }

        public void SlewToTarget()
        /* done AR */
        {
            if (Scope.TargetRaSet && Scope.TargetDecSet)
            {
                Common.SlewToRaDec(Scope.TargetRa, Scope.TargetDec);
                while (Scope.isSlewing)
                {
                    Thread.Sleep(500);
                    Application.DoEvents();
                }
            }
        }

        public void SlewToTargetAsync()
        /* done AR */
        {
            if (Scope.TargetRaSet && Scope.TargetDecSet)
            {
                Common.SlewToRaDec(Scope.TargetRa, Scope.TargetDec);
            }
        }

        public bool Slewing
        /* done AR */
        {
            get { return Scope.isSlewing; }
        }

        public void SyncToAltAz(double Azimuth, double Altitude)
        /* done AR */
        /* no scope does SyncAltAz */
        {
            throw new ASCOM.MethodNotImplementedException(Common.DriverId + "SyncToAltAz() failed");
        }

        public void SyncToCoordinates(double RightAscension, double Declination)
        /* done AR */
        {
            Common.Sync(RightAscension, Declination);
        }

        public void SyncToTarget()
        /* done AR */
        {
            if (Scope.TargetRaSet && Scope.TargetDecSet)
            {
                Common.Sync(Scope.TargetRa, Scope.TargetDec);
            }
        }

        public double TargetDeclination
        /* done AR */
        {
            get { return Common.GetTargetDeclination(); }
            set { Common.SetTargetDeclination(value); }
        }

        public double TargetRightAscension
        /* done AR */
        {
            get { return Common.GetTargetRightAscension(); }
            set { Common.SetTargetRightAscension(value); }
        }

        public bool Tracking
        /* done AR */
        {
            get { return Scope.isTracking; }
            set { Common.SetTracking(value); }
        }

        public DriveRates TrackingRate
        /* TODO: check/debug */
        {
            get { return Scope.TrackingRate; }
            set { Common.SetTrackingRate(value); }
        }

        public ITrackingRates TrackingRates
        /* done AR */
        {
            get { return Scope.TrackingRates; }
        }

        public DateTime UTCDate
        /* done AR */
        {
            get { return Common.GetUtcDate(); }
            set { Common.SetUtcDate(value); }
        }

        public void Unpark()
        /* TODO: implement */
        {
            throw new ASCOM.MethodNotImplementedException();
        }
    }
}
