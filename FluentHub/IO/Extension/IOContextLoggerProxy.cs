using FluentHub.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.IO.Extension
{
    public class IOContextLoggerProxy<T> : IIOContext<T>
    {
        private ILogger logger;
        private IIOContext<T> realContext;
        private string realName;

        public bool IsAny => realContext.IsAny;
        public bool CanUse => realContext.CanUse;

        public IOContextLoggerProxy(IIOContext<T> realContext, ILogger logger)
        {
            this.realContext = realContext;
            this.logger = logger;
            this.realContext.Received += RealContext_Received;
            this.realName = realContext.GetType().Name;
        }

        private void RealContext_Received(object sender, EventArgs e)
        {
            try
            {
                this.logger.Debug($"recv {this.realName} {DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffff")}");
                if (Received == null)
                {
                    return;
                }
                Received(sender, e);
            }
            catch (Exception ex)
            {
                this.logger.Exception(ex);
                throw;
            }
        }

        public event EventHandler Received;

        public void Dispose()
        {
            try
            {
                this.realContext.Dispose();
            }
            catch (Exception ex)
            {
                this.logger.Exception(ex);
                throw;
            }
        }

        public T Read(Func<T, bool> predicate)
        {
            try
            {
                return this.realContext.Read(predicate);
            }
            catch (Exception ex)
            {
                this.logger.Exception(ex);
                throw;
            }

        }

        public IEnumerable<T> ReadAll()
        {
            try
            {
                return this.realContext.ReadAll();
            }
            catch (Exception ex)
            {
                this.logger.Exception(ex);
                throw;
            }
        }

        public T Read()
        {
            try
            {
                return this.realContext.Read();
            }
            catch (Exception ex)
            {
                this.logger.Exception(ex);
                throw;
            }
        }

        public void Write(T model)
        {
            try
            {
                this.realContext.Write(model);
                this.logger.Debug($"send {this.realName} {DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffff")}");
            }
            catch (Exception ex)
            {
                this.logger.Exception(ex);
                throw;
            }
        }

        public void WriteAll(IEnumerable<T> models)
        {
            try
            {
                this.realContext.WriteAll(models);
                this.logger.Debug($"send {this.realName} {DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.ffff")}");
            }
            catch (Exception ex)
            {
                this.logger.Exception(ex);
                throw;
            }
        }
    }
}
