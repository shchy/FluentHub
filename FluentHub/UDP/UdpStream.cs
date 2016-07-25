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
    public class UdpStream : Stream
    {
        private Func<byte[]> recv;
        private Action<byte[]> send;
        private List<byte> cache;

        public UdpStream(string host, int sendPort, int recvPort)
        {
            this.cache = new List<byte>();
            this.recv = MakeRecv(recvPort);
            this.send = MakeSend(host, sendPort);
        }

        private Action<byte[]> MakeSend(string host, int sendPort)
        {
            return bytes =>
            {
                using (var udp = new UdpClient())
                {
                    var sended = 0;
                    do
                    {
                        sended += udp.Send(bytes, bytes.Length - sended, host, sendPort);
                        if (sended < bytes.Length)
                        {
                            Buffer.BlockCopy(bytes, sended, bytes, 0, bytes.Length - sended);
                        }
                    }
                    while (sended < bytes.Length);
                }
            };
        }

        private Func<byte[]> MakeRecv(int recvPort)
        {
            return () =>
            {
                var point = new IPEndPoint(IPAddress.Any, recvPort);

                using (var udp = new UdpClient(point))
                {
                    return udp.Receive(ref point);
                }
            };
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override void Flush()
        {

        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var data = recv();
            var taked = null as byte[];
            lock ((cache as ICollection).SyncRoot)
            {
                cache.AddRange(data);
                taked = cache.Take(count).ToArray();
                cache.RemoveRange(0, taked.Length);
            }
            Buffer.BlockCopy(taked, 0, buffer, offset, taked.Length);

            return taked.Length;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer.Length != count || offset != 0)
            {
                var xs = new byte[count];
                Buffer.BlockCopy(buffer, offset, xs, 0, count);
                buffer = xs;
            }
            send(buffer);
        }
    }

}
