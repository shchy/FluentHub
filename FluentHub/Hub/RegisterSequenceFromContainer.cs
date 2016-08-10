using FluentHub.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public static class RegisterSequenceFromContainer
    {

        public static IApplicationContainer RegisterSequence<T1>(
            this IApplicationContainer @this
            , Action<T1> lambda)
        {
            return ModuleExtension.RegisterSequence(@this, lambda.Method, () => lambda.Target);
        }

        public static IApplicationContainer RegisterSequence<T1, T2>(
            this IApplicationContainer @this
            , Action<T1, T2> lambda)
        {
            return ModuleExtension.RegisterSequence(@this, lambda.Method, () => lambda.Target);
        }


        public static IApplicationContainer RegisterSequence<T1, T2, T3>(
            this IApplicationContainer @this
            , Action<T1, T2, T3> lambda)
        {
            return ModuleExtension.RegisterSequence(@this, lambda.Method, () => lambda.Target);
        }
        public static IApplicationContainer RegisterSequence<T1, T2, T3, T4>(
            this IApplicationContainer @this
            , Action<T1, T2, T3, T4> lambda)
        {
            return ModuleExtension.RegisterSequence(@this, lambda.Method, () => lambda.Target);
        }


        public static IApplicationContainer RegisterSequence<T1, T2, T3, T4, T5>(
            this IApplicationContainer @this
            , Action<T1, T2, T3, T4, T5> lambda)
        {
            return ModuleExtension.RegisterSequence(@this, lambda.Method, () => lambda.Target);
        }


        public static IApplicationContainer RegisterSequence<T1, T2, T3, T4, T5, T6>(
            this IApplicationContainer @this
            , Action<T1, T2, T3, T4, T5, T6> lambda)
        {
            return ModuleExtension.RegisterSequence(@this, lambda.Method, () => lambda.Target);
        }

        public static IApplicationContainer RegisterSequence<T1, T2, T3, T4, T5, T6, T7>(
            this IApplicationContainer @this
            , Action<T1, T2, T3, T4, T5, T6, T7> lambda)
        {
            return ModuleExtension.RegisterSequence(@this, lambda.Method, () => lambda.Target);
        }

        public static IApplicationContainer RegisterSequence<T1, T2, T3, T4, T5, T6, T7, T8>(
            this IApplicationContainer @this
            , Action<T1, T2, T3, T4, T5, T6, T7, T8> lambda)
        {
            return ModuleExtension.RegisterSequence(@this, lambda.Method, () => lambda.Target);
        }

        public static IApplicationContainer RegisterInitializeSequence<T1>(
            this IApplicationContainer @this
            , Action<T1> lambda)
        {
            return ModuleExtension.RegisterInitializeSequence(@this, lambda.Method, () => lambda.Target);
        }

        public static IApplicationContainer RegisterInitializeSequence<T1, T2>(
            this IApplicationContainer @this
            , Action<T1, T2> lambda)
        {
            return ModuleExtension.RegisterInitializeSequence(@this, lambda.Method, () => lambda.Target);
        }

        public static IApplicationContainer RegisterInitializeSequence<T1, T2, T3>(
            this IApplicationContainer @this
            , Action<T1, T2, T3> lambda)
        {
            return ModuleExtension.RegisterInitializeSequence(@this, lambda.Method, () => lambda.Target);
        }

        public static IApplicationContainer RegisterInitializeSequence<T1, T2, T3, T4>(
            this IApplicationContainer @this
            , Action<T1, T2, T3, T4> lambda)
        {
            return ModuleExtension.RegisterInitializeSequence(@this, lambda.Method, () => lambda.Target);
        }

        public static IApplicationContainer RegisterInitializeSequence<T1, T2, T3, T4, T5>(
            this IApplicationContainer @this
            , Action<T1, T2, T3, T4, T5> lambda)
        {
            return ModuleExtension.RegisterInitializeSequence(@this, lambda.Method, () => lambda.Target);
        }

        public static IApplicationContainer RegisterInitializeSequence<T1, T2, T3, T4, T5, T6>(
            this IApplicationContainer @this
            , Action<T1, T2, T3, T4, T5, T6> lambda)
        {
            return ModuleExtension.RegisterInitializeSequence(@this, lambda.Method, () => lambda.Target);
        }

        public static IApplicationContainer RegisterInitializeSequence<T1, T2, T3, T4, T5, T6, T7>(
            this IApplicationContainer @this
            , Action<T1, T2, T3, T4, T5, T6, T7> lambda)
        {
            return ModuleExtension.RegisterInitializeSequence(@this, lambda.Method, () => lambda.Target);
        }

        public static IApplicationContainer RegisterInitializeSequence<T1, T2, T3, T4, T5, T6, T7, T8>(
            this IApplicationContainer @this
            , Action<T1, T2, T3, T4, T5, T6, T7, T8> lambda)
        {
            return ModuleExtension.RegisterInitializeSequence(@this, lambda.Method, () => lambda.Target);
        }





        // 
        public static Return InstantSequence<T1, Return>(
            this IApplicationContainer @this
            , Func<T1, Return> lambda)
        {
            var injected = ModuleExtension.MakeFunc<Return>(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            return @this.Logger.TrySafe(() => injected())
                .Item2;
        }

        public static Return InstantSequence<T1, T2, Return>(
            this IApplicationContainer @this
            , Func<T1, T2, Return> lambda)
        {
            var injected = ModuleExtension.MakeFunc<Return>(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            return @this.Logger.TrySafe(() => injected())
                .Item2;
        }

        public static Return InstantSequence<T1, T2, T3, Return>(
            this IApplicationContainer @this
            , Func<T1, T2, T3, Return> lambda)
        {
            var injected = ModuleExtension.MakeFunc<Return>(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            return @this.Logger.TrySafe(() => injected())
                .Item2;
        }

        public static Return InstantSequence<T1, T2, T3, T4, Return>(
            this IApplicationContainer @this
            , Func<T1, T2, T3, T4, Return> lambda)
        {
            var injected = ModuleExtension.MakeFunc<Return>(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            return @this.Logger.TrySafe(() => injected())
                .Item2;
        }

        public static Return InstantSequence<T1, T2, T3, T4, T5, Return>(
            this IApplicationContainer @this
            , Func<T1, T2, T3, T4, T5, Return> lambda)
        {
            var injected = ModuleExtension.MakeFunc<Return>(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            return @this.Logger.TrySafe(() => injected())
                .Item2;
        }

        public static Return InstantSequence<T1, T2, T3, T4, T5, T6, Return>(
            this IApplicationContainer @this
            , Func<T1, T2, T3, T4, T5, T6, Return> lambda)
        {
            var injected = ModuleExtension.MakeFunc<Return>(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            return @this.Logger.TrySafe(() => injected())
                .Item2;
        }

        public static Return InstantSequence<T1, T2, T3, T4, T5, T6, T7, Return>(
            this IApplicationContainer @this
            , Func<T1, T2, T3, T4, T5, T6, T7, Return> lambda)
        {
            var injected = ModuleExtension.MakeFunc<Return>(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            return @this.Logger.TrySafe(() => injected())
                .Item2;
        }

        public static Return InstantSequence<T1, T2, T3, T4, T5, T6, T7, T8, Return>(
            this IApplicationContainer @this
            , Func<T1, T2, T3, T4, T5, T6, T7, T8, Return> lambda)
        {
            var injected = ModuleExtension.MakeFunc<Return>(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            return @this.Logger.TrySafe(() => injected())
                .Item2;
        }

        public static void InstantSequence<T1>(
            this IApplicationContainer @this
            , Action<T1> lambda)
        {
            var injected = ModuleExtension.MakeAction(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            @this.Logger.TrySafe(() => injected());
        }

        public static void InstantSequence<T1, T2>(
            this IApplicationContainer @this
            , Action<T1, T2> lambda)
        {
            var injected = ModuleExtension.MakeAction(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            @this.Logger.TrySafe(() => injected());
        }

        public static void InstantSequence<T1, T2, T3>(
            this IApplicationContainer @this
            , Action<T1, T2, T3> lambda)
        {
            var injected = ModuleExtension.MakeAction(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            @this.Logger.TrySafe(() => injected());
        }
        public static void InstantSequence<T1, T2, T3, T4>(
            this IApplicationContainer @this
            , Action<T1, T2, T3, T4> lambda)
        {
            var injected = ModuleExtension.MakeAction(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            @this.Logger.TrySafe(() => injected());
        }
        public static void InstantSequence<T1, T2, T3, T4, T5>(
            this IApplicationContainer @this
            , Action<T1, T2, T3, T4, T5> lambda)
        {
            var injected = ModuleExtension.MakeAction(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            @this.Logger.TrySafe(() => injected());
        }
        public static void InstantSequence<T1, T2, T3, T4, T5, T6>(
            this IApplicationContainer @this
            , Action<T1, T2, T3, T4, T5, T6> lambda)
        {
            var injected = ModuleExtension.MakeAction(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            @this.Logger.TrySafe(() => injected());
        }
        public static void InstantSequence<T1, T2, T3, T4, T5, T6, T7>(
            this IApplicationContainer @this
            , Action<T1, T2, T3, T4, T5, T6, T7> lambda)
        {
            var injected = ModuleExtension.MakeAction(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            @this.Logger.TrySafe(() => injected());
        }
        public static void InstantSequence<T1, T2, T3, T4, T5, T6, T7, T8>(
            this IApplicationContainer @this
            , Action<T1, T2, T3, T4, T5, T6, T7, T8> lambda)
        {
            var injected = ModuleExtension.MakeAction(lambda.Method, () => lambda.Target, @this.ModuleInjection);
            @this.Logger.TrySafe(() => injected());
        }
    }
}
