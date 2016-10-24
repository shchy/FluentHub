using FluentHub.IO.Extension;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.UDP
{
    public class UDPIO : IIO
    {
        private UdpClient client;
        public IPEndPoint LocalPoint { get; private set; }
        public IPEndPoint RemotePoint { get; private set; }
        

        public UDPIO(
            IPEndPoint localPoint
            , IPEndPoint remotePoint)
        {
            // memo sendPortとrecvPortを分けなくてもいいんだけど１PCでテストできるように分けることも可能にしておく
            this.LocalPoint = localPoint;
            this.RemotePoint = remotePoint;
            this.client = new UdpClient(this.LocalPoint);
        }

        public byte[] Read()
        {
            var _ = null as IPEndPoint;
            var bytes = client.Receive(ref _);
            if (_.Address.Equals(this.RemotePoint.Address) == false 
                && this.RemotePoint.Address.Equals(IPAddress.Any) == false)
            {
                throw new Exception($"Received from an unexpected IP listen={this.RemotePoint.Address} recv={_.Address}");
            }
            return bytes;
        }

        public int Write(byte[] data) 
        {
            var sended = 0;
            do
            {
                sended += client.Send(data, data.Length - sended, RemotePoint);
                if (sended < data.Length)
                {
                    Buffer.BlockCopy(data, sended, data, 0, data.Length - sended);
                }
            }
            while (sended < data.Length);
            return data.Length;
        }
        
        public void Dispose()
        {   
        }
    }
}
