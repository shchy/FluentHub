﻿using FluentHub.IO;
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

        #region work for intellisence
        public static IAppBuilder<AppIF> RegisterConverter<AppIF>(
            this IAppBuilder<AppIF> @this
            , IModelConverter<AppIF> converter)
        {
            return @this.RegisterConverter<AppIF, IAppBuilder<AppIF>>(converter);
        }


        public static IAppBuilder<AppIF> RegisterValidator<AppIF>(
            this IAppBuilder<AppIF> @this
            , IModelValidator<AppIF> validator)
        {
            return @this.RegisterValidator<AppIF, IAppBuilder<AppIF>>(validator);
        } 
        #endregion

        public static Builder RegisterConverter<AppIF, Builder>(
            this Builder @this
            , IModelConverter<AppIF> converter)
            where Builder : IAppBuilder<AppIF>
        {
            @this.ModelConverters.Add(converter);
            return @this;
        }

        public static Builder RegisterValidator<AppIF,Builder>(
            this Builder @this
            , IModelValidator<AppIF> validator)
            where Builder : IAppBuilder<AppIF>
        {
            @this.ModelValidators.Add(validator);
            return @this;
        }

        public static IAppBuilder<AppIF> Use<AppIF>(
            this IAppBuilder<AppIF> @this
            , Action<IAppBuilder<AppIF>> configure)
        {
            configure(@this);
            return @this;
        }

        public static IAppBuilder<AppIF, NativeIO> Use<AppIF, NativeIO>(
            this IAppBuilder<AppIF, NativeIO> @this
            , Action<IAppBuilder<AppIF, NativeIO>> configure)
        {
            configure(@this);
            return @this;
        }

        public static IAppBuilder<AppIF, NativeIO> MakeApp<AppIF, NativeIO>(
            this ContainerBootstrap @this
            , INativeIOFactory<NativeIO> nativeFactory
            , Func<NativeIO, IIOContext<byte[]>> nativeToStreamContext)
        {
            var appBuilder =
                new AppBuilder<AppIF, NativeIO>(
                    @this.Logger
                    , @this.DependencyContainer
                    , nativeFactory);
            appBuilder.NativeToStreamContext = nativeToStreamContext;
            @this.AppBuilders.Add(appBuilder);

            return
                appBuilder;
        }

        public static IAppBuilder<AppIF, NativeIO> MakeApp<AppIF, NativeIO>(
            this ContainerBootstrap @this
            , INativeIOFactory<NativeIO> primary
            , INativeIOFactory<NativeIO> secondary
            , int switchMillisecond
            , Func<NativeIO, IIOContext<byte[]>> nativeToStreamContext)
        {
            return
                @this.MakeApp<AppIF, NativeIO>(
                    new DualNativeIOFactory<NativeIO>(primary, secondary, switchMillisecond)
                    , nativeToStreamContext);
        }

        public static IAppBuilder<AppIF, NativeIO> MakeApp<AppIF, NativeIO>(
            this ContainerBootstrap @this
            , IEnumerable<INativeIOFactory<NativeIO>> nativeIOFactorys
            , Func<NativeIO, IIOContext<byte[]>> nativeToStreamContext)
        {
            return
                @this.MakeApp<AppIF, NativeIO>(
                    new MultiNativeIOFactory<NativeIO>(nativeIOFactorys)
                    , nativeToStreamContext);
            
        }

        public static ISession GetSession<AppIF>(
            this IContextApplication<AppIF> app
            , IIOContext<AppIF> context)
        {
            return
                app.GetTypedSession(context, typeof(ISession));
        }

        public static ISession GetTypedSession<AppIF>(
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
            var method = typeof(BuildExtension).GetMethod(nameof(BuildExtension.GetTypedSession), BindingFlags.Public | BindingFlags.Static);
            var typedmethod = method.MakeGenericMethod(new[] { appType });
            var session = (ISession)typedmethod.Invoke(null, new object[] { app, context, sessionType });
            return session;
        }
    }
}
