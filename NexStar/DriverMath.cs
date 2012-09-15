using System;
using System.Runtime.InteropServices;

namespace ASCOM.NexStar
{
    [ComVisible(false)]
    internal static class DriverMath
    {
        private const double MJD = 2400000.5d;
        private const double MJD0 = 2415020.0d;
        private const double J2000 = (2451545.0 - MJD0);
        /* ratio of from synodic (solar) to sidereal (stellar) rate */
        private const double SIDRATE = .9972695677d;

        public static double DegRad(double x)
        {
            return x * Math.PI / 180d;
        }

        public static double RadHr(double x)
        {
            return DegHr(RadDeg(x));
        }

        public static double RadDeg(double x)
        {
            return x * 180d / Math.PI;
        }

        public static double DegHr(double x)
        {
            return x / 15d;
        }

        /* given an mjd, truncate it to the beginning of the whole day */
        public static double MjdDay(double mj)
        {
            return (Math.Floor(mj - 0.5) + 0.5);
        }

        /* given an mjd, return the number of hours past midnight of the whole day */
        public static double MjdHr(double mj)
        {
            return ((mj - MjdDay(mj)) * 24.0);
        }

        public static void UtcGst(double mj, double utc, out double gst)
        {
            double t0 = GmSt0(mj);
            gst = (1.0 / SIDRATE) * utc + t0;
            Range(ref gst, 24.0);
        }

        /* gmst0() - return Greenwich Mean Sidereal Time at 0h UT; stern */
        public static double GmSt0(double mj)	/* date at 0h UT in julian days since MJD0 */
        {
            double T = ((int)(mj - 0.5) + 0.5 - J2000) / 36525.0;
            double x = 24110.54841 + (8640184.812866 + (0.093104 - 6.2e-6 * T) * T) * T;
            x /= 3600.0;
            Range(ref x, 24.0);
            return (x);
        }

        public static void Range(ref double v, double r)
        {
            v -= r * Math.Floor(v / r);
        }

        public static double ToJulianDate(DateTime DateTime)
        {
            double yr, mn, dy, h, m, s, JD, a, b, c, d, f, g;
            yr = DateTime.Year;
            mn = DateTime.Month;
            dy = DateTime.Day;
            h = DateTime.Hour;
            m = DateTime.Minute;
            s = DateTime.Second;
            f = dy + ((h + (m / 60) + (s / 60 / 60)) / 24);
            if (yr < 1582)
            {
                g = 0;
            }
            else
            {
                g = 1;
            }
            if ((mn == 1) || (mn == 2))
            {
                yr -= 1;
                mn += 12;
            }
            a = (long)Math.Floor(yr / 100);
            b = (2 - a + (long)Math.Floor(a / 4)) * g;
            if (yr < 0)
            {
                c = (int)Math.Floor((365.25 * yr) - 0.75);
            }
            else
            {
                c = (int)Math.Floor(365.25 * yr);
            }
            d = (int)Math.Floor(30.6001 * (mn + 1));
            JD = b + c + d + 1720994.5;
            JD += f;
            return JD;
        }

        public static double ToModifiedJulianDay(double JulianDate)
        {
            return JulianDate -= MJD;
        }

        public static double ToJ2000(double JulianDate)
        {
            return JulianDate -= MJD0;
        }

        public static void ConvertCoordinate(double x, out double Degrees, out double Minutes, out double Seconds)
            /* convert double to deg, min, sec */
        {
            Seconds = Math.Round(x * 3600);
            Degrees = Seconds / 3600;
            Seconds = Math.Abs(Seconds % 3600);
            Minutes = Seconds / 60;
            Seconds %= 60;
        }
    }
}
