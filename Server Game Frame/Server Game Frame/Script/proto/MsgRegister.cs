using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Game_Frame.Script.proto
{
    public class MsgRegister:MsgBase
    {
        public MsgRegister()
        {
            proName = "MsgRegister";
        }

        public string id = "";

        public string pw;

        //true 代表成功  false 代表失败
        public int result;
    }
}
