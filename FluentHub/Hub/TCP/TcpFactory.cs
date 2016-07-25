using FluentHub.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub.TCP
{
    public abstract class TcpFactory : IRunnableFactory<TcpClient>
    {
        protected bool isDisporsed;

        public Action<TcpClient> Maked { get; set; }

        public void Run()
        {
            while (this.isDisporsed == false)
            {
                var client = GetTcpClient();
                if (client == null)
                {
                    continue;
                }
                OnMaked(client);
            }
        }

        private void OnMaked(TcpClient client)
        {
            if (Maked == null)
            {
                client.Close();
            }
            Maked(client);
        }

        protected abstract TcpClient GetTcpClient();

        protected bool IsEnd(Task task)
        {
            return
                task.IsCanceled
                || task.IsCompleted
                || task.IsFaulted;
        }

        public virtual void Dispose()
        {
            this.isDisporsed = true;
        }
    }

}
