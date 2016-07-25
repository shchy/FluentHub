﻿using FluentHub.IO;
using FluentHub.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public interface IApplicationContainer : IDisposable
    {
        ILogger Logger { get; }

        void Add<T>(IContextApplication<T> app);
        IContextApplication<T> GetApp<T>();
        IContextPool<T> MakeContextPool<T>();
        void Run();
    }
}