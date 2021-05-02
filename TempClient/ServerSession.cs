using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TempClient
{
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
            
            ArraySegment<byte> byteString = new ArraySegment<byte>(byteData.Array,4 +byteData.Offset , size-4);
            message = Encoding.Unicode.GetString(byteString.Array, 0, size - 4);
        }

        public override ArraySegment<byte> Write()
        {
            ArraySegment<byte> sendBuffer = SendBufferHelper.Open(4096);

            Buffer.BlockCopy(BitConverter.GetBytes(size), 0, sendBuffer.Array, sendBuffer.Offset, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(packetNumber), 0, sendBuffer.Array, sendBuffer.Offset + 2, 2);
            Buffer.BlockCopy(Encoding.Unicode.GetBytes(message), 0,sendBuffer.Array, sendBuffer.Offset+4 ,Encoding.Unicode.GetByteCount(message));
    
            return SendBufferHelper.Close(Encoding.Unicode.GetByteCount(message)+4);
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


    class ServerSession : PacketSession
    {
        public Queue<Packet> queue = new Queue<Packet>();
        public object queueLockObject = new object();
        public override void OnConnect(EndPoint endPoint)
        {
            Console.WriteLine("연결 완료");
            while (true)
            {
                lock (queueLockObject)
                {

                    if (queue.Count > 0)
                    {
                        Packet sendPacket = queue.Dequeue();
                        if(sendPacket.packetNumber == (short)PacketId.SendPosition)
                        {
                            PositionInfo positionInfo = (PositionInfo)sendPacket;
                            //byte[] sendBuffer = new byte[12]; -1

                            //ArraySegment<byte> sendBuffer = SendBufferHelper.Open(4096);

                            //Buffer.BlockCopy(BitConverter.GetBytes(positionInfo.size), 0, sendBuffer.Array, sendBuffer.Offset, 2);
                            //Buffer.BlockCopy(BitConverter.GetBytes(positionInfo.packetNumber), 0, sendBuffer.Array, sendBuffer.Offset+2, 2);
                            //Buffer.BlockCopy(BitConverter.GetBytes(positionInfo.xPos), 0, sendBuffer.Array, sendBuffer.Offset+4, 4);
                            //Buffer.BlockCopy(BitConverter.GetBytes(positionInfo.yPos), 0, sendBuffer.Array, sendBuffer.Offset+8, 4);


                            //Send(SendBufferHelper.Close(12));
                            // Send(new ArraySegment<byte>(sendBuffer, 0, sendBuffer.Length)); -1

                            Send(positionInfo.Write());
                        }
                        else if(sendPacket.packetNumber == (short)PacketId.SendMessage)
                        {
                            SendMessage sendMessage = (SendMessage)sendPacket;
                            // byte[] sendBuffer = new byte[sendMessage.size];

                            //Buffer.BlockCopy(BitConverter.GetBytes(sendMessage.size), 0, sendBuffer, 0, 2);
                            //Buffer.BlockCopy(BitConverter.GetBytes(sendMessage.packetNumber), 0, sendBuffer, 2, 2);
                            //Buffer.BlockCopy(sendMessage.data, 0, sendBuffer, 4, sendMessage.data.Length);


                            //Send(new ArraySegment<byte>(sendBuffer, 0, sendBuffer.Length));
                            Send(sendMessage.Write());

                        }

                    }
                }
            }
        }

        public override void OnDispose()
        {
            Console.WriteLine("연결 종료");
        }

        public override int OnPacketRecv(ArraySegment<byte> packetSegment)
        {
            return 0;
        }

        public override void OnSend(int bytesCount)
        {
            Console.WriteLine($"{bytesCount} bytes 전송 완료");
            return;
        }
    }
}
