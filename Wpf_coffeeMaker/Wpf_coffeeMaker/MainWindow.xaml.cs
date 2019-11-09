using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
//using System.Windows.Media;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Intel.RealSense;
using Size = System.Drawing.Size;
using PointF = System.Drawing.PointF;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using Image = System.Windows.Controls.Image;
using Stream = Intel.RealSense.Stream;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;
using Alturos.Yolo;
using Alturos.Yolo.Model;
using myObjects;
using myEmguLibrary;
using UrRobot.Socket;
using UrRobot.Coordinates;
using System.IO;
using myActionBase;
using myTCP;
using Emgu.CV.Util;
using Picking;

namespace Wpf_coffeeMaker
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {

        private imgType showType = 0;
        enum imgType
        {
            none = 0,
            mix = 1,
            color_full = 2,
            depth = 3,
            color_resize = 4
        }
        public static List<ActionLine> mainAction = new List<ActionLine>();

        public static int timeTick = 0;

        #region //---UI---//
        private void setConnectCircle(int S)
        {
            cir_UrState_off.Fill = new SolidColorBrush(Colors.LightGray);
            cir_UrState_connecting.Fill = new SolidColorBrush(Colors.LightGray);
            cir_UrState_on.Fill = new SolidColorBrush(Colors.LightGray);
            if (S == 1)
            {
                cir_UrState_off.Fill = new SolidColorBrush(Color.FromArgb(255, 230, 50, 50));
            }
            else if (S == 2)
            {
                cir_UrState_connecting.Fill = new SolidColorBrush(Color.FromArgb(255, 220, 220, 20));
            }
            else if (S == 3)
            {
                cir_UrState_on.Fill = new SolidColorBrush(Color.FromArgb(255, 40, 210, 40));
            }
        }
        bool setColor = true;
        private void setActiionCircle(string str)
        {

        }

        private void setRecordRect(int S)
        {
            rect_record.Fill = new SolidColorBrush(Color.FromArgb(20, 200, 50, 50));
            rect_endRecord.Fill = new SolidColorBrush(Color.FromArgb(20, 193, 193, 80));
            if (S == 0)
            {
            }
            else if (S == 1)
            {
                rect_record.Fill = new SolidColorBrush(Color.FromArgb(100, 200, 50, 50));
            }
            else if (S == 2)
            {
                rect_endRecord.Fill = new SolidColorBrush(Color.FromArgb(100, 193, 193, 80));
            }
        }
        #endregion //---UI---//

        #region //---public value---//
        //public static Point sVal_gripOffset_cup = new Point(-13, -16);
        public static int sVal_gripOffset_cup_x = -20;
        public static int sVal_gripOffset_cup_y = -47;
        //public static Point sVal_alignment_offset = new Point(0, -80);
        public static int sVal_alignment_offset_x = 0;
        public static int sVal_alignment_offset_y = -80;
        public static int sVal_gripCenter_x = 667;
        public static int sVal_gripCenter_y = 485;

        public static float sVal_levelX = 0.015f;
        public static float sVal_levelY = -0.01f;

        public static bool show_thres = true;
        public static bool show_level = false;

        public static int sVal_bineray_threshold = 26;

        public void SaveData()
        {
            StreamWriter txt = new StreamWriter("data.txt", false);

            txt.WriteLine($"int\t{nameof(sVal_gripOffset_cup_x)}\t{sVal_gripOffset_cup_x.ToString()}");
            txt.WriteLine($"int\t{nameof(sVal_gripOffset_cup_y)}\t{sVal_gripOffset_cup_y.ToString()}");
            txt.WriteLine($"int\t{nameof(sVal_alignment_offset_x)}\t{sVal_alignment_offset_x.ToString()}");
            txt.WriteLine($"int\t{nameof(sVal_alignment_offset_y)}\t{sVal_alignment_offset_y.ToString()}");
            txt.WriteLine($"float\t{nameof(sVal_levelX)}\t{sVal_levelX.ToString()}");
            txt.WriteLine($"float\t{nameof(sVal_levelY)}\t{sVal_levelY.ToString()}");
            txt.WriteLine($"int\t{nameof(sVal_gripCenter_x)}\t{sVal_gripCenter_x.ToString()}");
            txt.WriteLine($"int\t{nameof(sVal_gripCenter_y)}\t{sVal_gripCenter_y.ToString()}");
            txt.WriteLine($"int\t{nameof(sVal_bineray_threshold)}\t{sVal_bineray_threshold.ToString()}");


            txt.Flush();
            txt.Close();

        }

        public void ReadData()
        {
            foreach (string varible in File.ReadAllLines("data.txt"))
            {

                string type = varible.Split('\t')[0];
                if (type == "float")
                {
                    this.GetType().GetField(varible.Split('\t')[1]).SetValue(this, varible.Split('\t')[2].toFloat());
                }
                else if (type == "bool")
                {
                    this.GetType().GetField(varible.Split('\t')[1]).SetValue(this, varible.Split('\t')[2].toBool());
                }
                else if (type == "int")
                {
                    this.GetType().GetField(varible.Split('\t')[1]).SetValue(this, varible.Split('\t')[2].toInt());
                }
                else if (type == "dobule")
                {
                    this.GetType().GetField(varible.Split('\t')[1]).SetValue(this, varible.Split('\t')[2].toDouble());
                }
            }
        }
        #endregion ---public value---

        public MainWindow()
        {
            InitializeComponent();
            ReadData();
            SaveData();//if theres new var , will add here. next time oppend app will update
            Size mappingColor = new Size(900, 500);
            PointF[] src = new[] {
                    new PointF(0,0),
                    new PointF(0,720),
                    new PointF(1280,720),
                    new PointF(1280,0) };

            PointF[] dst = new[] {
                    new PointF(178,112),
                    new PointF(176,614),
                    new PointF(1062,608),
                    new PointF(1058,105) };

            creatObject();
            creatAction();
            UR.stateChange += OnUrStateChange;
            UR.dynamicGrip = new UrSocketControl.ControlFunction(goDynamicGrip);

        }

        public static List<Objects> objList = new List<Objects>();
        private void creatObject()
        {
            objList.Add(new Objects(0, "none"));
            objList.Add(new Objects(1, "Blue cup"));
            objList.Add(new Objects(2, "Pink cup"));
            objList.Add(new Objects(3, "Pot"));
            objList.Add(new Objects(4, "Spoon"));
            objList.Add(new Objects(5, "Powder box"));
            objList.Add(new Objects(6, "Red pill box"));
            objList.Add(new Objects(7, "Green pill box"));
            objList.Add(new Objects(8, "Blue pill box"));
        }

        public static List<myAction> actList = new List<myAction>();
        private void creatAction()
        {
            actList.Add(new myAction(0, "none"));
            actList.Add(myActionAdder.Pick());
            actList.Add(myActionAdder.Place());
            actList.Add(myActionAdder.Pour());
            actList.Add(myActionAdder.Scoop());
            actList.Add(myActionAdder.AddIn());
            actList.Add(myActionAdder.Stir());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //Setup / start camera
                //CameraStart();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Shutdown();
            }
            Rect_actionBaseTopColor.Fill = new SolidColorBrush(Colors.Transparent);
            setRecordRect(0);
            Cb_rotateDesk_Click(cb_rotateDesk, null);
        }


        #region //---griping mode---//


        #endregion ---griping mode---
        private void Btn_adminWindow_Click(object sender, RoutedEventArgs e)
        {
            adminWindow adminWindow = new adminWindow();
            adminWindow.Show();
        }
        //UR
        #region  //---UR server---//
        static UrSocketControl UR = new UrSocketControl();
        private void Button_startServer_Click(object sender, RoutedEventArgs e)
        {
            UR.startServer("auto", 888);
        }
        private void Button_disconnect_Click(object sender, RoutedEventArgs e)
        {
            UR.stopServer();
        }
        private void OnUrStateChange(tcpState S)
        {
            try
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    if (S == tcpState.Connect)
                        setConnectCircle(3);
                    else if (S == tcpState.Disconnect)
                        setConnectCircle(1);
                    else if (S == tcpState.waitAccept)
                        setConnectCircle(2);
                    else if (S == tcpState.startListener)
                        setConnectCircle(2);
                    else
                        setConnectCircle(1);
                }));
            }
            catch
            {
                Console.WriteLine("server thread already end");
            }



        }

        //client 
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)sender).IsChecked == true)
                UR.creatClient("192.168.1.104");
            else
                UR.closeClient();
        }
        #endregion ---UR server---

        #region //---Record---//
        private void Button_recordMode_Click(object sender, RoutedEventArgs e)
        {
            // UR.Record_start(Tb_recordName.Text);
            UR.startRecord(Tb_recordName.Text + ".path");
            setRecordRect(1);
        }

        private void Button_recordWrite_Click(object sender, RoutedEventArgs e)
        {
            // UR.Record_writePos();
            UR.Record_joint();
        }
        private void Button_recordEnd_Click(object sender, RoutedEventArgs e)
        {
            UR.endRecord();
            // UR.Record_stop();
            setRecordRect(2);
        }

        private void Button_grip_Click(object sender, RoutedEventArgs e)
        {
            UR.Record_grip(int.Parse(Tb_gripVal.Text), true);
            // UR.goGrip((byte)int.Parse(Tb_gripVal.Text));
        }
        private void Button_grip2_Click(object sender, RoutedEventArgs e)
        {
            UR.Record_grip(int.Parse(Tb_gripVal2.Text), true);
            //UR.goGrip((byte)int.Parse(Tb_gripVal2.Text));
        }
        private void Button_grip3_Click(object sender, RoutedEventArgs e)
        {
            // UR.goGrip((byte)int.Parse(Tb_gripVal3.Text));
            UR.Record_grip(int.Parse(Tb_gripVal3.Text), true);
        }
        private void Button_recordSleep_Click(object sender, RoutedEventArgs e)
        {
            UR.Record_sleep(Tb_sleepVal.Text.toInt());
        }
        #endregion ---Record---

        #region //---Play path---//
        private void Button_goPosHome_Click(object sender, RoutedEventArgs e)
        {
            UR.creatClient("192.168.1.102");
            UR.client_SendData("movej([1.0322,-2.00041,1.71881,-1.29203,-1.56557,2.60226])");
            UR.closeClient();

        }
        private void Cb_Path_DropDownOpened(object sender, EventArgs e)
        {
            Cb_Path.Items.Clear();
            DirectoryInfo PathFolder = new DirectoryInfo(UR.rootPath);
            FileInfo[] ActFiles = PathFolder.GetFiles("*.path");
            foreach (FileInfo file in ActFiles)
            {
                Cb_Path.Items.Add(file.Name);
            }
        }
        private void Btn_goPath_Click(object sender, RoutedEventArgs e)
        {
            string name = "";
            try
            {
                name = Cb_Path.SelectedValue.ToString();
                Task.Run(() =>
            {
                UR.goFile(UR.rootPath + name);
            });
            }
            catch
            {
                MessageBox.Show("沒選擇");
            }


        }
        #endregion ---Play path---

        //action base
        #region //--- Action base Control---//
        static bool startDemo = false;
        private void Button_startRecord_Click(object sender, RoutedEventArgs e)
        {
            LV_actionBase.Items.Clear();
            mainAction.Clear();
            Rect_actionBaseTopColor.Fill = new SolidColorBrush(Color.FromArgb(200, 40, 210, 80));
            startDemo = true;
        }

        private void Button_endDemo_Click(object sender, RoutedEventArgs e)
        {
            if (startDemo == false)
            {
                MessageBox.Show("已經結束了");
            }
            Rect_actionBaseTopColor.Fill = new SolidColorBrush(Color.FromArgb(200, 210, 80, 40));
            startDemo = false;
        }
        private void Button_createAction_Click(object sender, RoutedEventArgs e)
        {
            if (startDemo == true) //need startDemo == false
            {
                MessageBox.Show("尚未結束示範，請按下[End Demo]按鈕");
                return;
            }
            if (RS_Task ==null)
            {
                MessageBox.Show("Real Sense 沒開");
                return;
            }

            string fileName = "Reproduction";

            //必須放新的物件座標!! 才能計算
            for (int i = 0; i < mainAction.Count(); i++)
            {
                if (mainAction[i].target.Name != "pos")//如果不是pos才要更新 //基本上不會發生
                    mainAction[i].target.nowPos = objList[mainAction[i].target.index].nowPos;
                if (mainAction[i].destination.Name != "pos")//如果不是pos才要更新 //也就是像place到某個點
                    mainAction[i].destination.nowPos = objList[mainAction[i].destination.index].nowPos;
            }
            ActionBase2Cmd(mainAction, fileName);

            //執行
            if (UR.isServerRunning == false)
            {
                MessageBox.Show("UR手臂未連結，但已經規劃好路徑");
                return;
            }
            UR.goFile(fileName);
        }
        public void ActionLine2ListView(ActionLine al)
        {
            ActionBaseAdder ab;
            if (al.destination.Name == "pos")
                ab = new ActionBaseAdder(al.Action.Name, al.target.Name, al.destination.nowPos.ToString("(3)"), new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Black));
            else
                ab = new ActionBaseAdder(al.Action.Name, al.target.Name, al.destination.Name, new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Black));
            this.LV_actionBase.Items.Add(ab);
            Console.WriteLine($"{ al.Action.Name}");

            Action action = delegate { };
            Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Input, action);
        }

        public void ActionBase2Cmd(List<ActionLine> actLine, string fileName)
        {
            if (fileName.IndexOf(".path") < 0)
                fileName += ".path";
            StreamWriter txt;
            txt = new StreamWriter($"Path//{fileName}", false);
            foreach (ActionLine al in actLine)
            {
                var tmp = al.getCmdText();
                foreach (string str in tmp)
                    txt.WriteLine(str);
            }

            txt.Flush();
            txt.Close();
        }

        #endregion --- Action base Control---


        #region //---Connect Python Action recognition---//
        ezTCP TCP = new ezTCP();
        private void Button_askActionRecognition_Click(object sender, RoutedEventArgs e)
        {

            Task.Run(() =>
            {
                setActiionCircle("");
                if (!TCP.creatClient("192.168.1.107", 777))
                    return;
                this.Dispatcher.Invoke((Action)(() => { cir_connectAct.Fill = new SolidColorBrush(Colors.DarkSeaGreen); }));
            });

        }

        private void Button_startAR_Click(object sender, RoutedEventArgs e)
        {
            TCP.client_SendData("start_demo");
            string msg = TCP.client_ReadData();
            Console.WriteLine(msg);
        }

        private void Button_endAR_Click(object sender, RoutedEventArgs e)
        {
            TCP.client_SendData("end_demo");
            string msg = TCP.client_ReadData();
            //"71,1,1,-0.00867,-0.21019,0,-0.001,-0.001;83,3,1,0.06899,-0.24355,3,-0.13042,-0.03562;96,2,1,-0.07257,-0.41176,0,-0.001,-0.001"
            Console.WriteLine(msg);
            //string msg = "35,1,2,0.0079,-0.19952,0,-0.001,-0.001;48,3,2,0.09142,-0.24673,1,0.08751,-0.35133;59,2,2,-0.0767,-0.41837,0,-0.001,-0.001;68,1,1,0.07609,-0.34578,0,-0.001,-0.001;77,3,1,-0.08773,-0.32993,2,-0.07449,-0.42459;89,2,1,0.0015,-0.19447,0,-0.001,-0.001;91,1,8,0.17031,-0.2567,0,-0.001,-0.001;92,2,8,0.16127,-0.24134,0,-0.001,-0.001;94,1,8,0.16524,-0.24719,0,-0.001,-0.001;111,6,8,0.02053,-0.18653,3,0.04068,-0.12663;134,2,8,0.15161,-0.30327,0,-0.001,-0.001";
            //"35,1,2,0.0079,-0.19952,0,-0.001,-0.001;48,3,2,0.09142,-0.24673,1,0.08751,-0.35133;59,2,2,-0.0767,-0.41837,0,-0.001,-0.001;68,1,1,0.07609,-0.34578,0,-0.001,-0.001;77,3,1,-0.08773,-0.32993,2,-0.07449,-0.42459;89,2,1,0.0015,-0.19447,0,-0.001,-0.001;91,1,8,0.17031,-0.2567,0,-0.001,-0.001;92,2,8,0.16127,-0.24134,0,-0.001,-0.001;94,1,8,0.16524,-0.24719,0,-0.001,-0.001;111,6,8,0.02053,-0.18653,3,0.04068,-0.12663;134,2,8,0.15161,-0.30327,0,-0.001,-0.001"
            string[] oneAction = msg.Split(';');
            for (int i = 0; i < oneAction.Length; i++)
            {
                string[] actInfo = oneAction[i].Split(',');
                myAction act = new myAction(actInfo[1].toInt(), actList[actInfo[1].toInt()].Name);

                Objects tar = new Objects(actInfo[2].toInt(), objList[actInfo[2].toInt()].Name);
                Objects des;
                if (actList[actInfo[1].toInt()].Name == ActionName.Place)
                {
                    //如果是place，就放入target的位置
                    des = new Objects(9, "pos");
                    Unit x = actInfo[3].toFloat().M();
                    Unit y = actInfo[4].toFloat().M();

                    des.nowPos = new URCoordinates(x, y, 0.M());
                }
                else
                {
                    des = new Objects(actInfo[5].toInt(), objList[actInfo[5].toInt()].Name);
                }
                ActionLine al = new ActionLine(act, tar, des);
                mainAction.Add(al);
                ActionLine2ListView(al);
            }
        }

        private void Button_getAR_Click(object sender, RoutedEventArgs e)
        {
            TCP.client_SendData("get_obj_pos");
            string msg = TCP.client_ReadData();
            //"5,-1,-1;,3,-189.1027119345294,-540.4202861686027;,3,-1,-1;,2,143.33660559739894,-279.61526760880236;,1,82.4412088908121,-463.4023999918194;,8,-1,-1;,6,-1,-1;,7,-1,-1;,4,-1,-1;,1001,-1,-1;,1010,-1,-1;,11,-1,-1;"
            Console.WriteLine(msg);

        }

        #endregion ---Connect Python Action recognition---


        #region //---RealSense---//
        private void Button_startCamera_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CameraStart();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void CameraStart()
        {
            // Setup config settings
            var cfg = new Config();
            cfg.EnableStream(Stream.Depth, 1280, 720, Format.Z16, 30);
            cfg.EnableStream(Stream.Color, 1280, 720, Format.Rgb8, 30);

            // Pipeline start
            Pipeline pipeline = new Pipeline();
            PipelineProfile pp = pipeline.Start(cfg);

            using (var p = pp.GetStream(Stream.Color) as VideoStreamProfile)
                Img_main.Source = new WriteableBitmap(p.Width, p.Height, 96d, 96d, System.Windows.Media.PixelFormats.Rgb24, null);

            Action<VideoFrame> updateColor = UpdateImage(Img_main);

            // Setup filter / alignment settings
            SetupFilters(out Colorizer colorizer, out DecimationFilter decimate, out SpatialFilter spatial, out TemporalFilter temp, out HoleFillingFilter holeFill, out Align align_to);
            // Setup frame processing
            CustomProcessingBlock processingBlock = SetupProcessingBlock(pipeline, colorizer, decimate, spatial, temp, holeFill, align_to);
            // Start frame processing
            StartProcessingBlock(processingBlock, pp, updateColor, pipeline);
        }
        private void SetupFilters(out Colorizer colorizer, out DecimationFilter decimate, out SpatialFilter spatial, out TemporalFilter temp, out HoleFillingFilter holeFill, out Align align_to)
        {
            // Colorizer is used to visualize depth data
            colorizer = new Colorizer();

            // Decimation filter reduces the amount of data (while preserving best samples)
            decimate = new DecimationFilter();
            decimate.Options[Option.FilterMagnitude].Value = 1.0F;

            // Define spatial filter (edge-preserving)
            spatial = new SpatialFilter();
            // Enable hole-filling
            // Hole filling is an agressive heuristic and it gets the depth wrong many times
            // However, this demo is not built to handle holes
            // (the shortest-path will always prefer to "cut" through the holes since they have zero 3D distance)
            spatial.Options[Option.HolesFill].Value = 5.0F;
            spatial.Options[Option.FilterMagnitude].Value = 5.0F;
            spatial.Options[Option.FilterSmoothAlpha].Value = 1.0F;
            spatial.Options[Option.FilterSmoothDelta].Value = 50.0F;

            // Define temporal filter
            temp = new TemporalFilter();

            // Define holefill filter
            holeFill = new HoleFillingFilter();

            // Aline color to depth
            align_to = new Align(Stream.Depth);
        }
        private CustomProcessingBlock SetupProcessingBlock(Pipeline pipeline, Colorizer colorizer, DecimationFilter decimate, SpatialFilter spatial, TemporalFilter temp, HoleFillingFilter holeFill, Align align_to)
        {
            CustomProcessingBlock processingBlock = null;
            //if (showType == imgType.mix)
            //{
            // Setup / start frame processing
            processingBlock = new CustomProcessingBlock((f, src) =>
            {
                using (var releaser = new FramesReleaser())
                {
                    using (var frames = pipeline.WaitForFrames().DisposeWith(releaser))
                    {
                        var processedFrames = frames
                        .ApplyFilter(align_to).DisposeWith(releaser)
                        .ApplyFilter(decimate).DisposeWith(releaser)
                        .ApplyFilter(spatial).DisposeWith(releaser)
                        .ApplyFilter(temp).DisposeWith(releaser)
                        .ApplyFilter(holeFill).DisposeWith(releaser)
                        .ApplyFilter(colorizer).DisposeWith(releaser);

                        // Send it to the next processing stage
                        src.FramesReady(processedFrames);
                    }
                }
            });
            // }

            return processingBlock;
        }

        float[,,] posMap = new float[1280, 720, 3];//整張距離圖
        int[,,] valueMap = new int[1280, 720, 3];
        int mapPivit = 0;

        //偵測到的物件資訊
        PointF center_obj = new PointF();
        SizeF size_obj = new Size();
        float angel_obj = 0;
        float armMoveX;
        float armMoveY;

        private void StartProcessingBlock(CustomProcessingBlock processingBlock, PipelineProfile pp, Action<VideoFrame> updateColor, Pipeline pipeline)
        {
            showType = imgType.depth;

            if (showType == imgType.color_full)
                Process_color(processingBlock, pp, updateColor, pipeline);
            else if (showType == imgType.mix)
                Process_mix(processingBlock, pp, updateColor, pipeline);
            else if (showType == imgType.depth)
                Process_depth(processingBlock, pp, updateColor, pipeline);
        }

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();//token 可用來關閉task factory
        private Task RS_Task = null;
        private void Process_color(CustomProcessingBlock processingBlock, PipelineProfile pp, Action<VideoFrame> updateColor, Pipeline pipeline)
        {
            processingBlock.Start(f =>
            {
                using (var frames = FrameSet.FromFrame(f))
                {
                    //var color_frame = frames.ColorFrame.DisposeWith(frames);
                    //Dispatcher.Invoke(DispatcherPriority.Render, updateColor, color_frame);

                    var depthintr = (pp.GetStream(Stream.Depth) as VideoStreamProfile).GetIntrinsics();//這好像是取得什麼參數的，用來傳換成實際深度
                    var depth_frame = frames.DepthFrame.DisposeWith(frames);//若 depth_frame不使用 FrameSet後的frame，那數值會少許多(可能是沒有filter吧)

                    //製作 整張的距離圖
                    unsafe
                    {
                        Int16* pixelPtr_byte = (Int16*)depth_frame.Data;
                        for (int i = 0; i < 1280; i++)
                            for (int j = 0; j < 720; j++)
                            {
                                var tmpF = HelperClass.DeprojectPixelToPoint(depthintr, new PointF(i, j), (float)pixelPtr_byte[j * 1280 + i] / 1000f);
                                posMap[i, j, 0] = tmpF[0];
                                posMap[i, j, 1] = tmpF[1];
                                posMap[i, j, 2] = tmpF[2];
                            }
                    }
                }
            });

            var token = _tokenSource.Token;
            RS_Task = Task.Factory.StartNew(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    using (var frames = pipeline.WaitForFrames())
                    {
                        //這裡的color_frame沒有經過 processing block不是FrameSet後的成果，所以顯示的是原本的影像
                        VideoFrame color_frame = frames.ColorFrame.DisposeWith(frames);
                        Dispatcher.Invoke(DispatcherPriority.Render, updateColor, color_frame);//顯示 updateColor感覺是個指標，把color_frame放入updateColor

                        processingBlock.ProcessFrames(frames);

                    }
                }
            }, token);

        }
        private void Process_mix(CustomProcessingBlock processingBlock, PipelineProfile pp, Action<VideoFrame> updateColor, Pipeline pipeline)
        {
            //Mix Processing
            processingBlock.Start(f =>
            {
                using (var frames = FrameSet.FromFrame(f))
                {
                    var mix_frame = frames.ColorFrame.DisposeWith(frames);
                    Dispatcher.Invoke(DispatcherPriority.Render, updateColor, mix_frame);

                    var depthintr = (pp.GetStream(Stream.Depth) as VideoStreamProfile).GetIntrinsics();//這好像是取得什麼參數的，用來傳換成實際深度
                    var depth_frame = frames.DepthFrame.DisposeWith(frames);//若 depth_frame不使用 FrameSet後的frame，那數值會少許多(可能是沒有filter吧)

                    //製作 整張的距離圖
                    unsafe
                    {
                        Int16* pixelPtr_byte = (Int16*)depth_frame.Data;
                        for (int i = 0; i < 1280; i++)
                            for (int j = 0; j < 720; j++)
                            {
                                var tmpF = HelperClass.DeprojectPixelToPoint(depthintr, new PointF(i, j), (float)pixelPtr_byte[j * 1280 + i] / 1000f);
                                posMap[i, j, 0] = tmpF[0];
                                posMap[i, j, 1] = tmpF[1];
                                posMap[i, j, 2] = tmpF[2];
                            }
                    }

                }
            });


            var token = _tokenSource.Token;
            var t = Task.Factory.StartNew(() =>//執行續  ， 裡面執行 processing block
            {
                while (!token.IsCancellationRequested)
                {
                    using (var frames = pipeline.WaitForFrames())
                    {
                        // Invoke custom processing block
                        processingBlock.ProcessFrames(frames);
                    }
                }
            }, token);
        }
        private void Process_depth(CustomProcessingBlock processingBlock, PipelineProfile pp, Action<VideoFrame> updateColor, Pipeline pipeline)
        {
            Size RS_depthSize = new Size(1280, 720);
            Mat img_depth = new Mat(RS_depthSize, DepthType.Cv8U, 1);
            processingBlock.Start(f =>
            {
                using (var frames = FrameSet.FromFrame(f))
                {
                    var depthintr = (pp.GetStream(Stream.Depth) as VideoStreamProfile).GetIntrinsics();//這好像是取得什麼參數的，用來傳換成實際深度
                    var depth_frame = frames.DepthFrame.DisposeWith(frames);//若 depth_frame不使用 FrameSet後的frame，那數值會少許多(可能是沒有filter吧)

                    //製作 整張的距離圖
                    unsafe
                    {
                        Int16* pixelPtr_byte = (Int16*)depth_frame.Data;
                        for (int i = 0; i < 1280; i++)
                            for (int j = 0; j < 720; j++)
                            {
                                var tmpF = HelperClass.DeprojectPixelToPoint(depthintr, new PointF(i, j), (float)pixelPtr_byte[j * 1280 + i] / 1000f);
                                posMap[i, j, 0] = tmpF[0];
                                posMap[i, j, 1] = tmpF[1];
                                posMap[i, j, 2] = tmpF[2];
                            }
                    }

                }
            });

            var token = _tokenSource.Token;
            RS_Task = Task.Factory.StartNew(() =>//執行續  ， 裡面執行 processing block
            {
                while (!token.IsCancellationRequested)
                {
                    using (var frames = pipeline.WaitForFrames())
                    {
                        // processingBlock.ProcessFrames(frames);//有filter (效果是不是比較差啊...

                        var depthintr = (pp.GetStream(Stream.Depth) as VideoStreamProfile).GetIntrinsics();//這好像是取得什麼參數的，用來傳換成實際深度
                        var depth_frame = frames.DepthFrame.DisposeWith(frames);//若 depth_frame不使用 FrameSet後的frame，那數值會少許多(可能是沒有filter吧)

                        //製作 整張的距離圖
                        unsafe
                        {
                            Int16* pixelPtr_byte = (Int16*)depth_frame.Data;
                            for (int i = 0; i < 1280; i++)
                                for (int j = 0; j < 720; j++)
                                {
                                    var tmpF = HelperClass.DeprojectPixelToPoint(depthintr, new PointF(i, j), (float)pixelPtr_byte[j * 1280 + i] / 1000f);
                                    posMap[i, j, 0] = tmpF[0];
                                    posMap[i, j, 1] = tmpF[1];
                                    posMap[i, j, 2] = tmpF[2];
                                }
                        }

                        //value
                        int thresVal = 128;
                        unsafe
                        {
                            byte* pixelPtr_byte = (byte*)img_depth.DataPointer;
                            for (int i = 0; i < 1280; i++)//x
                                for (int j = 0; j < 720; j++)//y
                                {

                                    int value = (int)(posMap[i, j, 2] * 1000);
                                    //(x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min
                                    int in_min = 200;
                                    int in_max = 370;
                                    value = (value - in_min) * (255 - 0) / (in_max - in_min) + 0;
                                    if (show_level == false)//如果要顯示就不要校正了
                                    {
                                        //水平校正
                                        value += (int)(j * sVal_levelX);//depth img 下面多 要增加
                                        value += (int)(i * sVal_levelY);//depth img 右邊多 要增加
                                    }

                                    value = 255 - value;//反向 讓越高越白
                                    if (value > 255) value = 255;
                                    if (value < 0) value = 0;

                                    if (value == 255)
                                        value = 0;//去除一些 原本測不到的(太近的，黑色)

                                    if (show_thres)
                                    {
                                        if (value > sVal_bineray_threshold)
                                            value = thresVal;
                                        if (value < sVal_bineray_threshold)
                                            value = 0;
                                    }

                                    //filter
                                    valueMap[i, j, mapPivit] = value;
                                    mapPivit++;
                                    if (mapPivit >= valueMap.GetLength(2)) mapPivit = 0;

                                    float outValue = 0;
                                    int c = 0;
                                    for (int m = 0; m < valueMap.GetLength(2); m++)
                                        if (valueMap[i, j, m] != 0)
                                        {
                                            outValue += valueMap[i, j, m];
                                            c++;
                                        }
                                    if (c == 0) outValue = 0;
                                    else outValue /= c;

                                    pixelPtr_byte[j * 1280 + i] = (byte)((int)outValue);
                                }
                        }

                        //影像處理開始--抓物件
                        //{X = 666.838562 Y = 484.8097}
                        Point grip_center = new Point(sVal_gripCenter_x, sVal_gripCenter_y);//夾爪中心，如果我找到..則旋轉會不影響中心點

                        Mat mat_img_show = new Mat(RS_depthSize, DepthType.Cv8U, 3);
                        Mat img_depth_ch3 = new Mat(RS_depthSize, DepthType.Cv8U, 3);
                        CvInvoke.CvtColor(img_depth, img_depth_ch3, ColorConversion.Gray2Bgr);
                        CvInvoke.CvtColor(img_depth, mat_img_show, ColorConversion.Gray2Bgr);

                        //先一次 膨脹侵蝕 讓東西閉合(例如杯子 破碎的東西)
                        Mat element = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new Size(7, 7), new Point(-1, -1));
                        CvInvoke.Dilate(mat_img_show, mat_img_show, element, new Point(-1, -1), 4, BorderType.Default, new MCvScalar(0, 0, 0));
                        CvInvoke.Erode(mat_img_show, mat_img_show, element, new Point(-1, -1), 2, BorderType.Default, new MCvScalar(0, 0, 0));

                        //畫線條 有碰到的地方就會被填滿，不然只會有一點，離開那一點就找不到物件了
                        int size = 90;
                        CvInvoke.Line(mat_img_show, new Point(grip_center.X + sVal_alignment_offset_x + size, grip_center.Y + sVal_alignment_offset_y), new Point(grip_center.X + sVal_alignment_offset_x - size, grip_center.Y + sVal_alignment_offset_y), new MCvScalar(thresVal, thresVal, thresVal));
                        CvInvoke.Line(mat_img_show, new Point(grip_center.X + sVal_alignment_offset_x, grip_center.Y + sVal_alignment_offset_y + size), new Point(grip_center.X + sVal_alignment_offset_x, grip_center.Y + sVal_alignment_offset_y - size), new MCvScalar(thresVal, thresVal, thresVal));
                        CvInvoke.Rectangle(mat_img_show, new Rectangle(grip_center.X + sVal_alignment_offset_x - size, grip_center.Y + sVal_alignment_offset_y - size, size * 2, size * 2), new MCvScalar(thresVal, thresVal, thresVal));

                        //fill 找到的物件
                        MCvScalar fillColor = new MCvScalar(90, 110, 177);
                        Mat filling_mask = new Mat(RS_depthSize.Height + 2, RS_depthSize.Width + 2, DepthType.Cv8U, 1);
                        filling_mask.SetTo(new MCvScalar(0, 0, 0));
                        CvInvoke.FloodFill(mat_img_show, filling_mask, new Point(grip_center.X + sVal_alignment_offset_x, grip_center.Y + sVal_alignment_offset_y), fillColor, out Rectangle rect, new MCvScalar(1, 1, 1), new MCvScalar(1, 1, 1));

                        //去掉中間畫的框框
                        element = CvInvoke.GetStructuringElement(ElementShape.Cross, new Size(3, 3), new Point(-1, -1));
                        CvInvoke.Erode(mat_img_show, mat_img_show, element, new Point(-1, -1), 2, BorderType.Default, new MCvScalar(0, 0, 0));
                        CvInvoke.Dilate(mat_img_show, mat_img_show, element, new Point(-1, -1), 4, BorderType.Default, new MCvScalar(0, 0, 0));

                        //找物件mask
                        Mat mat_img_object_mask = new Mat(RS_depthSize, DepthType.Cv8U, 1);
                        CvInvoke.InRange(mat_img_show, new ScalarArray(fillColor), new ScalarArray(fillColor), mat_img_object_mask);


                        //畫最小矩形
                        using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                        {
                            CvInvoke.FindContours(mat_img_object_mask, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                            for (int i = 0; i < contours.Size; i++)
                            {
                                using (VectorOfPoint contour = contours[i])
                                {
                                    if (contour.Size < 50)
                                        continue;

                                    RotatedRect BoundingBox = CvInvoke.MinAreaRect(contour);
                                    angel_obj = BoundingBox.Angle + 4;//校正度數
                                    center_obj = BoundingBox.Center;
                                    size_obj = BoundingBox.Size;

                                    //要調整角度
                                    if (size_obj.Width > size_obj.Height)
                                        angel_obj = angel_obj + 90;

                                    if (size_obj.Height.IsBetween(150, 200) && size_obj.Width.IsBetween(150, 200))
                                    {
                                        var a = grip_center.X - center_obj.X;
                                        var b = grip_center.Y - center_obj.Y;
                                        center_obj.Y += sVal_gripOffset_cup_x;//辨識到cup就要位移 (因為高度問題 會有錯誤)
                                        center_obj.X += sVal_gripOffset_cup_y;
                                        CvInvoke.PutText(mat_img_show, "Cup", new Point((int)center_obj.X - 20, (int)center_obj.Y - 20), FontFace.HersheyDuplex, 2, new MCvScalar(200, 200, 250), 2);
                                        CvInvoke.Circle(mat_img_show, new Point((int)center_obj.X, (int)center_obj.Y), 10, new MCvScalar(100, 50, 200), -1);
                                        CvInvoke.Circle(mat_img_show, new Point((int)BoundingBox.Center.X, (int)BoundingBox.Center.Y), (int)size_obj.Height / 2, new MCvScalar(50, 180, 200), 3);
                                    }
                                    else if (
                                    (size_obj.Height.IsBetween(190, 250) && size_obj.Width.IsBetween(10, 110)) ||
                                    (size_obj.Width.IsBetween(190, 250) && size_obj.Height.IsBetween(10, 110)))
                                    {
                                        CvInvoke.PutText(mat_img_show, "Pill box", new Point((int)center_obj.X - 30, (int)center_obj.Y - 20), FontFace.HersheyDuplex, 2, new MCvScalar(200, 200, 250), 2);
                                        CvInvoke.Circle(mat_img_show, new Point((int)BoundingBox.Center.X, (int)BoundingBox.Center.Y), 10, new MCvScalar(100, 50, 200), -1);
                                        CvInvoke.Polylines(mat_img_show, Array.ConvertAll(BoundingBox.GetVertices(), Point.Round), true, new MCvScalar(50, 180, 200), 3);
                                    }
                                    else if (
                                    (size_obj.Height.IsBetween(10, 190) && size_obj.Width.IsBetween(10, 110)) ||
                                    (size_obj.Width.IsBetween(10, 190) && size_obj.Height.IsBetween(10, 110)))
                                    {
                                        CvInvoke.PutText(mat_img_show, "Spoon", new Point((int)center_obj.X - 30, (int)center_obj.Y - 20), FontFace.HersheyDuplex, 2, new MCvScalar(200, 200, 250), 2);
                                        CvInvoke.Circle(mat_img_show, new Point((int)BoundingBox.Center.X, (int)BoundingBox.Center.Y), 10, new MCvScalar(100, 50, 200), -1);
                                        CvInvoke.Polylines(mat_img_show, Array.ConvertAll(BoundingBox.GetVertices(), Point.Round), true, new MCvScalar(50, 180, 200), 3);
                                        //湯匙有規定的夾取角，所以在一次的修改角度
                                        if (angel_obj.IsBetween(0, 90))
                                            angel_obj = 180 - angel_obj;
                                        //angel_obj = BoundingBox.Angle;
                                    }

                                }
                            }
                        }

                        //畫出容許線
                        CvInvoke.Line(mat_img_show, new Point(grip_center.X + sVal_alignment_offset_x + size, grip_center.Y + sVal_alignment_offset_y), new Point(grip_center.X + sVal_alignment_offset_x - size, grip_center.Y + sVal_alignment_offset_y), new MCvScalar(thresVal, thresVal, thresVal));
                        CvInvoke.Line(mat_img_show, new Point(grip_center.X + sVal_alignment_offset_x, grip_center.Y + sVal_alignment_offset_y + size), new Point(grip_center.X + sVal_alignment_offset_x, grip_center.Y + sVal_alignment_offset_y - size), new MCvScalar(thresVal, thresVal, thresVal));
                        CvInvoke.Rectangle(mat_img_show, new Rectangle(grip_center.X + sVal_alignment_offset_x - size, grip_center.Y + sVal_alignment_offset_y - size, size * 2, size * 2), new MCvScalar(thresVal, thresVal, thresVal));
                        //畫出夾爪中心
                        CvInvoke.Circle(mat_img_show, grip_center, 10, new MCvScalar(192, 167, 100), 5);
                        MyInvoke.drawCross(ref mat_img_show, grip_center, 15, new MCvScalar(192, 167, 100), 3);

                        if (show_level)
                        {
                            CvInvoke.Circle(mat_img_show, new Point(640, 30), 10, new MCvScalar(150, 255, 100), 5);
                            CvInvoke.Circle(mat_img_show, new Point(640, 630), 10, new MCvScalar(150, 255, 100), 5);
                            byte x0 = MyInvoke.GetValue<byte>(img_depth, 30, 640);
                            byte x1 = MyInvoke.GetValue<byte>(img_depth, 630, 640);
                            sVal_levelX = (float)(x1 - x0) / 600f;

                            CvInvoke.Circle(mat_img_show, new Point(260, 360), 10, new MCvScalar(150, 255, 100), 5);
                            CvInvoke.Circle(mat_img_show, new Point(1060, 360), 10, new MCvScalar(150, 255, 100), 5);
                            byte y0 = MyInvoke.GetValue<byte>(img_depth, 360, 260);
                            byte y1 = MyInvoke.GetValue<byte>(img_depth, 360, 1060);
                            sVal_levelY = (float)(y1 - y0) / 800;

                            SaveData();
                        }

                        armMoveX = (grip_center.X - center_obj.X);
                        armMoveY = (center_obj.Y - grip_center.Y);

                        //armMoveX += alignment_offset.X;
                        //armMoveY += alignment_offset.Y;

                        //pixel to mm
                        armMoveX = armMoveX * (170f / 329f);
                        armMoveY = armMoveY * (210f / 410f);//mm


                        Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => { tb_object_msg.Text = $"distanse:({armMoveX.ToString("0.00")},{armMoveY.ToString("0.00")})mm,\n degree:{angel_obj.ToString("0.0")},\n size:{size_obj.Width.ToString("0.0")},{size_obj.Height.ToString("0.0")}"; }));
                        Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => { Img_main.Source = BitmapSourceConvert.ToBitmapSource(mat_img_show); }));
                    }
                }
            }, token);
        }

        static Action<VideoFrame> UpdateImage(Image img)
        {
            var wbmp = img.Source as WriteableBitmap;
            return new Action<VideoFrame>(frame =>
            {
                using (frame)
                {
                    var rect = new Int32Rect(0, 0, frame.Width, frame.Height);
                    wbmp.WritePixels(rect, frame.Data, frame.Stride * frame.Height, frame.Stride);
                }
            });
        }
        #endregion \\---RealSense---//


        #region //---Grip mode---\\
        private void Button_goDynamicGrip_box(object sender, RoutedEventArgs e)
        {
            goDynamicGrip("box");
        }
        private void Button_goDynamicGrip_cup(object sender, RoutedEventArgs e)
        {
            goDynamicGrip("cup");
        }
        private void Button_goDynamicGrip_spoon(object sender, RoutedEventArgs e)
        {
            goDynamicGrip("spoon");
        }
        private bool goDynamicGrip(string msg)
        {
            UR.goRelativePosition(0.M(), 20.mm(), 0.M());

            for (int i = 0; i < 100; i++)
            {
                Action action = delegate { };
                Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Input, action);
                Thread.Sleep(10);
            }

            if (msg == "Home" || msg == "home")
            {
                UR.goFile("Home");
            }
            else if (msg == "cup")
            {
                URCoordinates nowPos = new URCoordinates();
                UR.getPosition(ref nowPos);

                Unit x = nowPos.X + armMoveX.mm();
                Unit y = nowPos.Y + armMoveY.mm();
                Unit z = 0.2.M();
                Angle Rx = 3.14.rad();
                Angle Ry = 0.rad();
                Angle Rz = 0.rad();
                URCoordinates goPos = new URCoordinates(x, y, z, Rx, Ry, Rz);
                UR.goPosition2(goPos); //到夾爪正中心

                URCoordinates goPos2 = PickingPos.PickPose(goPos.X, goPos.Y, goPos.Z, msg);
                UR.goPosition2(goPos2);

                UR.goRelativePosition(0.M(), 0.M(), (-130).mm());//向下
                UR.goGripper(70);
                UR.goRelativePosition(0.M(), 0.M(), (130).mm());//向上
            }
            else if (msg == "spoon")
            {

            }
            else if (msg == "pillBox")
            {
                URCoordinates nowPos = new URCoordinates();
                UR.getPosition(ref nowPos);
                URCoordinates goPos = new URCoordinates(nowPos.X + armMoveX.mm(), nowPos.Y + armMoveY.mm(), 0.2.M(), 3.14.rad(), 0.rad(), (0).rad());
                UR.goPosition2(goPos);
                UR.goRelativeJoint(j6: (angel_obj).deg());

                UR.goRelativePosition(0.M(), 0.M(), (-170).mm());//向下
                UR.goGripper(190);
                UR.goRelativePosition(0.M(), 0.M(), (170).mm());//向上
            }
            //    UR.cmd = mode.stop();
            return true;
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            //int x = (int)e.GetPosition(grid_img_grip).X;
            //int y = (int)e.GetPosition(grid_img_grip).Y;
            //Console.WriteLine($"mouse at({x},{y})");
            //Console.WriteLine($"Z({posMap[x * 2, y * 2, 2]})");
        }

        #endregion \\---Grip mode---//



        #region //---desk control---\\

        private void Tool2Img(Unit Tx, Unit Ty, out int Ix, out int Iy)
        {
            Ix = (int)Ty.mm + 600;
            Iy = (int)Tx.mm + 250;

        }
        private void Img2Tool(int Ix, int Iy, out Unit Tx, out Unit Ty)
        {
            Tx = (Iy - 250).mm();
            Ty = (Ix - 600).mm();
        }
        private void ToolRotate(ref Unit Tx, ref Unit Ty)
        {
            //newX = (W+X0)-(newX-X0)
            Tx.mm = 500 + (-250) - (Tx.mm - (-250));
            Ty.mm = 700 + (-600) - (Ty.mm - (-600));
        }
        private void ImgRotate(ref int Ix, ref int Iy)
        {
            //newX = (W+X0)-(newX-X0)
            Ix = 700 - Ix;
            Iy = 500 - Iy;
        }

        private void Grid_dask_MouseMove(object sender, MouseEventArgs e)
        {
            int x = (int)e.GetPosition((Grid)sender).X;
            int y = (int)e.GetPosition((Grid)sender).Y;
            Img2Tool(x, y, out Unit Tx, out Unit Ty);

            if (rotateDesk)
            {
                ToolRotate(ref Tx, ref Ty);

                if (PickingPos.PickCheck(Tx, Ty))
                {
                    rect_urWrist.Margin = new Thickness(x - 5, y, 0, 0);
                    float angle = PickingPos.PickAngle(Tx, Ty).deg;
                    System.Windows.Media.RotateTransform rotateTransform1 = new System.Windows.Media.RotateTransform(angle, 5, 0);
                    rect_urWrist.RenderTransform = rotateTransform1;

                    PickingPos.PourPos(Tx, Ty, out Unit Px, out Unit Py);
                    ToolRotate(ref Px, ref Py);
                    Tool2Img(Px, Py, out int showx, out int showy);
                    circle_pourPos.Margin = new Thickness(showx - circle_pourPos.Width / 2, showy - circle_pourPos.Height / 2, 0, 0);

                    PickingPos.AddInPos(Tx, Ty, out Unit Ax, out Unit Ay);
                    ToolRotate(ref Ax, ref Ay);
                    Tool2Img(Ax, Ay, out int showAx, out int showAy);
                    circle_addPos.Margin = new Thickness(showAx - circle_pourPos.Width / 2, showAy - circle_pourPos.Height / 2, 0, 0);
                }
            }

            tb_desk_xy.Text = $"({Tx.mm},{Ty.mm})";
            tb_desk_xy.Margin = new Thickness(x, y - 20, 0, 0);
        }
        int clickTx = 0;
        int clickTy = 0;
        int clickAngle = 0;
        private void Grid_desk_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int x = (int)e.GetPosition((Grid)sender).X;
            int y = (int)e.GetPosition((Grid)sender).Y;
            Img2Tool(x, y, out Unit Tx, out Unit Ty);

            if (rotateDesk)
            {
                ToolRotate(ref Tx, ref Ty);

                if (PickingPos.PickCheck(Tx, Ty))
                {
                    rect_urWrist.Margin = new Thickness(x - 5, y, 0, 0);
                    clickAngle = (int)PickingPos.PickAngle(Tx, Ty).deg;
                    System.Windows.Media.RotateTransform rotateTransform1 = new System.Windows.Media.RotateTransform(clickAngle, 5, 0);
                    rect_urWrist.RenderTransform = rotateTransform1;
                }
                else
                {
                    MessageBox.Show("超出工作區");
                    return;
                }
            }
            clickTx = (int)Tx.mm;
            clickTy = (int)Ty.mm;
            tb_desk_xy.Text = $"({clickTx},{clickTy})";
            tb_desk_xy.Margin = new Thickness(x, y - 20, 0, 0);
        }
        private void Button_desk_goPos(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                URCoordinates.Vector3 rpy = new URCoordinates.Vector3(-90.deg(), 180.deg(), (-90 - clickAngle).deg());
                URCoordinates.Vector3 rotation = URCoordinates.ToRotVector(rpy);
                URCoordinates goPos = new URCoordinates(clickTx.mm(), clickTy.mm(), 0.2.M(), rotation.X.rad(), rotation.Y.rad(), rotation.Z.rad());
                UR.goPosition2(goPos);
            });

        }


        DispatcherTimer posUdate = new DispatcherTimer();
        string ClientIP = "192.168.1.101";
        private void Cb_UpdatePos_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)((CheckBox)sender).IsChecked)
            {
                UR.creatClient(ClientIP);
                UR.client_StartGetRobotInfo();
                posUdate.Interval = TimeSpan.FromMilliseconds(100);
                posUdate.Tick += posUdate_Tick;
                posUdate.Start();
            }
            else//結束
            {
                posUdate.Stop();
            }
        }
        void posUdate_Tick(object sender, EventArgs e)
        {
            URCoordinates nowPos = UR.ClientPos;
            tb_urPos.Text = nowPos.ToString("(3)", "0.000");
            Unit Tx = nowPos.X;
            Unit Ty = nowPos.Y;

            Tool2Img(Tx, Ty, out int x, out int y);

            if (rotateDesk)
            {
                ImgRotate(ref x, ref y);
            }
            circle_gripPos.Margin = new Thickness(x - (circle_gripPos.Width / 2), y - (circle_gripPos.Height / 2), 0, 0);

            //   Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => { tb_object_msg.Text = $"distanse:({armMoveX.ToString("0.00")},{armMoveY.ToString("0.00")})mm,\n degree:{angel_obj.ToString("0.0")},\n size:{size_obj.Width.ToString("0.0")},{size_obj.Height.ToString("0.0")}"; }));
        }

        bool rotateDesk = true;
        private void Cb_rotateDesk_Click(object sender, RoutedEventArgs e)
        {
            rotateDesk = (bool)((CheckBox)sender).IsChecked;
            if (rotateDesk)
                cirle_URonDesk.Margin = new Thickness(40, 190, 0, 0);
            else
                cirle_URonDesk.Margin = new Thickness(540, 190, 0, 0);
        }

        private void Button_desk_goPos_V(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                URCoordinates goPos = PickingPos.PickPose(clickTx.mm(), clickTy.mm(), 0.2.M(), "V");
                UR.goPosition2(goPos);
            });
        }
        private void Button_desk_goPos_H(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                URCoordinates goPos = PickingPos.PickPose(clickTx.mm(), clickTy.mm(), 0.2.M(), "H");
                PickingPos.PourPos(goPos.X, goPos.Y, out goPos.X, out goPos.Y);
                UR.goPosition2(goPos);
            });
        }
        private void Button_desk_goPos_cupPour(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                URCoordinates goPos = PickingPos.PickPose(clickTx.mm(), clickTy.mm(), 0.2.M(), "cup");
                PickingPos.PourPos(goPos.X, goPos.Y, out goPos.X, out goPos.Y);
                UR.goPosition2(goPos);
            });
        }
        private void Button_desk_goPos_addIn(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                URCoordinates goPos = PickingPos.PickPose(clickTx.mm(), clickTy.mm(), 0.2.M(), "add");
                PickingPos.AddInPos(goPos.X, goPos.Y, out goPos.X, out goPos.Y);
                UR.goPosition2(goPos);
            });
        }
        private void Button_desk_goPos_cup(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                URCoordinates goPos = PickingPos.PickPose(clickTx.mm(), clickTy.mm(), 0.2.M(), "cup");
                UR.goPosition2(goPos);
            });
        }
        //client control
        private void Button_desk_client_goPos(object sender, RoutedEventArgs e)
        {
            UR.creatClient(ClientIP);
            UR.client_SendData("movej([1.03176,-1.99993,1.71905,-1.29134,-1.56545,-3.68115])\n");
        }
        #endregion \\---desk control---//

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            UR.creatClient("192.168.1.105");
            Task.Run(() =>
            {
                while (true)
                {
                    var msg = UR.client_readData();
                    if (msg != "")
                        Console.WriteLine("get" + msg);
                }
            });
        }
        private void Button_Click_11(object sender, RoutedEventArgs e)
        {
            string str = "actual_TCP_pose\n";
            UR.client_SendData(str);

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

            URCoordinates.Vector3 rpy =
                new URCoordinates.Vector3(105.deg(), 180.deg(), 60.deg());
            URCoordinates.Vector3 rotation = URCoordinates.ToRotVector(rpy);

        }

        public void Btn_addSimAction_Click(object sender, RoutedEventArgs e)
        {
            PickingPos.PourPos(new Unit(), new Unit(-0.5), out Unit Px, out Unit Py);

        }

        private void Button_UR_V_Click(object sender, RoutedEventArgs e)
        {
            URCoordinates nowPos = new URCoordinates();
            UR.getPosition(ref nowPos);
            URCoordinates goPos = new URCoordinates(nowPos.X, nowPos.Y, 0.2.M(), 3.14.rad(), 0.rad(), (0).rad());
            UR.goPosition2(goPos);
        }


    }//class
    public static class BitmapSourceConvert
    {
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        public static BitmapSource ToBitmapSource(IImage image)
        {
            using (System.Drawing.Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap();

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr);
                return bs;
            }
        }
    }

    public class ActionBaseAdder
    {
        public ActionBaseAdder(string action, string target, string destination, SolidColorBrush C1, SolidColorBrush C2, SolidColorBrush C3)
        {
            Action = action;
            Target = target;
            Destination = destination;
            Color1 = (C1);
            Color2 = (C2);
            Color3 = (C3);
        }
        public static string Action { get; set; }
        public static string Target { get; set; }
        public static string Destination { get; set; }
        public static SolidColorBrush Color1 { get; set; } = new SolidColorBrush(Colors.Black);
        public static SolidColorBrush Color2 { get; set; } = new SolidColorBrush(Colors.Black);
        public static SolidColorBrush Color3 { get; set; } = new SolidColorBrush(Colors.Black);
    }

    public static class MatExtension
    {
        public static dynamic GetValue(this Mat mat, int row, int col)
        {
            var value = CreateElement(mat.Depth);
            Marshal.Copy(mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, value, 0, 1);
            return value[0];
        }

        public static void SetValue(this Mat mat, int row, int col, dynamic value)
        {
            var target = CreateElement(mat.Depth, value);
            Marshal.Copy(target, 0, mat.DataPointer + (row * mat.Cols + col) * mat.ElementSize, 1);
        }
        private static dynamic CreateElement(DepthType depthType, dynamic value)
        {
            var element = CreateElement(depthType);
            element[0] = value;
            return element;
        }

        private static dynamic CreateElement(DepthType depthType)
        {
            if (depthType == DepthType.Cv8S)
            {
                return new sbyte[1];
            }
            if (depthType == DepthType.Cv8U)
            {
                return new byte[1];
            }
            if (depthType == DepthType.Cv16S)
            {
                return new short[1];
            }
            if (depthType == DepthType.Cv16U)
            {
                return new ushort[1];
            }
            if (depthType == DepthType.Cv32S)
            {
                return new int[1];
            }
            if (depthType == DepthType.Cv32F)
            {
                return new float[1];
            }
            if (depthType == DepthType.Cv64F)
            {
                return new double[1];
            }
            return new float[1];
        }
    }

}//namespace
