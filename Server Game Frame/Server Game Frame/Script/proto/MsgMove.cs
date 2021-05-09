using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Game_Frame.Script.proto
{
    public class MsgMove:MsgBase
    {
        public MsgMove()
        {
            proName = "MsgMove";
        }
        public int x;

        public int y;

        public int z;
    }
}
