using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    public abstract class PacketSession : Session
    {
        readonly short packetSize = 2;

        // 패킷 구조
        // [size][size][p.number][p.number][..][..][..][..][..][size][size][p.number][p.number][..][..][..][..][..]
        byte[] tempPacket = new byte[1024];
        int curLength = 0;


        public sealed override int OnRecv(ArraySegment<byte> recvbuf)
        {
            // 해당 코드는 패킷을 단 하나도 놓치지 않고 잡는 코드이다.
            // 패킷이 완전하게 오지 않는다면 저장해 놓고 있다가, 다음 OnRecv에서 합친다.
            short size = 0;

            while (true)
            {
                
                if(curLength ==0 && recvbuf.Count < packetSize)
                {
                    return -1;
                }

                if (curLength == 0)
                {
                    size = BitConverter.ToInt16(recvbuf.Array, recvbuf.Offset);

                    if(recvbuf.Count == size)
                    {
                        OnPacketRecv(new ArraySegment<byte>(recvbuf.Array, 0,size));
                        return size;
                    }
                    else
                    {
                        tempPacket = recvbuf.Array;
                    }
                }
                else
                {
                    // 패킷 바이트 하나씩 복사해주기
                    for (int i = curLength; i < curLength + size; i++)
                        tempPacket[i] = recvbuf.Array[i-curLength];
                    
                    curLength += recvbuf.Count;
                }

                if(curLength == Convert.ToInt32(size))
                {
                    OnPacketRecv(new ArraySegment<byte>(tempPacket, 0, size));
                    curLength = 0;
                }

                return curLength;
                
            }
        }

        public abstract int OnPacketRecv(ArraySegment<byte> packetSegment);
    }

    public abstract class Session
    {
        Socket _socket;
        SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs RecvArgs = new SocketAsyncEventArgs();

        RecvBuffer recvBuffer = new RecvBuffer(1024);
        List<ArraySegment<byte>> pendingList = new List<ArraySegment<byte>>();

        public abstract void OnSend(int bytesCount);
        public abstract int OnRecv(ArraySegment<byte> recvbuf);
        public abstract void OnConnect(EndPoint endPoint);
        public abstract void OnDispose();



        public void Start(Socket socket)
        {
            _socket = socket;

            RecvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReciveCompleted);
            sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
            sendArgs.BufferList = pendingList;

           // RecvArgs.SetBuffer(new byte[1024], 0, 1024);
            RegisterRecv(RecvArgs);
        }

        public void RegisterRecv(SocketAsyncEventArgs args)
        {
            Console.WriteLine("RegisterRecv");
            // 커서 이동 방지
            recvBuffer.Clean();
            args.SetBuffer(recvBuffer.WriteSegment.Array,0,recvBuffer.FreeSize);

            bool pending = false;
            pending = _socket.ReceiveAsync(args);
            if(pending == false)
            {
                OnReciveCompleted(null, args);
            }
        }

        public void OnReciveCompleted(object obj, SocketAsyncEventArgs args)
        {
            Console.WriteLine("OnReciveCompleted");
            if (args.SocketError == SocketError.Success && args.BytesTransferred > 0)
            {
                //string recvString = Encoding.UTF8.GetString(args.Buffer);
                //Console.WriteLine($"[From Client]: {recvString}");

                //  받은 만큼 커서 이동
                if(recvBuffer.OnWrite(args.BytesTransferred)==false)
                {
                    Dispose();
                    return;
                }

                // 컨텐츠쪽으로 데이터를 넘겨준다. 
                // OnRecv(args.Buffer);
                int processLength = OnRecv(recvBuffer.ReadSegment);
                if(processLength < 0 || processLength > recvBuffer.DataSize)
                {
                    Dispose();
                    return;
                }

                // 처리를 한 커서를 이동해준다.
                if(recvBuffer.OnRead(processLength) == false)
                {
                    Dispose();
                    return;
                }

                
                RegisterRecv(args);
            }
            else
            {
                //Thread.Sleep(1000);
                Console.WriteLine("_socket.Shutdown(SocketShutdown.Both);");
                //_socket.Shutdown(SocketShutdown.Both);
                //_socket.Close();
                //Dispose();
            }
            
        }

        object lockOjbect = new object();
        void Dispose()
        {
            lock (lockOjbect)
            {
                if (_socket != null)
                {
                    OnDispose();

                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Close();
                    _socket = null;
                }
                
            }
        }

        Queue<ArraySegment<byte>> sendQueue = new Queue<ArraySegment<byte>>();
        object sendLock = new object();

        public void Send(ArraySegment<byte> sendBuf)
        {
            lock (sendLock)
            {
                sendQueue.Enqueue(sendBuf);
                if(pendingList.Count == 0)
                    RegisterSend(sendArgs);
            }
        }

        void RegisterSend(SocketAsyncEventArgs args)
        {
            // sendQueue에서 Count == 0 이 될 떄 까지 다 뽑아서
            // SetBufferList에 넣기
            // 그다음 전송

            while(sendQueue.Count != 0)
            {
                ArraySegment<byte> temp = sendQueue.Dequeue();
                pendingList.Add(temp);
            }

            args.BufferList = pendingList;

            bool _pending = _socket.SendAsync(sendArgs);
            if (_pending == false)
            {
                OnSendCompleted(null, sendArgs);
            }
            
            Console.WriteLine($"args.BufferList.Count: {args.BufferList.Count}");


        }


        object sendC = new object();

        public void OnSendCompleted(object obj, SocketAsyncEventArgs args)
        {
            lock (sendC)
            {
                try
                {
                    if (args.SocketError == SocketError.Success && args.BytesTransferred > 0)
                    {
                        //Console.WriteLine($"OnSendCompleted, 전송된 bytes:{args.BytesTransferred} byte");
                        OnSend(args.BytesTransferred);
                        //Send(args.Buffer);
                        args.BufferList.Clear();
                        if (sendQueue.Count != 0)
                            RegisterSend(args);
                    }
                    else
                    {
                        //Thread.Sleep(1000);
                        Console.WriteLine("_socket.Shutdown(SocketShutdown.Both);");
                        //_socket.Shutdown(SocketShutdown.Both);
                        //_socket.Close();
                        // Dispose();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

     }
}
