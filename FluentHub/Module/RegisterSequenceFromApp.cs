using FluentHub.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public static class RegisterSequenceFromApp
    {

        public static IAppBuilder<AppIF> RegisterSequence<AppIF,T1>(
            this IAppBuilder<AppIF> @this
            , Action<T1> lambda)
        {
            var result = ModuleExtension.RegisterSequenceApp(@this, lambda.Method, () => lambda.Target);
            if (result == false)
            {
                throw new ArgumentException($"maybe lambda method type error");
            }
            return @this;
        }

        public static IAppBuilder<AppIF> RegisterSequence<AppIF, T1, T2>(
            this IAppBuilder<AppIF> @this
            , Action<T1, T2> lambda)
        {
            var result = ModuleExtension.RegisterSequenceApp(@this, lambda.Method, () => lambda.Target);
            if (result == false)
            {
                throw new ArgumentException($"maybe lambda method type error");
            }
            return @this;
        }

        public static IAppBuilder<AppIF> RegisterSequence<AppIF, T1, T2, T3>(
            this IAppBuilder<AppIF> @this
            , Action<T1, T2, T3> lambda)
        {
            var result = ModuleExtension.RegisterSequenceApp(@this, lambda.Method, () => lambda.Target);
            if (result == false)
            {
                throw new ArgumentException($"maybe lambda method type error");
            }
            return @this;
        }
        public static IAppBuilder<AppIF> RegisterSequence<AppIF, T1, T2, T3, T4>(
            this IAppBuilder<AppIF> @this
            , Action<T1, T2, T3, T4> lambda)
        {
            var result = ModuleExtension.RegisterSequenceApp(@this, lambda.Method, () => lambda.Target);
            if (result == false)
            {
                throw new ArgumentException($"maybe lambda method type error");
            }
            return @this;
        }
        public static IAppBuilder<AppIF> RegisterSequence<AppIF, T1, T2, T3, T4, T5>(
            this IAppBuilder<AppIF> @this
            , Action<T1, T2, T3, T4, T5> lambda)
        {
            var result = ModuleExtension.RegisterSequenceApp(@this, lambda.Method, () => lambda.Target);
            if (result == false)
            {
                throw new ArgumentException($"maybe lambda method type error");
            }
            return @this;
        }
        public static IAppBuilder<AppIF> RegisterSequence<AppIF, T1, T2, T3, T4, T5, T6>(
            this IAppBuilder<AppIF> @this
            , Action<T1, T2, T3, T4, T5, T6> lambda)
        {
            var result = ModuleExtension.RegisterSequenceApp(@this, lambda.Method, () => lambda.Target);
            if (result == false)
            {
                throw new ArgumentException($"maybe lambda method type error");
            }
            return @this;
        }
        public static IAppBuilder<AppIF> RegisterSequence<AppIF, T1, T2, T3, T4, T5, T6, T7>(
            this IAppBuilder<AppIF> @this
            , Action<T1, T2, T3, T4, T5, T6, T7> lambda)
        {
            var result = ModuleExtension.RegisterSequenceApp(@this, lambda.Method, () => lambda.Target);
            if (result == false)
            {
                throw new ArgumentException($"maybe lambda method type error");
            }
            return @this;
        }
        public static IAppBuilder<AppIF> RegisterSequence<AppIF, T1, T2, T3, T4, T5, T6, T7, T8>(
            this IAppBuilder<AppIF> @this
            , Action<T1, T2, T3, T4, T5, T6, T7, T8> lambda)
        {
            var result = ModuleExtension.RegisterSequenceApp(@this, lambda.Method, () => lambda.Target);
            if (result == false)
            {
                throw new ArgumentException($"maybe lambda method type error");
            }
            return @this;
        }

        public static IAppBuilder<AppIF> RegisterInitializeSequence<AppIF, T1>(
            this IAppBuilder<AppIF> @this
            , Action<T1> lambda)
        {
            var result = ModuleExtension.RegisterInitializeSequenceApp(@this, lambda.Method, () => lambda.Target);
            if (result == false)
            {
                throw new ArgumentException($"maybe lambda method type error");
            }
            return @this;
        }


        public static IAppBuilder<AppIF> RegisterInitializeSequence<AppIF, T1, T2>(
            this IAppBuilder<AppIF> @this
            , Action<T1, T2> lambda)
        {
            var result = ModuleExtension.RegisterInitializeSequenceApp(@this, lambda.Method, () => lambda.Target);
            if (result == false)
            {
                throw new ArgumentException($"maybe lambda method type error");
            }
            return @this;
        }

        public static IAppBuilder<AppIF> RegisterInitializeSequence<AppIF, T1, T2, T3>(
            this IAppBuilder<AppIF> @this
            , Action<T1, T2, T3> lambda)
        {
            var result = ModuleExtension.RegisterInitializeSequenceApp(@this, lambda.Method, () => lambda.Target);
            if (result == false)
            {
                throw new ArgumentException($"maybe lambda method type error");
            }
            return @this;
        }

        public static IAppBuilder<AppIF> RegisterInitializeSequence<AppIF, T1, T2, T3, T4>(
            this IAppBuilder<AppIF> @this
            , Action<T1, T2, T3, T4> lambda)
        {
            var result = ModuleExtension.RegisterInitializeSequenceApp(@this, lambda.Method, () => lambda.Target);
            if (result == false)
            {
                throw new ArgumentException($"maybe lambda method type error");
            }
            return @this;
        }

        public static IAppBuilder<AppIF> RegisterInitializeSequence<AppIF, T1, T2, T3, T4, T5>(
            this IAppBuilder<AppIF> @this
            , Action<T1, T2, T3, T4, T5> lambda)
        {
            var result = ModuleExtension.RegisterInitializeSequenceApp(@this, lambda.Method, () => lambda.Target);
            if (result == false)
            {
                throw new ArgumentException($"maybe lambda method type error");
            }
            return @this;
        }

        public static IAppBuilder<AppIF> RegisterInitializeSequence<AppIF, T1, T2, T3, T4, T5, T6>(
            this IAppBuilder<AppIF> @this
            , Action<T1, T2, T3, T4, T5, T6> lambda)
        {
            var result = ModuleExtension.RegisterInitializeSequenceApp(@this, lambda.Method, () => lambda.Target);
            if (result == false)
            {
                throw new ArgumentException($"maybe lambda method type error");
            }
            return @this;
        }

        public static IAppBuilder<AppIF> RegisterInitializeSequence<AppIF, T1, T2, T3, T4, T5, T6, T7>(
            this IAppBuilder<AppIF> @this
            , Action<T1, T2, T3, T4, T5, T6, T7> lambda)
        {
            var result = ModuleExtension.RegisterInitializeSequenceApp(@this, lambda.Method, () => lambda.Target);
            if (result == false)
            {
                throw new ArgumentException($"maybe lambda method type error");
            }
            return @this;
        }

        public static IAppBuilder<AppIF> RegisterInitializeSequence<AppIF, T1, T2, T3, T4, T5, T6, T7, T8>(
            this IAppBuilder<AppIF> @this
            , Action<T1, T2, T3, T4, T5, T6, T7, T8> lambda)
        {
            var result = ModuleExtension.RegisterInitializeSequenceApp(@this, lambda.Method, () => lambda.Target);
            if (result == false)
            {
                throw new ArgumentException($"maybe lambda method type error");
            }
            return @this;
        }


        // 
        public static Return InstantSequence<AppIF, T1, Return>(
            this IContextApplication<AppIF> @this
            , Func<T1, Return> lambda)
        {
            var injected = ModuleExtension.MakeFunc<Return>(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            return @this.Logger.TrySafe(() => injected())
                .Item2;
        }

        public static Return InstantSequence<AppIF,T1, T2, Return>(
            this IContextApplication<AppIF> @this
            , Func<T1, T2, Return> lambda)
        {
            var injected = ModuleExtension.MakeFunc<Return>(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            return @this.Logger.TrySafe(() => injected())
                .Item2;
        }

        public static Return InstantSequence<AppIF,T1, T2, T3, Return>(
            this IContextApplication<AppIF> @this
            , Func<T1, T2, T3, Return> lambda)
        {
            var injected = ModuleExtension.MakeFunc<Return>(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            return @this.Logger.TrySafe(() => injected())
                .Item2;
        }

        public static Return InstantSequence<AppIF,T1, T2, T3, T4, Return>(
            this IContextApplication<AppIF> @this
            , Func<T1, T2, T3, T4, Return> lambda)
        {
            var injected = ModuleExtension.MakeFunc<Return>(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            return @this.Logger.TrySafe(() => injected())
                .Item2;
        }

        public static Return InstantSequence<AppIF,T1, T2, T3, T4, T5, Return>(
            this IContextApplication<AppIF> @this
            , Func<T1, T2, T3, T4, T5, Return> lambda)
        {
            var injected = ModuleExtension.MakeFunc<Return>(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            return @this.Logger.TrySafe(() => injected())
                .Item2;
        }

        public static Return InstantSequence<AppIF,T1, T2, T3, T4, T5, T6, Return>(
            this IContextApplication<AppIF> @this
            , Func<T1, T2, T3, T4, T5, T6, Return> lambda)
        {
            var injected = ModuleExtension.MakeFunc<Return>(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            return @this.Logger.TrySafe(() => injected())
                .Item2;
        }

        public static Return InstantSequence<AppIF,T1, T2, T3, T4, T5, T6, T7, Return>(
            this IContextApplication<AppIF> @this
            , Func<T1, T2, T3, T4, T5, T6, T7, Return> lambda)
        {
            var injected = ModuleExtension.MakeFunc<Return>(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            return @this.Logger.TrySafe(() => injected())
                .Item2;
        }

        public static Return InstantSequence<AppIF,T1, T2, T3, T4, T5, T6, T7, T8, Return>(
            this IContextApplication<AppIF> @this
            , Func<T1, T2, T3, T4, T5, T6, T7, T8, Return> lambda)
        {
            var injected = ModuleExtension.MakeFunc<Return>(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            return @this.Logger.TrySafe(() => injected())
                .Item2;
        }

        public static void InstantSequence<AppIF,T1>(
            this IContextApplication<AppIF> @this
            , Action<T1> lambda)
        {
            var injected = ModuleExtension.MakeAction(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            @this.Logger.TrySafe(() => injected());
        }

        public static void InstantSequence<AppIF,T1, T2>(
            this IContextApplication<AppIF> @this
            , Action<T1, T2> lambda)
        {
            var injected = ModuleExtension.MakeAction(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            @this.Logger.TrySafe(() => injected());
        }

        public static void InstantSequence<AppIF,T1, T2, T3>(
            this IContextApplication<AppIF> @this
            , Action<T1, T2, T3> lambda)
        {
            var injected = ModuleExtension.MakeAction(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            @this.Logger.TrySafe(() => injected());
        }
        public static void InstantSequence<AppIF,T1, T2, T3, T4>(
            this IContextApplication<AppIF> @this
            , Action<T1, T2, T3, T4> lambda)
        {
            var injected = ModuleExtension.MakeAction(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            @this.Logger.TrySafe(() => injected());
        }
        public static void InstantSequence<AppIF,T1, T2, T3, T4, T5>(
            this IContextApplication<AppIF> @this
            , Action<T1, T2, T3, T4, T5> lambda)
        {
            var injected = ModuleExtension.MakeAction(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            @this.Logger.TrySafe(() => injected());
        }
        public static void InstantSequence<AppIF,T1, T2, T3, T4, T5, T6>(
            this IContextApplication<AppIF> @this
            , Action<T1, T2, T3, T4, T5, T6> lambda)
        {
            var injected = ModuleExtension.MakeAction(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            @this.Logger.TrySafe(() => injected());
        }
        public static void InstantSequence<AppIF,T1, T2, T3, T4, T5, T6, T7>(
            this IContextApplication<AppIF> @this
            , Action<T1, T2, T3, T4, T5, T6, T7> lambda)
        {
            var injected = ModuleExtension.MakeAction(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            @this.Logger.TrySafe(() => injected());
        }
        public static void InstantSequence<AppIF,T1, T2, T3, T4, T5, T6, T7, T8>(
            this IContextApplication<AppIF> @this
            , Action<T1, T2, T3, T4, T5, T6, T7, T8> lambda)
        {
            var injected = ModuleExtension.MakeAction(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            @this.Logger.TrySafe(() => injected());
        }
    }

}
