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
using UrRobot;
using System.IO;

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
            color = 2
        }

        Mat matrix;

        static YoloWrapper yoloWrapper;
        static IEnumerable<YoloItem> items;

        Mat mat_cup = new Mat(100, 500, DepthType.Cv8U, 3);

        public static int timeTick = 0;
        public static Objects[] cups = new Objects[2];
        public static Objects machine = new Objects();
        public static Objects handing = new Objects();

        #region //---UI---//
        private void setConnectCircle(int S)
        {
            cir_UrState_off.Fill = new SolidColorBrush(Colors.LightGray);
            cir_UrState_connecting.Fill = new SolidColorBrush(Colors.LightGray);
            cir_UrState_on.Fill = new SolidColorBrush(Colors.LightGray );
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
        }
        private void creatObject()
        {
            machine.Name = "machine";

            cups[0] = new Objects();
            cups[0].Name = "blue cup";
            //cups[0].color = Color.FromArgb(153, 217, 234);

            cups[1] = new Objects();
            cups[1].Name = "pink cup";
            // cups[1].color = Color.FromArgb(255, 180, 200);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //Setup / start camera
                CameraStart();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Application.Current.Shutdown();
            }
        }
        private void CameraStart()
        {
            // Setup config settings
            var cfg = new Config();
            cfg.EnableStream(Stream.Depth, 1280, 720, Format.Z16, 30);
            cfg.EnableStream(Stream.Color, 1280, 720, Format.Bgr8, 30);

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
            if (showType == imgType.color)
            {
                processingBlock = new CustomProcessingBlock((f, src) =>
                {
                    using (var releaser = new FramesReleaser())
                    {
                        using (var frames = pipeline.WaitForFrames().DisposeWith(releaser))
                        {
                            var processedFrames = frames
                            .ApplyFilter(align_to).DisposeWith(releaser);
                            // Send it to the next processing stage
                            src.FramesReady(processedFrames);
                        }
                    }
                });
            }
            else if (showType == imgType.mix)
            {
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
            }

            return processingBlock;
        }


        private void StartProcessingBlock(CustomProcessingBlock processingBlock, PipelineProfile pp, Action<VideoFrame> updateColor, Pipeline pipeline)
        {
            PointF thePoint = new PointF();
            float[] thePos = new float[3];
            float[,,] posMap = new float[1280, 720, 3];

            Size RS_depthSize = new Size(1280, 720);
            Mat processMat = new Mat(RS_depthSize, DepthType.Cv8U, 3);
            // Register to results of processing via a callback:
            processingBlock.Start(f =>
            {
                using (var frames = FrameSet.FromFrame(f))
                {
                    var color_frame = frames.ColorFrame.DisposeWith(frames);
                    var depthintr = (pp.GetStream(Stream.Depth) as VideoStreamProfile).GetIntrinsics();
                    var depth_frame = frames.DepthFrame.DisposeWith(frames);

                    System.Windows.PointConverter pointConverter = new System.Windows.PointConverter();


                    color_frame.CopyTo(processMat.DataPointer);

                    //float depth = depth_frame.GetDistance((int)thePoint.X,(int)thePoint.Y); //From
                    //thePos = HelperClass.DeprojectPixelToPoint(depthintr, thePoint, depth);

                    for (int i = 0; i < 1280; i++)
                        for (int j = 0; j < 720; j++)
                        {
                            float depth = depth_frame.GetDistance(i, j); //From
                            var tmpF = HelperClass.DeprojectPixelToPoint(depthintr, new PointF(i, j), depth);
                            posMap[i, j, 0] = tmpF[0];
                            posMap[i, j, 1] = tmpF[1];
                            posMap[i, j, 2] = tmpF[2];
                        }

                    // Dispatcher.Invoke(DispatcherPriority.Render, updateColor, color_frame);//顯示用
                    //Console.WriteLine($"depth({pos[0]},{pos[1]},{pos[2]})");
                    // Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => DistanceTB.Text = "Distance: " + Math.Round(distance * 100, 2) + " cm"));
                }
            });


            //start
            var token = _tokenSource.Token;
            Action<VideoFrame> updateOriginColor = UpdateImage(Img_main);
            var t = Task.Factory.StartNew(() =>
            {
                Mat color_orig = new Mat(RS_depthSize, DepthType.Cv8U, 3);
                Mat color_resize = new Mat(RS_depthSize, DepthType.Cv8U, 3);

                yoloWrapper = new YoloWrapper("modle\\yolov3-tiny-3obj.cfg", "modle\\yolov3-tiny-3obj_3cup.weights", "modle\\obj.names");
                string detectionSystemDetail = string.Empty;
                if (!string.IsNullOrEmpty(yoloWrapper.EnvironmentReport.GraphicDeviceName))
                    detectionSystemDetail = $"({yoloWrapper.EnvironmentReport.GraphicDeviceName})";
                Console.WriteLine($"Detection System:{yoloWrapper.DetectionSystem}{detectionSystemDetail}");


                while (!token.IsCancellationRequested)
                {
                    using (var frames = pipeline.WaitForFrames())
                    {
                        if (showType == imgType.color)
                        {
                            VideoFrame color_frame = frames.ColorFrame.DisposeWith(frames);
                            color_frame.CopyTo(color_orig.DataPointer);

                            CvInvoke.WarpPerspective(color_orig, color_resize, matrix, new Size(1280, 720));


                            CvInvoke.Imwrite("yolo1.png", color_resize);
                            try { items = yoloWrapper.Detect(@"yolo1.png"); }
                            catch { break; }
                            CvInvoke.CvtColor(color_resize, color_resize, ColorConversion.Bgr2Rgb);
                            processingBlock.ProcessFrames(frames);

                            foreach (YoloItem item in items)
                            {
                                string name = item.Type;
                                int x = item.X;
                                int y = item.Y;
                                int H = item.Height;
                                int W = item.Width;
                                Point center = item.Center();

                                CvInvoke.PutText(color_resize, name, new Point(x, y), FontFace.HersheySimplex, 1, new MCvScalar(50, 230, 230));
                                CvInvoke.PutText(color_resize, item.Confidence.ToString("0.0"), new Point(x, y - 20), FontFace.HersheySimplex, 0.5, new MCvScalar(50, 230, 230));
                                CvInvoke.Rectangle(color_resize, new Rectangle(x, y, W, H), new MCvScalar(50, 230, 230), 3);

                                float[] objPos = new float[] { posMap[center.X, center.Y, 0], posMap[center.X, center.Y, 1], posMap[center.X, center.Y, 2] };

                                if (name == "blue cup")//index 0
                                {
                                    process_actionOfCups(cups[0], mat_cup, TB_Bcup_msg, TB_Bcup_state, objPos, 10, 40);
                                    //this.Dispatcher.Invoke((Action)(() => { TB_Bcup.Text = $"({posMap[center.X, center.Y, 0].ToString("0.000")},{posMap[center.X, center.Y, 1].ToString("0.000")},{posMap[center.X, center.Y, 2].ToString("0.000")})"; }));
                                    CvInvoke.Circle(color_resize, center, 10, new MCvScalar(200, 200, 20), -1);
                                }
                                else if (name == "pink cup")//index 1
                                {
                                    process_actionOfCups(cups[1], mat_cup, TB_Pcup_msg, TB_Pcup_state, objPos, 60, 90);
                                    //this.Dispatcher.Invoke((Action)(() => { TB_Pcup.Text = $"({posMap[center.X, center.Y, 0].ToString("0.000")},{posMap[center.X, center.Y, 1].ToString("0.000")},{posMap[center.X, center.Y, 2].ToString("0.000")})"; }));
                                    CvInvoke.Circle(color_resize, center, 10, new MCvScalar(200, 200, 20), -1);
                                }

                            }//foreach cups 
                            timeTick++;
                            this.Dispatcher.Invoke((Action)(() =>
                            {
                                img_cupState.Source = BitmapSourceConvert.ToBitmapSource(mat_cup);
                            }));
                            color_frame.CopyFrom(color_resize.DataPointer);
                            Dispatcher.Invoke(DispatcherPriority.Render, updateOriginColor, color_frame);
                        }
                        else if (showType == imgType.mix)//顯示 mix 圖
                        {
                            processingBlock.ProcessFrames(frames);
                        }
                    }
                }
            }, token);
        }
        void process_actionOfCups(Objects detectObject, Mat mat, TextBlock label_msg, TextBlock label_s, float[] pos, int drawY1, int drawY2)
        {
            detectObject.setPos_m(pos[0], pos[1], pos[2]);
            Objects.states s = detectObject.State();

            if (s == Objects.states.stop)
            {
                CvInvoke.Line(mat, new Point(timeTick, drawY1), new Point(timeTick, drawY2), new MCvScalar(50, 50, 50));
            }
            else if (s == Objects.states.move)
            {
                CvInvoke.Line(mat, new Point(timeTick, drawY1), new Point(timeTick, drawY2), new MCvScalar(50, 150, 150));
                if (handing == detectObject)//如果拿著的東西跟移動的東西一樣，代表繼續移動
                { }
                else//如果拿著的東西跟移動的東西"不"一樣，代表要新增東西
                {
                    //if (handing == new Objects())//代表第一次
                    if (cups.All(cup => cup != handing))//代表前面不是杯子 這樣才會有place
                    { }
                    else//代表前面有東西
                    {
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            LV_actionBase.Items.Add(new ActionBaseList("Place", handing.Name));
                        }));
                        //ActionBaseAdd("Place", handing.Name, color1: Color.FromArgb(240, 230, 176), color2: handing.color);
                    }
                    this.Dispatcher.Invoke((Action)(() =>
                    {
                        LV_actionBase.Items.Add(new ActionBaseList("Pick up", detectObject.Name));
                    }));
                    // ActionBaseAdd("Pick up", detectObject.Name, color1: Color.FromArgb(255, 200, 14), color2: detectObject.color);
                }
                handing = detectObject;
            }

            this.Dispatcher.Invoke((Action)(() =>
            {
                label_s.Text = s.ToString();
                label_msg.Text = ($"{ (detectObject.getX_m() * 1000).ToString("0.0")},{(detectObject.getY_m() * 1000).ToString("0.0")},{(detectObject.getZ_m() * 1000).ToString("0.0")}mm");
            }));

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
        private void Btn_resetTimeTick_Click(object sender, RoutedEventArgs e)
        {
            handing = new Objects();
            LV_actionBase.Items.Clear();
            MyInvoke.setToZero(ref mat_cup);
            timeTick = 0;
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
                showType = imgType.color;
            }

        }



        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LV_actionBase.Items.Add(new ActionBaseList("pick", "cup B"));
        }

        robotControl UR = new robotControl();
        private void Button_startServer_Click(object sender, RoutedEventArgs e)
        {
            UR.ServerOn("192.168.1.100", 888);
        }
        private void OnUrStateChange(object sender, LinkArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                if (e.state == "Connect")
                    setConnectCircle(3);
                else if (e.state == "Disconnect")
                    setConnectCircle(1);
                else if (e.state == "Wait Connect")
                    setConnectCircle(2);
            }));


        }

        private void Button_goPosHome_Click(object sender, RoutedEventArgs e)
        {
            UR.goToPos(new URCoordinates());
        }

        #region //---Record---//
        private void Button_recordMode_Click(object sender, RoutedEventArgs e)
        {
            UR.Record_start("t");
        }

        private void Button_recordWrite_Click(object sender, RoutedEventArgs e)
        {
            UR.Record_write();
        }
        private void Button_recordEnd_Click(object sender, RoutedEventArgs e)
        {
            UR.Record_stop();
        }
        #endregion //---Record---//


        private void Cb_Path_DropDownOpened(object sender, EventArgs e)
        {
            DirectoryInfo PathFolder = new DirectoryInfo(UR.folder_Path);
            FileInfo[] ActFiles = PathFolder.GetFiles("*.path");
            foreach (FileInfo file in ActFiles)
            {
                Cb_Path.Items.Add(file.Name);
            }
        }

        private void Btn_goPath_Click(object sender, RoutedEventArgs e)
        {
            UR.goFilePath(Cb_Path.SelectedValue.ToString());
        }

        private void Button_startAction_Click(object sender, RoutedEventArgs e)
        {

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
        public ActionBaseList(string action, string detial)
        {
            Action = action;
            Detial = detial;
        }
        public string Action { get; set; }
        public string Detial { get; set; }
    }

}//namespace
