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
            return @this.RegisterModule<Module>(()=>module);
        }

        public static IApplicationContainer RegisterModule<Module>(
            this IApplicationContainer @this
            , Func<object> getModule)
        {
            // シーケンスモジュールのpublicメソッドを取り出す
            var methods =
                from method in typeof(Module).GetMethods()
                where method.IsPublic
                where method.DeclaringType == typeof(Module)
                select method;

            foreach (var method in methods)
            {
                @this.RegisterSequence(method, getModule);
            }
            return @this;
        }
        
        public static IApplicationContainer RegisterSequence(
            this IApplicationContainer @this
            , MethodInfo method
            , Func<object> getModule)
        {
            foreach (var app in @this.GetApps().ToArray())
            {
                // ModuleExtension.RegisterSequenceを呼びたいだけ
                var appType = app.GetType().GetGenericArguments()[0];
                var registerSequence = typeof(ModuleExtension).GetMethod(nameof(ModuleExtension.RegisterSequence), BindingFlags.NonPublic | BindingFlags.Static);
                var typedRegisterSequence = registerSequence.MakeGenericMethod(new[] { appType });
                typedRegisterSequence.Invoke(null, new object[] { app, @this.ModuleInjection, method, getModule });
            }
            return @this;
        }

        static void MakeSequence<AppIF>(
            IContextApplication<AppIF> app
            , IModuleInjection injection
            , MethodInfo method
            , Func<object> getModule)
        {
            // 引数の型をチェックして次の引数がいずれかあればシーケンスとみなす。
            // AppIF型の何か
            // IIOContext<AppIF>型のコンテキスト
            var prms = method.GetParameters();
            var isKnown1 = (Func<Type, bool>)(t => typeof(AppIF).IsAssignableFrom(t));
            var isKnown2 = (Func<Type, bool>)(t => t == typeof(IIOContext<AppIF>));
            var isTestOk =
                prms.Any(p => isKnown1(p.ParameterType) || isKnown2(p.ParameterType));
            // テストに落ちたらさようなら
            if (isTestOk == false)
            {
                return;
            }

            // シーケンスと見なしたメソッドをappのシーケンスに追加する
            app.Logger.Debug($"register module method to {typeof(AppIF).Name} sequence : {method.ReturnType.Name} {method.DeclaringType.Name}.{method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))})");
            
            // シーケンスを生成
            var sequence = MakeSequence<AppIF>(method, getModule, injection);
            
            // シーケンスを登録
            app.AddSequence(sequence);
        }

        // シーケンスメソッドを生成
        static Action<IIOContext<AppIF>> MakeSequence<AppIF>(
            MethodInfo method
            , Func<object> getModule
            , IModuleInjection moduleInjection)
        {
            return context =>
            {
                // contextをDIに登録した子オブジェクトを生成
                var contextinjection = new ContextModuleInjection<AppIF>(moduleInjection, context);
                // DIを解決して
                var injectioned = MakeAction(method, getModule, contextinjection);
                // 実行
                injectioned();
            };
        }

        // 引数なしのDIメソッドを生成
        private static Action MakeAction(MethodInfo method, Func<object> getModule, IModuleInjection moduleInjection)
        {
            var parameterTypes =
                method.GetParameters().Select(p => p.ParameterType).ToArray();

            return () =>
            {
                // メソッドの引数を解決して
                var parameters = GetParameters(moduleInjection, parameterTypes);
                // 解決できなかったら実行しない
                if (parameters == null || parameters.Any(x => x == null))
                {
                    return;
                }
                // メソッドの持ち主を取得して
                var instance = getModule();
                // 実行
                method.Invoke(instance, parameters);
            };
        }

        static object[] GetParameters(this IModuleInjection @this, Type[] parameterTypes)
        {
            var query =
                from t in parameterTypes
                let o = @this.Resolve(t)
                select o;
            return query.ToArray();
        }
    }

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
