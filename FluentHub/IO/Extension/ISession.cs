﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.IO.Extension
{
    public interface ISession
    {
        object NativeIO { get; }
    }

    public class DefaultSession : ISession
    {
        public object NativeIO { get; set; }
    }

    public interface ISessionContext<AppIF, SessionType> : IIOContext<AppIF>
        where SessionType : ISession
    {
        SessionType Session { get; }
    }

    public class SessionContext<AppIF, SessionType> : ISessionContext<AppIF, SessionType>
        where SessionType : ISession
    {
        private IIOContext<AppIF> context;

        public event EventHandler Received { add { context.Received += value; } remove { context.Received -= value; } }

        public SessionType Session { get; private set; }

        public bool IsAny => context.IsAny;
        public bool CanUse => context.CanUse;

        public SessionContext(IIOContext<AppIF> context, SessionType session)
        {
            this.context = context;
            this.Session = session;
        }
        
        public void Write(AppIF model)
        {
            context.Write(model);
        }

        public AppIF Read()
        {
            return context.Read();
        }

        public AppIF Read(Func<AppIF, bool> predicate)
        {
            return context.Read(predicate);
        }

        public void Dispose()
        {
            context.Dispose();
            Session = default(SessionType);
            context = null;
        }
    }
}
