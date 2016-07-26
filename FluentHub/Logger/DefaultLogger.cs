using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FluentHub.Logger
{
    public class DefaultLogger : ILogger
    {
        public void Debug(string message)
        {
            Log("D", message);
        }

        public void Exception(Exception ex)
        {
            Log("E", ex.Message);
        }

        public void Info(string message)
        {
            Log("I", message);
        }

        public void Warn(string message)
        {
            Log("W", message);
        }

        private void Log(string type, string message)
        {
            Console.WriteLine($"[{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffff")}][{type}][{Thread.CurrentThread.ManagedThreadId.ToString("X4")}]:{message}");
        }
    }
}
