using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;


namespace UrRobot
{
    public enum mode
    {
        End = -1,
        stop = 0,
        recordj = 1,
        recordp = 2,
        jmovej = 3,
        pmovep = 4,
        gripper = 5,
        grip = 6,
        jog = 10,
        moveByFile = 99
    }
    public class robotControl
    {
        public mode cmd = mode.stop;
        public static bool serverOn = false;
        public bool serverRunning
        {
            get { return serverOn; }
        }

        public delegate void UREventHandler(object sender, LinkArgs e);
        public event UREventHandler stateChange;

        //file
        static string ReadingFile = "";
        public StreamWriter txt_record;

        string sMsg = string.Empty;
        public string Msg { get { return sMsg; } }
        static byte rq_pos = 0;

        private void theServer(TcpListener tcp, ref mode cmd)
        {
            try
            {
                tcp.Start();
                OnLinkState(new LinkArgs("Wait Connect"));
            }
            catch (Exception ex) { Console.WriteLine("tcp start error\n" + ex); return; }
            TcpClient UR_Client = tcp.AcceptTcpClient();
            if (UR_Client.Client.Connected)
            {
                NetworkStream stream = UR_Client.GetStream();
                stream.WriteTimeout = 100;
                OnLinkState(new LinkArgs("Connect"));
                serverOn = true;
                cmd = mode.stop;
                while (serverOn)
                {
                    if (cmd == mode.moveByFile)
                    {
                        string[] fileList = System.IO.File.ReadAllLines(ReadingFile);
                        for (int p = 0; p < fileList.Count();)
                        {
                            if (p >= fileList.Count())//代表最後一行了
                                break;
                            if (fileList[p] == "")
                            { p++; continue; }

                            switch (fileList[p])
                            {
                                case "position":
                                    while (true)
                                    {
                                        p++;
                                        if (p >= fileList.Count())//代表最後一行了
                                            break;
                                        if (fileList[p].IndexOf('[') == -1)//代表是指令
                                            break;
                                        _sendMsg("pmovep", ref cmd);//record pos模式

                                        sMsg = _waitRead(ref cmd); if (sMsg == "End") break;
                                        Console.WriteLine("Robot : " + sMsg);//應該會是獲得 movep

                                        byte[] _pcount = new byte[1] { 1 };
                                        stream.Write(_pcount, 0, 1);//@test   1個點

                                        sMsg = _waitRead(ref cmd); if (sMsg == "End") break;
                                        Console.WriteLine("Robot : " + sMsg);//應該會是獲得"set" 也就是要開始給座標點

                                        _sendMsg("(" + fileList[p].Substring(2, fileList[p].Length - 3) + ")", ref cmd);//point

                                        sMsg = _waitRead(ref cmd); if (sMsg == "End") break;
                                        Console.WriteLine("Robot : " + sMsg);//應該會是獲得"work done" 
                                    }
                                    break;
                                case "joint":
                                    while (true)
                                    {
                                        p++;
                                        if (p >= fileList.Count())//代表最後一行了
                                            break;
                                        if (fileList[p].IndexOf('[') == -1)//代表是指令
                                            break;
                                        _sendMsg("jmovej", ref cmd);//jmovej模式

                                        sMsg = _waitRead(ref cmd); if (sMsg == "End") break;
                                        Console.WriteLine("Robot : " + sMsg);//應該會是獲得 movej

                                        byte[] _pcount = new byte[1] { 1 };
                                        stream.Write(_pcount, 0, 1);//@test   1個點

                                        sMsg = _waitRead(ref cmd); if (sMsg == "End") break;
                                        Console.WriteLine("Robot : " + sMsg);//應該會是獲得"set" 也就是要開始給座標點

                                        _sendMsg("(" + fileList[p].Substring(1, fileList[p].Length - 2) + ")", ref cmd);//joint

                                        sMsg = _waitRead(ref cmd); if (sMsg == "End") break;
                                        Console.WriteLine("Robot : " + sMsg);//應該會是獲得"work done" 
                                    }
                                    break;
                                case "Rmovej":
                                    while (true)
                                    {
                                        p++;
                                        if (p >= fileList.Count())//代表最後一行了
                                            break;
                                        if (fileList[p].IndexOf('[') == -1)//代表是指令
                                            break;
                                        _sendMsg("Rmovej", ref cmd);//Rmovej模式 //相對移動 joint

                                        sMsg = _waitRead(ref cmd); if (sMsg == "End") break;
                                        Console.WriteLine("Robot : " + sMsg);//應該會是獲得 Rmovej

                                        byte[] _pcount = new byte[1] { 1 };
                                        stream.Write(_pcount, 0, 1);//@test   1個點

                                        sMsg = _waitRead(ref cmd); if (sMsg == "End") break;
                                        Console.WriteLine("Robot : " + sMsg);//應該會是獲得"set" 也就是要開始給座標點

                                        _sendMsg("(" + fileList[p].Substring(1, fileList[p].Length - 2) + ")", ref cmd);

                                        sMsg = _waitRead(ref cmd); if (sMsg == "End") break;
                                        Console.WriteLine("Robot : " + sMsg);//應該會是獲得"work done" 
                                    }
                                    break;
                                case "Rmovep":
                                    while (true)
                                    {
                                        p++;
                                        if (p >= fileList.Count())//代表最後一行了
                                            break;
                                        if (fileList[p].IndexOf('[') == -1)//代表是指令
                                            break;
                                        _sendMsg("Rmovep", ref cmd);//Rmovep模式

                                        sMsg = _waitRead(ref cmd); if (sMsg == "End") break;
                                        Console.WriteLine("Robot : " + sMsg);//應該會是獲得 Rmovej

                                        byte[] _pcount = new byte[1] { 1 };
                                        stream.Write(_pcount, 0, 1);//@test   1個點

                                        sMsg = _waitRead(ref cmd); if (sMsg == "End") break;
                                        Console.WriteLine("Robot : " + sMsg);//應該會是獲得"set" 也就是要開始給座標點

                                        _sendMsg("(" + fileList[p].Substring(2, fileList[p].Length - 3) + ")", ref cmd);//point

                                        sMsg = _waitRead(ref cmd); if (sMsg == "End") break;
                                        Console.WriteLine("Robot : " + sMsg);//應該會是獲得"work done" 
                                    }
                                    break;
                                case "pmovej":
                                    while (true)
                                    {
                                        p++;
                                        if (p >= fileList.Count())//代表最後一行了
                                            break;
                                        if (fileList[p].IndexOf('[') == -1)//代表是指令
                                            break;
                                        _sendMsg("pmovej", ref cmd);//record pos模式

                                        sMsg = _waitRead(ref cmd); if (sMsg == "End") break;
                                        Console.WriteLine("Robot : " + sMsg);//應該會是獲得 movep

                                        byte[] _pcount = new byte[1] { 1 };
                                        stream.Write(_pcount, 0, 1);//@test   1個點

                                        sMsg = _waitRead(ref cmd); if (sMsg == "End") break;
                                        Console.WriteLine("Robot : " + sMsg);//應該會是獲得"set" 也就是要開始給座標點

                                        _sendMsg("(" + fileList[p].Substring(2, fileList[p].Length - 3) + ")", ref cmd);//point

                                        sMsg = _waitRead(ref cmd); if (sMsg == "End") break;
                                        Console.WriteLine("Robot : " + sMsg);//應該會是獲得"work done" 
                                    }
                                    break;
                                case "gripper"://gripper 只能一行 gripper 一行 數字
                                    p++;
                                    _sendMsg("gripper", ref cmd);//gripper模式

                                    sMsg = _waitRead(ref cmd); if (sMsg == "End") break;
                                    Console.WriteLine("Robot : " + sMsg);//應該會是獲得 gripper

                                    byte[] pcount = new byte[3] { (byte)fileList[p].toInt(), 0, 150 };
                                    stream.Write(pcount, 0, 3);//pos force speed
                                                               //_sendMsg("(" + rq_pos + "," + 0 + "," + 150 + ")");

                                    sMsg = _waitRead(ref cmd); if (sMsg == "End") break;
                                    Console.WriteLine("Robot : " + sMsg);//應該會是獲得"work done" 

                                    p++;
                                    break;
                                case "sleep":
                                    p++;
                                    Thread.Sleep(fileList[p].toInt());
                                    p++;
                                    break;

                                case "test":
                                    p++;
                                    if (p >= fileList.Count())//代表最後一行了
                                        break;
                                    if (fileList[p].IndexOf('[') == -1)//代表是指令
                                        break;
                                    _sendMsg("test", ref cmd);

                                    List<string> servoCmd = new List<string>();
                                    servoCmd.Add(fileList[p]);
                                    while (true)
                                    {
                                        p++;
                                        if (p >= fileList.Count())//代表最後一行了
                                            break;
                                        if (fileList[p].IndexOf('[') == -1)//代表是指令
                                            break;
                                        servoCmd.Add(fileList[p]);
                                    }

                                    sMsg = _waitRead(ref cmd); if (sMsg == "End") break;
                                    Console.WriteLine("Robot : " + sMsg);//應該會是獲得 test

                                    for (int i = 0; i < servoCmd.Count; i++)
                                    {
                                        string str = $"( {servoCmd[i].Substring(1, servoCmd[i].Length - 2)})";
                                        _sendMsg(str, ref cmd);
                                    }
                                    _sendMsg("(0,0,-100,0,0,0)", ref cmd);

                                    sMsg = _waitRead(ref cmd); if (sMsg == "End") break;
                                    Console.WriteLine("Robot : " + sMsg);//應該會是獲得"work done" 
                                    break;
                            }
                            if (sMsg == "End") break;
                        }
                        cmd = mode.stop;
                    }

                    else if (cmd == mode.stop)
                    {
                        while (cmd == mode.stop)
                        {
                            _sendMsg("stop", ref cmd);
                            sMsg = _waitRead(ref cmd);
                            Console.WriteLine("Robot : " + sMsg);//應該會是獲得 stop
                        }
                    }
                    else if (cmd == mode.recordj)
                    {
                        txt_record.WriteLine("joint");
                        while (cmd == mode.recordj)
                        {
                            _sendMsg("recordj", ref cmd);//record joint模式

                            sMsg = _waitRead(ref cmd);
                            Console.WriteLine("Robot : " + sMsg);//應該會是獲得 robot joint
                        }
                    }
                    else if (cmd == mode.recordp)
                    {
                        txt_record = new StreamWriter("UR3_posList_p.txt", false);
                        txt_record.WriteLine("position");
                        while (cmd == mode.recordp)
                        {
                            _sendMsg("recordp", ref cmd);//record pos模式

                            sMsg = _waitRead(ref cmd);
                            Console.WriteLine("Robot : " + sMsg);//應該會是獲得 robot pos
                        }
                        txt_record.Flush();
                        txt_record.Close();
                    }
                    else if (cmd == mode.jog)
                    {
                        _sendMsg("jog", ref cmd);//record pos模式

                        sMsg = _waitRead(ref cmd);
                        Console.WriteLine("Robot : " + sMsg);//應該會是獲得 jog

                        //_sendMsg("(0,0,1,0,0,0)");//point

                        //sMsg = _waitRead(); if (sMsg == "End") break;
                        //Console.WriteLine("Robot : " + sMsg);//應該會是獲得"work done" 

                        while (cmd != mode.stop) ;
                    }
                    else if (cmd == mode.gripper)
                    {
                        _sendMsg("gripper", ref cmd);//gripper模式

                        sMsg = _waitRead(ref cmd); if (sMsg == "End") break;
                        Console.WriteLine("Robot : " + sMsg);//應該會是獲得 gripper

                        byte[] pcount = new byte[3] { rq_pos, 0, 150 };
                        stream.Write(pcount, 0, 3);//pos force speed

                        sMsg = _waitRead(ref cmd); if (sMsg == "End") break;
                        Console.WriteLine("Robot : " + sMsg);//應該會是獲得"work done" 
                        cmd = mode.stop;
                    }
                    else if (cmd == mode.grip)
                    {
                        _sendMsg("gripper", ref cmd);//gripper模式

                        sMsg = _waitRead(ref cmd); if (sMsg == "End") break;
                        Console.WriteLine("Robot : " + sMsg);//應該會是獲得 gripper

                        byte[] pcount = new byte[3] { rq_pos, 0, 150 };
                        stream.Write(pcount, 0, 3);//pos force speed

                        sMsg = _waitRead(ref cmd); if (sMsg == "End") break;
                        Console.WriteLine("Robot : " + sMsg);//應該會是獲得"work done" 
                        cmd = mode.recordj;
                    }
                    else if (cmd == mode.End)
                    {
                        break;
                    }
                }//while serverOn
                stream.Close();

                #region subfunction
                string _waitRead(ref mode c)
                {
                    string msg_read = "";
                    while (serverOn)
                    {
                        byte[] arrayBytesRequest = new byte[UR_Client.Available];
                        try
                        {
                            int nRead = stream.Read(arrayBytesRequest, 0, arrayBytesRequest.Length);//讀取UR手臂的數值
                            if (nRead > 0)//確認有收到東西後
                            {
                                msg_read = ASCIIEncoding.ASCII.GetString(arrayBytesRequest);
                                return msg_read;
                            }//read>0 //所以如果沒有收到的話就繼續再收一次
                        }
                        catch
                        {
                            c = mode.End;
                            return "End";
                        }
                    }
                    return "End";
                }
                void _sendMsg(string str, ref mode c)
                {
                    try
                    {
                        byte[] arrayBytesAnswer = ASCIIEncoding.ASCII.GetBytes(str);
                        stream.Write(arrayBytesAnswer, 0, arrayBytesAnswer.Length);
                    }
                    catch
                    {
                        //可能是client先結束了
                        Console.WriteLine("可能是client先結束了");
                        c = mode.End;
                    }
                }
                #endregion subfunction
            }
            UR_Client.Dispose();
            tcp.Stop();
            OnLinkState(new LinkArgs("disconnect"));
            serverOn = false;
        }
        public void ServerOn(string ip, int port)
        {
            if (serverOn) // is the server is on, return
            { Console.WriteLine("已經在執行了"); return; }

            IPAddress IPAddress = IPAddress.Parse(ip);
            TcpListener tcpListener = new TcpListener(IPAddress, port);
            Thread thread_server = new Thread(() => theServer(tcpListener, ref cmd));
            thread_server.Start();
        }
        public void ServerOff()
        {
            serverOn = false;
            OnLinkState(new LinkArgs("disconnect"));
        }

