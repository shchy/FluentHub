using FluentHub.IO.Extension;
using FluentHub.Logger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.IO
{
    public class ModelContext<T> : IIOContext<T>
    {
        private IIOContext<byte> byteContext;
        private IEnumerable<IModelConverter<T>> converters;
        private List<byte> bytecache;
        private List<T> modelcache;
        private bool isDisposed;
        private object syncObj = new object();

        public event EventHandler Received;
        public bool IsAny
        {
            get
            {
                lock ((modelcache as ICollection).SyncRoot)
                {
                    return modelcache.Any();
                }
            }
        }
        public bool CanUse => byteContext.CanUse;


        public ModelContext(IIOContext<byte> byteContext
            , IEnumerable<IModelConverter<T>> converters)
        {
            this.byteContext = byteContext;
            this.converters = converters;
            this.bytecache = new List<byte>();
            this.modelcache = new List<T>();
            this.byteContext.Received += ByteContext_Received;
        }

        private void ByteContext_Received(object sender, EventArgs e)
        {
            var isBuilded = false;
            lock (this.syncObj)
            {
                var bytes = this.byteContext.ReadAll().ToArray();
                lock ((this.bytecache as ICollection).SyncRoot)
                {
                    this.bytecache.AddRange(bytes);
                    lock ((this.modelcache as ICollection).SyncRoot)
                    {
                        while (TryBuildModel(this.bytecache, this.modelcache))
                        {
                            isBuilded = true;
                        }
                    }
                }
            }
            if (isBuilded && Received != null)
            {
                Received(this, EventArgs.Empty);
            }
        }


        bool TryBuildModel(List<byte> bytes, IList<T> models)
        {
            // todo 不要なメッセージは破棄しないと
            var result =
                this.converters
                .Where(c => c.CanBytesToModel(bytes))
                .Select(c => c.ToModel(bytes))
                .Where(r => r.Item1 != null)
                .FirstOrDefault();
                
            if (result == null ||result.Item1 == null)
            {
                return false;
            }
            models.Add(result.Item1);
            bytes.RemoveRange(0, result.Item2);
            return true;
        }

        public T Read(Func<T, bool> predicate)
        {
            lock ((this.modelcache as ICollection).SyncRoot)
            {
                var items = this.modelcache.Where(predicate).ToArray();
                if (items.Any() == false)
                {
                    return default(T);
                }
                var item = items.First();
                this.modelcache.Remove(item);
                return item;
            }
        }

        public IEnumerable<T> ReadAll()
        {
            lock ((this.modelcache as ICollection).SyncRoot)
            {
                var items = this.modelcache.ToArray();
                this.modelcache.Clear();
                return items;
            }
        }

        public T Read()
        {
            lock ((this.modelcache as ICollection).SyncRoot)
            {
                if (this.modelcache.Any() == false)
                {
                    return default(T);
                }
                var item = this.modelcache[0];
                this.modelcache.Remove(item);
                return item;
            }
        }

        public void Write(T model)
        {
            lock (this.syncObj)
            {
                var converter = this.converters.FirstOrDefault(c => c.CanModelToBytes(model));
                System.Diagnostics.Debug.Assert(converter != null, $"can not convert model {model.GetType().Name}");
                var bytes = converter.ToBytes(model);
                this.byteContext.WriteAll(bytes);
            }
        }

        public void WriteAll(IEnumerable<T> models)
        {
            foreach (var item in models)
            {
                Write(item);
            }
        }

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }
            this.isDisposed = true;
            this.modelcache.Clear();
            this.bytecache.Clear();
            this.byteContext.Dispose();
        }
    }

    public static class ModelContext
    {
        public static IIOContext<T> BuildContext<T>(
            this IIOContext<byte> @this
            , IEnumerable<IModelConverter<T>> converters
            , ILogger logger)
        {
            return
                new ModelContext<T>(
                    new IOContextLoggerProxy<byte>(
                        @this
                        , logger)
                    , converters);
        }
    }
}
