using FluentHub.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentHub.ModelConverter;
using System.Reflection;
using FluentHub.IO.Extension;
using FluentHub.Logger;
using FluentHub.Module;
using FluentHub.Hub;
using FluentHub.Hub.ModelValidator;

namespace FluentHub
{
    public static class BuildExtension
    {
        public static IAppBuilder<AppIF> RegisterConverter<AppIF>(
            this IAppBuilder<AppIF> @this
            , IModelConverter<AppIF> converter)
        {
            @this.ModelConverters.Add(converter);
            return @this;
        }

        public static IAppBuilder<AppIF> RegisterValidator<AppIF>(
            this IAppBuilder<AppIF> @this
            , IModelValidator<AppIF> validator)
        {
            @this.ModelValidators.Add(validator);
            return @this;
        }

        public static IAppBuilder<AppIF> RegisterSession<AppIF>(
            this IAppBuilder<AppIF> @this
            , Func<object,ISession> makeSession)
        {
            @this.MakeSession = makeSession;
            return @this;
        }

        public static ISession GetSession<AppIF>(
            this IContextApplication<AppIF> app
            , IIOContext<AppIF> context)
        {
            return
                app.GetSession(context, typeof(ISession));
        }

        public static ISession GetSession<AppIF>(
            this IContextApplication<AppIF> app
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
            var method = typeof(BuildExtension).GetMethod(nameof(BuildExtension.GetContexts), BindingFlags.Public | BindingFlags.Static);
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
            var method = typeof(BuildExtension).GetMethod(nameof(BuildExtension.GetSession), BindingFlags.Public | BindingFlags.Static);
            var typedmethod = method.MakeGenericMethod(new[] { appType });
            var session = (ISession)typedmethod.Invoke(null, new object[] { app, context, sessionType });
            return session;
        }
    }
}
