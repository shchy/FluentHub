using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Logger
{
    public interface ILogger
    {
        void Exception(Exception ex);
        void Debug(string message);
        void Info(string message);
        void Warn(string message);
    }

    public static class LoggerExtension
    {
        public static void Try(this ILogger @this, Action method)
        {
            try
            {
                method();
            }
            catch (Exception ex)
            {
                @this.Exception(ex);
                throw;
            }
        }

        public static T Try<T>(this ILogger @this, Func<T> method)
        {
            try
            {
                return method();
            }
            catch (Exception ex)
            {
                @this.Exception(ex);
                throw;
            }
        }

        public static bool TrySafe(this ILogger @this, Action method)
        {
            try
            {
                method();
            }
            catch (Exception ex)
            {
                @this.Exception(ex);
                return false;
            }
            return true;
        }

        public static Tuple<bool,T> TrySafe<T>(this ILogger @this, Func<T> method)
        {
            try
            {
                return
                    Tuple.Create(true, method());
                    
            }
            catch (Exception ex)
            {
                @this.Exception(ex);
                return
                    Tuple.Create(false, default(T));
            }
        }

    }
}
