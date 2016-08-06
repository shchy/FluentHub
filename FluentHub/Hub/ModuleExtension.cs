using FluentHub.IO;
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

        // todo いいのか
        public static IApplicationContainer RegisterModule<Module>(
            this IApplicationContainer @this
            , Module module)
        {
            var app



            // コンテナに登録済のアプリケーションを取り出す
            var apps = @this.GetApps().ToArray();
            // コンテナが解決可能な型について、シーケンスメソッドにDIする為のType->objectメソッド
            Func<Type, object> injection = MakeInjection(@this);

            // シーケンスモジュールのpublicメソッドを取り出す
            // かつそのメソッドの引数に電文型の仮引数、またはIIOContext<電文型>の仮引数があればそのメソッドはシーケンスとみなす
            // todo 他のアプリの電文型が混合されてる場合、両方に反応しちゃうね。
            // シーケンスとみなしたメソッドを電文型のアプリケーションのシーケンスに追加するためのModuleSeauenceオブジェクトを生成する
            var moduleSequences =
                from method in typeof(Module).GetMethods()
                where method.IsPublic
                where method.DeclaringType == typeof(Module)
                let prms = method.GetParameters().ToArray()
                from app in apps
                let appType = app.GetType().GetGenericArguments()[0]
                let isKnown1 = (Func<Type, bool>)(t => appType.IsAssignableFrom(t))
                let isKnown2 = (Func<Type, bool>)(t => t == typeof(IIOContext<>).MakeGenericType(appType))
                let isKnown =
                    prms.Any(p => isKnown1(p.ParameterType) || isKnown2(p.ParameterType))
                where isKnown
                let moduleSequenceType = typeof(ModuleSeauence<>).MakeGenericType(appType)
                let moduleSequence =
                    Activator.CreateInstance(moduleSequenceType, new object[] { app, (Func<object>)(() => module), method, injection })
                select moduleSequence as IModuleSeauence;

            // モジュールのシーケンスらしきメソッドをアプリのシーケンスに登録
            foreach (var method in moduleSequences.ToArray())
            {
                method.Setup();
            }

            return @this;
        }


        private static Func<Type, object> MakeInjection(IApplicationContainer @this)
        {
            return ptype =>
            {
                // IEnumerable<IIOContext<T>> 型を求められたらコンテナにありそうなので取ってくる
                if (ptype.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    var nestedType = ptype.GetGenericArguments()[0];
                    if (nestedType.GetGenericTypeDefinition() != typeof(IIOContext<>))
                    {
                        return null as object;
                    }

                    var messageType = nestedType.GetGenericArguments()[0];
                    var appType = typeof(IContextApplication<>).MakeGenericType(messageType);
                    var app =
                        @this.GetApps()
                        .FirstOrDefault(a => appType.IsAssignableFrom( a.GetType() ));
                    if (app == null)
                    {
                        return
                            Array.CreateInstance(typeof(IIOContext<>).MakeGenericType(messageType), 0);
                    }

                    var tryGetPoolContexts = typeof(ModuleExtension).GetMethod("TryGetPoolContexts");
                    var tryGetPoolContextsTyped = tryGetPoolContexts.MakeGenericMethod(messageType);
                    return tryGetPoolContextsTyped.Invoke(null, new[] { app });
                }
                else
                {
                    // todo 他の型は知らないのでUnityとかを使ってDIするよ。
                    return null as object;
                }
            };
        }

        public static IEnumerable<IIOContext<T>> TryGetPoolContexts<T>(object maybeApp)
        {
            var typedApp = maybeApp as IContextApplication<T>;
            if (typedApp == null)
            {
                return Enumerable.Empty<IIOContext<T>>();
            }
            return typedApp.Pool.Get().ToArray();
        }


        public interface IModuleSeauence
        {
            void Setup();
        }


        public class ModuleSeauence<T> : IModuleSeauence
        {
            private IContextApplication<T> app;
            private Func<object> getInstance;
            private Func<Type, object> injection;
            private MethodInfo method;

            public ModuleSeauence(IContextApplication<T> app
                , Func<object> getInstance
                , MethodInfo method
                , Func<Type, object> injection)
            {
                this.getInstance = getInstance;
                this.method = method;
                this.app = app;
                this.injection = injection;
            }

            public void Setup()
            {
                var parameterTypes =
                           this.method.GetParameters().Select(p => p.ParameterType).ToArray();

                this.app.AddSequence(context =>
                {
                    var parameters = new object[parameterTypes.Length];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var pType = parameterTypes[i];
                        var value = null as object;
                        if (typeof(IIOContext<T>) == pType)
                        {
                            value = context;
                        }
                        // メッセージの実装型だったら
                        else if (typeof(T).IsAssignableFrom(pType))
                        {
                            var msg = context.Read(m => m.GetType() == pType);
                            if (msg == null)
                            {
                                // メッセージが必要で、受信できてなかったら呼ばない
                                return;
                            }
                            value = msg;
                        }
                        else
                        {
                            value = this.injection(pType);
                        }
                        parameters[i] = value;
                    }
                    var instance = getInstance();
                    method.Invoke(instance, parameters);
                });
            }
        }

    }

}
