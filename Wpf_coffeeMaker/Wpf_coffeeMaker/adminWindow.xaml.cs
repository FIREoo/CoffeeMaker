using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using myActionBase;
using UrRobot.Coordinates;


namespace Wpf_coffeeMaker
{

    public partial class adminWindow : Window
    {
        MainWindow mw = (MainWindow)Application.Current.MainWindow;
        public adminWindow()
        {
            InitializeComponent();

            List<string> itemNames = ActionList();//準備資料
            cb_action.ItemsSource = itemNames;  //資料繫結

            List<string> itemNames2 = TargetList();//準備資料
            cb_target.ItemsSource = itemNames2;  //資料繫結

            List<string> itemNames3 = DestinationList();//準備資料
            cb_destination.ItemsSource = itemNames3;  //資料繫結

            List<string> itemNames4 = TemplateList();//準備資料
            cb_simTemplate.ItemsSource = itemNames4;  //資料繫結
        }
        private List<string> ActionList()
        {
            List<string> list = new List<string>();
            foreach (var act in MainWindow.actList)
                list.Add(act.Name);
            return list;
        }
        private List<string> TargetList()
        {
            List<string> list = new List<string>();
            foreach (var obj in MainWindow.objList)
                list.Add(obj.Name);
            return list;
        }
        private List<string> DestinationList()
        {
            List<string> list = new List<string>();

            foreach (var obj in MainWindow.objList)
                list.Add(obj.Name);

            list.Add("pos");
            return list;
        }
        private List<string> TemplateList()
        {
            List<string> list = new List<string>();
            list.Add("pick");
            list.Add("place");
            list.Add("pour");
            list.Add("組1");
            list.Add("PPP");
            return list;
        }
        private void Cb_simTemplate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_simTemplate.SelectedIndex == 0)
            {
                cb_action.SelectedIndex = 1;
                cb_target.SelectedIndex = 1;
                cb_destination.SelectedIndex = 0;
            }
            else if (cb_simTemplate.SelectedIndex == 1)
            {
                {
                    cb_action.SelectedIndex = 2;
                    cb_target.SelectedIndex = 1;
                    cb_destination.SelectedIndex = 9;//pos
                }
            }
            else if (cb_simTemplate.SelectedIndex == 2)
            {
                {
                    cb_action.SelectedIndex = 3;
                    cb_target.SelectedIndex = 1;//blue cup
                    cb_destination.SelectedIndex = 2;//pink
                }
            }
            else if (cb_simTemplate.SelectedIndex == 3)
            {
                {
                    cb_action.SelectedIndex = 1;//pick
                    cb_target.SelectedIndex = 1;//blue cup
                    cb_destination.SelectedIndex = 0;
                    Btn_addSimAction_Click(null, null);


                    cb_action.SelectedIndex = 3;//pour
                    cb_target.SelectedIndex = 1;//blue cup
                    cb_destination.SelectedIndex = 2;//pink
                    Btn_addSimAction_Click(null, null);


                    cb_action.SelectedIndex = 2;//place
                    cb_target.SelectedIndex = 1;//blue cup
                    cb_destination.SelectedIndex = 9;//pos
                    tb_pos_x.Text = "-0.1"; tb_pos_y.Text = "-0.35";
                    Btn_addSimAction_Click(null, null);


                    cb_action.SelectedIndex = 1;//pick
                    cb_target.SelectedIndex = 2;//pink cup
                    cb_destination.SelectedIndex = 0;
                    Btn_addSimAction_Click(null, null);


                    cb_action.SelectedIndex = 2;//place
                    cb_target.SelectedIndex = 2;//pink cup
                    cb_destination.SelectedIndex = 9;//pos
                    tb_pos_x.Text = "-0.1"; tb_pos_y.Text = "-0.22";
                    Btn_addSimAction_Click(null, null);

                }
            }
            else if (cb_simTemplate.SelectedIndex == 4)
            {
                cb_action.SelectedIndex = 1;//pick
                cb_target.SelectedIndex = 1;//blue cup
                cb_destination.SelectedIndex = 0;
                Btn_addSimAction_Click(null, null);

                cb_action.SelectedIndex = 3;
                cb_target.SelectedIndex = 1;//blue cup
                cb_destination.SelectedIndex = 2;//pink
                Btn_addSimAction_Click(null, null);

                cb_action.SelectedIndex = 2;
                cb_target.SelectedIndex = 1;
                cb_destination.SelectedIndex = 9;//pos
                Btn_addSimAction_Click(null, null);
            }
        }
        private void Btn_addSimAction_Click(object sender, RoutedEventArgs e)
        {
            ActionLine actLine;
            if (cb_destination.SelectedIndex >= MainWindow.objList.Count)//代表選到pos
            {
                myObjects.Objects pos = new myObjects.Objects(100, "pos");
                pos.nowPos = new URCoordinates(tb_pos_x.Text.toFloat(), tb_pos_y.Text.toFloat(), tb_pos_z.Text.toFloat(), 0, 0, 0);
                actLine = new ActionLine(MainWindow.actList[cb_action.SelectedIndex], MainWindow.objList[cb_target.SelectedIndex], pos);
            }
            else
            {
                try
                {
                    actLine = new ActionLine(MainWindow.actList[cb_action.SelectedIndex], MainWindow.objList[cb_target.SelectedIndex], MainWindow.objList[cb_destination.SelectedIndex]);
                }
                catch
                {
                    MessageBox.Show("沒選??");
                    return;
                }
            }

            MainWindow.mainAction.Add(actLine);
            mw.ActionLine2ListView(actLine);
        }


        private void Button_fake_blueCup_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.objList[1].nowPos = new URCoordinates(0.mm(), -220.mm(), 200.mm());
        }
        private void Button_fake_spoon_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.objList[4].nowPos = new URCoordinates(150.mm(), -220.mm(), 200.mm());
        }
        private void Button_fake_pinkCup_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.objList[2].nowPos = new URCoordinates(80.mm(), -350.mm(), 200.mm());
        }
        private void Button_fake_redPillBox_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.objList[6].nowPos = new URCoordinates(-100.mm(), -350.mm(), 200.mm());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            val_gripOffset_cup_x.Text = MainWindow.sVal_gripOffset_cup_x.ToString();
            val_gripOffset_cup_y.Text = MainWindow.sVal_gripOffset_cup_y.ToString();
            tb_bineray_threshold.Text = MainWindow.sVal_bineray_threshold.ToString();
        }

        //settings
        private void CheckBox_thres_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.show_thres = (bool)((CheckBox)sender).IsChecked;
        }
        private void CheckBox_level_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.show_level = (bool)((CheckBox)sender).IsChecked;
        }
        private void Button_set_gripOffset(object sender, RoutedEventArgs e)
        {
            MainWindow.sVal_gripOffset_cup_x = val_gripOffset_cup_x.Text.toInt();
            MainWindow.sVal_gripOffset_cup_y = val_gripOffset_cup_y.Text.toInt();
            mw.SaveData();
        }

        private void Button_set_bineray_threshold(object sender, RoutedEventArgs e)
        {
            MainWindow.sVal_bineray_threshold = tb_bineray_threshold.Text.toInt();
            mw.SaveData();
        }

        private void Button_toRotateVector_Click(object sender, RoutedEventArgs e)
        {
            URCoordinates.Vector3 rpy = new URCoordinates.Vector3(tb_degX.Text.toInt().deg(), tb_degY.Text.toInt().deg(), tb_degZ.Text.toInt().deg());
            URCoordinates.Vector3 rotation = URCoordinates.ToRotVector(rpy);
            tb_RvX.Text = rotation.X.ToString("0.000");
            tb_RvY.Text = rotation.Y.ToString("0.000");
            tb_RvZ.Text = rotation.Z.ToString("0.000");


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //connect 30002
            ClientConnect("192.168.1.101", 30002);
        }
        private void Button_read_Click(object sender, RoutedEventArgs e)
        {

            Task.Run(() =>
            {
                while (true)
                {
                    int bufferSize = myTcpClient.ReceiveBufferSize;
                    byte[] myBufferBytes = new byte[bufferSize];
                    int dataLength = clientSocket.Receive(myBufferBytes);

                    this.Dispatcher.Invoke((Action)(() => { tb_URmsg.Text = dataLength.ToString(); }));
                    string str = "";
                    str = UrDecode(myBufferBytes);
                    //int s = 300;
                    //for (int L = 0; L < 10; L++)
                    //{
                    //    for (int i = 0; i < 10; i++)
                    //    {
                    //        int _int = myBufferBytes[L * 10 + i + s];
                    //        str += _int.ToString();
                    //        str += "\t";
                    //    }
                    //    str += "\n";
                    //}




                    /*
               
                    int j1 = 57;
                    int index = j1;
                    byte[] b = new byte[8];
                    b[7] = myBufferBytes[index];
                    b[6] = myBufferBytes[index + 1];
                    b[5] = myBufferBytes[index + 2];
                    b[4] = myBufferBytes[index + 3];
                    double rad = BitConverter.ToDouble(b, 0);
                    str += (rad*180/Math.PI).ToString("0.00000");

                    str += "\n";
                    int j2 = j1 + 41;
                    index = j2;
                     b = new byte[8];
                    b[7] = myBufferBytes[index];
                    b[6] = myBufferBytes[index + 1];
                    b[5] = myBufferBytes[index + 2];
                    b[4] = myBufferBytes[index + 3];
                     rad = BitConverter.ToDouble(b, 0);
                    str += (rad * 180 / Math.PI).ToString("0.00000");
                    str += "\n";

                    int j3 = j2 + 41;
                    index = j3;
                    b = new byte[8];
                    b[7] = myBufferBytes[index];
                    b[6] = myBufferBytes[index + 1];
                    b[5] = myBufferBytes[index + 2];
                    b[4] = myBufferBytes[index + 3];
                    rad = BitConverter.ToDouble(b, 0);
                    str += (rad * 180 / Math.PI).ToString("0.00000");
                    str += "\n";

                    int j4 = j3 + 41;
                    index = j4;
                    b = new byte[8];
                    b[7] = myBufferBytes[index];
                    b[6] = myBufferBytes[index + 1];
                    b[5] = myBufferBytes[index + 2];
                    b[4] = myBufferBytes[index + 3];
                    rad = BitConverter.ToDouble(b, 0);
                    str += (rad * 180 / Math.PI).ToString("0.00000");
                    str += "\n";

                    int j5 = j4 + 41;
                    index = j5;
                    b = new byte[8];
                    b[7] = myBufferBytes[index];
                    b[6] = myBufferBytes[index + 1];
                    b[5] = myBufferBytes[index + 2];
                    b[4] = myBufferBytes[index + 3];
                    rad = BitConverter.ToDouble(b, 0);
                    str += (rad * 180 / Math.PI).ToString("0.00000");
                    str += "\n";

                    int j6 = j5 + 41;
                    index = j6;
                    b = new byte[8];
                    b[7] = myBufferBytes[index];
                    b[6] = myBufferBytes[index + 1];
                    b[5] = myBufferBytes[index + 2];
                    b[4] = myBufferBytes[index + 3];
                    rad = BitConverter.ToDouble(b, 0);
                    str += (rad * 180 / Math.PI).ToString("0.00000");
                    */
                    this.Dispatcher.Invoke((Action)(() => { text_ur.Text = str; }));
                }
            });



        }

        private string UrDecode(byte[] buffer)
        {
            string rtn = "";
            int index = 308;
            for (int j = 0; j < 6; j++)
            {
                byte[] b = new byte[8];
                for (int i = 0; i < 8; i++)
                {
                    b[7-i] = buffer[j * 8 + i + index];
                }
                    double v = BitConverter.ToDouble(b, 0);
                rtn += v.ToString("0.0000") + "\n";
            }
            return  rtn;
        }

        TcpClient myTcpClient;
        Socket clientSocket;
        private bool isConect = false;
        public void ClientConnect(string IP, int port)
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



            }
            catch
            {
                Console.WriteLine
                           ("主機 {0} 通訊埠 {1} 無法連接  !!", hostName, connectPort);
                return;
            }
        }

    }



}
