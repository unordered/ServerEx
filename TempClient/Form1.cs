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
using System.Windows.Documents;
using System.Windows.Forms;
using ServerCore;

namespace TempClient
{
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

                PositionInfo packet = new PositionInfo();
                packet.size = 12;
                packet.packetNumber = (short)PacketId.SendPosition;
                packet.xPos = x;
                packet.yPos = y;
                // packet.data = new byte[8];



                //byte[] sendBuffer = new byte[12];


                //Buffer.BlockCopy(BitConverter.GetBytes(packet.size), 0, sendBuffer, 0, 2);
                //Buffer.BlockCopy(BitConverter.GetBytes(packet.packetNumber), 0, sendBuffer, 2, 2);
                //Buffer.BlockCopy(BitConverter.GetBytes(x), 0, sendBuffer, 4, 4);
                //Buffer.BlockCopy(BitConverter.GetBytes(y), 0, sendBuffer, 8, 4);

                // packet.size = Convert.ToInt16(packet.data.Length + 4);

                //byte[] sendbuff = new byte[packet.size];

                //Buffer.BlockCopy(BitConverter.GetBytes(packet.size), 0, sendbuff, 0, 2);
                //Buffer.BlockCopy(BitConverter.GetBytes(packet.packetNumber), 0, sendbuff, 2, 2);
                //Buffer.BlockCopy(packet.data, 0, sendbuff, 4, packet.data.Length);
                gameSession.queue.Enqueue(packet);


                Console.WriteLine($"x: {e.X} y: {e.Y}");
                Console.WriteLine($"전송시작, 패킷 크기{packet.size}bytes."); ;


                Thread.Sleep(30);
            }


        }

        ServerSession gameSession = new ServerSession();
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

                const int MAX_READ = 100;
                int currentRead = 0;
                while(!connector.isConnected())
                {
                    Console.WriteLine($"서버 연결중...{currentRead}/100");
                    Thread.Sleep(100);
                    if(currentRead > MAX_READ)
                    {
                        throw new Exception("서버 연결 실패");
                    }
                    currentRead++;
                }

            }
            catch(Exception exc)
            {
                
                MessageBox.Show(exc.Message, "경고");
                Close();
            }

        }

        // 텍스트 전송
        private void button1_Click(object sender, EventArgs e)
        {
            lock (gameSession.queueLockObject)
            {
                //Console.Write("x 좌표:");
                //int x = e.X;
                //Console.Write("y 좌표:");
                //int y = e.Y;



               SendMessage packet = new SendMessage();
                // packet.packetNumber = 7799;
                packet.packetNumber = (short)PacketId.SendMessage;

                // byte[] textbuff = Encoding.UTF8.GetBytes(textBox1.Text);

                packet.message = textBox1.Text;
                packet.size = (short)(Encoding.Unicode.GetByteCount(packet.message)+4);
                
                //Buffer.BlockCopy(BitConverter.GetBytes(x), 0, packet.data, 0, 4);
                //Buffer.BlockCopy(BitConverter.GetBytes(y), 0, packet.data, 4, 4);

                // packet.size = Convert.ToInt16(packet.data.Length + 4);

                //byte[] sendbuff = new byte[packet.size];


                gameSession.queue.Enqueue(packet);

                //     BitConverter.TryWriteBytes(new Span<byte>(sendbuff, 4, textBox1.Text.Length), textBox1.Text);


                //Buffer.BlockCopy(BitConverter.GetBytes(packet.size), 0, sendbuff, 0, 2);
                //Buffer.BlockCopy(BitConverter.GetBytes(packet.packetNumber), 0, sendbuff, 2, 2);
                ////  Buffer.BlockCopy(packet.data, 0, sendbuff, 4, packet.data.Length);

                //Buffer.BlockCopy(textbuff, 0, sendbuff, 4, textbuff.Length);
              
                //gameSession.queue.Enqueue(sendbuff);


                //Console.WriteLine($"x: {e.X} y: {e.Y}");
                Console.WriteLine($"보낸 내용: {textBox1.Text}");
                Console.WriteLine($"전송시작, 패킷 크기{packet.size}bytes."); ;

                textBox1.Text = "";
                Thread.Sleep(30);
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            button1_Click(sender, e);
        }
    }


}
