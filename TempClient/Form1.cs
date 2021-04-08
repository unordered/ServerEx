using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ServerCore;

namespace TempClient
{
    struct Packet
    {
        public short size;
        public short packetNumber;
        public byte[] data;
    }



    class GameSession : PacketSession
    {
        public Queue<byte[]> queue = new Queue<byte[]>();
        public object queueLockObject = new object();
        public override void OnConnect(EndPoint endPoint)
        {
            Console.WriteLine("연결 완료");
            while(true)
            {
                lock (queueLockObject)
                {
                    
                    if (queue.Count > 0)
                    {
                        byte[] sendBuff = queue.Dequeue();
                        Send(new ArraySegment<byte>(sendBuff, 0, sendBuff.Length));
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

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {

            lock (gameSession.queueLockObject)
            {
                //Console.Write("x 좌표:");
                int x = e.X;
                //Console.Write("y 좌표:");
                int y = e.Y;

                Packet packet = new Packet();
                packet.packetNumber = 8164;
                packet.data = new byte[8];

                Buffer.BlockCopy(BitConverter.GetBytes(x), 0, packet.data, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(y), 0, packet.data, 4, 4);

                packet.size = Convert.ToInt16(packet.data.Length + 4);

                byte[] sendbuff = new byte[packet.size];

                Buffer.BlockCopy(BitConverter.GetBytes(packet.size), 0, sendbuff, 0, 2);
                Buffer.BlockCopy(BitConverter.GetBytes(packet.packetNumber), 0, sendbuff, 2, 2);
                Buffer.BlockCopy(packet.data, 0, sendbuff, 4, packet.data.Length);
                gameSession.queue.Enqueue(sendbuff);


                Console.WriteLine($"x: {e.X} y: {e.Y}");
                Console.WriteLine($"전송시작, 패킷 크기{sendbuff.Length}bytes."); ;


                Thread.Sleep(30);
            }

        }

        GameSession gameSession = new GameSession();
      //  Connector connector;

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                Connector connector = new Connector();
                string hostName = Dns.GetHostName();

                //   IPAddress iPAddress = Dns.GetHostEntry(hostName).AddressList[0];

                IPAddress iPAddress; 
                System.Net.IPAddress.TryParse("175.192.72.95", out iPAddress);

                IPEndPoint iPEndPoint = new IPEndPoint(iPAddress, 7777);
                connector.Connect(iPEndPoint, () => { return gameSession; });
            }
            catch(Exception exc)
            {
                
                MessageBox.Show(exc.Message, "서버 연결 실패!");
            }

        }
    }
}
