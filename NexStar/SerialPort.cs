using System;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Timers;
using System.Runtime.InteropServices;

namespace ASCOM.NexStar
{
    [ComVisible(false)]
    internal class SerialPort : IDisposable
    {
        private System.IO.Ports.SerialPort sp = null;
        private int readTout = 4;
        private int writeTout = 4;
        private object SyncLock = null;
        private Encoding encoding = null;
        private bool IsDisposed = true;

        public bool isOpen
        {
            get { return sp.IsOpen; }
        }

        public static string[] GetPortNames()
        {
            return System.IO.Ports.SerialPort.GetPortNames();
        }

        public int ReadTimeOut
        {
            get { return readTout; }
            set { readTout = value; }
        }

        public int WriteTimeOut
        {
            get { return writeTout; }
            set { writeTout = value; }
        }

        public SerialPort(int port)
        {
            encoding = System.Text.Encoding.GetEncoding(1252);
            SyncLock = new object();
            sp = new System.IO.Ports.SerialPort("COM" + port);
            sp.DiscardNull = false;
            sp.BaudRate = 9600;
            sp.Handshake = Handshake.None;
            sp.DataBits = 8;
            sp.StopBits = StopBits.One;
            sp.Parity = Parity.None;
            sp.ReadTimeout = readTout * 1000;
            sp.WriteTimeout = writeTout * 1000;
            sp.Encoding = encoding;
            sp.Disposed += new EventHandler(Disposed);
            IsDisposed = false;
        }

        ~SerialPort()
        {
            int count = 0;
            if (sp.IsOpen)
            {
                sp.DiscardInBuffer();
                sp.DiscardOutBuffer();
                sp.Close();
            }
            sp.Dispose();
            while (!IsDisposed)
            {
                count++;
                Thread.Sleep(500);
                if (count == 60)
                {
                    break;
                }
            }
            sp = null;
            GC.Collect();
        }

        public bool Open()
        {
            try
            {
                if (sp != null)
                {
                    lock (SyncLock)
                    {
                        sp.Open();
                    }
                    return sp.IsOpen;
                }
                return false;
            }
            catch (Exception Ex)
            {
                Common.Log.LogMessage(Common.DriverId, "Open() failed " + Ex.Message);
                throw;
            }
        }

        public bool Close()
        {
            if (sp != null && sp.IsOpen)
            {
                lock (SyncLock)
                {
                    sp.DiscardInBuffer();
                    sp.DiscardOutBuffer();
                    sp.Close();
                }
            }
            return sp.IsOpen;
        }

        public void TransmitBinary(byte[] TxBuffer)
        {
            try
            {
                if (sp != null && sp.IsOpen)
                {
                    lock (SyncLock)
                    {
                        sp.Write(TxBuffer, 0, TxBuffer.Length);
                    }
                }
            }
            catch (Exception Ex)
            {
                Common.Log.LogMessage(Common.DriverId, "TransmitBinary() failed " + Ex.Message);
                throw;
            }
        }

        public byte[] ReceiveCountedBinary(int Count)
        {
            try
            {
                if (sp != null && sp.IsOpen)
                {
                    lock (SyncLock)
                    {
                        int count = 0;
                        string recieved = "";
                        while (recieved.Length < Count)
                        {
                            recieved += sp.ReadExisting();
                            Thread.Sleep(1);
                            count++;
                            if (count == sp.ReadTimeout)
                            {
                                throw new TimeoutException();
                            }
                        }
                        return encoding.GetBytes(recieved);
                    }
                }
                return null;
            }
            catch (Exception Ex)
            {
                Common.Log.LogMessage(Common.DriverId, "ReceiveCountedBinary() failed " + Ex.Message);
                throw;
            }
        }

        public byte[] ReceiveTerminatedBinary(byte[] Terminator)
        {
            try
            {
                if (sp != null && sp.IsOpen)
                {
                    lock (SyncLock)
                    {
                        int count = 0;
                        string recieved = "";
                        while (!recieved.Contains(encoding.GetString(Terminator)))
                        {
                            recieved += sp.ReadExisting();
                            Thread.Sleep(1);
                            count++;
                            if (count == sp.ReadTimeout)
                            {
                                throw new TimeoutException();
                            }
                        }
                        return encoding.GetBytes(recieved);
                    }
                }
                return null;
            }
            catch (Exception Ex)
            {
                Common.Log.LogMessage(Common.DriverId, "ReceiveTerminatedBinary() failed " + Ex.Message);
                throw;
            }
        }

        public void ClearBuffers()
        {
            try
            {
                if (sp != null && sp.IsOpen)
                {
                    lock (SyncLock)
                    {
                        sp.DiscardInBuffer();
                        sp.DiscardOutBuffer();
                    }
                }
            }
            catch (Exception Ex)
            {
                Common.Log.LogMessage(Common.DriverId, "ClearBuffers() failed " + Ex.Message);
                throw;
            }
        }

        private void Disposed(object sender, EventArgs e)
        {
            IsDisposed = true;
        }

        public void Dispose()
        {
            sp.Close();
        }
    }
}
