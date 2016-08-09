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
    using FluentHub.Unity;
    using ChatContext = IIOContext<IChatProtocol>;

    class Server
    {
        public void Run()
        {
            using (var lifetime = new ContainerControlledLifetimeManager())
            {
                var unitycontainer = new UnityContainer();
                unitycontainer.RegisterType<ChatServer>(lifetime);

                var serverContainer = new ApplicationContainer(moduleInjection:new UnityModuleInjection(unitycontainer));
                var serverApp = serverContainer.MakeAppByTcpServer<IChatProtocol>(54321)
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

                serverContainer.RegisterModule<ChatServer>();

                Task.Run(() => serverContainer.Run());

                var server = unitycontainer.Resolve<ChatServer>();
                Console.ReadLine();
            }
        }
    }

    class ChatServer
    {
        private Dictionary<int, List<Log>> logs;
        private Dictionary<int, Room> rooms;
        private Dictionary<ChatContext, User> users;

        public ChatServer()
        {
            this.rooms = new Dictionary<int, Room>();
            this.users = new Dictionary<ChatContext, User>();
            this.logs = new Dictionary<int, List<Log>>();
        }

        public void Login(ChatContext sender, ReqLogin message, IEnumerable<ChatContext> others)
        {
            var user = new User
            {
                Id = message.UserId
            };

            // todo 認証はそのうち
            // セッションに記憶
            this.users[sender] = user;

            sender.Write(new ResLogin { IsOK = true });
        }

        public void CreateRoom(ChatContext sender, ReqCreateRoom message, IEnumerable<ChatContext> others)
        {
            var room = new Room
            {
                Id = this.rooms.Any() ? this.rooms.Max(x => x.Key) + 1 : 0,
                Name = message.Name,
                Created = DateTime.Now,
            };
            this.rooms[room.Id] = room;

            // todo 成否はおいとこ
            sender.Write(new ResCreateRoom { IsOK = true });

            // 他の奴らにも知らせる
            var notify = new NotifyUpdatedRooms { RoomID = room.Id };
            foreach (var other in others)
            {
                other.Write(notify);
            }
        }

        public void GetRoom(ChatContext sender, ReqRooms message)
        {
            var res = new ResRooms
            {
                Rooms = this.rooms.Values.ToArray()
            };
            sender.Write(res);
        }


        public void Write(ChatContext sender, ReqWriteText message, IEnumerable<ChatContext> others)
        {
            if (this.logs.ContainsKey(message.RoomId) == false)
            {
                this.logs[message.RoomId] = new List<Log>();
            }
            var roomLogs = this.logs[message.RoomId];
            var log = new Log
            {
                Id = roomLogs.Any() ? roomLogs.Max(x => x.Id) + 1 : 0,
                Text = message.Text,
                User = this.users[sender],
                Writed = DateTime.Now,
            };
            roomLogs.Add(log);


            // todo 成否はおいとこ
            sender.Write(new ResWriteText { IsOK = true });

            // 他の奴らにも知らせる
            var notify = new NotifyUpdatedRooms { RoomID = message.RoomId };
            foreach (var other in others)
            {
                other.Write(notify);
            }
        }

        public void GetText(ChatContext sender, ReqGetText message)
        {
            if (this.logs.ContainsKey(message.RoomId) == false)
            {
                this.logs[message.RoomId] = new List<Log>();
            }
            var roomLogs = this.logs[message.RoomId];


            var res = new ResGetText
            {
                RoomId = message.RoomId,
                Logs = roomLogs.ToArray(),
            };
            sender.Write(res);
        }
    }
}
