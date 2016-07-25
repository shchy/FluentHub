using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHub.IO.Extension
{
    public class StreamContext : IIOContext<byte>
    {
        private object syncObj = new object();
        private bool disposed;
        private Task readTask;
        private List<byte> cache;
        private Stream stream;
        private CancellationTokenSource readCancelToken;
        private Action disposing;

        public event EventHandler Received;
        public bool IsAny
        {
            get
            {
                lock ((cache as ICollection).SyncRoot)
                {
                    return cache.Any();
                }
            }
        }
        public bool CanUse => stream != null && stream.CanRead && stream.CanWrite;


        public StreamContext(Stream stream, Action disposing)
        {
            this.disposing = disposing;
            this.stream = stream;
            this.readCancelToken = new System.Threading.CancellationTokenSource();
            this.cache = new List<byte>();
            this.readTask = Task.Run(() => TakeBuffer(this.stream));
            this.readTask.ContinueWith(EndTakeBuffer);

        }

        private void EndTakeBuffer(Task task)
        {
            if (task.Exception != null)
            {
                this.Dispose();
            }
        }

        private void TakeBuffer(Stream stream)
        {
            var buff = new byte[1024];
            while (this.disposed == false)
            {
                var readTask = null as Task<int>;
                lock (this.syncObj)
                {
                    readTask = stream.ReadAsync(buff, 0, buff.Length);
                }

                // disposeでキャンセル可能にする
                readTask.Wait(this.readCancelToken.Token);
                // キャンセルを判定する
                if (readTask.IsCanceled || readTask.IsFaulted)
                {
                    continue;
                }
                var readedLength = readTask.Result;
                // キャッシュに突っ込む
                ReadToBuffer(buff, readedLength);
                // イベントがあれば発行
                if (this.Received == null)
                {
                    return;
                }
                this.Received(this, EventArgs.Empty);
            }
        }

        private void ReadToBuffer(byte[] buff, int readedLength)
        {
            var buffer = new byte[readedLength];
            Buffer.BlockCopy(buff, 0, buffer, 0, readedLength);

            lock ((this.cache as ICollection).SyncRoot)
            {
                this.cache.AddRange(buffer);
            }
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }
            this.disposed = true;
            this.readCancelToken.Cancel();
            if (this.readTask.IsCompleted == false)
            {
                this.readTask.Wait();
            }
            this.readTask.Dispose();

            lock ((this.cache as ICollection).SyncRoot)
            {
                this.cache.Clear();
            }

            lock (this.syncObj)
            {
                this.stream.Dispose();
            }

            if (this.disposing != null)
            {
                this.disposing();
            }
        }

        public byte Read(Func<byte, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<byte> ReadAll()
        {
            lock ((this.cache as ICollection).SyncRoot)
            {
                var buff = this.cache.ToArray();
                this.cache.Clear();
                return buff;
            }
        }

        public byte Read()
        {
            throw new NotImplementedException();
        }

        public void Write(byte model)
        {
            lock (this.syncObj)
            {
                this.stream.WriteByte(model);
            }
        }

        public void WriteAll(IEnumerable<byte> models)
        {
            var data = models.ToArray();
            lock (this.syncObj)
            {
                this.stream.Write(data, 0, data.Length);
            }
        }
    }
}
