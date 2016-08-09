using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.IO.Extension
{
    public interface ISuspendedDisposalSource : IDisposable
    {
        void Run();
        void Stop();
        ISuspendedDisposal MakeToken();
    }
}
