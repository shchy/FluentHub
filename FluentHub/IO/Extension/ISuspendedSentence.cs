using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.IO.Extension
{
    public interface ISuspendedSentence : IDisposable
    {
        void Run();
        void Sentence(Action method);
        void Expiration();
    }
}
