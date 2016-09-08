using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentHub.Hub
{
    public class DualNativeIOFactory<NativeIO> : INativeIOFactory<NativeIO>
    {
        private INativeIOFactory<NativeIO> current;
        private INativeIOFactory<NativeIO> primary;
        private INativeIOFactory<NativeIO> secondary;
        private int switchMillisecond;
        private DateTime? switchTimeoutTime;

        public DualNativeIOFactory(
            INativeIOFactory<NativeIO> primary
            , INativeIOFactory<NativeIO> secondary
            , int switchMillisecond)
        {
            this.primary = primary;
            this.secondary = secondary;
            this.switchMillisecond = switchMillisecond;
            this.current = primary;
            this.switchTimeoutTime = null;
        }

        public void Dispose()
        {
            this.primary.Dispose();
            this.secondary.Dispose();
            this.switchTimeoutTime = null;
        }

        public bool IsAlreadyEnough()
        {
            return this.current.IsAlreadyEnough();
        }

        public NativeIO Make()
        {
            // すでに必要な接続を確立できている場合
            if (this.current.IsAlreadyEnough())
            {
                return default(NativeIO);
            }

            var io = this.current.Make();
            // 生成できれば問題ないのでそのまま返す
            if (io != null)
            {
                this.switchTimeoutTime = null;
                return io;
            }

            // 接続に失敗した時にスイッチする時刻を計算
            if (this.switchTimeoutTime.HasValue == false)
            {
                this.switchTimeoutTime = DateTime.Now.AddMilliseconds(this.switchMillisecond);
            }

            // スイッチ時刻を経過してたら
            if (this.switchTimeoutTime < DateTime.Now)
            {
                SwitchConnection();
            }
            return io;
        }

        private void SwitchConnection()
        {
            this.current =
                this.current == this.primary
                ? this.secondary
                : this.primary;
        }
    }

}
