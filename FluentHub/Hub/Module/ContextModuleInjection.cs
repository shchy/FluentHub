using FluentHub.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub.Module
{
    class ContextModuleInjection<AppIF> : ModuleInjection
    {
        private IIOContext<AppIF> context;

        public ContextModuleInjection(IModuleInjection parent, IIOContext<AppIF> context) : base(parent)
        {
            this.context = context;
            this.Add<IIOContext<AppIF>>(() => context);
        }

        public override object Resolve(Type type)
        {
            if (typeof(AppIF).IsAssignableFrom(type))
            {
                // AppIFの実装型だったらメッセージを読み込んでから入れてあげる。
                var msg = context.Read(m => m.GetType() == type);
                if (msg == null)
                {
                    // 受信できてなかったら呼ばない
                    return null;
                }
                return msg;
            }
            return base.Resolve(type);
        }
    }
}
