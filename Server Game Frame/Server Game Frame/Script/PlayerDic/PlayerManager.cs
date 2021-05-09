﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Game_Frame.Script.PlayerDic
{
    public class PlayerManager
    {
        //玩家列表
        static Dictionary<string, Player> players = new Dictionary<string, Player>();

        public static bool IsOnline(string id)
        {
            return players.ContainsKey(id);
        }

        //获取玩家
        public static Player GetPlayer(string id)
        {
            if(players.ContainsKey(id))
            {
                return players[id];
            }
            return null;
        }

        //添加对象
        public static void AddPlayer(string id, Player player)
        {
            players.Add(id, player);
        }

        //删除玩家
        public static void RemovePlaer(string id)
        {
            players.Remove(id);
        }
    }
}
