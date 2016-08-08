using FluentHub.IO;
using FluentHub.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub.Module
{
    public class ModuleRegisterHelper<AppIF, Module> : IModuleRegisterHelper
    {
        private IContextApplication<AppIF> app;
        private Func<Module> getModule;
        private IModuleInjection injection;
        private ILogger logger;

        public ModuleRegisterHelper(
            IContextApplication<AppIF> app
            , Func<Module> getModule
            , IModuleInjection injection
            , ILogger logger)
        {
            this.app = app;
            this.getModule = getModule;
            this.injection = injection;
            this.logger = logger;
        }

        public void Setup()
        {
            var moduleSequences =
                // シーケンスモジュールのpublicメソッドを取り出す
                from method in typeof(Module).GetMethods()
                where method.IsPublic
                where method.DeclaringType == typeof(Module)
                // かつそのメソッドの引数に電文型の仮引数、またはIIOContext<電文型>の仮引数があればそのメソッドはシーケンスとみなす 
                // todo 他のアプリの電文型が混合されてる場合、両方に反応しちゃうね。
                let prms = method.GetParameters().ToArray()
                let isKnown1 = (Func<Type, bool>)(t => typeof(AppIF).IsAssignableFrom(t))
                let isKnown2 = (Func<Type, bool>)(t => t == typeof(IIOContext<AppIF>))
                let isKnown =
                    prms.Any(p => isKnown1(p.ParameterType) || isKnown2(p.ParameterType))
                where isKnown
                select method;

            // シーケンスと見なしたメソッドをappのシーケンスに追加する
            foreach (var method in moduleSequences)
            {
                var parameterTypes =
                    method.GetParameters().Select(p => p.ParameterType).ToArray();

                this.logger.Debug($"register module method to {typeof(AppIF).Name} sequence : {method.ReturnType.Name} {method.DeclaringType.Name}.{method.Name}({string.Join(", ", parameterTypes.Select(pt=>pt.Name).ToArray())})");
                
                this.app.AddSequence(context =>
                {
                    // メソッドの引数を解決して
                    var parameters = GetParameters(context, parameterTypes);
                    // 解決できなかったら実行しない
                    if (parameters == null)
                    {
                        return;
                    }
                    // メソッドの持ち主を取得して
                    var instance = getModule();
                    // 実行
                    method.Invoke(instance, parameters);
                });
            }
        }

        private object[] GetParameters(IIOContext<AppIF> context, Type[] parameterTypes)
        {
            var parameters = new object[parameterTypes.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var pType = parameterTypes[i];
                var value = null as object;
                if (typeof(IIOContext<AppIF>) == pType)
                {
                    // senderを求めてたらそのまま入れてあげる
                    value = context;
                }
                else if (typeof(AppIF).IsAssignableFrom(pType))
                {
                    // AppIFの実装型だったらメッセージを読み込んでから入れてあげる。
                    var msg = context.Read(m => m.GetType() == pType);
                    if (msg == null)
                    {
                        // 受信できてなかったら呼ばない
                        return null;
                    }
                    value = msg;
                }
                else
                {
                    // その他の型だったらここでは解決できないので外から解決してもらう
                    value = this.injection.Resolve(pType);
                }
                parameters[i] = value;
            }
            // todo null があったら呼び出さないようにする？
            return parameters;
        }
    }

}
