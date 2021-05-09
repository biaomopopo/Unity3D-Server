using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Server_Game_Frame.Script.proto
{
    public class MsgBase
    {
        public string proName = "";

        static JavaScriptSerializer Js = new JavaScriptSerializer();

        //编码
        public static byte[] Encode(MsgBase msgBase)
        {
            Console.WriteLine(Js.Serialize("编码的信息是："+ msgBase));

            string s = Js.Serialize(msgBase);

            return Encoding.UTF8.GetBytes(s);
        }

        //解码
        public static MsgBase Decode(string proName, byte[] bytes, int offset, int count)
        {
            StringBuilder stringBuilder = new StringBuilder("Server_Game_Frame.Script.proto.");

            string s = Encoding.UTF8.GetString(bytes, offset, count);

            stringBuilder.Append(proName);

            MsgBase msgBase = (MsgBase)Js.Deserialize(s, Type.GetType(stringBuilder.ToString()));

            return msgBase;
        }

        //协议名编码
        public static byte[] EncodeName(MsgBase msgBase)
        {
            byte[] nameBytes = Encoding.UTF8.GetBytes(msgBase.proName);

            Int16 len = (Int16)nameBytes.Length;

            byte[] bytes = new byte[2 + len];

            bytes[0] = (byte)(len % 256);

            bytes[1] = (byte)(len / 256);

            Array.Copy(nameBytes, 0, bytes, 2, nameBytes.Length);

            return bytes;
        }

        //协议名解码
        public static string DecodeName(byte[] bytes, int offset, out int count)
        {
            count = 0;

            Int16 len = (Int16)(bytes[offset + 1] << 8 | bytes[offset]);

            if (offset + 2 + len > bytes.Length)
            {
                return "";
            }

            string s = Encoding.UTF8.GetString(bytes, offset + 2, len);

            count = len + 2;

            return s;
        }
    }
}
