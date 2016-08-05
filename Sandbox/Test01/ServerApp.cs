using FluentHub.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Test01
{
    public class ServerApp
    {
        public void ReceivedPing(IIOContext<IPingPongAppMessage> sender, Ping message)
        {
            sender.Write(new Pong());
        }

        public void TunnelReceive(IIOContext<IPingPongAppMessage> sender, Tunnel recvMessage, IEnumerable<IIOContext<IThirdAppMessage>> thirdAppContexts)
        {
            // 接続中のIThirdAppMessageプロトコルを持つ相手にPangを送信
            foreach (var thirdContext in thirdAppContexts)
            {
                var pang = new Pang
                {
                    InnerModel = new InnerModel { Value1 = 11, Value2 = 12 },
                    Array = new[] { new InnerModel { Value1 = 1, Value2 = 2 }, new InnerModel { Value1 = 3, Value2 = 4 } },
                    FixedArray = new[] { new InnerModel { Value1 = 5, Value2 = 6 }, new InnerModel { Value1 = 7, Value2 = 8 } },
                    InnerModel2 = new InnerModel { Value1 = 9, Value2 = 10 },
                    StructArray = new byte[] { 0x01, 0x02, 0x03 },
                };
                thirdContext.Write(pang);
            }
            // 送信元のsenderにPongを返送
            sender.Write(new Pong());
        }
    }

}
