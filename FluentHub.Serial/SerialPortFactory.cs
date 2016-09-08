using System;
using System.IO.Ports;
using FluentHub.IO;
using FluentHub.Hub;
using System.Threading;

namespace FluentHub.Serial
{
    internal class SerialPortFactory : INativeIOFactory<SerialPort>
    {
        private SerialPort connectedPort;
        private bool isDisposed;
        private Func<SerialPort> make;

        public SerialPortFactory(Func<SerialPort> make)
        {
            this.make = make;
        }

        public void Dispose()
        {
            this.isDisposed = true;
        }

        public bool IsAlreadyEnough()
        {
            return this.connectedPort != null
                && connectedPort.IsOpen;
        }

        public SerialPort Make()
        {
            if (this.isDisposed)
            {
                return null;
            }

            // 接続済だったら接続しない
            if (IsAlreadyEnough())
            {
                Thread.Sleep(1000);
                return null;
            }
            
            var port = make();
            if (port.IsOpen == false)
            {
                port.Open();
            }

            this.connectedPort = port;
            return port;

        }
    }
}