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
        public static List<ActionLine> mainActionList = new List<ActionLine>();

        public static int timeTick = 0;

        #region //---UI---\\
        enum connectTpye
        {
            none = 0,
            Disconnect = 1,
            Connecting = 2,
            isConnect = 3,
            Canceling = 4
        }
        private void setConnectCircle(connectTpye S)
        {
            cir_UrState_off.Fill = new SolidColorBrush(Colors.LightGray);
            cir_UrState_connecting.Fill = new SolidColorBrush(Colors.LightGray);
            cir_UrState_on.Fill = new SolidColorBrush(Colors.LightGray);
            if (S == connectTpye.Disconnect)
            {
                cir_UrState_off.Fill = new SolidColorBrush(Color.FromArgb(255, 230, 50, 50));
            }
            else if (S == connectTpye.Connecting)
            {
                cir_UrState_connecting.Fill = new SolidColorBrush(Color.FromArgb(255, 220, 220, 20));
            }
            else if (S == connectTpye.isConnect)
            {
                cir_UrState_on.Fill = new SolidColorBrush(Color.FromArgb(255, 40, 210, 40));
            }
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

        private void setArServerConnectCircle(connectTpye S)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                rect_ARconnect_con.Opacity = 0.1;
                rect_ARconnect_dis.Opacity = 0.1;
                if (S == connectTpye.Disconnect)//dis 
                    rect_ARconnect_dis.Opacity = 1;
                else if (S == connectTpye.isConnect)//connect
                    rect_ARconnect_con.Opacity = 1;
            }));
        }
        #endregion \\---UI---//

        #region //---public value---\\
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

        public static float sVal_pixel2mmX = 0.5167173f;
        public static float sVal_pixel2mmY = 0.512195f;

        public static bool show_thres = true;
        public static bool show_level = false;

        public static int sVal_depthMapMin = 200;
        public static int sVal_depthMapMax = 370;

        public static int sVal_bineray_threshold = 26;

        string UR_IP = "192.168.1.100";
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
            txt.WriteLine($"float\t{nameof(sVal_pixel2mmX)}\t{sVal_pixel2mmX.ToString()}");
            txt.WriteLine($"float\t{nameof(sVal_pixel2mmY)}\t{sVal_pixel2mmY.ToString()}");

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
        #endregion \\---public value---//

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
            setArServerConnectCircle(connectTpye.Disconnect);
            Cb_rotateDesk_Click(mItem_rotateDesk, null);
        }

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
            tracking = false;
            UR.startServer("auto", 888);
        }
        private void Button_disconnect_Click(object sender, RoutedEventArgs e)
        {
            tracking = false;
            UR.stopServer();
        }
        private void OnUrStateChange(tcpState S)
        {
            try
            {
                this.Dispatcher.Invoke((Action)(() =>
                {
                    if (S == tcpState.Connect)
                        setConnectCircle(connectTpye.isConnect);
                    else if (S == tcpState.Disconnect)
                    {
                        tracking = false;
                        setConnectCircle(connectTpye.Disconnect);
                    }
                    else if (S == tcpState.waitAccept)
                        setConnectCircle(connectTpye.Connecting);
                    else if (S == tcpState.startListener)
                        setConnectCircle(connectTpye.Connecting);
                    else
                        setConnectCircle(connectTpye.Disconnect);
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
                UR.ClientConnect(UR_IP);
            else
                UR.ClientDisconnect();
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
            UR.ClientConnect(UR_IP);
            foreach (string str in File.ReadAllLines("Path\\j_top.path"))//防止下個點失敗手臂 
                UR.ClientSend(str + "\n");
            //    UR.ClientDisconnect();

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
        #region //--- Action base Control---\\
        static bool startDemo = false;
        private void Button_startRecord_Click(object sender, RoutedEventArgs e)
        {
            LV_actionBase.Items.Clear();
            mainActionList.Clear();
            Rect_actionBaseTopColor.Fill = new SolidColorBrush(Color.FromArgb(200, 40, 210, 80));
            startDemo = true;

            myTcp.client_SendData("start_demo");
            string msg = myTcp.client_ReadData();
            Console.WriteLine(msg);

        }
        private void Button_endDemo_Click(object sender, RoutedEventArgs e)
        {
            if (startDemo == false)
            {
                MessageBox.Show("已經結束了");
            }
            Rect_actionBaseTopColor.Fill = new SolidColorBrush(Color.FromArgb(200, 210, 80, 40));
            startDemo = false;

            myTcp.client_SendData("end_demo");
            string msg = myTcp.client_ReadData();

            Console.WriteLine(msg);//收到動作
            mainActionList.Clear();
            mainActionList = msg2ActionLineList(msg);
            ActionLineList2ListView(mainActionList);
        }
        private void Button_createAction_Click(object sender, RoutedEventArgs e)
        {
            if (startDemo == true) //need startDemo == false
            {
                MessageBox.Show("尚未結束示範，請按下[End Demo]按鈕");
                return;
            }
            if (RS_Task == null)
            {
                MessageBox.Show("Real Sense 沒開");
                return;
            }

            string fileName = "Reproduction";

            ActionBase2Cmd(mainActionList, fileName);

            //執行
            if (UR.isServerRunning == false)
            {
                MessageBox.Show("UR手臂未連結，但已經規劃好路徑");
                return;
            }

            Task.Run(() =>
            {
                UR.goFile("j_top.path");
                UR.goFile(fileName);
                UR.goFile("j_top.path");
            });

        }

        public void ActionLineList2ListView(List<ActionLine> ls_al)
        {
            LV_actionBase.Items.Clear();
            foreach (ActionLine al in ls_al)
                ActionLine2ListView(al);
        }
        public void ActionLine2ListView(ActionLine al)
        {
            ActionBaseAdder ab;
            if (al.destination.Name == "pos")
                ab = new ActionBaseAdder(al.Action.Name, al.target.Name, al.destination.nowPos.ToString("(2)", "0.000"), new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Black));
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

        private void SelectCurrentItem(object sender, KeyboardFocusChangedEventArgs e)
        {
            ListViewItem item = (ListViewItem)sender;
            item.IsSelected = true;

        }

        private void Button_deleteActionBase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mainActionList.RemoveAt(LV_actionBase.SelectedIndex);
                ActionLineList2ListView(mainActionList);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion \\--- Action base Control---//


        #region //---RealSense---\\
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
        int[,,] valueMap = new int[1280, 720, 5];//[x,y,filter size]
        int mapPivit = 0;

        //偵測到的物件資訊
        PointF center_obj = new PointF();
        SizeF size_obj = new Size();
        float angel_obj = 0;
        float armMoveX;
        float armMoveY;
        int trackDistanse_Ix = 0;

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
                                    value = (value - sVal_depthMapMin) * (255 - 0) / (sVal_depthMapMax - sVal_depthMapMin) + 0;
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
                                    if (c == 0)
                                        outValue = 0;
                                    else
                                    {
                                        if (show_thres)
                                            outValue = thresVal;//應該不是平均，因為我希望有一次 就有，而不是被平均過去
                                        else
                                            outValue /= c;
                                    }



                                    pixelPtr_byte[j * 1280 + i] = (byte)((int)outValue);
                                }
                        }

                        //影像處理開始--抓物件
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

                        //劃出不要的地方
                        CvInvoke.Rectangle(mat_img_show, new Rectangle(0, 530, 430, 190), new MCvScalar(0, 0, 0), -1);
                        CvInvoke.Rectangle(mat_img_show, new Rectangle(230, 620, 620, 100), new MCvScalar(0, 0, 0), -1);
                        CvInvoke.Rectangle(mat_img_show, new Rectangle(850, 530, 430, 190), new MCvScalar(0, 0, 0), -1);
                        if (tracking == true)
                        {
                            CvInvoke.Line(mat_img_show, new Point(0, 100), new Point(RS_depthSize.Width, 100), new MCvScalar(0, 0, 0), 5);//切斷線 ，避免手軸也被偵測
                            int of = 100;
                            CvInvoke.Line(mat_img_show, new Point(100, 200), new Point(RS_depthSize.Width - 100, 200), new MCvScalar(thresVal, thresVal, thresVal));
                            CvInvoke.Line(mat_img_show, new Point(100, 300), new Point(RS_depthSize.Width - 100, 300), new MCvScalar(thresVal, thresVal, thresVal));
                            CvInvoke.Line(mat_img_show, new Point(100, 400), new Point(RS_depthSize.Width - 100, 400), new MCvScalar(thresVal, thresVal, thresVal));
                        }

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
                            if (contours.Size == 0)//nothing
                                center_obj = grip_center;

                            for (int i = 0; i < contours.Size; i++)
                            {
                                using (VectorOfPoint contour = contours[i])
                                {
                                    if (contour.Size < 50)
                                        continue;
                                    if (tracking)
                                        if (contour.Size < 300)
                                            continue;

                                    RotatedRect BoundingBox = CvInvoke.MinAreaRect(contour);
                                    angel_obj = BoundingBox.Angle + 4;//校正度數
                                    center_obj = BoundingBox.Center;
                                    CvInvoke.Circle(mat_img_show, new Point((int)center_obj.X, (int)center_obj.Y), 10, new MCvScalar(100, 250, 50), -1);
                                    size_obj = BoundingBox.Size;

                                    //要調整角度
                                    if (size_obj.Width > size_obj.Height)
                                        angel_obj = angel_obj + 90;

                                    if (size_obj.Height.IsBetween(120, 200) && size_obj.Width.IsBetween(120, 200))
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
                            }//畫物件 for
                        }

                        //畫出容許線
                        CvInvoke.Line(mat_img_show, new Point(grip_center.X + sVal_alignment_offset_x + size, grip_center.Y + sVal_alignment_offset_y), new Point(grip_center.X + sVal_alignment_offset_x - size, grip_center.Y + sVal_alignment_offset_y), new MCvScalar(thresVal, thresVal, thresVal));
                        CvInvoke.Line(mat_img_show, new Point(grip_center.X + sVal_alignment_offset_x, grip_center.Y + sVal_alignment_offset_y + size), new Point(grip_center.X + sVal_alignment_offset_x, grip_center.Y + sVal_alignment_offset_y - size), new MCvScalar(thresVal, thresVal, thresVal));
                        CvInvoke.Rectangle(mat_img_show, new Rectangle(grip_center.X + sVal_alignment_offset_x - size, grip_center.Y + sVal_alignment_offset_y - size, size * 2, size * 2), new MCvScalar(thresVal, thresVal, thresVal));
                        //block area
                        CvInvoke.Rectangle(mat_img_show, new Rectangle(0, 530, 430, 190), new MCvScalar(100, 100, 100), -1);
                        CvInvoke.Rectangle(mat_img_show, new Rectangle(230, 620, 620, 100), new MCvScalar(100, 100, 100), -1);
                        CvInvoke.Rectangle(mat_img_show, new Rectangle(850, 530, 430, 190), new MCvScalar(100, 100, 100), -1);
                        if (tracking)
                        {
                            //track mode line
                            CvInvoke.Line(mat_img_show, new Point(100, 200), new Point(RS_depthSize.Width - 100, 200), new MCvScalar(thresVal, thresVal, thresVal));
                            CvInvoke.Line(mat_img_show, new Point(100, 300), new Point(RS_depthSize.Width - 100, 300), new MCvScalar(thresVal, thresVal, thresVal));
                            CvInvoke.Line(mat_img_show, new Point(100, 400), new Point(RS_depthSize.Width - 100, 400), new MCvScalar(thresVal, thresVal, thresVal));
                        }

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

                        trackDistanse_Ix = (int)(grip_center.X - center_obj.X);

                        //pixel to mm
                        armMoveX = armMoveX * (sVal_pixel2mmX);
                        armMoveY = armMoveY * (sVal_pixel2mmY);//mm


                        Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => { tb_object_msg.Text = $"distanse:({armMoveX.ToString("0")},{armMoveY.ToString("0")})mm,\n degree:{angel_obj.ToString("0.0")},\n size:{size_obj.Width.ToString("0.0")},{size_obj.Height.ToString("0.0")} \n track:{trackDistanse_Ix}"; }));
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
        private void Button_goCenter(object sender, RoutedEventArgs e)
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
        }
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

        bool tracking = true;
        Task task_tracking;
        private void Button_goDynamic_Tracking(object sender, RoutedEventArgs e)
        {

            if (mItem_dynamicTrack.IsChecked == true)
            {
                tracking = true;
                URCoordinates nowPos = new URCoordinates();
                task_tracking = Task.Run(() =>
                 {
                     while (tracking)
                     {
                         nowPos = UR.ClientPos;

                         //float dx = armMoveX;
                         //float dy = armMoveY;

                         //Unit x = nowPos.X + dx.mm();
                         //Unit y = nowPos.Y + dy.mm() + 70.mm();//往Y+走一點 不然容易被夾爪擋住

                         Unit x = 150.mm();
                         Unit y = nowPos.Y;

                         if (trackDistanse_Ix > 50)
                         {
                             y.mm = y.mm + 20;
                             URCoordinates goPos = PickingPos.PickPose(x, y, 0.2.M(), "cup");
                           UR.goTrack(goPos);
                         }
                         else if (trackDistanse_Ix < -50)
                         {
                             y.mm = y.mm - 20;
                             URCoordinates goPos = PickingPos.PickPose(x, y, 0.2.M(), "cup");
                             UR.goTrack(goPos);
                         }

                         //Unit z = 0.2.M();
                         //Angle Rx = 3.14.rad();
                         //Angle Ry = 0.rad();
                         //Angle Rz = 0.rad();

                         //URCoordinates goPos = PickingPos.PickPose(clickTx.mm(), clickTy.mm(), 0.2.M(), "cup");
                         //UR.goPosition2(goPos);

                         //URCoordinates goPos = new URCoordinates(x, y, z, Rx, Ry, Rz);


                         //URCoordinates goPos = PickingPos.PickPose(x, y, 0.2.M(), "cup");

                         //float distanse = (float)Math.Sqrt((dx * dx) + (dy * dy));
                         //if (distanse > 30)
                         //{
                         //    UR.goTrack(goPos);
                         // Console.WriteLine(goPos.ToString("(2)","0.000"));
                         //}
                         //else
                         //Console.WriteLine("too close");
                         //    UR.goPosition(goPos);
                         Thread.Sleep(100);
                     }

                 });
            }
            else
            {
                tracking = false;
                if (task_tracking != null)
                    task_tracking.Wait();//wait tracking to end
            }







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
        private void updateDeskObject()
        {
            Unit x = objList[1].nowPos.X;
            Unit y = objList[1].nowPos.Y;
            if (x.M == 0 && y.M == 0)
                img_object_Bcup.Opacity = 0;
            else
                img_object_Bcup.Opacity = 1;
            if (rotateDesk)
                ToolRotate(ref x, ref y);
            Tool2Img(x, y, out int Ix, out int Iy);
            img_object_Bcup.Margin = new Thickness(Ix, Iy, 0, 0);


            x = objList[2].nowPos.X;
            y = objList[2].nowPos.Y;
            if (x.M == 0 && y.M == 0)
                img_object_Pcup.Opacity = 0;
            else
                img_object_Pcup.Opacity = 1;
            if (rotateDesk)
                ToolRotate(ref x, ref y);
            Tool2Img(x, y, out Ix, out Iy);
            img_object_Pcup.Margin = new Thickness(Ix, Iy, 0, 0);

            x = objList[6].nowPos.X;
            y = objList[6].nowPos.Y;
            if (x.M == 0 && y.M == 0)
                img_object_RpillBox.Opacity = 0;
            else
                img_object_RpillBox.Opacity = 1;
            if (rotateDesk)
                ToolRotate(ref x, ref y);
            Tool2Img(x, y, out Ix, out Iy);
            img_object_RpillBox.Margin = new Thickness(Ix, Iy, 0, 0);

            x = objList[7].nowPos.X;
            y = objList[7].nowPos.Y;
            if (x.M == 0 && y.M == 0)
                img_object_GpillBox.Opacity = 0;
            else
                img_object_GpillBox.Opacity = 1;
            if (rotateDesk)
                ToolRotate(ref x, ref y);
            Tool2Img(x, y, out Ix, out Iy);
            img_object_GpillBox.Margin = new Thickness(Ix, Iy, 0, 0);

        }

        bool show_pourPos = false;
        private void Grid_dask_MouseMove(object sender, MouseEventArgs e)
        {
            int x = (int)e.GetPosition((Grid)sender).X;
            int y = (int)e.GetPosition((Grid)sender).Y;
            Img2Tool(x, y, out Unit Tx, out Unit Ty);

            if (rotateDesk)
                ToolRotate(ref Tx, ref Ty);

            if (PickingPos.PickCheck(Tx, Ty))
            {
                rect_urWrist.Fill.Opacity = 0.6;
                rect_urWrist.Margin = new Thickness(x - rect_urWrist.Width / 2, y - 20, 0, 0);
                float angle = PickingPos.PickAngle(Tx, Ty).deg;
                if (angle == 999) return;
                if (!rotateDesk) angle = angle - 180;//這裡算角度反過來了
                System.Windows.Media.RotateTransform rotateTransform1 = new System.Windows.Media.RotateTransform(angle, rect_urWrist.Width / 2, 20);
                rect_urWrist.RenderTransform = rotateTransform1;

                if (show_pourPos)
                {
                    PickingPos.PourPos(Tx, Ty, out Unit Px, out Unit Py);
                    if (rotateDesk)
                        ToolRotate(ref Px, ref Py);
                    Tool2Img(Px, Py, out int showx, out int showy);
                    circle_pourPos.Margin = new Thickness(showx - circle_pourPos.Width / 2, showy - circle_pourPos.Height / 2, 0, 0);

                    PickingPos.AddInPos(Tx, Ty, out Unit Ax, out Unit Ay);
                    if (rotateDesk)
                        ToolRotate(ref Ax, ref Ay);
                    Tool2Img(Ax, Ay, out int showAx, out int showAy);
                    circle_addPos.Margin = new Thickness(showAx - circle_pourPos.Width / 2, showAy - circle_pourPos.Height / 2, 0, 0);
                }

            }
            else
            {
                rect_urWrist.Fill.Opacity = 0.2;
            }

            tb_desk_xy.Text = $"({Tx.mm},{Ty.mm})";
            tb_desk_xy.Margin = new Thickness(x - 30, y - 50, 0, 0);
        }
        int clickTx = 0;
        int clickTy = 0;
        int clickAngle = 0;
        private void Grid_desk_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int x = (int)e.GetPosition((Grid)sender).X;
            int y = (int)e.GetPosition((Grid)sender).Y;
            Img2Tool(x, y, out Unit Tx, out Unit Ty);

            if (rotateDesk) ToolRotate(ref Tx, ref Ty);

            if (PickingPos.PickCheck(Tx, Ty))
            {
                rect_urWrist.Margin = new Thickness(x - rect_urWrist.Width / 2, y - 20, 0, 0);
                float angle = PickingPos.PickAngle(Tx, Ty).deg;
                if (angle == 999) return;
                if (!rotateDesk) angle = angle - 180;//這裡算角度反過來了

                clickAngle = (int)angle;
            }
            else
            {
                MessageBox.Show("超出工作區");
                return;
            }

            clickTx = (int)Tx.mm;
            clickTy = (int)Ty.mm;

            tb_desk_xy.Text = $"({clickTx},{clickTy})";
            tb_desk_xy.Margin = new Thickness(x, y - 20, 0, 0);
        }

        DispatcherTimer posUdate = new DispatcherTimer();
        private void Cb_UpdatePos_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)((CheckBox)sender).IsChecked)
            {
                if (!UR.ClientConnect(UR_IP))
                    return;
                UR.Client_RTDE();
                posUdate.Interval = TimeSpan.FromMilliseconds(100);
                posUdate.Tick += posUdate_Tick;
                posUdate.Start();
            }
            else//結束
            {
                posUdate.Stop();
            }
        }
        // static URCoordinates ClientPosition =new URCoordinates();
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

        }

        bool rotateDesk = true;
        private void Cb_rotateDesk_Click(object sender, RoutedEventArgs e)
        {
            rotateDesk = mItem_rotateDesk.IsChecked;
            if (rotateDesk)
            {
                cirle_URonDesk.Margin = new Thickness(40, 190, 0, 0);
                float angle = 180;
                img_deskBackground.RenderTransform = new System.Windows.Media.RotateTransform(angle, img_deskBackground.Width / 2, img_deskBackground.Height / 2);
            }
            else
            {
                cirle_URonDesk.Margin = new Thickness(540, 190, 0, 0);
                float angle = 0;
                img_deskBackground.RenderTransform = new System.Windows.Media.RotateTransform(angle, 0, 0);
            }
        }
        private void Cb_showPourPos_Click(object sender, RoutedEventArgs e)
        {
            show_pourPos = mItem_showPourPos.IsChecked;
            if (show_pourPos)
            {
                circle_pourPos.Visibility = Visibility.Visible;
                circle_addPos.Visibility = Visibility.Visible;
            }
            else
            {
                circle_pourPos.Visibility = Visibility.Hidden;
                circle_addPos.Visibility = Visibility.Hidden;
            }
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

        }

    
        #endregion \\---desk control---//

        #region //---Connect Python Action recognition---\\
        // ezTCP TCP = new ezTCP();
        mySocket.SocketTool myTcp = new mySocket.SocketTool();
        string AR_IP = "192.168.1.105";
        private void Button_connectArServer_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                setArServerConnectCircle(connectTpye.Disconnect);

                if (!myTcp.ClientConnect(AR_IP, 777)) return;

                setArServerConnectCircle(connectTpye.isConnect);

            });
        }

        public List<ActionLine> msg2ActionLineList(string msg)
        {
            List<ActionLine> rtn = new List<ActionLine>();
            string[] oneAction = msg.Split(';');
            for (int i = 0; i < oneAction.Length; i++)
            {
                string[] actInfo = oneAction[i].Split(',');
                if (actInfo.Count() <= 1)
                    return rtn;
                int actIndex = actInfo[1].toInt();
                myAction act = new myAction(actIndex, actList[actIndex].Name);

                int tarIndex = actInfo[2].toInt();
                Objects tar = new Objects(tarIndex, objList[tarIndex].Name);
                Unit x = actInfo[3].toFloat().M();
                Unit y = actInfo[4].toFloat().M();
                tar.nowPos = new URCoordinates(x, y, 0.M());

                int desIndex = actInfo[5].toInt();
                Objects des = new Objects(desIndex, objList[desIndex].Name);
                x = actInfo[6].toFloat().M();
                y = actInfo[7].toFloat().M();
                des.nowPos = new URCoordinates(x, y, 0.M());


                if (act.Name == ActionName.Place)
                {
                    //如果是place，就放入target的位置
                    des = new Objects(9, "pos");
                    des.nowPos = tar.nowPos;
                }

                rtn.Add(new ActionLine(act, tar, des));

            }
            return rtn;
        }
    
        private void Button_getAR_Click(object sender, RoutedEventArgs e)
        {
            myTcp.client_SendData("get_obj_pos");
            string msg = myTcp.client_ReadData();
            updateObjectFromMsg(msg);
        }
        public void updateObjectFromMsg(string msg)
        {
            //5,-0.17719,-0.41207;3,0,0;3,0,0;2,0.20174,-0.42822;1,0.07944,-0.34077;8,0,0;6,0,0;7,0,0; 
            //5,-0.17625,-0.41337;3,0,0;3,0,0;2,0,0;1,-0.0027,-0.19354;8,0,0;6,0,0;7,0,0;  
            string[] oneObject = msg.Split(';');
            for (int i = 0; i < oneObject.Count(); i++)
            {
                string[] objInfo = oneObject[i].Split(',');
                if (objInfo.Count() < 3)//代表是 "空白"
                    continue;
                int objectIndex = objInfo[0].toInt();

                if (objectIndex > objList.Count())
                    continue;

                Unit x = objInfo[1].toFloat().M();
                Unit y = objInfo[2].toFloat().M();
                objList[objectIndex].nowPos = new URCoordinates(x, y);
            }

            //update object position
            for (int i = 0; i < mainActionList.Count(); i++)
            {
                if (mainActionList[i].target.Name != "pos")//如果不是pos才要更新 //基本上不會發生
                    mainActionList[i].target.nowPos = objList[mainActionList[i].target.index].nowPos;
                if (mainActionList[i].destination.Name != "pos")//如果不是pos才要更新 //也就是像place到某個點
                    mainActionList[i].destination.nowPos = objList[mainActionList[i].destination.index].nowPos;
            }

            //  updateDeskObject();
            //A//4,-0.10942,-0.55016
            //B//4,0.09075,-0.54936
        }

        private void Button_testGetting_Click(object sender, RoutedEventArgs e)
        {
            //1//"23,1,2,-0.00537,-0.19776,0,-0.001,-0.001;34,3,2,0.0818,-0.26488,1,0.09241,-0.34049;48,2,2,-0.07506,-0.41016,0,-0.001,-0.001"
            //2//"13,1,4,-0.07997,-0.55134,0,-0.001,-0.001;32,4,4,-0.18751,-0.36944,5,-0.17444,-0.40835;49,5,4,0.11488,-0.28648,1,0.07984,-0.34667;72,2,4,0.09088,-0.54576,0,-0.001,-0.001            
            //3//"14,1,4,0.13284,-0.54551,0,-0.001,-0.001;34,4,4,-0.15142,-0.3726,5,-0.1685,-0.4027;58,5,4,0.25247,-0.38447,1,0.19471,-0.43088;68,6,4,0.1953,-0.43022,1,0.19875,-0.43159;100,2,4,-0.11012,-0.54689,0,-0.001,-0.001                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                "
            //4//"18,1,4,-0.06568,-0.53504,0,-0.001,-0.001;43,6,4,-0.10248,-0.26245,2,-0.09081,-0.2734;76,2,4,-0.11012,-0.54689,0,-0.001,-0.001;89,1,1,0.19953,-0.24564,0,-0.001,-0.001;102,3,1,-0.10925,-0.19736,2,-0.08245,-0.26977;125,2,1,-0.07509,-0.41453,0,-0.001,-0.001                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   "
            //5//"24,1,7,-0.15884,0.00758,0,-0.001,-0.001;93,2,7,-0.15877,0.00736,0,-0.001,-0.001;100,1,4,0.19268,-0.24221,0,-0.001,-0.001;114,2,4,0.09088,-0.54576,0,-0.001,-0.001;122,1,2,0.2017,-0.23103,0,-0.001,-0.001;130,2,2,0.19793,-0.22469,0,-0.001,-0.001;137,1,1,0.20246,-0.44121,0,-0.001,-0.001;140,3,1,0.21143,-0.39486,2,0.24188,-0.40872;161,1,4,-0.04446,-0.29796,0,-0.001,-0.001;170,2,4,0.09088,-0.54576,0,-0.001,-0.001;184,1,4,0.10567,-0.52964,0,-0.001,-0.001;200,2,4,0.18413,-0.28641,0,-0.001,-0.001;210,6,4,0.23796,-0.4231,1,0.20295,-0.43669;241,2,4,0.09088,-0.54576,0,-0.001,-0.001                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                "
            //5//"24,1,7,-0.15884,0.00758,0,-0.001,-0.001;93,2,7,-0.15877,0.00736,0,-0.001,-0.001;100,1,4,0.19268,-0.24221,0,-0.001,-0.001;114,2,4,0.09088,-0.54576,0,-0.001,-0.001;122,1,2,0.2017,-0.23103,0,-0.001,-0.001;130,2,2,0.19793,-0.22469,0,-0.001,-0.001;137,1,1,0.20246,-0.44121,0,-0.001,-0.001;140,3,1,0.21143,-0.39486,2,0.24188,-0.40872;161,1,4,-0.04446,-0.29796,0,-0.001,-0.001;170,2,4,0.09088,-0.54576,0,-0.001,-0.001;184,1,4,0.10567,-0.52964,0,-0.001,-0.001;200,2,4,0.18413,-0.28641,0,-0.001,-0.001;210,6,4,0.23796,-0.4231,1,0.20295,-0.43669;241,2,4,0.09088,-0.54576,0,-0.001,-0.001                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                "

            //5//"14,1,4,-0.07265,-0.54712,0,-0.001,-0.001;50,2,4,-0.11012,-0.54689,0,-0.001,-0.001;57,1,4,-0.14563,-0.36921,0,-0.001,-0.001;80,4,4,0.25816,-0.18512,2,0.19944,-0.23754;100,2,4,0.09088,-0.54576,0,-0.001,-0.001;116,1,2,0.1991,-0.23762,0,-0.001,-0.001;129,3,2,0.24285,-0.41267,1,0.24285,-0.41267;154,2,2,-0.09625,-0.271,0,-0.001,-0.001;170,1,4,0.10827,-0.54002,0,-0.001,-0.001;196,6,4,0.23367,-0.42215,1,0.20102,-0.43642;231,2,4,-0.11012,-0.54689,0,-0.001,-0.001                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       "
            //  "14,1,7,-0.15875,0.00718,0,-0.001,-0.001;99,2,7,-0.15874,0.00745,0,-0.001,-0.001;100,1,4,0.11626,-0.32446,0,-0.001,-0.001;101,2,4,0.11393,-0.32129,0,-0.001,-0.001;104,1,2,-0.00263,-0.20617,0,-0.001,-0.001;109,3,2,-0.16681,-0.4004,1,0.20035,-0.4308;124,2,2,0.18821,-0.22122,0,-0.001,-0.001;135,1,7,0.21665,-0.24571,0,-0.001,-0.001;140,2,7,-0.15889,0.0078,0,-0.001,-0.001                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                "
            //需濾波 //77//"13,1,4,0.00873,-0.23147,0,-0.001,-0.001;28,4,4,-0.16183,-0.37458,5,-0.16835,-0.40322;46,5,4,0.115,-0.27463,1,0.08383,-0.34124;60,2,4,-0.10292,-0.20053,0,-0.001,-0.001;61,1,4,-0.11984,-0.20548,0,-0.001,-0.001;62,2,4,-0.12159,-0.22758,0,-0.001,-0.001;64,1,4,-0.13755,-0.24061,0,-0.001,-0.001;65,2,4,-0.13517,-0.23938,0,-0.001,-0.001;80,1,2,0.19937,-0.23806,0,-0.001,-0.001;89,3,2,0.06713,-0.26123,1,0.0768,-0.35312;105,2,2,0.199,-0.23747,0,-0.001,-0.001                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             "
            //需濾波//55//"22,1,5,-0.16937,-0.40511,0,-0.001,-0.001;30,2,5,-0.16934,-0.40528,0,-0.001,-0.001;39,1,4,0.06919,-0.40515,0,-0.001,-0.001;48,4,4,-0.16418,-0.37253,5,-0.16894,-0.40492;66,5,4,0.16154,-0.15513,2,0.11894,-0.19597;80,2,4,0.06638,-0.41944,0,-0.001,-0.001;89,1,2,0.11725,-0.20086,0,-0.001,-0.001;100,3,2,-0.07366,-0.111,1,-0.06731,-0.20327;116,2,2,0.1047,-0.21836,0,-0.001,-0.001;124,1,4,0.06156,-0.41119,0,-0.001,-0.001;136,6,4,-0.0585,-0.17208,1,-0.06842,-0.19789;154,2,4,0.09452,-0.38075,0,-0.001,-0.001                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            "
            //需濾波//55//"52,1,4,0.08454,-0.38265,0,-0.001,-0.001;66,4,4,-0.16204,-0.37432,5,-0.16885,-0.4057;83,5,4,0.15229,-0.16443,2,0.10367,-0.21851;95,2,4,0.07848,-0.42991,0,-0.001,-0.001;96,1,4,0.07675,-0.43341,0,-0.001,-0.001;97,2,4,0.07773,-0.43681,0,-0.001,-0.001;107,1,2,0.10376,-0.21888,0,-0.001,-0.001;117,3,2,-0.06857,-0.1312,1,-0.06822,-0.20221;133,2,2,0.12258,-0.30027,0,-0.001,-0.001;142,1,4,0.07899,-0.43627,0,-0.001,-0.001;155,6,4,-0.06738,-0.17403,1,-0.06848,-0.2;174,2,4,0.12916,-0.21498,0,-0.001,-0.001                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               "
            //需濾波//55//"40,1,4,0.09298,-0.11192,0,-0.001,-0.001;49,4,4,-0.16325,-0.36687,5,-0.16871,-0.40595;66,5,4,0.16231,-0.24987,2,0.12494,-0.29982;75,2,4,0.12914,-0.44115,0,-0.001,-0.001;90,1,2,0.1236,-0.30014,0,-0.001,-0.001;99,3,2,-0.08566,-0.09013,1,-0.06753,-0.19833;119,2,2,-0.0006,-0.37389,0,-0.001,-0.001;134,1,4,0.13586,-0.44878,0,-0.001,-0.001;147,6,4,-0.05079,-0.17611,1,-0.07002,-0.19383;162,2,4,0.14238,-0.35511,0,-0.001,-0.001                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            "
            //需濾波//55//"16,1,4,0.1271,-0.25888,0,-0.001,-0.001;30,5,4,-0.16188,-0.37145,5,-0.16763,-0.40383;33,4,4,-0.13288,-0.39132,5,-0.16762,-0.40747;48,5,4,-0.02395,-0.13014,1,-0.06989,-0.18686;68,2,4,0.1808,-0.32178,0,-0.001,-0.001;80,1,2,-0.00488,-0.37035,0,-0.001,-0.001;89,3,2,-0.0901,-0.0889,1,-0.06801,-0.19427;108,2,2,0.08373,-0.37212,0,-0.001,-0.001;120,1,4,0.1739,-0.3265,0,-0.001,-0.001;135,6,4,0.08534,-0.34857,2,0.08401,-0.37852;152,2,4,0.13711,-0.25898,0,-0.001,-0.001                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   "
            //需濾波//55後面有問題//"25,1,4,0.11285,-0.4239,0,-0.001,-0.001;42,4,4,-0.16664,-0.36401,5,-0.16914,-0.39978;65,5,4,-0.02219,-0.11904,1,-0.05246,-0.21961;91,2,4,0.03169,-0.38346,0,-0.001,-0.001;105,1,1,-0.05269,-0.2181,0,-0.001,-0.001;123,2,1,0.25523,-0.09358,0,-0.001,-0.001;126,1,1,0.24497,-0.1355,0,-0.001,-0.001;133,3,1,0.23636,-0.16489,2,0.20768,-0.22934;161,2,1,-0.05678,-0.20814,0,-0.001,-0.001;172,1,4,0.03122,-0.39077,0,-0.001,-0.001;183,2,4,0.25741,-0.08778,0,-0.001,-0.001;194,6,4,0.21104,-0.20883,2,0.20761,-0.2295;213,4,4,0.18341,-0.17546,2,0.20786,-0.22703                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               "
            //需濾波//55//"24,1,4,0.07782,-0.25234,0,-0.001,-0.001;32,4,4,0.08011,-0.14059,2,-0.02908,-0.20038;63,5,4,-0.01462,-0.28044,1,-0.04371,-0.34291;79,2,4,0.14801,-0.28417,0,-0.001,-0.001;98,1,1,-0.04344,-0.34325,0,-0.001,-0.001;121,3,1,-0.03958,-0.10947,2,-0.03444,-0.20533;150,2,1,-0.0119,-0.42643,0,-0.001,-0.001;166,1,4,0.14798,-0.28374,0,-0.001,-0.001;167,2,4,0.14748,-0.28492,0,-0.001,-0.001;168,1,4,0.14676,-0.28538,0,-0.001,-0.001;189,6,4,-0.04042,-0.18317,2,-0.03029,-0.20236;207,4,4,0.21332,-0.03333,2,-0.03034,-0.20112;215,2,4,0.15309,-0.3549,0,-0.001,-0.001                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          "
            //需濾波//77//40,1,4,-0.2081,-0.18089,0,-0.001,-0.001;49,4,4,-0.07741,-0.15182,2,0.00554,-0.19557;59,4,4,-0.15678,-0.40956,5,-0.17151,-0.4438;78,5,4,0.12392,-0.28041,1,0.08972,-0.33874;105,2,4,-0.001,-0.001,0,-0.001,-0.001;106,1,2,0.00873,-0.195,0,-0.001,-0.001;107,2,2,0.00354,-0.19454,0,-0.001,-0.001;112,1,1,0.08986,-0.34494,0,-0.001,-0.001;122,3,1,0.0005,-0.1055,2,0.00433,-0.20132;138,2,1,0.19133,-0.42942,0,-0.001,-0.001
            //需濾波//55//
            string msg = "14,1,4,0.13284,-0.54551,0,-0.001,-0.001;34,4,4,-0.15142,-0.3726,5,-0.1685,-0.4027;58,5,4,0.25247,-0.38447,1,0.19471,-0.43088;68,6,4,0.1953,-0.43022,1,0.19875,-0.43159;100,2,4,-0.11012,-0.54689,0,-0.001,-0.001";
            Console.WriteLine(msg);
            mainActionList.Clear();
            mainActionList = msg2ActionLineList(msg);

            foreach (ActionLine al in mainActionList)
                ActionLine2ListView(al);

        }
        #endregion \\---Connect Python Action recognition---//

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
