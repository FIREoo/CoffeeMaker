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

        Mat matrix;

        static YoloWrapper yoloWrapper;
        static IEnumerable<YoloItem> items;

        Mat mat_cup = new Mat(100, 500, DepthType.Cv8U, 3);

        public static int timeTick = 0;
        public static Objects[] cups = new Objects[2];
        public static Objects machine = new Objects();
        public static Objects handing = new Objects();

        ActionBase actionBase;

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

            matrix = CvInvoke.GetPerspectiveTransform(src, dst);
            creatObject();

            UR.stateChange += OnUrStateChange;

            actionBase = new ActionBase("demoAct.path");
        }
        private void creatObject()
        {
            machine.Name = "machine";

            cups[0] = new Objects();
            cups[0].Name = "blue cup";
            cups[0].color = System.Windows.Media.Color.FromArgb(255, 103, 167, 184);

            cups[1] = new Objects();
            cups[1].Name = "pink cup";
            cups[1].color = System.Windows.Media.Color.FromArgb(255, 205, 130, 150);
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
            showType = imgType.color_full;
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


        private void StartProcessingBlock(CustomProcessingBlock processingBlock, PipelineProfile pp, Action<VideoFrame> updateColor, Pipeline pipeline)
        {
            showType = imgType.color_full;

            float[,,] posMap = new float[1280, 720, 3];//整張距離圖
            float[,,] posMap2 = new float[1280, 720, 3];//整張距離圖

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


            //start
            //var token = _tokenSource.Token;
            //Action<VideoFrame> updateOriginColor = UpdateImage(Img_main);
            //var t = Task.Factory.StartNew(() =>
            //{
            //    Mat color_orig = new Mat(RS_depthSize, DepthType.Cv8U, 3);
            //    Mat color_resize = new Mat(RS_depthSize, DepthType.Cv8U, 3);

            //    yoloWrapper = new YoloWrapper("modle\\yolov3-tiny-3obj.cfg", "modle\\yolov3-tiny-3obj_3cup.weights", "modle\\obj.names");
            //    string detectionSystemDetail = string.Empty;
            //    if (!string.IsNullOrEmpty(yoloWrapper.EnvironmentReport.GraphicDeviceName))
            //        detectionSystemDetail = $"({yoloWrapper.EnvironmentReport.GraphicDeviceName})";
            //    Console.WriteLine($"Detection System:{yoloWrapper.DetectionSystem}{detectionSystemDetail}");

            //    while (!token.IsCancellationRequested)
            //    {
            //        using (var frames = pipeline.WaitForFrames())
            //        {
            //            if (showType == imgType.color)
            //            {
            //                VideoFrame color_frame = frames.ColorFrame.DisposeWith(frames);
            //                color_frame.CopyTo(color_orig.DataPointer);

            //                timeTick++;
            //                color_frame.CopyFrom(color_resize.DataPointer);
            //                Dispatcher.Invoke(DispatcherPriority.Render, updateOriginColor, color_frame);
            //            }
            //            else if (showType == imgType.mix)//顯示 mix 圖
            //            {
            //                processingBlock.ProcessFrames(frames);

            //            }

            //        }
            //    }
            //}, token);
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
            evil_toggleOnce = false;
            cir_toggleOnce.Fill = new SolidColorBrush(Colors.Gray);

            handing = new Objects();
            LV_actionBase.Items.Clear();
            MyInvoke.setToZero(ref mat_cup);
            timeTick = 0;

            ActionList.Clear();
            Rect_actionBaseTopColor.Fill = new SolidColorBrush(Color.FromArgb(200, 40, 210, 40));
            startDemo = true;
        }
        private void Button_endDemo_Click(object sender, RoutedEventArgs e)
        {
            if (startDemo == false)
            {
                MessageBox.Show("已經結束了");
                return;
            }

            ActionList.Add(new ActionBaseList("Place", handing.Name, new SolidColorBrush(Colors.Black), new SolidColorBrush(handing.color)));
            LV_actionBase.Items.Add(ActionList[ActionList.Count() - 1]);
            //if (handing.Distanse(dripTrayPos) < dripTrayD)//代表在drip tray上
            //{
            //    ActionList.Add(new ActionBaseList("     to", subactInfo.place.DripTray.ToString(), new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Black)));
            //    LV_actionBase.Items.Add(ActionList[ActionList.Count() - 1]);
            //}
            //else
            //{
            //    ActionList.Add(new ActionBaseList("     to", (handing.getNowPos()).ToString("mm", "3(", "0"), new SolidColorBrush(Colors.Black), new SolidColorBrush(handing.color)));
            //    LV_actionBase.Items.Add(ActionList[ActionList.Count() - 1]);
            //}
            Rect_actionBaseTopColor.Fill = new SolidColorBrush(Colors.Transparent);

            startDemo = false;
        }
        private void Button_creatAction_Click(object sender, RoutedEventArgs e)
        {
            if (startDemo == true) //need startDemo == false
            {
                MessageBox.Show("尚未結束示範，請按下[End Demo]按鈕");
                return;
            }
            actionBase.start(cups);

            for (int i = 0; i < ActionList.Count(); i++)
            {

                if (ActionList[i].Action == Subact.Name.Pick)
                {
                    if (ActionList[i].Detial == subactInfo.place.DripTray.ToString())
                        actionBase.add(Subact.Pick(subactInfo.place.DripTray));
                    else if (ActionList[i].Detial == subactInfo.thing.Case.ToString())
                        actionBase.add(Subact.Pick(subactInfo.thing.Case));
                    else
                        foreach (Objects obj in cups)
                            if (obj.Name == ActionList[i].Detial)
                                actionBase.add(Subact.Pick(obj));
                }
                else if (ActionList[i].Action == Subact.Name.Place)
                {
                    foreach (Objects obj in cups)
                        if (obj.Name == ActionList[i].Detial)
                        {
                            i++;
                            string detial = ActionList[i].Detial;
                            if (detial == subactInfo.place.DripTray.ToString())
                                actionBase.add(Subact.Place(subactInfo.place.DripTray));
                            else
                            {
                                detial = detial.Substring(1, detial.Length - 2);
                                string[] pos = detial.Split(',');
                                //actionBase.add(Subact.Place(obj, new URCoordinates(float.Parse(pos[0]) / 1000f, float.Parse(pos[1]) / 1000f, float.Parse(pos[2]) / 1000f, 0, 0, 0)));
                            }

                        }
                }
                else if (ActionList[i].Action == Subact.Name.Pour)
                {

                    if (cups[0].Name == ActionList[i].Detial)
                    {
                        actionBase.add(Subact.Pour(cups[1]));
                    }
                    else if (cups[1].Name == ActionList[i].Detial)
                    {
                        actionBase.add(Subact.Pour(cups[0]));
                    }
                    else
                    {
                        MessageBox.Show("error!!! 不是杯子");
                    }
                    i++;
                }
                else if (ActionList[i].Action == Subact.Name.Trigger)
                {
                    actionBase.add(Subact.Trigger());
                }
                else if (ActionList[i].Action == Subact.Name.PutBoxIn)
                {
                    actionBase.add(Subact.PutBoxIn());
                }

            }
            actionBase.saveFile();

        }
        #endregion //--- Action base Control---//

        #region //---Connect Python Action recognition---//
        private void Button_addPour_Click(object sender, RoutedEventArgs e)
        {
            if (startDemo == true)
            {
                if (nowAct == "Pour")
                    return;
                if (handing == cups[0])
                {
                    ActionList.Add(new ActionBaseList(Subact.Name.Pour, cups[0].Name, new SolidColorBrush(Colors.Black), new SolidColorBrush(cups[0].color)));
                    LV_actionBase.Items.Add(ActionList[ActionList.Count() - 1]);
                    ActionList.Add(new ActionBaseList("    to", cups[1].Name, new SolidColorBrush(Colors.Black), new SolidColorBrush(cups[1].color)));
                    LV_actionBase.Items.Add(ActionList[ActionList.Count() - 1]);
                }
                else if (handing == cups[1])
                {
                    ActionList.Add(new ActionBaseList(Subact.Name.Pour, cups[1].Name, new SolidColorBrush(Colors.Black), new SolidColorBrush(cups[1].color)));
                    LV_actionBase.Items.Add(ActionList[ActionList.Count() - 1]);
                    ActionList.Add(new ActionBaseList("    to", cups[0].Name, new SolidColorBrush(Colors.Black), new SolidColorBrush(cups[0].color)));
                    LV_actionBase.Items.Add(ActionList[ActionList.Count() - 1]);
                }
                nowAct = "Pour";
            }

        }
        bool evil_toggleOnce = false;
        private void Button_addToggle_Click(object sender, RoutedEventArgs e)
        {
            if (startDemo == true)
            {
                if (evil_toggleOnce == true)
                    return;
                if (nowAct == "Toggle")
                    return;
                ActionList.Add(new ActionBaseList("Place", handing.Name, new SolidColorBrush(Colors.Black), new SolidColorBrush(handing.color)));
                LV_actionBase.Items.Add(ActionList[ActionList.Count() - 1]);
                //if (handing.Distanse(dripTrayPos) < 0.02)//代表在drip tray上
                //{
                //    ActionList.Add(new ActionBaseList("     to", subactInfo.place.DripTray.ToString(), new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Black)));
                //    LV_actionBase.Items.Add(ActionList[ActionList.Count() - 1]);
                //}
                //else
                //{
                //    ActionList.Add(new ActionBaseList("     to", (handing.getNowPos()).ToString("mm", "3(", "0"), new SolidColorBrush(Colors.Black), new SolidColorBrush(handing.color)));
                //    LV_actionBase.Items.Add(ActionList[ActionList.Count() - 1]);
                //}
                handing = machine;
                ActionList.Add(new ActionBaseList(Subact.Name.Trigger, "", new SolidColorBrush(Colors.Black), new SolidColorBrush(Colors.Black)));
                LV_actionBase.Items.Add(ActionList[ActionList.Count() - 1]);
                nowAct = "Toggle";

                evil_toggleOnce = true;
                cir_toggleOnce.Fill = new SolidColorBrush(Colors.Salmon);
            }

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ListViewItem lbi = LV_actionBase.ItemContainerGenerator.ContainerFromIndex(3) as ListViewItem;
            lbi.Foreground = new SolidColorBrush(Colors.Red);
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
