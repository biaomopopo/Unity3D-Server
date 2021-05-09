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
    //����������
    static bool isConnecting = false;
    static bool isClosing = false;
    static bool isUsePing = true;

    //���ͱ�����
    public static int pingInterVal = 30; //����ʱ����
    static float lastPingTime = 0;  //��һ�η���ping�����ʱ��
    static float lastPongTime = 0;  //��һ���յ�pong�����ʱ��



    //���ܱ�����
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

    //���º���
    public static void Update()
    {
        MsgUpdate();
    }

    //��Ϣ���º���
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


    //����¼�����
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

    //ɾ���¼�����
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

    //�����¼���(�ַ�)����
    public static void FireEvent(NetEvent netEvent, String err)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent](err);
        }
    }

    //���ӷ�����
    public static void Connect(string ip, int port)
    {
        //��������
        if (socket != null && socket.Connected)
        {
            Debug.Log("Connect Fail, Already Connect Script NetManager Connect �������Ӵ� ");
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

    //���Ӻ����Ļص�����
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

    //���Ӻ����ĳ�ʼ��
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
            Debug.Log("����Close ����");

            socket.Close();

            FireEvent(NetEvent.Close, "OnClose");
        }
    }

    //���ͺ���
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

        //���ݱ���
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

    //���ͺ����Ļص�����
    public static void SendCallBack(IAsyncResult ar)
    {
        Socket socket = (Socket)ar.AsyncState;

        if (socket == null || !socket.Connected)
        {
            return;
        }
        int count = socket.EndSend(ar);

        Debug.Log("�ɹ����ͣ��ܹ�Ϊ�� " + count);

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
            Debug.Log("���йر�����");

            socket.Close();
        }
    }

    //���ݵĽ���
    public static void ReceiveCallBack(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;

            int count = socket.EndReceive(ar);

            if (count == 0)
            {
                Debug.Log("���ùرպ���");

                Close();

                return;
            }

            readBuffer.writeIndex += count;

            //�����������Ϣ
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

        //��ȡ��Ϣ�ĳ���
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
            Debug.Log("������ץ��" + ex.ToString());
        }
    }

    //������Ϣ����
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

    //ɾ����Ϣ����
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

    //�ַ���Ϣ
    public static void FireMsg(string msgName, MsgBase msgBase)
    {
        if(msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName](msgBase);
        }

    }

    //MsgPongЭ�麯��
    public static void OnMsgPong(MsgBase msgBase)
    {
        lastPongTime = Time.time;
    }
}

//�¼�������
public enum NetEvent
{
    ConnectSucc = 1,

    ConnectFial = 2,

    Close = 3
}

