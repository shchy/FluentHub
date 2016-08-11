using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.UDP
{
    public class FakeStream : Stream
    {
        private List<byte> cache;
        private bool isDisposed;
        private IIO real;

        public FakeStream(IIO real)
        {
            this.cache = new List<byte>();
            this.real = real;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            this.isDisposed = true;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var data = real.Read();
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

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer.Length != count || offset != 0)
            {
                var xs = new byte[count];
                Buffer.BlockCopy(buffer, offset, xs, 0, count);
                buffer = xs;
            }
            this.real.Write(buffer);
        }

        public override bool CanRead => !this.isDisposed;

        public override bool CanSeek => false;

        public override bool CanWrite => !this.isDisposed;

        public override void Flush()
        {

        }

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

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

    }

}
