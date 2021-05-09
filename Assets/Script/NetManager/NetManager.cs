using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public static class NetManager
{
    //基础变量区
    static bool isConnecting = false;
    static bool isClosing = false;
    static bool isUsePing = true;

    //整型变量区
    public static int pingInterVal = 30; //心跳时间间隔
    static float lastPingTime = 0;  //上一次发送ping命令的时间
    static float lastPongTime = 0;  //上一次收到pong命令的时间



    //功能变量区
    public static Socket socket;

    static ByteArray readBuffer;

    static Queue<ByteArray> writeQueen = new Queue<ByteArray>();

    public delegate void EventListener(String err);

    public static Dictionary<NetEvent, EventListener> eventListeners = new Dictionary<NetEvent, EventListener>();

    public delegate void MsgListener(MsgBase msgBase);

    static Dictionary<string, MsgListener> msgListeners = new Dictionary<string, MsgListener>();

    static List<MsgBase> msgList = new List<MsgBase>();

    static int msgCount = 0;

    readonly static int MAX_MESSAGE_FIRE = 10;

    //更新函数
    public static void Update()
    {
        MsgUpdate();
    }

    //消息更新函数
    public static void MsgUpdate()
    {
        if (msgCount == 0)
        {
            return;
        }

        for (int i = 0; i < MAX_MESSAGE_FIRE; i++)
        {
            MsgBase msgBase = null;

            lock (msgList)
            {
                if (msgList.Count > 0)
                {
                    msgBase = msgList[0];

                    msgList.RemoveAt(0);

                    msgCount--;
                }
            }

            if (msgBase != null)
            {
                FireMsg(msgBase.proName, msgBase);
            }
            else
            {
                break;
            }
        }
    }


    //添加事件监听
    public static void AddEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] += listener;
        }
        else
        {
            eventListeners[netEvent] = listener;
        }
    }

    //删除事件监听
    public static void DeleteEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] -= listener;

            if (eventListeners[netEvent] == null)
            {
                eventListeners.Remove(netEvent);
            }
        }
    }

    //对于事件的(分发)处理
    public static void FireEvent(NetEvent netEvent, String err)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent](err);
        }
    }

    //连接服务器
    public static void Connect(string ip, int port)
    {
        //重判连接
        if (socket != null && socket.Connected)
        {
            Debug.Log("Connect Fail, Already Connect Script NetManager Connect 重判连接处 ");
        }
        if (isConnecting)
        {
            Debug.Log(" Connect Fail, isConnecting");
        }

        InitState();

        socket.NoDelay = true;

        isConnecting = true;

        socket.BeginConnect(IPAddress.Parse(ip), port, ConnectCallBack, socket);
    }

    //连接函数的回调函数
    public static void ConnectCallBack(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;

            socket.EndConnect(ar);

            Debug.Log("Socket Connect Success ");

            FireEvent(NetEvent.ConnectSucc, "OnConnectSucc");

            isConnecting = true;

            socket.BeginReceive(readBuffer.bytes, readBuffer.writeIndex, readBuffer.remain, 0, ReceiveCallBack, socket);
        }
        catch (Exception ex)
        {
            Debug.Log("Socket Connect Fail" + ex.ToString());

            FireEvent(NetEvent.ConnectFial, "OnConnectFail");

            isConnecting = false;
        }
    }

    //连接函数的初始化
    private static void InitState()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        readBuffer = new ByteArray();

        lastPingTime = Time.time;

        lastPongTime = Time.time;

        msgCount = 0;

        isConnecting = false;

        if (!msgListeners.ContainsKey("MsgPong"))
        {
            AddMsgListeners("MsgPong", OnMsgPong);
        }
    }

    public static void Close()
    {
        if (socket == null || !socket.Connected)
        {
            return;
        }
        if (!isConnecting)
        {
            return;
        }

        if (writeQueen.Count > 0)
        {
            isConnecting = true;
        }
        else
        {
            Debug.Log("调用Close 函数");

            socket.Close();

            FireEvent(NetEvent.Close, "OnClose");
        }
    }

    //发送函数
    public static void Send(MsgBase msgBase)
    {
        if (socket == null || !socket.Connected)
        {
            return;
        }
        if (!isConnecting)
        {
            return;
        }
        if (isClosing)
        {
            return;
        }

        //数据编码
        byte[] nameBytes = MsgBase.EncodeName(msgBase);

        byte[] bodyBytes = MsgBase.Encode(msgBase);

        int len = nameBytes.Length + bodyBytes.Length;

        byte[] sendBytes = new byte[2 + len];

        sendBytes[0] = (byte)(len % 256);

        sendBytes[1] = (byte)(len / 256);

        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);

        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);

        ByteArray ba = new ByteArray(sendBytes);

        int count = 0;

        lock (writeQueen)
        {
            writeQueen.Enqueue(ba);

            count = writeQueen.Count;
        }
        if (count != 0)
        {
            socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallBack, socket);
        }
    }

    //发送函数的回调函数
    public static void SendCallBack(IAsyncResult ar)
    {
        Socket socket = (Socket)ar.AsyncState;

        if (socket == null || !socket.Connected)
        {
            return;
        }
        int count = socket.EndSend(ar);

        Debug.Log("成功发送，总共为： " + count);

        ByteArray ba;

        lock (writeQueen)
        {
            ba = writeQueen.First();
        }
        ba.readIndex += count;

        if (ba.length == 0)
        {
            lock (writeQueen)
            {
                writeQueen.Dequeue();

                ba = writeQueen.First();
            }
        }

        if (ba != null)
        {
            socket.BeginSend(ba.bytes, ba.readIndex, ba.length, 0, SendCallBack, socket);
        }
        else if (isClosing)
        {
            Debug.Log("进行关闭连接");

            socket.Close();
        }
    }

    //数据的接收
    public static void ReceiveCallBack(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;

            int count = socket.EndReceive(ar);

            if (count == 0)
            {
                Debug.Log("调用关闭函数");

                Close();

                return;
            }

            readBuffer.writeIndex += count;

            //处理二进制消息
            OnReceiveData();

            if (readBuffer.remain < 8)
            {
                readBuffer.MoveBytes();

                readBuffer.ReSize(readBuffer.length + 2);
            }

            socket.BeginReceive(readBuffer.bytes, readBuffer.writeIndex, readBuffer.remain, 0, ReceiveCallBack, socket);
        }
        catch (Exception ex)
        {
            Debug.Log("Socket Receive Fail " + ex.ToString());
        }
    }

    static void OnReceiveData()
    {
        if (readBuffer.length <= 2)
        {
            return;
        }

        //获取消息的长度
        Int16 bodyLength = readBuffer.ReadInt16();

        if (readBuffer.length < bodyLength)
        {
            return;
        }

        try
        {
            int nameCount = 0;

            string proName = MsgBase.DecodeName(readBuffer.bytes, readBuffer.readIndex, out nameCount);

            if (proName == "")
            {
                Debug.Log("OnReceiveData MsgBase.Decode Fail ");

                return;
            }

            readBuffer.readIndex += nameCount;

            int bodyCount = bodyLength - nameCount;

            MsgBase msgBase = MsgBase.Decode(proName, readBuffer.bytes, readBuffer.readIndex, bodyCount);

            readBuffer.readIndex += bodyCount;

            readBuffer.CheckAndMoveBytes();

            lock (msgList)
            {
                msgList.Add(msgBase);

                msgCount++;
            }

            if (readBuffer.length > 2)
            {
                OnReceiveData();
            }
            else if (readBuffer.length == 0)
            {
                readBuffer.MoveBytes();
            }

        }
        catch(Exception ex)
        {
            Debug.Log("出错普抓：" + ex.ToString());
        }
    }

    //增加消息监听
    public static void AddMsgListeners(string msgName, MsgListener msgBase)
    {
        if(msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] += msgBase;
        }
        else
        {
            msgListeners[msgName] = msgBase;
        }
    }

    //删除消息监听
    public static void RemoveMsgListeners(string msgName, MsgListener listener)
    {
        if(msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] -= listener;

            if(msgListeners[msgName] == null)
            {
                msgListeners.Remove(msgName);
            }
        }
    }

    //分发消息
    public static void FireMsg(string msgName, MsgBase msgBase)
    {
        if(msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName](msgBase);
        }

    }

    //MsgPong协议函数
    public static void OnMsgPong(MsgBase msgBase)
    {
        lastPongTime = Time.time;
    }
}

//事件的类型
public enum NetEvent
{
    ConnectSucc = 1,

    ConnectFial = 2,

    Close = 3
}

