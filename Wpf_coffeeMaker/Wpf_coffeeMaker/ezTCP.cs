using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;//TCP

namespace myTCP
{
    class ezTCP
    {
        private static bool serverState = true;
        public ezTCP()
        {
            serverState = true;
        }


        public void serverOn(string IP, int port)
        {
            serverState = true;
            Task.Run(() =>
            {
                System.Net.IPAddress theIPAddress;
                //建立 IPAddress 物件(本機)
                theIPAddress = System.Net.IPAddress.Parse(IP);

                //建立監聽物件
                TcpListener myTcpListener = new TcpListener(theIPAddress, port);
                //啟動監聽
                myTcpListener.Start();
                Console.WriteLine($"通訊埠 {port} 等待用戶端連線......");
                Socket mySocket = myTcpListener.AcceptSocket();

                while (serverState)
                {
                    try
                    {
                        if (mySocket.Connected)
                        {
                            int dataLength;
                            Console.WriteLine("連線成功 !!");
                            byte[] myBufferBytes = new byte[1000];
                            //取得用戶端寫入的資料
                            dataLength = mySocket.Receive(myBufferBytes);

                            Console.WriteLine($"接收到資料內容:");
                            Console.WriteLine(Encoding.ASCII.GetString(myBufferBytes, 0, dataLength));
                            Console.WriteLine("資料長度 {0}", dataLength.ToString());

                            Console.WriteLine("將資料回傳至用戶端 !!");
                            //將接收到的資料回傳給用戶端
                            mySocket.Send(myBufferBytes, dataLength, 0);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message, "server off");
                        mySocket.Close();
                        serverState = false;
                        break;
                    }
                }
                Console.WriteLine("Server out!");
            });

        }

        public void serverStop()
        {
            Console.WriteLine("關閉伺服器");
            serverState = false;
        }

        //宣告網路資料流變數
        NetworkStream myNetworkStream;
        //宣告 Tcp 用戶端物件
        TcpClient myTcpClient;
        private bool isConect = false;
        public bool creatClient(string IP, int port)
        {

            string hostName = IP;
            int connectPort = port;
            //建立 TcpClient 物件
            myTcpClient = new TcpClient();
            try
            {
                //測試連線至遠端主機
                myTcpClient.Connect(hostName, connectPort);
                Console.WriteLine("連線成功 !!\n");
                isConect = true;
                return true;
            }
            catch
            {
                Console.WriteLine
                           ("主機 {0} 通訊埠 {1} 無法連接  !!", hostName, connectPort);
                return false;
            }
        }
        public void client_sendMyData(List<float[]> dataList)
        {
            byte[] sendData = new byte[4 * 6];//6 obj
            for (int i = 0; i < sendData.Count(); i++)
                sendData[i] = 255;

            foreach (float[] data in dataList)
            {
                int offset = (int)data[0] * 4;
                //0 1 2 3  //4 5 6 7 //8 9 10 11
                //x y z t
                for (int i = 0; i < 3; i++)
                    sendData[i + offset] = (byte)((data[i + 1] * 10) + 128);
                sendData[3 + offset] = (byte)(data[3 + 1]);
            }
            client_SendData(sendData);
        }
        public void client_SendData(byte[] myBytes)
        {
            Console.WriteLine("建立網路資料流 !!");
            //建立網路資料流
            myNetworkStream = myTcpClient.GetStream();

            Console.WriteLine("將字串寫入資料流");
            //將字串寫入資料流
            myNetworkStream.Write(myBytes, 0, myBytes.Length);
        }
        public void client_SendData(string msg)
        {
            if (!isConect) { Console.WriteLine("尚未連線"); return; }

            String str = msg;
            //將字串轉 byte 陣列，使用 ASCII 編碼
            Byte[] myBytes = Encoding.ASCII.GetBytes(str);

           // Console.WriteLine("建立網路資料流 !!");
            //建立網路資料流
            myNetworkStream = myTcpClient.GetStream();

            //Console.WriteLine("將字串寫入資料流");
            //將字串寫入資料流
            myNetworkStream.Write(myBytes, 0, myBytes.Length);
        }
        public string client_ReadData()
        {
            if (!isConect) { Console.WriteLine("尚未連線"); return ""; }

            //Console.WriteLine("從網路資料流讀取資料 !!");
            //從網路資料流讀取資料
            int bufferSize = myTcpClient.ReceiveBufferSize;
            byte[] myBufferBytes = new byte[bufferSize];
            myNetworkStream.Read(myBufferBytes, 0, bufferSize);
            //取得資料並且解碼文字
            string msg = Encoding.ASCII.GetString(myBufferBytes, 0, bufferSize);
            //Console.WriteLine($"讀取到資料內容:");
            //Console.WriteLine(msg.Substring(0, msg.IndexOf('\0')));
            return msg.Substring(0, msg.IndexOf('\0'));
        }
        public void client_readMyData()
        {
            if (!isConect) { Console.WriteLine("尚未連線"); return; }


            //從網路資料流讀取資料
            int bufferSize = 4 * 6;
            byte[] myBufferBytes = new byte[4 * 6];
            myNetworkStream.Read(myBufferBytes, 0, bufferSize);
            for (int i = 0; i < 6; i++)
            {
                int offset = i * 4;
                int obj = i;
                float x = (myBufferBytes[offset] - 128) / 10f;
                float y = (myBufferBytes[1 + offset] - 128) / 10f;
                float z = (myBufferBytes[2 + offset] - 128) / 10f;
                float t = myBufferBytes[3 + offset];
                if (myBufferBytes[3 + offset] == 255)
                {
                    Console.WriteLine($"Obj:{obj}-NA");
                }
                else
                {
                    Console.WriteLine($"Obj:{obj}[{x},{y},{z},{t}]");
                }

            }

        }
    }
}