        public void goToPos(URCoordinates goC, bool waitDone = false)
        {
            StreamWriter txt;
            txt = new StreamWriter("Pos//GoTo.pos", false);
            txt.WriteLine("position");

            txt.WriteLine(goC.ToPos());
            txt.Flush();
            txt.Close();

            ReadingFile = "Pos//GoTo.pos";
            cmd = mode.moveByFile;
            if (waitDone)
                while (cmd == mode.stop) ;
        }

        public void goToFilePos(string file)
        {
            ReadingFile = $"Pos//{file}";
            cmd = mode.moveByFile;
        }
        public void goFilePath(string file)
        {
            ReadingFile = $"Path//{file}"; ;
            cmd = mode.moveByFile;
        }
        protected virtual void OnLinkState(LinkArgs e)
        {
            if (stateChange != null)
                stateChange(this, e);
        }
    }
    public class URCoordinates//CSYS //Coordinate Systems
    {
        public float X = 0;//meters
        public float Y = 0;//meters
        public float Z = 0;//meters
        public float Rx = 0;//rad
        public float Ry = 0;
        public float Rz = 0;
        public byte Grip = 0;
        public URCoordinates(float _x = 0, float _y = 0, float _z = 0, float _Rx = 0, float _Ry = 0, float _Rz = 0, byte _G = 0)
        {
            X = _x;
            Y = _y;
            Z = _z;
            Rx = _Rx;
            Ry = _Ry;
            Rz = _Rz;
            Grip = _G;
        }
        public URCoordinates(URCoordinates input)
        {
            X = input.X;
            Y = input.Y;
            Z = input.Z;
            Rx = input.Rx;
            Ry = input.Ry;
            Rz = input.Rz;
        }
        public string ToPos(bool withp = true)
        {
            if (withp)
                return $"p[{X},{Y},{Z},{Rx},{Ry},{Rz}]";
            return $"[{X},{Y},{Z},{Rx},{Ry},{Rz}]";
        }

    }
    public class LinkArgs : EventArgs
    {
        public LinkArgs(string s = "")
        {
            state = s;
        }
        public string state { get; set; }
    }

    static class ex
    {
        static public int toInt(this string str)
        {
            if (str == "-∞")
                return int.MinValue;
            if (str == "∞")
                return int.MaxValue;

            return int.Parse(str);
        }
    }
}
