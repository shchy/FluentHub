﻿using FluentHub.IO;
using FluentHub.ModelConverter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public interface IModelContextFactory<AppIF>
    {
        void Run();
        void Stop();
        event Action<IIOContext<AppIF>, object> Maked;
    }
}
