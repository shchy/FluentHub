using FluentHub.Hub;
using FluentHub.IO;
using FluentHub.Logger;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sandbox.Test02
{
    using Servers = IEnumerable<IIOContext<IChatProtocol>>;

    public partial class ClientForm : Form
    {
        private IContextApplication<IChatProtocol> app;
        private ILogger logger;

        public ClientForm(IContextApplication<IChatProtocol> app, ILogger logger)
        {
            InitializeComponent();
            this.app = app;
            this.logger = logger;
            this.rooms.SelectedValueChanged += Rooms_SelectedValueChanged;
        }

        private void Rooms_SelectedValueChanged(object _, EventArgs e)
        {
            var selectedRoom = this.rooms.SelectedValue as Room;
            if (selectedRoom == null)
            {
                return;
            }

            var logs = GetLogs(selectedRoom.Id);
            var index = this.logs.SelectedIndex;
            this.logs.DataSource = logs;
            this.logs.SelectedIndex = index;

        }

        private IEnumerable<Log> GetLogs(int roomId)
        {
            return Seq(sender =>
            {
                sender.Write(new ReqGetText { RoomId = roomId });
                var res = sender.Read(m => m is ResGetText, 10 * 1000) as ResGetText;
                if (res == null)
                {
                    return Enumerable.Empty<Log>();
                }
                return res.Logs;
            });
        }

        void Seq(Action<IIOContext<IChatProtocol>> sequence)
        {
            app.InstantSequence((Servers xs) =>
            {
                if (xs.Any() == false)
                {
                    return;
                }
                var x = xs.First();
                sequence(x);
            });
        }

        Return Seq<Return>(Func<IIOContext<IChatProtocol>, Return> sequence)
        {
            return
                app.InstantSequence((Servers xs) =>
                {
                    if (xs.Any() == false)
                    {
                        return default(Return);
                    }
                    var x = xs.First();
                    return sequence(x);
                });
        }

        public void ReceiveNotifyUpdateRoom(NotifyUpdatedRooms message)
        {
            var rooms = Seq(sender =>
            {
                sender.Write(new ReqRooms());
                var res = sender.Read(m => m is ResRooms, 10 * 1000) as ResRooms;
                return res.Rooms;
            }).ToArray();

            this.Invoke((Action)(() =>
            {
                try
                {
                    this.rooms.SelectedValueChanged -= Rooms_SelectedValueChanged;
                    var selectedRoom = this.rooms.SelectedValue as Room;
                    this.rooms.DataSource = null;
                    this.rooms.DataSource = rooms;
                    this.rooms.DisplayMember = "Name";
                    //this.rooms.ValueMember = "Id";
                    if (selectedRoom != null)
                    {
                        var find = rooms.FirstOrDefault(m => m.Id == selectedRoom.Id);
                        if (find != null)
                        {
                            var findindex = Array.IndexOf(rooms, find);
                            this.rooms.SelectedIndex = findindex;
                        }
                    }
                    this.rooms.SelectedValueChanged += Rooms_SelectedValueChanged;
                }
                catch (Exception ex)
                {
                    logger.Exception(ex);
                }

                {
                    var selectedRoom = this.rooms.SelectedValue as Room;
                    if (selectedRoom == null)
                    {
                        return;
                    }
                    var logs = GetLogs(selectedRoom.Id);
                    var index = this.logs.SelectedIndex;
                    this.logs.DataSource = logs;
                    this.logs.SelectedIndex = index;
                }
            }));

        }

        private void makeRoom_Click(object _, EventArgs e)
        {
            Seq(sender =>
            {
                sender.Write(new ReqCreateRoom { Name = "testRoom" });
                var res = sender.ReadAs<IChatProtocol, ResCreateRoom>(10 * 1000);
                if (res == null)
                {
                    return;
                }
                this.logger.Debug($"{res.GetType().Name} is {res.IsOK}");
            });
        }

        private void send_Click(object _, EventArgs e)
        {
            Seq(sender =>
            {
                var selectedRoom = this.rooms.SelectedValue as Room;
                if (selectedRoom == null)
                {
                    return;
                }

                sender.Write(new ReqLogin { UserId = "testKun", Password = "aaa" });
                var resLogin = sender.Read(m => m is ResLogin, 1000 * 10) as ResLogin;
                if (resLogin == null)
                {
                    return;
                }


                sender.Write(new ReqWriteText { RoomId = selectedRoom.Id, Text = this.comment.Text });
                var res = sender.ReadAs<IChatProtocol, ResWriteText>(10 * 1000);
                if (res == null)
                {
                    return;
                }
                this.logger.Debug($"{res.GetType().Name} is {res.IsOK}");
            });
        }
    }
}
