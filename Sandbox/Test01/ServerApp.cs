using FluentHub.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Test01
{
    using ACTXT = IIOContext<IPingPongAppMessage>;
    using BCTXT = IIOContext<IThirdAppMessage>;
    using ACTXTS = IEnumerable<IIOContext<IPingPongAppMessage>>;
    using BCTXTS = IEnumerable<IIOContext<IThirdAppMessage>>;
    using FluentHub.IO.Extension;

    public class ServerApp
    {
        public void ReceivedPing(ACTXT sender, DebugSession session, ISessionContext<IPingPongAppMessage, DebugSession> sender2, Ping message)
        {
            sender2.Write(new Pong());
            session.Test = "unko";

        }

        public void TunnelReceive(ACTXT sender, DebugSession session, Tunnel recvMessage, BCTXTS thirdAppContexts, IEnumerable<ISessionContext<IThirdAppMessage, ISession>> others)
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
