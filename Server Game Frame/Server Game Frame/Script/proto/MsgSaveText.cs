using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Game_Frame.Script.proto
{
    public class MsgSaveText:MsgBase
    {
        public MsgSaveText()
        {
            proName = "MsgSaveText";
        }

        public string text = "";

        //0 表示成功 1 代表失败
        public int result = 0;
    }
}
