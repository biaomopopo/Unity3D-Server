using Server_Game_Frame.Script.logic;
using Server_Game_Frame.Script.proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Server_Game_Frame.Script.Manager
{
    public class NetManager
    {
        //判断变量区
        

        //基础变量区
        public static Socket listenfd;

        public static long pingInterval = 30;   //记录ping的时间


        //功能变量区
        public static Dictionary<Socket, ClientState> clients = new Dictionary<Socket, ClientState>();

        public static List<Socket> checkRead = new List<Socket>(); //用于Select的检查列表

        public static string msgBase { get; private set; }

        public static void StartLoop(int port)
        {
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ipAdrr = IPAddress.Parse("127.0.0.1");

            IPEndPoint ipEp = new IPEndPoint(ipAdrr, port);

            listenfd.Bind(ipEp);

            listenfd.Listen(0);

            Console.WriteLine("[ 服务器启动成功 ] ");

            while(true)
            {
                ResetCheckRead(); // 重置checkRead

                Socket.Select(checkRead, null, null, 1000);

                for (int i = 0; i < checkRead.Count; i++)
                {
                    Socket s = checkRead[i];

                    if(s == listenfd)
                    {
                        ReadListenfd(s);
                    }
                    else
                    {
                        ReadClientfd(s);
                    }
                }

                Timer();
            }
        }

        //重置Select中的监听链表
        public static void ResetCheckRead()
        {
            checkRead.Clear();

            checkRead.Add(listenfd);

            foreach (var item in clients.Values)
            {
                checkRead.Add(item.socket);
            }
        }

        public static void ReadListenfd(Socket listenfd)
        {
            try
            {
                Socket clientfd = listenfd.Accept();

                Console.WriteLine("Accept " + clientfd.RemoteEndPoint.ToString());

                ClientState state = new ClientState();

                state.lastPingTime = GetTimerStamp();

                state.socket = clientfd;

                clients.Add(clientfd, state);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Accept Fail " + ex.ToString());
            }
        }

        public static void ReadClientfd(Socket clientfd)
        {
            ClientState state = clients[clientfd];

            ByteArray readBuffer = state.readBuffer;

            int count = 0;

            if(readBuffer.remain <= 0)
            {
                OnReceiveDate(state);

                readBuffer.MoveBytes();
            }

            if(readBuffer.remain <= 0)
            {
                Console.WriteLine("Receive Fail , Mabe Msg Length > Buff Capacity Is Close");

                Close(state);

                return;
            }

            try
            {
                count = clientfd.Receive(readBuffer.bytes, readBuffer.writeIndex, readBuffer.remain, 0);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Receive SocketException " + ex.ToString());

                Close(state);

                return;
            }

            if(count <= 0)
            {
                Console.WriteLine("Socket Close " + clientfd.RemoteEndPoint.ToString());

                Close(state);

                return;
            }

            readBuffer.writeIndex += count;

            OnReceiveDate(state);

            readBuffer.CheckAndMoveBytes();
        }

        public static void Send(ClientState state, MsgBase msgBase)
        {
            if(!state.socket.Connected || state == null)
            {
                return;
            }

            byte[] nameBytes = MsgBase.EncodeName(msgBase);

            byte[] bodyBytes = MsgBase.Encode(msgBase);

            Int16 len = (Int16)(nameBytes.Length + bodyBytes.Length);

            byte[] readBuff = new byte[len + 2];

            readBuff[0] = (byte)(len % 256);

            readBuff[1] = (byte)(len / 256);

            Array.Copy(nameBytes, 0, readBuff, 2, nameBytes.Length);

            Array.Copy(bodyBytes, 0, readBuff, 2 + nameBytes.Length, bodyBytes.Length);

            ByteArray array = new ByteArray(readBuff);

            try
            {
                state.socket.BeginSend(array.bytes, array.readIndex, array.length, 0, SendCallBack, state);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Socket Close On BeginSend " + ex.ToString());
            }
        }

        public static void SendCallBack(IAsyncResult ar)
        {
            ClientState state = (ClientState)ar.AsyncState;

            int count = state.socket.EndSend(ar);

            ByteArray byteArray = new ByteArray();

            state.socket.BeginSend(byteArray.bytes, byteArray.readIndex, byteArray.length, 0, SendCallBack, state);
        }

        public static void OnReceiveDate(ClientState state)
        {
            Console.WriteLine(Encoding.UTF8.GetString(state.readBuffer.bytes));

            ByteArray readBuffer = state.readBuffer;

            //判断消息长度是否合法
            if (readBuffer.length <= 2)
            {
                return;
            }

            Int16 bodyLength = readBuffer.ReadInt16();

            if (readBuffer.length < bodyLength)
            {
                return;
            }

            //解析协议名
            int nameCount = 0;

            string proName = MsgBase.DecodeName(readBuffer.bytes, readBuffer.readIndex, out nameCount);

            if (proName == "")
            {
                Console.WriteLine("OnReceiveDate MsgBase.DecodeName Fail ");

                Close(state);

                return;
            }

            readBuffer.readIndex += nameCount;

            //解析协议体
            int bodyCount = bodyLength - nameCount;

            MsgBase msgBase = MsgBase.Decode(proName, readBuffer.bytes, readBuffer.readIndex, bodyCount);

            readBuffer.readIndex += bodyCount;

            readBuffer.MoveBytes();

            //分发消息
            MethodInfo mi = typeof(MsgHandler).GetMethod(proName);

            object[] o = { state, msgBase };

            if (mi != null)
            {
                mi.Invoke(null, o);
            }
            else
            {
                Console.WriteLine("OnReceiveData InVoke Fail " + proName);
            }

            if (readBuffer.length > 2)
            {
                //紧凑函数
                OnReceiveDate(state);
            }
        }

        public static void Close(ClientState state)
        {
            MethodInfo mi = typeof(EventsHandler).GetMethod("OnDisconnect");

            object[] o = {state };

            mi.Invoke(null, o);

            state.socket.Close();

            RemoveClientState(state);

            string s = Console.ReadLine();

            byte[] readBuffer = Encoding.UTF8.GetBytes(s);

            state.socket.Send(readBuffer);
        }

        public static void RemoveClientState(ClientState state)
        {
            if(clients.ContainsKey(state.socket))
            {
                Console.WriteLine(" Successfully Removed The Disconnected Connection ");

                clients.Remove(state.socket);
            }
        }

        public static void Timer()
        {
            MethodInfo mi = typeof(EventsHandler).GetMethod("OnTimer");

            object[] o = { };

            try
            {
                mi.Invoke(null, o);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Timer Fail " + ex.ToString());
            }
        }

        //获取时间戳
        public static long GetTimerStamp()
        {
            TimeSpan span = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);

            return Convert.ToInt64(span.TotalSeconds);
        }
    }
}
