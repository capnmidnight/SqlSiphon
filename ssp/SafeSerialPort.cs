using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace ssp
{
    public class SafeSerialPort : SerialPort
    {
        public SafeSerialPort(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
            : base(portName, baudRate, parity, dataBits, stopBits)
        {

        }

        public SafeSerialPort(string portName, int baudRate)
            : base(portName, baudRate)
        {

        }

        public SafeSerialPort()
            : base()
        {
        }

        public new void Open()
        {
            base.Open();
            GC.SuppressFinalize(this.BaseStream);
        }

        public new void Dispose()
        {
            this.Dispose(true);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.Container != null))
            {
                this.Container.Dispose();
            }
            try
            {
                if (this.BaseStream.CanRead)
                {
                    this.BaseStream.Close();
                    GC.ReRegisterForFinalize(this.BaseStream);
                }
            }
            catch
            {
                // ignore exception - bug with USB serial adapters.
            }
            base.Dispose(disposing);
        }
    }
}
