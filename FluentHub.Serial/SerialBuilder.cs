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
                   new ModelContextFactory<T,SerialPort>(
                        new SerialPortFactory(()=>new SerialPort(portName, baudRate, parity, dataBits, stopBits))
                        , (SerialPort x) => x.BuildContextBySerialPort()
                        , new SuspendedDisposalSource(1000) // todo 変更方法を考える
                        , @this.Logger));
        }

        public static IIOContext<byte[]> BuildContextBySerialPort(
            this SerialPort @this)
        {
            return
                 new StreamContext(
                    @this.BaseStream
                    , () => @this.Dispose());
        }

    }
}
