using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;//TCP

namespace mySocket
{
    class SocketTool
    {
        private static bool serverState = true;
        TcpListener serverListener;
        Socket acceptSocket;
        bool keepReceive = true;
        int receiveCountdown = -1;
        public delegate void SocketMsg(string msg);
        public event SocketMsg serverReceive;
        public SocketTool()
        {
            //serverReceive = new ServerReceive(consoleWrite);
        }
        ~SocketTool()
        {
            serverStop();
        }
        public void StartServer(string IP, int port)
        {
            serverState = true;
            Task.Run(() =>
            {
                System.Net.IPAddress theIPAddress;
                theIPAddress = System.Net.IPAddress.Parse(IP);
                serverListener = new TcpListener(theIPAddress, port);//建立 listener

                try
                {
                    serverListener.Start();//開始 listener
                    Console.WriteLine($"通訊埠 {port} 等待用戶端連線......");
                    acceptSocket = serverListener.AcceptSocket();//等待接受，接受後繼續執行
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return;
                }


                Task.Run(() =>
                {
                    keepReceive = true;
                    while (acceptSocket.Connected)//如果一直連線，則持續執行
                    {

                        for (; receiveCountdown > 0 || keepReceive; receiveCountdown--)
                        {
                            try
                            {
                                int bufferSize = myTcpClient.ReceiveBufferSize;
                                byte[] myBufferBytes = new byte[bufferSize];
                                int L = acceptSocket.Receive(myBufferBytes);
                                string msg = Encoding.ASCII.GetString(myBufferBytes, 0, L);
                                if (serverReceive != null) serverReceive.Invoke(msg);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error server recerive:" + ex.Message);
                                acceptSocket.Close();
                                break;
                            }
                        }

                    }
                    acceptSocket.Close();
                    serverListener.Stop();
                    Console.WriteLine("Server out!");
                });

            });

        }

        public void serverSend(string msg)
        {
            if (!acceptSocket.Connected) { Console.WriteLine("尚未連線"); return; }

            Byte[] myBytes = Encoding.ASCII.GetBytes(msg);
            acceptSocket.Send(myBytes, myBytes.Length, 0);
        }
        public void serverStop()
        {
            Console.WriteLine("關閉伺服器");
            //serverState = false;
            if (acceptSocket != null)
                acceptSocket.Close();
            if (serverListener != null)
                serverListener.Stop();
        }




        TcpClient myTcpClient;
        Socket clientSocket;
        private bool isConect = false;
        public bool ClientConnect(string IP, int port)
        {
            string hostName = IP;
            int connectPort = port;
            myTcpClient = new TcpClient();
            try
            {
                myTcpClient.Connect(hostName, connectPort);
                clientSocket = myTcpClient.Client;
                Console.WriteLine("連線成功 !!");
                isConect = true;
                return true;
            }
            catch
            {
                isConect = false;
                Console.WriteLine
                           ("主機 {0} 通訊埠 {1} 無法連接  !!", hostName, connectPort);
                return false;
            }
        }
        public void client_SendData(byte[] myBytes)
        {
            clientSocket.Send(myBytes, myBytes.Length, 0);
        }
        public void client_SendData(string msg)
        {
            if (!isConect) { Console.WriteLine("尚未連線"); return; }

            try
            {
                String str = msg;
                Byte[] myBytes = Encoding.ASCII.GetBytes(str);
                clientSocket.Send(myBytes, myBytes.Length, 0);
            }
            catch
            {
                Console.WriteLine("client send data error");
                return;
            }
        }
        public string client_ReadData()
        {
            if (!isConect) { Console.WriteLine("尚未連線(val:isConect)"); return ""; }
            if (myTcpClient == null) { Console.WriteLine("尚未連線:TcpClient"); return ""; }
            if (clientSocket == null) { Console.WriteLine("尚未連線:Socket"); return ""; }
            try
            {
                int bufferSize = myTcpClient.ReceiveBufferSize;
                byte[] myBufferBytes = new byte[bufferSize];
                int dataLength = clientSocket.Receive(myBufferBytes);
                string msg = Encoding.ASCII.GetString(myBufferBytes, 0, dataLength);

                return msg;
            }
            catch (Exception ex)
            {
                Console.WriteLine("client read error \n" + ex.ToString());
                return "";
            }

        }
        public string client_ReadData2()
        {
            if (!isConect) { Console.WriteLine("尚未連線(val:isConect)"); return ""; }
            if (myTcpClient == null) { Console.WriteLine("尚未連線:TcpClient"); return ""; }
            if (clientSocket == null) { Console.WriteLine("尚未連線:Socket"); return ""; }

            int bufferSize = myTcpClient.ReceiveBufferSize;
            byte[] myBufferBytes = new byte[bufferSize];
            int dataLength = clientSocket.Receive(myBufferBytes);
            string msg = Encoding.ASCII.GetString(myBufferBytes, 0, dataLength);
            return msg;

        }
    }
}
