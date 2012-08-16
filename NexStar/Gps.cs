using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ASCOM.NexStar
{
    internal class Gps
        /* all methods supported from version 1.6 */
        /* all returned values are UT */
    {

        public int isLinked()
        /* returns -1 on error as device may not exist */
        {
            if (Scope.isConnected)
            {
                short RxLength = 2;
                byte[] TxBuffer = { (byte)'P', 1, (byte)Common.eDeviceId.GPS, 55, 0, 0, 0, (byte)(RxLength - 1) };
                byte[] RxBuffer = { };
                if (Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    return RxBuffer[0];
                }
            }
            return -1;
        }

        public bool GetLatitude(out double Latitude)
        {
            if (Scope.isConnected)
            {
                short RxLength = 4;
                byte[] TxBuffer = { (byte)'P', 1, (byte)Common.eDeviceId.GPS, 1, 0, 0, 0, (byte)(RxLength - 1) };
                byte[] RxBuffer = { };
                if (Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength) == 0)
                {
                    Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    Latitude = ((RxBuffer[0] * 65536) + (RxBuffer[1] * 256) + RxBuffer[2]) * (360d / 16777216d);
                    return true;
                }
            }
            Latitude = 0;
            return false;
        }

        public bool GetLatitude(out double Degrees, out double Minutes, out double Seconds)
        {
            double Latitude;
            if (GetLatitude(out Latitude))
            {
                DriverMath.ConvertCoordinate(Latitude, out Degrees, out Minutes, out Seconds);
                return true;
            }
            Degrees = 0;
            Minutes = 0;
            Seconds = 0;
            return false;
        }

        public bool GetLongitude(out double Longitude)
        {
            if (Scope.isConnected)
            {
                short RxLength = 4;
                byte[] TxBuffer = { (byte)'P', 1, (byte)Common.eDeviceId.GPS, 2, 0, 0, 0, (byte)(RxLength - 1) };
                byte[] RxBuffer = { };
                if (Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength) == 0)
                {
                    Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    Longitude = ((RxBuffer[0] * 65536) + (RxBuffer[1] * 256) + RxBuffer[2]) * (360d / 16777216d);
                    if (Longitude > 180d)
                    {
                        Longitude -= 360d;
                    }
                    return true;
                }
            }
            Longitude = 0;
            return false;
        }

        public bool GetLongitude(out double Degrees, out double Minutes, out double Seconds)
        {
            double Longitude;
            if (GetLongitude(out Longitude))
            {
                DriverMath.ConvertCoordinate(Longitude, out Degrees, out Minutes, out Seconds);
                return true;
            }
            Degrees = 0;
            Minutes = 0;
            Seconds = 0;
            return false;
        }

        private bool GetDate(out byte Month, out byte Day)
        {
            if (Scope.isConnected)
            {
                short RxLength = 3;
                byte[] TxBuffer = { (byte)'P', 1, (byte)Common.eDeviceId.GPS, 3, 0, 0, 0, (byte)(RxLength - 1) };
                byte[] RxBuffer = { };
                if (Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    Month = RxBuffer[0];
                    Day = RxBuffer[1];
                    return true;
                }
            }
            Month = 0;
            Day = 0;
            return false;
        }

        private bool GetYear(out short Year)
        {
            if (Scope.isConnected)
            {
                short RxLength = 3;
                byte[] TxBuffer = { (byte)'P', 1, (byte)Common.eDeviceId.GPS, 4, 0, 0, 0, (byte)(RxLength - 1) };
                byte[] RxBuffer = { };
                if (Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength) == 0)
                {
                    Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    Year = (short)((RxBuffer[0] * 256) + RxBuffer[1]);
                    return true;
                }
            }
            Year = 0;
            return false;
        }

        private bool GetTime(out byte Hours, out byte Minutes, out byte Second)
        {
            if (Scope.isConnected)
            {
                short RxLength = 4;
                byte[] TxBuffer = { (byte)'P', 1, (byte)Common.eDeviceId.GPS, 51, 0, 0, 0, (byte)(RxLength - 1) };
                byte[] RxBuffer = { };
                if (Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength) == 0)
                {
                    Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    Hours = RxBuffer[0];
                    Minutes = RxBuffer[1];
                    Second = RxBuffer[2];
                    return true;
                }
            }
            Hours = 0;
            Minutes = 0;
            Second = 0;
            return false;
        }

        public bool GetDateTime(out DateTime GpsDateTime)
        /* returns GPS time as UTC */
        {
            if (Scope.isConnected)
            {
                byte Month = 0, Day = 0, Hours = 0, Minutes = 0, Seconds = 0;
                short Year = 0;
                if (GetDate(out Month, out Day) &&
                    GetYear(out Year) &&
                    GetTime(out Hours, out Minutes, out Seconds))
                {
                    GpsDateTime = new DateTime(Year, Month, Day, Hours, Minutes, Seconds);
                    GpsDateTime = DateTime.SpecifyKind(GpsDateTime, DateTimeKind.Utc);
                    return true;
                }
            }
            GpsDateTime = new DateTime();
            return false;
        }

        public bool GetCoordinates(out double Latitude, out double Longitude)
        {
            if (Scope.isConnected)
            {
                if (GetLatitude(out Latitude) &&
                    GetLongitude(out Longitude))
                {
                    return true;
                }
            }
            Longitude = 0;
            Latitude = 0;
            return false;
        }

        public bool isTimeValid()
        {
            if (Scope.isConnected)
            {
                short RxLength = 2;
                byte[] TxBuffer = { (byte)'P', 1, (byte)Common.eDeviceId.GPS, 54, 0, 0, 0, (byte)(RxLength - 1) };
                byte[] RxBuffer = { };
                if (Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    return RxBuffer[0] == 1;
                }
            }
            return false;
        }
    }
}
