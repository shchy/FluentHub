using FluentHub.IO;
using FluentHub.Logger;
using FluentHub.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub
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
        public static ContainerBootstrap RegisterModule<Module>(
            this ContainerBootstrap @this
            , Module module)
        {
            return @this.RegisterModule<Module>(()=>module);
        }

        public static ContainerBootstrap RegisterModule<Module>(
            this ContainerBootstrap @this
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
                RegisterSequence(@this, method, getModule);
            }
            return @this;
        }

        public static IAppBuilder<AppIF> RegisterModule<AppIF,Module>(
            this IAppBuilder<AppIF> @this
            , Module module)
        {
            return @this.RegisterModule<AppIF, Module>(() => module);
        }

        public static IAppBuilder<AppIF> RegisterModule<AppIF,Module>(
            this IAppBuilder<AppIF> @this
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
                RegisterSequenceApp(@this, method, getModule);
            }
            return @this;
        }

        public static ContainerBootstrap RegisterSequence(
            ContainerBootstrap @this
            , MethodInfo method
            , Func<object> getModule)
        {
            BridgeOfTypeRegisterSequence(@this, false, method, getModule);
            return @this;
        }

        public static ContainerBootstrap RegisterInitializeSequence(
            ContainerBootstrap @this
            , MethodInfo method
            , Func<object> getModule)
        {
            BridgeOfTypeRegisterSequence(@this, true, method, getModule);
            return @this;
        }

        // memo ModuleExtension.RegisterSequenceかModuleExtension.RegisterInitializeSequenceを呼ぶ。
        // なのでこの二つのメソッドの引数は一致させておいてね
        static void BridgeOfTypeRegisterSequence(
            ContainerBootstrap @this
            , bool isInitialize
            , MethodInfo method
            , Func<object> getModule)
        {
            // 呼び出すメソッド名
            // ↓の関数を型引数付きで呼びたいだけ
            var normalSequenceOrInitializeSequenceRegisterMethodName = isInitialize
                ? nameof(ModuleExtension.RegisterInitializeSequenceApp)
                : nameof(ModuleExtension.RegisterSequenceApp);
            
            foreach (var appBuilder in @this.Builders.ToArray())
            {
                var appType = appBuilder.GetType().GetGenericArguments()[0];
                var registerSequence = typeof(ModuleExtension).GetMethod(normalSequenceOrInitializeSequenceRegisterMethodName, BindingFlags.Public | BindingFlags.Static);
                var typedRegisterSequence = registerSequence.MakeGenericMethod(new[] { appType });
                typedRegisterSequence.Invoke(null, new object[] { appBuilder, method, getModule});
            }
        }

        public static bool RegisterSequenceApp<AppIF>(
            IAppBuilder<AppIF> @this
            , MethodInfo method
            , Func<object> getModule)
        {
            var methodStringForLog = $"{method.ReturnType.Name} {method.DeclaringType.Name}.{method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))}";
            var initializeStringForLog =  "";
            @this.Logger.Debug($"try register method to {initializeStringForLog} sequence : [{methodStringForLog}] -> [{typeof(AppIF).Name}] App)");

            // シーケンスになり得るメソッドだったらシーケンスっぽくする
            var sequence = MakeAppSequence(@this, method, getModule, @this.DependencyContainer);
            if (sequence == null)
            {
                @this.Logger.Debug($"failure register method to {initializeStringForLog} sequence : [{methodStringForLog}] -> [{typeof(AppIF).Name}] App )");
                return false;
            }

            @this.Logger.Debug($"done register method to {initializeStringForLog} sequence : [{methodStringForLog}] -> [{typeof(AppIF).Name}] App  )");

            // シーケンスを登録
            @this.Sequences.Add(sequence);
            return true;
        }

        public static bool RegisterInitializeSequenceApp<AppIF>(
            IAppBuilder<AppIF> @this
            , MethodInfo method
            , Func<object> getModule)
        {
            var methodStringForLog = $"{method.ReturnType.Name} {method.DeclaringType.Name}.{method.Name}({string.Join(", ", method.GetParameters().Select(p => p.ParameterType.Name))}";
            var initializeStringForLog = "initialize";
            @this.Logger.Debug($"try register method to {initializeStringForLog} sequence : [{methodStringForLog}] -> [{typeof(AppIF).Name}] App)");


            // シーケンスになり得るメソッドだったらシーケンスっぽくする
            var sequence = MakeAppSequence(@this, method, getModule, @this.DependencyContainer, true);
            if (sequence == null)
            {
                @this.Logger.Debug($"failure register method to {initializeStringForLog} sequence : [{methodStringForLog}] -> [{typeof(AppIF).Name}] App )");
                return false;
            }
            @this.Logger.Debug($"done register method to {initializeStringForLog} sequence : [{methodStringForLog}] -> [{typeof(AppIF).Name}] App  )");

            // シーケンスを登録
            @this.InitializeSequences.Add(sequence);
            return true;
        }


        private static Action<IIOContext<AppIF>> MakeAppSequence<AppIF>(
            IAppBuilder<AppIF> app
            , MethodInfo method
            , Func<object> getModule
            , IModuleDependencyContainer dependencyContainer
            , bool isRequireContext = false)
        {
            // 引数の型をチェックして次の引数がいずれかあればシーケンスとみなす。
            // AppIF型の何か
            // IIOContext<AppIF>型のコンテキスト
            var prms = method.GetParameters();
            var isKnown1 = (Func<Type, bool>)(t => typeof(AppIF).IsAssignableFrom(t));
            var isKnown2 = (Func<Type, bool>)(t => t == typeof(IIOContext<AppIF>));
            var test = (Func<ParameterInfo, bool>)(p => isKnown1(p.ParameterType) || isKnown2(p.ParameterType));
            var testIsRequire = (Func<ParameterInfo, bool>)(p => isKnown2(p.ParameterType));
            var isTestOk =
                prms.Any(isRequireContext ? testIsRequire : test);
            
            // テストに落ちたらさようなら
            if (isTestOk == false)
            {
                return null;
            }

            // シーケンスを生成
            return context =>
            {
                // contextをDIに登録した子オブジェクトを生成
                var contextinjection = new ContextModuleInjection<AppIF>(app.App, context);
                // DIを解決して
                var injectioned = MakeAction(method, getModule, contextinjection);
                // 実行
                injectioned();
            };
        }

        
        // 引数なしのDIメソッドを生成
        public static Action MakeAction(MethodInfo method, Func<object> getModule, IModuleDependencyContainer dependencyContainer)
        {
            var parameterTypes =
                method.GetParameters().Select(p => p.ParameterType).ToArray();

            return () =>
            {
                // メソッドの引数を解決して
                var parameters = dependencyContainer.ResolveTypes(parameterTypes);
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

        // 引数なしのDIメソッドを生成
        public static Func<Return> MakeFunc<Return>(MethodInfo method, Func<object> getModule, IModuleDependencyContainer dependencyContainer)
        {
            var parameterTypes =
                method.GetParameters().Select(p => p.ParameterType).ToArray();

            return () =>
            {
                // メソッドの引数を解決して
                var parameters = dependencyContainer.ResolveTypes(parameterTypes);
                // 解決できなかったら実行しない
                if (parameters == null || parameters.Any(x => x == null))
                {
                    return default(Return);
                }
                // メソッドの持ち主を取得して
                var instance = getModule();
                // 実行
                return (Return)method.Invoke(instance, parameters);
            };
        }

        static object[] ResolveTypes(this IModuleDependencyContainer @this, Type[] parameterTypes)
        {
            var query =
                from t in parameterTypes
                let o = @this.Resolve(t)
                select o;
            return query.ToArray();
        }
    }
    
}
