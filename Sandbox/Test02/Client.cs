using FluentHub.Hub;
using FluentHub.IO;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Test02
{
    using FluentHub.Logger;
    using System.Windows.Forms;
    

    public class Client
    {
        public void Run()
        {
            using (var lifetime = new ContainerControlledLifetimeManager())
            {
                var serverContainer = new ApplicationContainer();
                var serverApp = serverContainer.MakeAppByTcpClient<IChatProtocol>("localhost", 54321)
                    .RegisterConverter(new ReqCreateRoomConverter())
                    .RegisterConverter(new ResCreateRoomConverter())
                    .RegisterConverter(new ReqRoomsConverter())
                    .RegisterConverter(new ResRoomsConverter())
                    .RegisterConverter(new NotifyUpdatedRoomsConverter())
                    .RegisterConverter(new ReqWriteTextConverter())
                    .RegisterConverter(new ResWriteTextConverter())
                    .RegisterConverter(new ReqGetTextConverter())
                    .RegisterConverter(new ResGetTextConverter())
                    .RegisterConverter(new ReqLoginConverter())
                    .RegisterConverter(new ResLoginConverter());

                var container = serverContainer.MakeContainer();
                container.RegisterType<ClientForm>(lifetime);
                serverContainer.RegisterModule<ClientForm>(container);

                Task.Run(() => serverContainer.Run());

                var client = container.Resolve<ClientForm>();

                Application.EnableVisualStyles();
                Application.Run(client);
                //while (true)
                //{
                //    var line = Console.ReadLine().Split(' ');
                //    if (line.Count() < 2)
                //    {
                //        continue;
                //    }
                //    var cmd = line.First();
                //    var prm = line.Skip(1).First();
                //    switch (cmd)
                //    {
                //        case "makeroom":
                //            client.MakeRoom(prm);
                //            break;
                //        default:
                //            break;
                //    }

                //}
            }
        }
    }

    //class ChatClient : Form
    //{
    //    private IContextApplication<IChatProtocol> app;
    //    private ILogger logger;
    //    private ListBox rooms;
    //    private ListBox logs;
    //    private TextBox input;

    //    public ChatClient(IContextApplication<IChatProtocol> app, ILogger logger)
    //    {
    //        this.app = app;
    //        this.logger = logger;


    //        this.rooms = new ListBox();
    //        this.logs = new ListBox();
    //        this.input = new TextBox();
    //        this.Controls.Add(this.rooms);
    //        this.Controls.Add(this.logs);
    //        this.Controls.Add(this.input);
    //        rooms.Dock = DockStyle.Left;
    //        logs.Dock = DockStyle.Fill;
    //        input.Dock = DockStyle.Bottom;
    //    }

    //    void Seq(Action<IIOContext<IChatProtocol>> sequence)
    //    {
    //        app.InstantSequence((Servers xs)=>
    //        {
    //            if (xs.Any() == false)
    //            {
    //                return;
    //            }
    //            var x = xs.First();
    //            sequence(x);
    //        });
    //    }

    //    Return Seq<Return>(Func<IIOContext<IChatProtocol>, Return> sequence)
    //    {
    //        return 
    //            app.InstantSequence((Servers xs) =>
    //            {
    //                if (xs.Any() == false)
    //                {
    //                    return default(Return);
    //                }
    //                var x = xs.First();
    //                return sequence(x);
    //            });
    //    }

    //    public void MakeRoom(string roomName)
    //    {
    //        Seq(sender =>
    //        {
    //            sender.Write(new ReqCreateRoom { Name = roomName });
    //            var res = sender.ReadAs<IChatProtocol, ResCreateRoom>(10 * 1000);
    //            if (res == null)
    //            {
    //                return;
    //            }
    //            this.logger.Debug($"{res.GetType().Name} is {res.IsOK}");
    //        });
    //    }
    //}
}
