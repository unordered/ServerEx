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
    class Knight
    {
        public int hp;
        public int attack;
        public string name = "empty";
        public List<int> skills = new List<int>();
    }

    // 하나의 연결, 하나의 쓰레드가 점유하는 공간이라고 봐도 무방
    class GameSession : PacketSession
    {
        public override void OnConnect(EndPoint endPoint)
        {
            Console.WriteLine($"연결 완료 by {endPoint.ToString()}");


            //Console.WriteLine("session.Init(clientSocket)");
            //byte[] recvBuf = new byte[1024];
            //clientSocket.Receive(recvBuf, 0, 1024, SocketFlags.None);

            //string recvString = Encoding.UTF8.GetString(recvBuf);
            //Console.WriteLine($"[From Client]: {recvString}");


            // 문자열 보내기
            //string sendString = "서버에 연결하신 것을 환영합니다.";
            //byte[] bytes = Encoding.UTF8.GetBytes(sendString);


            // 패킷 보내기
            // send buffer 만들기
            // byte[] SendBuff = new byte[1024];


            // SendBuffer 활요하기
            ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);

            // Knight 정보 보내기
            Knight knight = new Knight() { hp = 100, attack = 10 };
            // 바이트로 바꾸기
            byte[] knightHpBuffer = BitConverter.GetBytes(knight.hp);
            byte[] knightAtacckbuffer = BitConverter.GetBytes(knight.attack);
            // bytes 버퍼에 해당 내용 담아서 보내기
            Array.Copy(knightHpBuffer, 0, openSegment.Array, openSegment.Offset, knightHpBuffer.Length);
            Array.Copy(knightAtacckbuffer, 0, openSegment.Array, openSegment.Offset + knightHpBuffer.Length, knightHpBuffer.Length);
            // SendBuffer 사용 다 했으면 닫아줍니다.
            ArraySegment<byte> sendBuff= SendBufferHelper.Close(knightHpBuffer.Length + knightHpBuffer.Length);
            Send(sendBuff);

            Console.WriteLine("session.Send(bytes);");
            // clientSocket.Send(bytes, SocketFlags.None);



            Thread.Sleep(1000);
        }

        public override void OnDispose()
        {

        }

        public override int OnPacketRecv(ArraySegment<byte> packetSegment)
        {
            // 패킷 크기 
            short packetSize = BitConverter.ToInt16(packetSegment.Array, 0);
            // 패킷 번호
            short packetNumber = BitConverter.ToInt16(packetSegment.Array, 2);
            // x,y좌표 받기
            //  [x][x][x][x][y][y][y][y]
            int xPos = -1;
            int yPos = -1;
            if ( packetNumber == 8164)
            {
                xPos = BitConverter.ToInt32(packetSegment.Array, 4);
                yPos = BitConverter.ToInt32(packetSegment.Array, 8);
            }

            Console.WriteLine($"packet size{packetSize}, packet number{packetNumber}");
            Console.WriteLine($"Player located by [{xPos}, {yPos}].");

            return 0;
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
