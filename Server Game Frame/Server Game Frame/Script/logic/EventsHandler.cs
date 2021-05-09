using Server_Game_Frame.Script.DB;
using Server_Game_Frame.Script.Manager;
using Server_Game_Frame.Script.PlayerDic;
using Server_Game_Frame.Script.proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Game_Frame.Script.logic
{
    public class EventsHandler
    {
        public static  void OnTimer()
        {
            CheckPing();
        }

        public static void CheckPing()
        {
            long timeNow = NetManager.GetTimerStamp();

            foreach (var item in NetManager.clients.Values)
            {
                if (timeNow - item.lastPingTime > 4 * NetManager.pingInterval)
                {
                    Console.WriteLine(" Time Now ：{0}， lastPingtime：{1}，  Time Interval ：{2}，  Total Time ：{3}", timeNow, item.lastPingTime, timeNow - item.lastPingTime, 4 * NetManager.pingInterval);

                    Console.WriteLine("Ping Close " + item.socket.RemoteEndPoint);

                    NetManager.Close(item);

                    return;
                }
            }
        }

        //退出功能
        public static void OnDisconnect(ClientState cs)
        {
            Console.WriteLine("Close");

            //Player 玩家下线
            if(cs.player != null)
            {
                //保存数据
                DbManager.UpdatePlayerData(cs.player.id, cs.player.data);

                //移除玩家
                PlayerManager.RemovePlaer(cs.player.id);
            }
        }
    }
}
