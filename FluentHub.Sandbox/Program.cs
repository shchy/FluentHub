using FluentHub.Hub;
using FluentHub.TCP;
using FluentHub.Serial;
using FluentHub.IO;
using FluentHub.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentHub.UDP;
using System.IO.Ports;
using FluentHub.ModelConverter;
using System.Linq.Expressions;

namespace Sandbox
{
    public class Program
    {
        static void Main(string[] args)
        {
            //new Test00.Test(true).Run(args);
            new Test01.TestServer().Run(args);
        }
    }
}
