using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    class GameSession : Session
    {
        public override void OnConnect(EndPoint endPoint)
        {
            Console.Write("서버에 보낼 메시지를 입력해주세요: ");
            string readString = Console.ReadLine();

            readString = "클라이언트에서 보내는 메시지...";
            byte[] bytes = Encoding.UTF8.GetBytes(readString);
            Send(bytes);

        }

        public override void OnDispose()
        {
        }

        public override int OnRecv(ArraySegment<byte> recvbuf)
        {
            // byte[] recvBuf = new byte[1024];
            // Receive(recvBuf, 0, 1024, SocketFlags.None);

            Console.WriteLine("socket.Receive");
            string recvString = Encoding.UTF8.GetString(recvbuf.Array);

            Console.WriteLine($"[From Server]: {recvString}");

            return recvbuf.Count;
        }

        public override void OnSend(int bytesCount)
        {
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            // 클라이언트
            Console.WriteLine("클라이언트");
            string hostName = Dns.GetHostName();
            IPHostEntry iPHostEntry = Dns.GetHostEntry(hostName);
            IPEndPoint iPEndPoint = new IPEndPoint(iPHostEntry.AddressList[0], 7777);
            //    Socket socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            //   socket.Bind(iPEndPoint);

            while (true)
            {
                try
                {
                    // Socket socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);

                    // socket.Connect(iPEndPoint);
                    Console.WriteLine("Connect 생성");
                    Connector connector = new Connector();
                    connector.Connect(iPEndPoint, () => { return new GameSession(); });


                    //Console.Write("서버에 보낼 메시지를 입력해주세요: ");
                    //string readString = Console.ReadLine();
                    //byte[] bytes = Encoding.UTF8.GetBytes(readString);

                    //socket.Send(bytes, SocketFlags.None);


                    //byte[] recvBuf = new byte[1024];
                    //socket.Receive(recvBuf, 0, 1024, SocketFlags.None);
                    //Console.WriteLine("socket.Receive");
                    //string recvString = Encoding.UTF8.GetString(recvBuf);

                    //Console.WriteLine($"[From Server]: {recvString}");
                    Console.WriteLine("진행하려면 엔터를 누르세요.");
                    Console.ReadLine();
                    //Thread.Sleep(1000);
                    
                    //socket.Shutdown(SocketShutdown.Both);
                    //socket.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
