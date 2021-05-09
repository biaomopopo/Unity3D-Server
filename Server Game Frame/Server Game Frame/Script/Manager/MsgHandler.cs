using Server_Game_Frame.Script.proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Game_Frame.Script.Manager
{
    public partial class MsgHandler
    {
        public static void MsgMove(ClientState state, MsgBase msgBase)
        {
            Console.WriteLine(" Successfully Called --MsgMove--  Processing Function ");

            Console.WriteLine(" Please Wait Patiently......... ");

            Console.WriteLine("-----------MsgMove----------");

            MsgMove msg = (MsgMove)msgBase;

            msg.x += 45;

            NetManager.Send(state, msg);
        }

        public static void MsgPing(ClientState state, MsgBase msgBase)
        {
            Console.WriteLine(" Successfully Called  --MsgPing--  Processing function ");

            Console.WriteLine(" Please Wait Patiently......... ");

            Console.WriteLine("-----------MsgPing----------");

            state.lastPingTime = NetManager.GetTimerStamp();

            MsgPong msgPong = new MsgPong();

            NetManager.Send(state, msgPong);
        }
    }
}
