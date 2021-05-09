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
        //获取文本的功能
        public static void MsgGetText(ClientState state, MsgBase msgBase)
        {
            MsgGetText msgGetText = (MsgGetText)msgBase;

            Player player = state.player;

            if (player == null)
            {
                return;
            }

            msgGetText.text = player.data.text;

            player.Send(msgGetText);
        }

        //保存文本的功能
        public static void MsgSaveText(ClientState state, MsgBase msgBase)
        {
            MsgSaveText msg = (MsgSaveText)msgBase;

            Player player = state.player;

            if(player == null)
            {
                return;
            }

            player.data.text = msg.text;

            //进行信息的更新
            Task<bool> task = Task<bool>.Run(() =>
            {
                return DbManager.UpdatePlayerData(state.player.id, player.data);
            });

            if(task.Result)
            {
                Console.WriteLine(" Saved Successfully ");

                player.Send(msg);
            }
            else
            {
                msg.result = 1;

                Console.WriteLine(" Saved Fail ");

                player.Send(msg);
            }
            
        }
    }

}
