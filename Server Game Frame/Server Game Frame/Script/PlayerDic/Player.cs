using Server_Game_Frame.Script.Manager;
using Server_Game_Frame.Script.proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Game_Frame.Script.PlayerDic
{
    public class Player
    {
        public string id = "";  //玩家的id

        public ClientState state;   //玩家所指向的客户端

        //玩家所在的坐标
        public int x;

        public int y;

        public int z;

        public PlayerData data;//进行数据库保存

        public Player(ClientState state)
        {
            this.state = state;
        }

        //封装发送消息函数
        public void Send(MsgBase msgBase)
        {
            NetManager.Send(this.state, msgBase);
        }
    }
}
