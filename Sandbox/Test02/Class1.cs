using FluentHub.ModelConverter;
using FluentHub.ModelConverter.FluentBuilderItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Test02
{

    public enum ChatProtocolCommand : byte
    {
        ReqLogin,
        ResLogin,
        ReqCreateRoom,
        ResCreateRoom,
        ReqGetRoom,
        ResGetRoom,
        NotifyUpdatedRooms,
        ReqWrite,
        ResWrite,
        ReqGetText,
        ResGetText,
    }

    public interface IChatProtocol
    {
        ChatProtocolCommand ID { get; }
    }



    public class ReqCreateRoom : IChatProtocol
    {
        public ChatProtocolCommand ID { get; } = ChatProtocolCommand.ResCreateRoom;
        public string Name { get; set; }
    }

    public class ReqCreateRoomConverter : WrapperModelConverter<IChatProtocol, ReqCreateRoom>
    {
        protected override IModelConverter<ReqCreateRoom> MakeConverter()
        {
            return new ReqCreateRoom().ToModelBuilder()
                .Constant(ChatProtocolCommand.ReqCreateRoom)
                .GetProperty(m => m.Name.Length).AsTag("nameLength")
                .ArrayProperty("nameLength", m => m.Name)
                .ToConverter();
        }
    }

    public class ResCreateRoom : IChatProtocol
    {
        public ChatProtocolCommand ID { get; } = ChatProtocolCommand.ResCreateRoom;
        public bool IsOK { get; set; }
    }


    public class ResCreateRoomConverter : WrapperModelConverter<IChatProtocol, ResCreateRoom>
    {
        protected override IModelConverter<ResCreateRoom> MakeConverter()
        {
            return new ResCreateRoom().ToModelBuilder()
                .Constant(ChatProtocolCommand.ResCreateRoom)
                .Property(m => m.IsOK)
                .ToConverter();
        }
    }

    public class ReqRooms : IChatProtocol
    {
        public ChatProtocolCommand ID { get; } = ChatProtocolCommand.ReqGetRoom;

    }

    public class ReqRoomsConverter : WrapperModelConverter<IChatProtocol, ReqRooms>
    {
        protected override IModelConverter<ReqRooms> MakeConverter()
        {
            return new ReqRooms().ToModelBuilder()
                .Constant(ChatProtocolCommand.ReqGetRoom)
                .ToConverter();
        }
    }

    public class ResRooms : IChatProtocol
    {
        public ChatProtocolCommand ID { get; } = ChatProtocolCommand.ResGetRoom;

        public IEnumerable<Room> Rooms { get; set; }
    }

    public class ResRoomsConverter : WrapperModelConverter<IChatProtocol, ResRooms>
    {
        protected override IModelConverter<ResRooms> MakeConverter()
        {
            return new ResRooms().ToModelBuilder()
                .RegisterTypeConverter((DateTime v) => BitConverter.GetBytes(v.Ticks), bytes => new DateTime(BitConverter.ToInt64(bytes, 0)), () => sizeof(long))
                .Constant(ChatProtocolCommand.ResGetRoom)
                .GetProperty(m => m.Rooms.Count()).AsTag("roomCount")
                .ArrayProperty("roomCount", m => m.Rooms
                                , r => r.Property(m => m.Id)
                                    .GetProperty(m => m.Name.Length).AsTag("nameLength")
                                    .ArrayProperty("nameLength", m => m.Name)
                                    .Property(m => m.Created))
                .ToConverter();
        }
    }



    public class NotifyUpdatedRooms : IChatProtocol
    {
        public ChatProtocolCommand ID { get; } = ChatProtocolCommand.NotifyUpdatedRooms;
        public int RoomID { get; set; }
    }

    public class NotifyUpdatedRoomsConverter : WrapperModelConverter<IChatProtocol, NotifyUpdatedRooms>
    {
        protected override IModelConverter<NotifyUpdatedRooms> MakeConverter()
        {
            return new NotifyUpdatedRooms().ToModelBuilder()
                .Constant(ChatProtocolCommand.NotifyUpdatedRooms)
                .Property(m => m.RoomID)
                .ToConverter();
        }
    }

    //public class ReqUpdateRoom : IChatProtocol
    //{
    //    public byte ID { get; } = 0x04;
    //}

    //public class ResUpdateRoom : IChatProtocol
    //{
    //    public byte ID { get; } = 0x05;
    //}

    //public class ReqDeleteRoom : IChatProtocol
    //{
    //    public byte ID { get; } = 0x06;
    //}

    //public class ResDeleteRoom : IChatProtocol
    //{
    //    public byte ID { get; } = 0x07;
    //}

    public class ReqWriteText : IChatProtocol
    {
        public ChatProtocolCommand ID { get; } = ChatProtocolCommand.ReqWrite;

        public int RoomId { get; set; }
        public string Text { get; set; }
    }

    public class ReqWriteTextConverter : WrapperModelConverter<IChatProtocol, ReqWriteText>
    {
        protected override IModelConverter<ReqWriteText> MakeConverter()
        {
            return new ReqWriteText().ToModelBuilder()
                .Constant(ChatProtocolCommand.ReqWrite)
                .Property(m => m.RoomId)
                .GetProperty(m => m.Text.Length).AsTag("textLength")
                .ArrayProperty("textLength", m => m.Text)
                .ToConverter();
        }
    }

    public class ResWriteText : IChatProtocol
    {
        public ChatProtocolCommand ID { get; } = ChatProtocolCommand.ResWrite;

        public bool IsOK { get; set; }
    }

    public class ResWriteTextConverter : WrapperModelConverter<IChatProtocol, ResWriteText>
    {
        protected override IModelConverter<ResWriteText> MakeConverter()
        {
            return new ResWriteText().ToModelBuilder()
                .Constant(ChatProtocolCommand.ResWrite)
                .Property(m => m.IsOK)
                .ToConverter();
        }
    }

    public class ReqGetText : IChatProtocol
    {
        public ChatProtocolCommand ID { get; } = ChatProtocolCommand.ReqGetText;

        public int RoomId { get; set; }
    }

    public class ReqGetTextConverter : WrapperModelConverter<IChatProtocol, ReqGetText>
    {
        protected override IModelConverter<ReqGetText> MakeConverter()
        {
            return new ReqGetText().ToModelBuilder()
                .Constant(ChatProtocolCommand.ReqGetText)
                .Property(m => m.RoomId)
                .ToConverter();
        }
    }

    public class ResGetText : IChatProtocol
    {
        public ChatProtocolCommand ID { get; } = ChatProtocolCommand.ResGetText;

        public int RoomId { get; set; }
        public IEnumerable<Log> Logs { get; set; }
    }


    public class ResGetTextConverter : WrapperModelConverter<IChatProtocol, ResGetText>
    {
        protected override IModelConverter<ResGetText> MakeConverter()
        {
            return new ResGetText().ToModelBuilder()
                .RegisterTypeConverter((DateTime v) => BitConverter.GetBytes(v.Ticks), bytes => new DateTime(BitConverter.ToInt64(bytes, 0)), () => sizeof(long))
                .Constant(ChatProtocolCommand.ResGetText)
                .Property(m => m.RoomId)
                .GetProperty(m => m.Logs.Count()).AsTag("logCount")
                .ArrayProperty("logCount", m => m.Logs, l =>
                                                        l.Property(m => m.Id)
                                                        .Property(m => m.Writed)
                                                        .GetProperty(m => m.User.Id.Length).AsTag("userIdLength")
                                                        .ArrayProperty("userIdLength", m => m.User.Id)
                                                        .GetProperty(m => m.Text.Length).AsTag("textLength")
                                                        .ArrayProperty("textLength", m => m.Text))
                .ToConverter();
        }
    }

    public class ReqLogin : IChatProtocol
    {
        public ChatProtocolCommand ID { get; } = ChatProtocolCommand.ReqLogin;
        public string UserId { get; set; }
        public string Password { get; set; }
    }

    public class ReqLoginConverter : WrapperModelConverter<IChatProtocol, ReqLogin>
    {
        protected override IModelConverter<ReqLogin> MakeConverter()
        {
            return new ReqLogin().ToModelBuilder()
                .Constant(ChatProtocolCommand.ReqLogin)
                .GetProperty(m => m.UserId.Length).AsTag("useridLength")
                .GetProperty(m => m.Password.Length).AsTag("passwordLength")
                .ArrayProperty("useridLength", m => m.UserId)
                .ArrayProperty("passwordLength", m => m.Password)
                .ToConverter();
        }
    }

    public class ResLogin : IChatProtocol
    {
        public ChatProtocolCommand ID { get; } = ChatProtocolCommand.ResLogin;
        public bool IsOK { get; set; }
    }

    public class ResLoginConverter : WrapperModelConverter<IChatProtocol, ResLogin>
    {
        protected override IModelConverter<ResLogin> MakeConverter()
        {
            return new ResLogin().ToModelBuilder()
                .Constant(ChatProtocolCommand.ResLogin)
                .Property(m => m.IsOK)
                .ToConverter();
        }
    }

    public class Room
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
    }

    public class User
    {
        public string Id { get; set; }
    }

    public class Log
    {
        public int Id { get; set; }
        public DateTime Writed { get; set; }
        public User User { get; set; }
        public string Text { get; set; }
        public override string ToString()
        {
            return $"{User.Id}\t{Text}";
        }
    }
}
