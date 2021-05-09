using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Game_Frame.Script.proto
{
    public class MsgKick:MsgBase
    {
        public MsgKick()
        {
            proName = "MsgKick";
        }

        public int reason = 0;
    }
}
