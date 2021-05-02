using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    //class Packet
    //{
    //    public short size;
    //    public short packetNumber;
    //    public byte[] data;
    //}

    //class PlayerInfoReq : Packet
    //{

    //}

    //class PlayerInfoOk : Packet
    //{

    //}

    //class SendMessage : Packet
    //{
    //    //  public byte[] data;
    //}

    //class PositionInfo : Packet
    //{
    //    int xPos;
    //    int yPos;
    //}

    abstract class Packet
    {
        public short size;
        public short packetNumber;

        abstract public ArraySegment<byte> Write();
        abstract public void Read(ArraySegment<byte> byteData);
    }

    class PlayerInfoReq : Packet
    {
        public override void Read(ArraySegment<byte> byteData)
        {
            throw new NotImplementedException();
        }

        public override ArraySegment<byte> Write()
        {
            throw new NotImplementedException();
        }
    }

    class PlayerInfoOk : Packet
    {
        public override void Read(ArraySegment<byte> byteData)
        {
            throw new NotImplementedException();
        }

        public override ArraySegment<byte> Write()
        {
            throw new NotImplementedException();
        }
    }

    class SendMessage : Packet
    {
        public string message;

        public override void Read(ArraySegment<byte> byteData)
        {
            size = BitConverter.ToInt16(byteData.Array, 0 + byteData.Offset);
            packetNumber = BitConverter.ToInt16(byteData.Array, 2 + byteData.Offset);

            ArraySegment<byte> byteString = new ArraySegment<byte>(byteData.Array, 4 + byteData.Offset, size - 4);
            message = Encoding.Unicode.GetString(byteString.Array,4+ byteData.Offset, size - 4);
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> sendBuffer = SendBufferHelper.Open(4096);

            Buffer.BlockCopy(BitConverter.GetBytes(size), 0, sendBuffer.Array, sendBuffer.Offset, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(packetNumber), 0, sendBuffer.Array, sendBuffer.Offset + 2, 2);
            Buffer.BlockCopy(Encoding.Unicode.GetBytes(message), 0, sendBuffer.Array, sendBuffer.Offset + 4, Encoding.Unicode.GetByteCount(message));

            return SendBufferHelper.Close(Encoding.Unicode.GetByteCount(message) + 4);
        }
    }

    class PositionInfo : Packet
    {
        public int xPos;
        public int yPos;

        public PositionInfo()
        {
            packetNumber = (short)PacketId.SendPosition;
        }

        public override void Read(ArraySegment<byte> byteData)
        {
            // 패킷 크기 
            size = BitConverter.ToInt16(byteData.Array, 0 + byteData.Offset);
            //size = BitConverter.ToInt64(new ReadOnlySpan<byte>(byteData.Array, byteData.Offset + size, byteData.Count - size));
            // 패킷 번호
            packetNumber = BitConverter.ToInt16(byteData.Array, 2 + byteData.Offset);

            xPos = BitConverter.ToInt32(byteData.Array, 4 + byteData.Offset);
            yPos = BitConverter.ToInt32(byteData.Array, 8 + byteData.Offset);


        }

        // 패킷 내용을 눌러짜서 ArraySegment<byte>로 반환한다.
        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> sendBuffer = SendBufferHelper.Open(4096);

            Buffer.BlockCopy(BitConverter.GetBytes(size), 0, sendBuffer.Array, sendBuffer.Offset, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(packetNumber), 0, sendBuffer.Array, sendBuffer.Offset + 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(xPos), 0, sendBuffer.Array, sendBuffer.Offset + 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(yPos), 0, sendBuffer.Array, sendBuffer.Offset + 8, 4);

            return SendBufferHelper.Close(12);
        }
    }

    public enum PacketId
    {
        PlayerInfoReq = 1,
        layerInfoOk = 2,
        SendMessage = 7799,
        SendPosition = 8164,
    }

    // 하나의 연결, 하나의 쓰레드가 점유하는 공간이라고 봐도 무방
    class ClientSession : PacketSession
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


            //// SendBuffer 활요하기
            //ArraySegment<byte> openSegment = SendBufferHelper.Open(4096);

            //// Knight 정보 보내기
            //Knight knight = new Knight() { hp = 100, attack = 10 };
            //// 바이트로 바꾸기
            //byte[] knightHpBuffer = BitConverter.GetBytes(knight.hp);
            //byte[] knightAtacckbuffer = BitConverter.GetBytes(knight.attack);
            //// bytes 버퍼에 해당 내용 담아서 보내기
            //Array.Copy(knightHpBuffer, 0, openSegment.Array, openSegment.Offset, knightHpBuffer.Length);
            //Array.Copy(knightAtacckbuffer, 0, openSegment.Array, openSegment.Offset + knightHpBuffer.Length, knightHpBuffer.Length);
            //// SendBuffer 사용 다 했으면 닫아줍니다.
            //ArraySegment<byte> sendBuff= SendBufferHelper.Close(knightHpBuffer.Length + knightHpBuffer.Length);
            //// Send(sendBuff);

            //Console.WriteLine("session.Send(bytes);");
            // clientSocket.Send(bytes, SocketFlags.None);



            Thread.Sleep(1000);
        }

        public override void OnDispose()
        {

        }

        public override int OnPacketRecv(ArraySegment<byte> packetSegment)
        {


            // 패킷 크기 
            short packetSize = BitConverter.ToInt16(packetSegment.Array, 0 + packetSegment.Offset);
            // 패킷 번호
            short packetNumber = BitConverter.ToInt16(packetSegment.Array, 2 + packetSegment.Offset);

            Console.WriteLine($"packet size{packetSize}, packet number{packetNumber}");

            if (packetNumber == (short)PacketId.SendPosition)
            {
                PositionInfo packet = new PositionInfo();
                packet.Read(packetSegment);
                // x,y좌표 받기
                //  [x][x][x][x][y][y][y][y]
                int xPos = -1;
                int yPos = -1;
                {
                    //xPos = BitConverter.ToInt32(packetSegment.Array, 4 + packetSegment.Offset);
                    xPos = packet.xPos;
                    //yPos = BitConverter.ToInt32(packetSegment.Array, 8 + packetSegment.Offset);
                    yPos = packet.yPos;
                }

                Console.WriteLine($"Player located by [{xPos}, {yPos}].");
            }
            else if (packetNumber == (short)PacketId.SendMessage)
            {
                SendMessage packet = new SendMessage();
                packet.Read(packetSegment);


                string str = packet.message;
                // string str = Encoding.UTF8.GetString(packetSegment.Array, 4 + packetSegment.Offset, packetSize - 4);
                Console.WriteLine();

                Console.WriteLine($"string: {str} / send by Client");
            }
            return 0;
        }


        public override void OnSend(int bytesCount)
        {
            Console.WriteLine($"OnSendCompleted, 전송된 bytes:{bytesCount} byte");


        }
    }

}
