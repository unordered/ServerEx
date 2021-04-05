using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;

namespace Server
{
    class GameSession : Session
    {
        public override void OnConnect(EndPoint endPoint)
        {
            Console.WriteLine($"연결 완료 by {endPoint.ToString()}");


            //Console.WriteLine("session.Init(clientSocket)");
            //byte[] recvBuf = new byte[1024];
            //clientSocket.Receive(recvBuf, 0, 1024, SocketFlags.None);

            //string recvString = Encoding.UTF8.GetString(recvBuf);
            //Console.WriteLine($"[From Client]: {recvString}");


            string sendString = "서버에 연결하신 것을 환영합니다.";
            byte[] bytes = Encoding.UTF8.GetBytes(sendString);
            Send(bytes);

            Console.WriteLine("session.Send(bytes);");
            // clientSocket.Send(bytes, SocketFlags.None);

            Thread.Sleep(1000);
        }

        public override void OnDispose()
        {

        }

        public override int OnRecv(ArraySegment<byte> recvbuf)
        {
            string recvString = Encoding.UTF8.GetString(recvbuf.Array);
            Console.WriteLine($"[From Client]: {recvString}");


            return recvbuf.Count;
        }

        public override void OnSend(int bytesCount)
        {
            Console.WriteLine($"OnSendCompleted, 전송된 bytes:{bytesCount} byte");


        }
    }

    class Program
    {


        static void Main(string[] args)
        {
            // 서버
            Console.WriteLine("서버");
            string hostName = Dns.GetHostName();
            IPHostEntry iPHostEntry = Dns.GetHostEntry(hostName);
            IPEndPoint iPEndPoint = new IPEndPoint(iPHostEntry.AddressList[0], 7777);
            // Socket socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);

            // socket.Bind(iPEndPoint);

            // socket.Listen(1000);

            Listener listener = new Listener();
            listener.Start(iPEndPoint, () => { return new GameSession(); });

            while (true)
            {
                //Socket clientSocket = socket.Accept();

                //byte[] recvBuf = new byte[1024];
                //clientSocket.Receive(recvBuf, 0, 1024, SocketFlags.None);

                //string recvString = Encoding.UTF8.GetString(recvBuf);
                //Console.WriteLine($"[From Client]: {recvString}");


                //string sendString = "서버에 연결하신 것을 환영합니다.";
                //byte[] bytes = Encoding.UTF8.GetBytes(sendString);

                //clientSocket.Send(bytes, SocketFlags.None);

                Thread.Sleep(1000);


            }
        }
    }
}
