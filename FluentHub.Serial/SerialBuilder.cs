using FluentHub.Hub;
using FluentHub.IO;
using FluentHub.IO.Extension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHub.Serial
{
    public static class SerialBuilder
    {
        public static IContextApplication<T> MakeAppBySerialPort<T>(
            this IApplicationContainer @this
            , IEnumerable<IModelConverter<T>> converters
            , string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            return
               @this.MakeApp(
                   converters
                   , new DelegateContextFactory<SerialPort>(
                       new NativeIORunnableFactory<SerialPort>(
                            new SerialPortFactory(()=>new SerialPort(portName, baudRate, parity, dataBits, stopBits))
                            , c => c.Close())
                       , (SerialPort x) => x.BuildContextBySerialPort()));
        }

        public static IIOContext<byte> BuildContextBySerialPort(
            this SerialPort @this)
        {
            return
                 new StreamContext(
                    @this.BaseStream
                    , () => @this.Dispose());
        }

    }
}
