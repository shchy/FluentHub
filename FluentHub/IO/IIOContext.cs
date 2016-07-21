using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.IO
{
    public interface IIOContext<T> : IDisposable
    {
        bool IsAny { get; }
        void Write(T model);
        void WriteAll(IEnumerable<T> models);
        T ReadOne();
        T Read(Func<T, bool> predicate);
        IEnumerable<T> ReadAll();
        event EventHandler Received;
    }

    public static class IOContextExtension
    {
        public static T Read<T>(
            this IIOContext<T> @this
            , Func<T,bool> predicate
            ,int timeoutMillisecond)
            where T : class
        {
            var target = DateTime.Now.AddMilliseconds(timeoutMillisecond);
            do
            {
                var model = @this.Read(predicate);
                if (model != null)
                {
                    return model;
                }
                
            } while (DateTime.Now < target);
            return null;
        }
    }
}
