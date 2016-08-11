using FluentHub.Hub;
using FluentHub.IO;
using FluentHub.IO.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Module
{
    class ContextModuleInjection<AppIF> : ModuleDependencyContainer
    {
        private IContextApplication<AppIF> app;
        private IIOContext<AppIF> context;

        public ContextModuleInjection(IContextApplication<AppIF> app, IIOContext<AppIF> context) : base(app.DependencyContainer)
        {
            this.app = app;
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
            else if (typeof(ISession).IsAssignableFrom(type))
            {
                // ISessionを求めていたらこの文脈のContextに紐づくSessionを返す
                var session = app.GetSession(context, type);
                return session;
            }
            else if (typeof(ISessionContext<,>) == type.GetGenericTypeDefinition())
            {
                // ISessionContext型を求めていたら
                // APPIFをチェック
                var appType = type.GetGenericArguments()[0];
                if (appType != typeof(AppIF))
                {
                    return null;
                }

                var sessionType = type.GetGenericArguments()[1];
                var session = app.GetSession(context, sessionType);
                var sessionContextType = typeof(SessionContext<,>).MakeGenericType(typeof(AppIF), sessionType);
                var sessionContext = Activator.CreateInstance(sessionContextType, context, session);
                return sessionContext;

            }
            return base.Resolve(type);
        }
    }
}
