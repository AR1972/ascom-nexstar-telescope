using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ASCOM.NexStar
{
    [ComVisible(false)]
    internal class Rtc
        /* get commands are supported from version 1.6 */
        /* set commands are supported from version  3.01 */
        /* commands are officially for CGE mounts only */
        /* set commands appear to work on the CPC */
    {

        public bool SetRtcDateTime(DateTime RtcDateTime)
        {
            if (Scope.isConnected)
            {
                if (SetRtcYear(RtcDateTime.Year) &&
                    SetRtcDate(RtcDateTime.Month, RtcDateTime.Day) &&
                    SetRtcTime(RtcDateTime.Hour, RtcDateTime.Minute, RtcDateTime.Second))
                {
                    return true;
                }
            }
            return false;
        }

        public bool GetRtcDateTime(ref DateTime RtcDateTime)
        {
            if (Scope.isConnected)
            {
                short Year = 0;
                short Month = 0;
                short Day = 0;
                short Hours = 0;
                short Minutes = 0;
                short Seconds = 0;
                if (GetRtcYear(ref Year) &&
                    GetRtcDate(ref Month, ref Day) &&
                    GetRtcTime(ref Hours, ref Minutes, ref Seconds))
                {
                    RtcDateTime = new DateTime(Year, Month, Day, Hours, Minutes, Seconds);
                    return true;
                }
            }
            return false;
        }

        private bool GetRtcDate(ref short Month, ref short Day)
        {
            if (Scope.isConnected)
            {
                short RxLength = 3;
                byte[] TxBuffer = { (byte)'P', 1, (byte)Common.eDeviceId.RTC, 3, 0, 0, 0, (byte)(RxLength - 1) };
                byte[] RxBuffer = { };
                if (Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength) == 0)
                {
                    Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    Month = RxBuffer[0];
                    Day = RxBuffer[1];
                    return true;
                }
            }
            return false;
        }

        private bool GetRtcYear(ref short Year)
        {
            if (Scope.isConnected)
            {
                short RxLength = 3;
                byte[] TxBuffer = { (byte)'P', 1, (byte)Common.eDeviceId.RTC, 4, 0, 0, 0, (byte)(RxLength - 1) };
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
            return false;
        }

        private bool GetRtcTime(ref short Hours, ref short Minutes, ref short Seconds)
        {
            if (Scope.isConnected)
            {
                short RxLength = 4;
                byte[] TxBuffer = { (byte)'P', 1, (byte)Common.eDeviceId.RTC, 51, 0, 0, 0, (byte)(RxLength - 1) };
                byte[] RxBuffer = { };
                if (Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength) == 0)
                {
                    Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer, RxLength);
                }
                if (RxBuffer.Length == RxLength && RxBuffer[RxLength - 1] == (byte)'#')
                {
                    Hours = RxBuffer[0];
                    Minutes = RxBuffer[1];
                    Seconds = RxBuffer[2];
                    return true;
                }
            }
            return false;
        }

        private bool SetRtcDate(double Month, double Day)
        {
            if (Scope.isConnected)
            {
                byte[] TxBuffer = { (byte)'P', 3, (byte)Common.eDeviceId.RTC, 131, (byte)Month, (byte)Day, 0, 0 };
                byte[] RxBuffer = { };
                if (Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                return RxBuffer[0] == (byte)'#';
            }
            return false;
        }

        private bool SetRtcYear(double Year)
        {
            if (Scope.isConnected)
            {
                byte YearHigh = (byte)(Year / 256);
                byte YearLow = (byte)(Year % 256);
                byte[] TxBuffer = { (byte)'P', 3, (byte)Common.eDeviceId.RTC, 132, YearHigh, YearLow, 0, 0 };
                byte[] RxBuffer = { };
                if (Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                return RxBuffer[0] == (byte)'#';
            }
            return false;
        }

        private bool SetRtcTime(double Hours, double Minutes, double Seconds)
        {
            if (Scope.isConnected)
            {
                byte[] TxBuffer = { (byte)'P', 4, (byte)Common.eDeviceId.RTC, 179, (byte)Hours, (byte)Minutes, (byte)Seconds, 0 };
                byte[] RxBuffer = { };
                if (Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer) == 0)
                {
                    Common.SendSerialPortCommand(ref TxBuffer, out RxBuffer);
                }
                return RxBuffer[0] == (byte)'#';
            }
            return false;
        }
    }
}
