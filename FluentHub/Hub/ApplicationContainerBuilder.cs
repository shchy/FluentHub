using FluentHub.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentHub.Logger;
using FluentHub.ModelConverter;
using System.Reflection;
using FluentHub.IO.Extension;
using System.Collections;

namespace FluentHub.Hub
{
    public static class ApplicationContainerBuilder
    {
        public static IContextApplication<T> MakeApp<T>(
            this IApplicationContainer @this
            , IModelContextFactory<T> modelContextFactory
            , Func<object, ISession> makeSession)
        {
            var app =
                new Application<T>(
                    @this.MakeContextPool<T>()
                    , modelContextFactory
                    , new SequenceRunnerFacade<T>(@this.Logger) // todo defaultはこれでいいけどどこかで変更できるようにはしたいよね
                    , @this.ModuleInjection
                    , @this.Logger
                    , makeSession
                    );
            @this.Add(app);
            return app;
        }

        public static IContextApplication<T> RegisterConverter<T>(
            this IContextApplication<T> @this
            , IModelConverter<T> converter)
        {
            @this.AddConverter(converter);
            return @this;
        }
        
        
        public static ISession GetSession<AppIF>(this IContextApplication<AppIF> app
            , IIOContext<AppIF> context
            , Type sessionType)
        {
            var haveSession = app.Sessions.ContainsKey(context);
            if (haveSession == false)
            {
                return null;
            }
            var session = app.Sessions[context];
            // 知らないセッション型だったら無効
            if (sessionType.IsInstanceOfType(session) == false)
            {
                return null;
            }
            return session;
        }


        // todo 適切なところへ移動
        // ModuleInjection_Missedからしか呼ばれてないはず。まとめたい。
        public static IEnumerable<IIOContext<AppIF>> GetContexts<AppIF>(
            this IContextApplication<AppIF> app)
        {
            return app.Pool.Get().ToArray();
        }


        public static IEnumerable<object> GetContextsNotTyped(
            this IContextApplication app)
        {
            var appType = app.GetType().GetGenericArguments()[0];
            var method = typeof(ApplicationContainerBuilder).GetMethod(nameof(ApplicationContainerBuilder.GetContexts), BindingFlags.Public | BindingFlags.Static);
            var typedmethod = method.MakeGenericMethod(new[] { appType });
            var contexts = typedmethod.Invoke(null, new object[] { app });
            return (IEnumerable<object>)contexts;
        }


        public static ISession GetSessionNotTyped(
            this IContextApplication app
            , object context
            , Type sessionType)
        {
            var appType = app.GetType().GetGenericArguments()[0];
            var method = typeof(ApplicationContainerBuilder).GetMethod(nameof(ApplicationContainerBuilder.GetSession), BindingFlags.Public | BindingFlags.Static);
            var typedmethod = method.MakeGenericMethod(new[] { appType });
            var session = (ISession)typedmethod.Invoke(null, new object[] { app, context, sessionType });
            return session;
        }


    }

    
}
