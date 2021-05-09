using Server_Game_Frame.Script.DB;
using Server_Game_Frame.Script.PlayerDic;
using Server_Game_Frame.Script.proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Game_Frame
{
    public class test_01
    {
        public static void Test()
        {
            MsgMove msgMove = new MsgMove();

            msgMove.x = 45;

            byte[] bytes = MsgBase.Encode(msgMove);

            Console.WriteLine(Encoding.UTF8.GetString(bytes));

            MsgMove msg = (MsgMove)MsgBase.Decode("MsgMove", bytes, 0, bytes.Length);

            Console.WriteLine(msg.x);

            if (DbManager.CreatePlayer("zjt"))
            {
                Console.WriteLine("成功创建");
            }

            if (DbManager.Register("wyl", "123456"))
            {
                Console.WriteLine("成功注册");
            }

            PlayerData playerData = DbManager.GetPlayerData("zjt");

            if (playerData != null)
            {
                Console.WriteLine("玩家的金币是：{0}", playerData.coin);
            }

        }

    }
}
