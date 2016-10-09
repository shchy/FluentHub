using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public class MultiNativeIOFactory<NativeIO> : INativeIOFactory<NativeIO>
    {
        private IEnumerable<INativeIOFactory<NativeIO>> factorys;

        public MultiNativeIOFactory(
            IEnumerable<INativeIOFactory<NativeIO>> factorys)
        {
            this.factorys = factorys;
        }

        public void Dispose()
        {
            foreach (var item in factorys)
            {
                item.Dispose();
            }
        }

        public bool IsAlreadyEnough()
        {
            return this.factorys.All(f => f.IsAlreadyEnough());
        }

        public NativeIO Make()
        {
            var factory = factorys.FirstOrDefault(f => f.IsAlreadyEnough());

            // すでに必要な接続を確立できている場合
            if (factory == null)
            {
                return default(NativeIO);
            }

            var io = factory.Make();
            // 生成できれば問題ないのでそのまま返す
            if (io != null)
            {
                return io;
            }
            return io;
        }
    }
}
