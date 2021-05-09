using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MsgBase
{
    public string proName = "";

    //编码
    public static byte[] Encode(MsgBase msgBase)
    {
        string s = JsonUtility.ToJson(msgBase);

        return Encoding.Default.GetBytes(s);
    }

    //解码
    public static MsgBase Decode(string proName, byte[] bytes, int offset, int count)
    {

        Debug.Log("proName: " + proName);

        try
        {
            string s = Encoding.UTF8.GetString(bytes, offset, count);

            MsgBase msgBase = (MsgBase)JsonUtility.FromJson(s, Type.GetType(proName));

            return msgBase;
        }
        catch(Exception ex)
        {
            Debug.Log("错处在解析协议体的时候; " + ex.ToString());

            return null;
        }
    }

    //协议名编码
    public static byte[] EncodeName(MsgBase msgBase)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(msgBase.proName);

        Int16 length = (Int16)bytes.Length;

        byte[] arrayByte = new byte[2 + length];

        arrayByte[0] = (byte)(length % 256);

        arrayByte[1] = (byte)(length / 256);

        Array.Copy(bytes, 0, arrayByte, 2, bytes.Length);

        return arrayByte;
    }

    //协议名解码
    public static string DecodeName(byte[] bytes, int offset,out int count)
    {
        count = 0;

        try
        {
            Int16 len = (Int16)(bytes[offset + 1] << 8 | bytes[offset]);

            Debug.Log("解析协议体的长度：" + (int)len);

            if (offset + 2 + len > bytes.Length)
            {
                return "";
            }

            string s = Encoding.UTF8.GetString(bytes, offset + 2, len);

            count = len + 2;

            return s;
        }
        catch(Exception ex)
        {
            Debug.Log("出错：" + ex.Message);

            return "";
        }
    }
}
