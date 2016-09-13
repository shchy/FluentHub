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

namespace FluentHub
{
    public static class SerialBuilder
    {
        public static IAppBuilder<T> MakeAppBySerialPort<T>(
            this ContainerBootstrap @this
            , string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            var appBuilder = new AppBuilder<T, SerialPort>();
            appBuilder.Logger = @this.Logger;
            appBuilder.DependencyContainer = @this.DependencyContainer;
            @this.AppBuilders.Add(appBuilder);

            appBuilder.NativeIOFactory = new SerialPortFactory(() => new SerialPort(portName, baudRate, parity, dataBits, stopBits));
            appBuilder.NativeToStreamContext = (SerialPort x) => x.BuildContextBySerialPort();

            return
                appBuilder;
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
