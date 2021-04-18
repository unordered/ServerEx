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
    class Program
    {


        static void Main(string[] args)
        {
            // 서버
            Console.WriteLine("서버");
            string hostName = Dns.GetHostName();
            IPHostEntry iPHostEntry = Dns.GetHostEntry(hostName);
            IPEndPoint iPEndPoint =null;

            foreach (var IP in iPHostEntry.AddressList)
            {
                if(IP.AddressFamily== System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    iPEndPoint = new IPEndPoint(IP, 7777);
                    break;
                }
            }

            //  IPEndPoint iPEndPoint = new IPEndPoint(iPHostEntry.AddressList[0], 7777);
            
            // Socket socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);

            // socket.Bind(iPEndPoint);

            // socket.Listen(1000);

            Listener listener = new Listener();
            listener.Start(iPEndPoint, () => { return new ClientSession(); });

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
