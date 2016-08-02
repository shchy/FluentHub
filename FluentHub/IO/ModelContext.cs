using FluentHub.IO.Extension;
using FluentHub.Logger;
using FluentHub.ModelConverter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHub.IO
{
    public class ModelContext<T> : IIOContext<T>
    {
        private IIOContext<byte[]> byteContext;
        private IEnumerable<IModelConverter<T>> converters;
        private List<byte> bytecache;
        private List<T> modelcache;
        private bool isDisposed;
        private object syncObj = new object();
        private ILogger logger;
        private ISuspendedSentence jammedPacketCleaner;

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


        public ModelContext(IIOContext<byte[]> byteContext
            , IEnumerable<IModelConverter<T>> converters
            , ISuspendedSentence jammedPacketCleaner
            , ILogger logger)
        {
            this.jammedPacketCleaner = jammedPacketCleaner;
            this.logger = logger;
            this.byteContext = byteContext;
            this.converters = converters;
            this.bytecache = new List<byte>();
            this.modelcache = new List<T>();
            this.byteContext.Received += ByteContext_Received;
        }

        private void ByteContext_Received(object sender, EventArgs e)
        {
            try
            {
                var isBuilded = false;
                lock (this.syncObj)
                {
                    var bytes = this.byteContext.Read().ToArray();
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
            catch (Exception ex)
            {
                logger.Exception(ex);
            }
        }

        bool TryBuildModel(List<byte> bytes, IList<T> models)
        {
            // 何もなければ何もしない。
            if (bytes.Any() == false)
            {
                return false;
            }
            // todo 不要なメッセージは破棄しないと
            var result =
                this.converters.TryToBuild(bytes);
                
            // 現在のパケットキャッシュからどのコンバーターもモデルへ変換できなかった
            if (result == null || result.Item1 == null)
            {
                // パケットが詰まってるかもしれないので執行猶予付きでパケットキャッシュをクリア
                this.jammedPacketCleaner.Sentence(ClearJammedPacket);
                return false;
            }
            // モデル変換できたということは疑いが晴れたので解放
            this.jammedPacketCleaner.Expiration();

            this.logger.Debug($"recv {result.Item1.GetType().Name}");

            models.Add(result.Item1);
            bytes.RemoveRange(0, result.Item2);

            return true;
        }

        void ClearJammedPacket()
        {
            lock((this.bytecache as ICollection).SyncRoot)
            {
                this.bytecache.Clear();
            }
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
                this.logger.Debug($"send {model.GetType().Name}");
                this.byteContext.Write(bytes);
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

    public static class ModelConverterExtension
    {
        public static Tuple<Model, int> TryToBuild<Model>(this IEnumerable<IModelConverter<Model>> @this, IEnumerable<byte> bytes)
        {
            return
                @this
                .Where(c => c.CanBytesToModel(bytes))
                .Select(c => c.ToModel(bytes))
                .Where(r => r.Item1 != null)
                .FirstOrDefault();
        }
    }
}
