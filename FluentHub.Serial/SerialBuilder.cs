using FluentHub.Hub;
using FluentHub.IO;
using FluentHub.IO.Extension;
using FluentHub.Serial;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public static class SerialBuilder
    {
        public static IContextApplication<T> MakeAppBySerialPort<T>(
            this IApplicationContainer @this
            , string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            return
               @this.MakeApp<T>(
                   new NativeIOToContextMaker<SerialPort>(
                        new SerialPortFactory(()=>new SerialPort(portName, baudRate, parity, dataBits, stopBits))
                        , (SerialPort x) => x.BuildContextBySerialPort()
                        , c => c.Close()));
        }

        public static IIOContext<byte[]> BuildContextBySerialPort(
            this SerialPort @this)
        {
            var context =
                 new StreamContext(
                    @this.BaseStream
                    , () => @this.Dispose());
            context.StartAutoTakeBuffer();
            return context;
        }

    }
}
