using FluentHub.Hub.Module;
using FluentHub.IO;
using FluentHub.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public static class ModuleExtension
    {
        /// <summary>
        /// IModuleInjectionにAppの持っているContextPoolを登録する
        /// </summary>
        /// <typeparam name="AppIF"></typeparam>
        /// <param name="this"></param>
        /// <param name="app"></param>
        static void RegisterResolver<AppIF>(this IModuleInjection @this, IContextApplication<AppIF> app)
        {
            @this.Add<IEnumerable<IIOContext<AppIF>>>(()=> app.Pool.Get().ToArray());
        }


        /// <summary>
        /// 型が不明なIContextApplicationをIContextApplication<Typed>に変換してRegisterResolverを呼び出す
        /// </summary>
        /// <param name="this"></param>
        /// <param name="app"></param>
        static void TryRegisterResolver(this IModuleInjection @this, IContextApplication app)
        {
            var appType = app.GetType().GetGenericArguments()[0];
            var typedApp = typeof(IContextApplication<>).MakeGenericType(appType);
            
            var registerResolverMethod = typeof(ModuleExtension).GetMethod(nameof(ModuleExtension.RegisterResolver), BindingFlags.NonPublic | BindingFlags.Static);
            var typedRegisterResolverMethod = registerResolverMethod.MakeGenericMethod(appType);
            typedRegisterResolverMethod.Invoke(null, new object[] { @this, app });
        }

        /// <summary>
        /// IModuleRegisterHelperを生成する
        /// </summary>
        /// <typeparam name="AppIF"></typeparam>
        /// <typeparam name="Module"></typeparam>
        /// <param name="app"></param>
        /// <param name="getModule"></param>
        /// <param name="injection"></param>
        /// <returns></returns>
        static IModuleRegisterHelper MakeHelper<AppIF, Module>(
            IContextApplication<AppIF> app
            , Func<Module> getModule
            , IModuleInjection injection)
        {
            return new ModuleRegisterHelper<AppIF, Module>(app, getModule, injection, app.Logger);
        }

        /// <summary>
        /// Module型のクラスが持っているPublicメソッドをAppのシーケンスメソッドとして登録する
        /// </summary>
        /// <typeparam name="Module"></typeparam>
        /// <param name="this"></param>
        /// <param name="module"></param>
        /// <returns></returns>
        public static IApplicationContainer RegisterModule<Module>(
            this IApplicationContainer @this
            , Module module)
        {
            // Moduleのメソッド引数を解決するDIコンテナ
            var resolver = new ModuleInjection();
            return @this.RegisterModule(resolver, ()=>module);
        }

        public static IApplicationContainer RegisterModule<Module>(
            this IApplicationContainer @this
            , IModuleInjection resolver
            , Func<Module> getModule)
        {
            // Moduleのメソッド引数を解決するDIコンテナ
            foreach (var app in @this.GetApps().ToArray())
            {
                resolver.TryRegisterResolver(app);
            }

            foreach (var app in @this.GetApps().ToArray())
            {
                var appType = app.GetType().GetGenericArguments()[0];
                var makeHelperMethod = typeof(ModuleExtension).GetMethod(nameof(ModuleExtension.MakeHelper), BindingFlags.NonPublic | BindingFlags.Static);
                var typedMakeHelperMethod = makeHelperMethod.MakeGenericMethod(new[] { appType, typeof(Module) });
                var helper = typedMakeHelperMethod.Invoke(null, new object[] { app, getModule, resolver }) as IModuleRegisterHelper;
                helper.Setup();
            }
            return @this;
        }




    }
}
