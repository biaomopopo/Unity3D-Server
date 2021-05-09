using Server_Game_Frame.Script.DB;
using Server_Game_Frame.Script.Manager;
using Server_Game_Frame.Script.PlayerDic;
using Server_Game_Frame.Script.proto;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Server_Game_Frame
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!DbManager.ConnectMySql("game1", "127.0.0.1", 3306, "root", ""))
            {
                Console.WriteLine("连接失败");

                return;
            }

            Task task = Task.Run(() =>
            {
                if (DbManager.Clear("account"))
                {
                    Console.WriteLine("删除表Account的信息成功");
                }

                if (DbManager.Clear("player"))
                {
                    Console.WriteLine("删除表Player中的信息成功");
                }
            });

            NetManager.StartLoop(8888);

            Console.ReadLine();
        }
    }
}
