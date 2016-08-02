using FluentHub.Hub;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHub.IO.Extension
{
    public class StreamContext : IIOContext<byte[]>
    {
        private object syncObj = new object();
        private bool disposed;
        private Task takingTask;
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
        public bool CanUse => stream != null && stream.CanRead && stream.CanWrite && IsSocketTest(stream as NetworkStream);

        private bool IsSocketTest(NetworkStream networkStream)
        {
            if (networkStream == null)
            {
                return true;
            }

            var maybeSocket = 
                typeof(NetworkStream).InvokeMember(
                    "Socket"
                    , System.Reflection.BindingFlags.Instance 
                    | System.Reflection.BindingFlags.GetProperty 
                    | System.Reflection.BindingFlags.Public 
                    | System.Reflection.BindingFlags.NonPublic
                    , null
                    , networkStream
                    , null);

            var socket = maybeSocket as Socket;
            if (socket == null)
            {
                return true;
            }

            try
            {
                if (socket.Poll(0, SelectMode.SelectRead))
                {
                    byte[] checkConn = new byte[1];
                    if (socket.Receive(checkConn, SocketFlags.Peek) == 0)
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public StreamContext(Stream stream, Action disposing)
        {
            this.disposing = disposing;
            this.stream = stream;
            this.readCancelToken = new System.Threading.CancellationTokenSource();
            this.cache = new List<byte>();
            this.takingTask = Task.Run(() => { try { TakeBuffer(this.stream); } catch (Exception) { this.Dispose(); } });
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
                readTask.SafeWait(this.readCancelToken.Token);
                // キャンセルを判定する
                if (readTask.IsCanceled || readTask.IsFaulted || this.readCancelToken.IsCancellationRequested)
                {
                    this.readCancelToken = new System.Threading.CancellationTokenSource();
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
                Task.Run(() => this.Received(this, EventArgs.Empty));
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
            if (this.takingTask.IsCompleted == false)
            {
                this.takingTask.Wait();
            }
            this.takingTask.Dispose();

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

        public byte[] Read(Func<byte[], bool> predicate)
        {
            // todo 使うことないよね？
            throw new NotImplementedException();
        }

        public byte[] Read()
        {
            lock ((this.cache as ICollection).SyncRoot)
            {
                var buff = this.cache.ToArray();
                this.cache.Clear();
                return buff;
            }
        }
        
        public void Write(byte[] models)
        {
            var data = models.ToArray();
            lock (this.syncObj)
            {
                this.stream.Write(data, 0, data.Length);
            }
        }
    }

    public static class StreamExtension
    {
        public static IIOContext<byte[]> BuildContextByStream(
           this Stream @this)
        {
            return
                new StreamContext(
                    @this
                    , () => @this.Close());
        }
    }
}
