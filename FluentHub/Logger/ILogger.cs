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
}
