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

namespace Wpf_coffeeMaker
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();//token 可用來關閉task factory

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
            cir_pour.Fill = new SolidColorBrush(Colors.LightGray);
            cir_toggle.Fill = new SolidColorBrush(Colors.LightGray);
            if (str == "Pour")
            {
                cir_back.Fill = new SolidColorBrush(Colors.LightGray);
                cir_pour.Fill = new SolidColorBrush(Color.FromArgb(255, 140, 90, 200));
            }
            else if (str == "Toggle")
            {
                cir_back.Fill = new SolidColorBrush(Colors.LightGray);
                cir_toggle.Fill = new SolidColorBrush(Color.FromArgb(255, 200, 170, 60));
            }
            else if (str == "Background")
            {
                if (setColor)
                    cir_back.Fill = new SolidColorBrush(Color.FromArgb(255, 60, 60, 60));
                else
                    cir_back.Fill = new SolidColorBrush(Colors.DimGray);

                setColor = !setColor;
            }
            else
            {
                cir_pour.Fill = new SolidColorBrush(Colors.LightGray);
                cir_toggle.Fill = new SolidColorBrush(Colors.LightGray);
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
        #endregion //---UI---//

        public MainWindow()
        {
            InitializeComponent();

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
            UR.dynamicGrip = new UrSocketControl.ControlFunction(() =>Button_goDynamicGrip(null,null));
        }

        public static List<Objects> objects = new List<Objects>();
        private void creatObject()
        {
            objects.Add(new Objects(0, "none"));
            objects.Add(new Objects(1, "Blue cup"));
            objects.Add(new Objects(2, "Pink cup"));
            objects.Add(new Objects(3, "Pot"));
            objects.Add(new Objects(4, "Spoon"));
            objects.Add(new Objects(5, "Powder box"));
            objects.Add(new Objects(6, "Red pill box"));
            objects.Add(new Objects(7, "Green pill box"));
            objects.Add(new Objects(8, "Blue pill box"));
        }

        //public static List<myAction> actions = new List<myAction>();
        public static List<myAction> actLv = new List<myAction>();
        private void creatAction()
        {
            //actions.Add(new myAction(0, "none"));
            //actions.Add(new myAction(1, "Pick up"));
            //actions.Add(new myAction(2, "Place"));
            //actions.Add(new myAction(3, "Pour"));
            //actions.Add(new myAction(4, "Scoop"));
            //actions.Add(new myAction(5, "Add in"));
            //actions.Add(new myAction(6, "Stir"));
            actLv.Add(new myAction(0, "none"));
            actLv.Add(myActionAdder.Pick());
            actLv.Add(myActionAdder.Place());
            actLv.Add(myActionAdder.Pour());
            actLv.Add(myActionAdder.Scoop());
            actLv.Add(myActionAdder.AddIn());
            actLv.Add(myActionAdder.Stir());
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
        }
     

        #region //---griping mode---//
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

        #endregion ---griping mode---

        //UR
        #region  //---UR server---//
        UrSocketControl UR = new UrSocketControl();
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
        #endregion //---UR server---//

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

        #endregion //---Record---//

        #region //---Play path---//
        private void Button_goPosHome_Click(object sender, RoutedEventArgs e)
        {
            //UR.goToFilePos("Home.pos");
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
            UR.goFile(UR.rootPath + Cb_Path.SelectedValue.ToString());
        }
        #endregion //---Play path---//

        //action base
        #region //--- Action base Control---//
        List<ActionBaseList> ActionList = new List<ActionBaseList>();
        static bool startDemo = false;
        private void Button_startRecord_Click(object sender, RoutedEventArgs e)
        {
            LV_actionBase.Items.Clear();

            Rect_actionBaseTopColor.Fill = new SolidColorBrush(Color.FromArgb(200, 40, 210, 80));
            startDemo = true;
        }
        private void Button_endDemo_Click(object sender, RoutedEventArgs e)
        {
            if (startDemo == false)
            {
                MessageBox.Show("已經結束了");
                return;
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
            string fileName = "testtt";

            //必須放新的物件座標!! 才能計算
            for (int i = 0; i < mainAction.Count(); i++)
            {
                if (mainAction[i].target.Name != "pos")//如果不是pos才要更新 //基本上不會發生
                    mainAction[i].target.nowPos = objects[mainAction[i].target.index].getPosition();
                if (mainAction[i].destination.Name != "pos")//如果不是pos才要更新 //也就是像place到某個點
                    mainAction[i].destination.nowPos = objects[mainAction[i].destination.index].getPosition();
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
                ab = new ActionBaseAdder(al.Action.Name, al.target.Name, al.destination.getPosition().ToString("(3)"), new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Black));
            else
                ab = new ActionBaseAdder(al.Action.Name, al.target.Name, al.destination.Name, new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Black));
            this.LV_actionBase.Items.Add(ab);
        }

        public void ActionBase2Cmd(List<ActionLine> actLine, string fileName)
        {
            if (fileName.IndexOf(".path") < 0)
                fileName += ".path";
            StreamWriter txt;
            txt = new StreamWriter($"Path//{fileName}", false);
            foreach (ActionLine al in actLine)
                foreach (string str in al.getCmdText())
                    txt.WriteLine(str);
            txt.Flush();
            txt.Close();
        }

        #endregion //--- Action base Control---//

        #region //---Connect Python Action recognition---//
        private void Button_addPour_Click(object sender, RoutedEventArgs e)
        {
            //if (startDemo == true)
            //{
            //    if (nowAct == "Pour")
            //        return;
            //    if (handing == cups[0])
            //    {
            //        ActionList.Add(new ActionBaseList(myActionAdder.Name.Pour, cups[0].Name, new SolidColorBrush(Colors.Black), new SolidColorBrush(cups[0].color)));
            //        LV_actionBase.Items.Add(ActionList[ActionList.Count() - 1]);
            //        ActionList.Add(new ActionBaseList("    to", cups[1].Name, new SolidColorBrush(Colors.Black), new SolidColorBrush(cups[1].color)));
            //        LV_actionBase.Items.Add(ActionList[ActionList.Count() - 1]);
            //    }
            //    else if (handing == cups[1])
            //    {
            //        ActionList.Add(new ActionBaseList(myActionAdder.Name.Pour, cups[1].Name, new SolidColorBrush(Colors.Black), new SolidColorBrush(cups[1].color)));
            //        LV_actionBase.Items.Add(ActionList[ActionList.Count() - 1]);
            //        ActionList.Add(new ActionBaseList("    to", cups[0].Name, new SolidColorBrush(Colors.Black), new SolidColorBrush(cups[0].color)));
            //        LV_actionBase.Items.Add(ActionList[ActionList.Count() - 1]);
            //    }
            //    nowAct = "Pour";
            //}

        }
        bool evil_toggleOnce = false;
        private void Button_addToggle_Click(object sender, RoutedEventArgs e)
        {
            //if (startDemo == true)
            //{
            //    if (evil_toggleOnce == true)
            //        return;
            //    if (nowAct == "Toggle")
            //        return;
            //    ActionList.Add(new ActionBaseList("Place", handing.Name, new SolidColorBrush(Colors.Black), new SolidColorBrush(handing.color)));
            //    LV_actionBase.Items.Add(ActionList[ActionList.Count() - 1]);
            //    //if (handing.Distanse(dripTrayPos) < 0.02)//代表在drip tray上
            //    //{
            //    //    ActionList.Add(new ActionBaseList("     to", subactInfo.place.DripTray.ToString(), new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Black)));
            //    //    LV_actionBase.Items.Add(ActionList[ActionList.Count() - 1]);
            //    //}
            //    //else
            //    //{
            //    //    ActionList.Add(new ActionBaseList("     to", (handing.getNowPos()).ToString("mm", "3(", "0"), new SolidColorBrush(Colors.Black), new SolidColorBrush(handing.color)));
            //    //    LV_actionBase.Items.Add(ActionList[ActionList.Count() - 1]);
            //    //}
            //    handing = machine;
            //    ActionList.Add(new ActionBaseList(myActionAdder.Name.Trigger, "", new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Black)));
            //    LV_actionBase.Items.Add(ActionList[ActionList.Count() - 1]);
            //    nowAct = "Toggle";

            //    evil_toggleOnce = true;
            //    cir_toggleOnce.Fill = new SolidColorBrush(Colors.Salmon);
            //}

        }
        string nowAct = "Background";
        private void Button_askActionRecognition_Click(object sender, RoutedEventArgs e)
        {
            ezTCP TCP = new ezTCP();
            setActiionCircle("");
            Task.Run(() =>
            {
                if (!TCP.creatClient("192.168.1.102", 777))
                    return;

                this.Dispatcher.Invoke((Action)(() => { cir_connectAct.Fill = new SolidColorBrush(Colors.DarkSeaGreen); }));
                while (true)
                {
                    try
                    {
                        TCP.client_SendData("hey");
                        string msg = TCP.client_ReadData();
                        if (msg == "")
                            break;

                        if (msg == "Pour")
                            this.Dispatcher.Invoke((Action)(() => { Button_addPour_Click(null, null); setActiionCircle("Pour"); }));
                        else if (msg == "Toggle")
                            this.Dispatcher.Invoke((Action)(() => { Button_addToggle_Click(null, null); setActiionCircle("Toggle"); }));
                        else if (msg == "Background")
                            this.Dispatcher.Invoke((Action)(() => { nowAct = "Background"; setActiionCircle("Background"); }));

                        Thread.Sleep(0);
                    }
                    catch
                    {
                        break;
                    }

                }
                Console.WriteLine("client 中斷");
                this.Dispatcher.Invoke((Action)(() => { cir_connectAct.Fill = new SolidColorBrush(Colors.Salmon); setActiionCircle(""); }));
            });

        }
        #endregion //---Connect Python Action recognition---//

        #region //---RealSense and gripMode---//
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

        //偵測到的物件資訊
        PointF center_obj = new PointF();
        float angel_obj = 0;
        SizeF size_obj = new Size();
        float armMoveX;
        float armMoveY;
        private void StartProcessingBlock(CustomProcessingBlock processingBlock, PipelineProfile pp, Action<VideoFrame> updateColor, Pipeline pipeline)
        {
            showType = imgType.depth;

            Size RS_depthSize = new Size(1280, 720);
            Mat processMat = new Mat(RS_depthSize, DepthType.Cv8U, 3);
            if (showType == imgType.color_full)
            {
                //depth Processing
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
                var t = Task.Factory.StartNew(() =>
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

            //MIXXXXXX
            if (showType == imgType.mix)
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

            //depth
            if (showType == imgType.depth)
            {
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
                var t = Task.Factory.StartNew(() =>//執行續  ， 裡面執行 processing block
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

                            int thres = 80;
                            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => { thres = tb_value_depthThres.Text.toInt(); }));
                            unsafe
                            {
                                byte* pixelPtr_byte = (byte*)img_depth.DataPointer;
                                for (int i = 0; i < 1280; i++)
                                    for (int j = 0; j < 720; j++)
                                    {
                                        int value = (int)(((posMap[i, j, 2] * 1000) - 300) * 1 + 200);
                                        value = 255 - value;//反向 讓越高越白
                                        if (value > 255)
                                            value = 255;
                                        if (value < 0)
                                            value = 0;


                                        if (value == 255)
                                            value = 0;//去除一些 原本測不到的(太近的，黑色)
                                        if (value > thres)
                                            value = 128;
                                        if (value < thres)
                                            value = 0;

                                        pixelPtr_byte[j * 1280 + i] = (byte)value;
                                    }
                            }

                            Point grip_center = new Point(710, 460);//夾爪中心，如果我找到..則旋轉會不影響中心點

                            Mat mat_img_show = new Mat(RS_depthSize, DepthType.Cv8U, 3);
                            Mat img_depth_ch3 = new Mat(RS_depthSize, DepthType.Cv8U, 3);
                            CvInvoke.CvtColor(img_depth, img_depth_ch3, ColorConversion.Gray2Bgr);
                            CvInvoke.CvtColor(img_depth, mat_img_show, ColorConversion.Gray2Bgr);

                            //畫線條 有碰到的地方就會被填滿，不然只會有一點，離開那一點就找不到物件了
                            int size = 80;
                            CvInvoke.Line(mat_img_show, new Point(grip_center.X + size, grip_center.Y), new Point(grip_center.X - size, grip_center.Y), new MCvScalar(128, 128, 128));
                            CvInvoke.Line(mat_img_show, new Point(grip_center.X, grip_center.Y + size), new Point(grip_center.X, grip_center.Y - size), new MCvScalar(128, 128, 128));
                            CvInvoke.Rectangle(mat_img_show, new Rectangle(grip_center.X - size, grip_center.Y - size, size * 2, size * 2), new MCvScalar(128, 128, 128));

                            //int _x = grip_center.X - 100;
                            //int _y = grip_center.Y - 100;
                            //Console.WriteLine($"({_x},{_y}) = {posMap[_x,_y,2]}");
                            // _x = grip_center.X + 100;
                            // _y = grip_center.Y - 100;
                            //Console.WriteLine($"({_x},{_y}) = {posMap[_x, _y, 2]}");
                            // _x = grip_center.X + 100;
                            // _y = grip_center.Y + 100;
                            //Console.WriteLine($"({_x},{_y}) = {posMap[_x, _y, 2]}");
                            // _x = grip_center.X - 100;
                            // _y = grip_center.Y + 100;
                            //Console.WriteLine($"({_x},{_y}) = {posMap[_x, _y, 2]}");
                            //Console.WriteLine($"-------------------------------");


                            //if (MyInvoke.GetValue<byte>(img_depth, 360, 700) != 0)
                            //{

                            Mat filling_mask = new Mat(RS_depthSize.Height + 2, RS_depthSize.Width + 2, DepthType.Cv8U, 1);
                            filling_mask.SetTo(new MCvScalar(0, 0, 0));
                            CvInvoke.FloodFill(mat_img_show, filling_mask, grip_center, new MCvScalar(70, 100, 250), out Rectangle rect, new MCvScalar(10, 10, 10), new MCvScalar(10, 10, 10));

                            //CvInvoke.Rectangle(mat_img_show, rect,new MCvScalar(200,50,20),3);
                            //CvInvoke.FloodFill(img_depth, filling_mask, new Point(700, 360), new MCvScalar(255), out Rectangle rect, new MCvScalar(10), new MCvScalar(10));
                            //}
                            Mat element = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new Size(3, 3), new Point(-1, -1));
                            CvInvoke.Erode(mat_img_show, mat_img_show, element, new Point(-1, -1), 2, BorderType.Default, new MCvScalar(0, 0, 0));
                            CvInvoke.Dilate(mat_img_show, mat_img_show, element, new Point(-1, -1), 4, BorderType.Default, new MCvScalar(0, 0, 0));

                            //找物件mask
                            Mat mat_img_object_mask = new Mat(RS_depthSize, DepthType.Cv8U, 1);
                            CvInvoke.InRange(mat_img_show, new ScalarArray(new MCvScalar(70, 100, 250)), new ScalarArray(new MCvScalar(70, 100, 250)), mat_img_object_mask);


                            // CvInvoke.CvtColor(mat_img_object_mask, mat_img_show, ColorConversion.Gray2Bgr);

                            //畫出中心
                            // CvInvoke.Circle(mat_img_show, new Point(700, 360), 5, new MCvScalar(20, 50, 200), -1);
                            CvInvoke.Line(mat_img_show, new Point(grip_center.X + size, grip_center.Y), new Point(grip_center.X - size, grip_center.Y), new MCvScalar(20, 50, 200), 2);
                            CvInvoke.Line(mat_img_show, new Point(grip_center.X, grip_center.Y + size), new Point(grip_center.X, grip_center.Y - size), new MCvScalar(20, 50, 200), 2);
                            CvInvoke.Rectangle(mat_img_show, new Rectangle(grip_center.X - size, grip_center.Y - size, size * 2, size * 2), new MCvScalar(20, 50, 200), 2);

                            //重心
                            //MCvMoments mu = CvInvoke.Moments(mat_img_object_mask);
                            // Point cP = new Point((int)(mu.M10 / mu.M00), (int)(mu.M01 / mu.M00));
                            //CvInvoke.Circle(mat_img_show, cP, 10, new MCvScalar(100, 50, 200), 3);

                            //畫最小矩形

                            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
                            {
                                CvInvoke.FindContours(mat_img_object_mask, contours, null, RetrType.External, ChainApproxMethod.ChainApproxSimple);
                                for (int i = 0; i < contours.Size; i++)
                                {
                                    using (VectorOfPoint contour = contours[i])
                                    {
                                        if (contour.Size < 100)
                                            continue;
                                        RotatedRect BoundingBox = CvInvoke.MinAreaRect(contour);
                                        angel_obj = BoundingBox.Angle;
                                        center_obj = BoundingBox.Center;
                                        size_obj = BoundingBox.Size;
                                        CvInvoke.Circle(mat_img_show, new Point((int)BoundingBox.Center.X, (int)BoundingBox.Center.Y), 10, new MCvScalar(100, 50, 200), 3);

                                        CvInvoke.Polylines(mat_img_show, Array.ConvertAll(BoundingBox.GetVertices(), Point.Round), true, new MCvScalar(50, 180, 200), 3);
                                    }
                                }
                            }

                            //手臂要移動的距離(注意座標轉換和scaling)
                            float xg = posMap[(int)grip_center.X, (int)grip_center.Y, 0];
                            float yg = posMap[(int)grip_center.X, (int)grip_center.Y, 1];

                            float xo = posMap[(int)center_obj.X, (int)center_obj.Y, 0];
                            float yo = posMap[(int)center_obj.X, (int)center_obj.Y, 1];

                            //float armMoveX = (center_obj.X - grip_center.X);
                            //float armMoveY = (grip_center.Y - center_obj.Y);

                            armMoveX = (xg - xo);
                            armMoveY = (yo - yg);

                            if (size_obj.Width > size_obj.Height)
                                angel_obj = angel_obj + 90;
                            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => { tb_object_msg.Text = $"distanse:({armMoveX * 1000},{armMoveY * 1000}),\n degree:{angel_obj},\n size:{size_obj.Width},{size_obj.Height}"; }));

                            //CvInvoke.Add(mat_img_show,img_depth_ch3, mat_img_show);

                            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => { Img_main.Source = BitmapSourceConvert.ToBitmapSource(mat_img_show); }));

                        }
                    }
                }, token);
            }//if type == depth
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            //int x = (int)e.GetPosition(grid_img_grip).X;
            //int y = (int)e.GetPosition(grid_img_grip).Y;
            //Console.WriteLine($"mouse at({x},{y})");
            //Console.WriteLine($"RS position({posMap[x * 2, y * 2, 0]},{posMap[x * 2, y * 2, 1]},{posMap[x * 2, y * 2, 2]})");
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
        private void Rb_imgShow_Checked(object sender, RoutedEventArgs e)
        {
            // _tokenSource.Cancel();
            if (((RadioButton)sender).Content.ToString() == "Mix")
            {
                showType = imgType.mix;
            }
            else if (((RadioButton)sender).Content.ToString() == "Color")
            {
                showType = imgType.color_full;
            }
        }

        private void Button_goDynamicGrip(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                //UR.getPosition(out URCoordinates nowPos);//等等 沒意義阿??
                UR.goRelativePosition(armMoveX.M(), armMoveY.M());
                UR.goRelativeJoint(j6: (angel_obj - 10).deg());//-10 是因為原本攝影機跟夾爪就有10度
            });


        }
        #endregion

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            UR.creatClient("192.168.1.108");
            string str = "movep(p[0.0,-0.22,0.2,3.14,0,0])\n";
            UR.client_SendData(str);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            UR.goFile("testttt");
        }

        private void Btn_adminWindow_Click(object sender, RoutedEventArgs e)
        {
            adminWindow adminWindow = new adminWindow();
            adminWindow.Show();
        }

        public void Btn_addSimAction_Click(object sender, RoutedEventArgs e)
        {
        }
        public string LabelText
        {
            get
            {
                return this.tb_object_msg.Text;
            }
            set
            {
                this.tb_object_msg.Text = value;
            }
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

    public class ActionBaseList
    {
        public ActionBaseList(string action, string detial, SolidColorBrush C1, SolidColorBrush C2)
        {
            Action = action;
            Detial = detial;
            Color1 = (C1);
            Color2 = (C2);
        }
        public string Action { get; set; }
        public string Detial { get; set; }
        public SolidColorBrush Color1 { get; set; } = new SolidColorBrush(Colors.Black);
        public SolidColorBrush Color2 { get; set; } = new SolidColorBrush(Colors.Black);
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
