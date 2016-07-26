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
        private string sendHost;
        private int sendPort;
        private int recvPort;


        public UDPIO(string host, int port)
            :this(host, port, port)
        {
        }


        public UDPIO(string host, int sendPort, int recvPort)
        {
            // todo sendPortとrecvPortを分けなくてもいいんだけど１PCでテストできるように分けることも可能にしておく
            this.client = new UdpClient(new IPEndPoint(IPAddress.Any, recvPort));
            this.sendHost = host;
            this.sendPort = sendPort;
            this.recvPort = recvPort;
        }

        public byte[] Read()
        {
            var remote = new IPEndPoint(IPAddress.Any, recvPort);
            return client.Receive(ref remote);
        }

        public int Write(byte[] data) 
        {
            var sended = 0;
            do
            {
                sended += client.Send(data, data.Length - sended, sendHost, sendPort);
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
