using FluentHub.IO;
using FluentHub.Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.SandboxClient
{
    class Program
    {
        static void Main(string[] args)
        {
            // ログ出力用
            var appContainer = FluentHub.Sandbox.Program.MakeApps( false);

            Task.Run((Action)appContainer.Run);

            FluentHub.Sandbox.Program.Controller(appContainer);

        }
    }
}
