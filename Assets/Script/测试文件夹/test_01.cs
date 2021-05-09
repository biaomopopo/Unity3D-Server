using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class test_01 : MonoBehaviour
{
    //点击测试序列化
    public void OnJsonClick()
    {
        MsgMove msgMove = new MsgMove();

        msgMove.x = 15;

        msgMove.y = 20;

        msgMove.z = 45;

        byte[] s = MsgBase.Encode(msgMove);

        Debug.Log(Encoding.UTF8.GetString(s));

        MsgMove msg = (MsgMove)MsgBase.Decode("MsgMove", s, 0, s.Length);

        Debug.Log(msg.x + "   " + msg.y);
    }

    //点击测试函数
}
