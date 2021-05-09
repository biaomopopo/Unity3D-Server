using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Game_Frame.Script.proto
{
    public class MsgLogin:MsgBase
    {
        public string id;

        public string pw;

        //0 代表成功 1 代表失败
        public int result = 0;

        public MsgLogin()
        {
            proName = "MsgLogin";
        }
    }
}
