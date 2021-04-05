using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Listener
    {
        Socket _ListenSocket;
      //  Action<Socket> _OnAcceptHandler;
        Func<Session> _makeSession;

        public void Start(IPEndPoint iPEndPoint, Func<Session> GameSession)
        {
            this._makeSession = GameSession;
            // _OnAcceptHandler = OnAcceptHandler;
            _ListenSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            _ListenSocket.Bind(iPEndPoint);
            _ListenSocket.Listen(1000);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);

            RegisterAccpet(args);
        }

        void RegisterAccpet(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;

            bool pending = false;
            pending = _ListenSocket.AcceptAsync(args);
            if(pending == false)
            {
                OnAcceptCompleted(null, args);
            }
        }

        void OnAcceptCompleted(object obj, SocketAsyncEventArgs args)
        {
            if(args.SocketError == SocketError.Success)
            {
                Console.WriteLine("클라이언트 연결 요청 Accpet Completed!");

                Session session = _makeSession.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnect(args.AcceptSocket.RemoteEndPoint);
                // _OnAcceptHandler.Invoke(args.AcceptSocket);
            }
            else
            {
                
            }

            RegisterAccpet(args);
        }

    }
}
