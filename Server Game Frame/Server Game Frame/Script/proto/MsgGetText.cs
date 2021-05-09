using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Game_Frame.Script.proto
{
    public class MsgGetText:MsgBase
    {
        public MsgGetText()
        {
            proName = "MsgGetText";
        }

        public string text = "";
    }
}
