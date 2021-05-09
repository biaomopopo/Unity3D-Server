using Server_Game_Frame.Script.DB;
using Server_Game_Frame.Script.PlayerDic;
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
        public static void MsgRegister(ClientState state, MsgBase msgBase)
        {
            MsgRegister msg = (MsgRegister)msgBase;

            if(DbManager.Register(msg.id,msg.pw))
            {
                DbManager.CreatePlayer(msg.id);

                msg.result = 0;
            }
            else
            {
                msg.result = 1;
            }

            NetManager.Send(state, msg);
        }

        //登录处理
        public static void MsgLogin(ClientState state, MsgBase msgBase)
        {
            MsgLogin msgLogin = (MsgLogin)msgBase;

            //密码效验
            if(!DbManager.CheckPassword(msgLogin.id,msgLogin.pw))
            {
                Console.WriteLine(" Incorrect Password Or Account Number ");

                msgLogin.result = 1;

                NetManager.Send(state, msgLogin);

                return;
            }

            //不容许再次登录
            if (state.player != null)
            {
                Console.WriteLine(" Repeated Login Is Not Allowed ");

                msgLogin.result = 1;

                NetManager.Send(state, msgLogin);

                return;
            }

            //如果已经登录，踢下线
            if(PlayerManager.IsOnline(msgLogin.id))
            {
                Player player = PlayerManager.GetPlayer(msgLogin.id);

                MsgKick msgKick = new MsgKick();

                msgKick.reason = 0;

                NetManager.Send(state, msgKick);
            }

            //获取玩家数据
            PlayerData playerData = DbManager.GetPlayerData(msgLogin.id);

            if(playerData == null)
            {
                msgLogin.result = 1;

                NetManager.Send(state, msgLogin);

                return;
            }

            //构建Player玩家
            Player player1 = new Player(state);

            player1.id = msgLogin.id;

            player1.data = playerData;

            PlayerManager.AddPlayer(msgLogin.id, player1);

            state.player = player1;

            //返回协议
            msgLogin.result = 0;

            player1.Send(msgLogin);
        }
    }
}
