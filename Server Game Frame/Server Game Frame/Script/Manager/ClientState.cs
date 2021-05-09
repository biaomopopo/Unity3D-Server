using Server_Game_Frame.Script.PlayerDic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server_Game_Frame.Script.Manager
{
    public class ClientState
    {
        //基础变量区
        public long lastPingTime = 0;


        //功能变量区
        public Socket socket;

        public ByteArray readBuffer = new ByteArray();

        public Player player;

    }
}
