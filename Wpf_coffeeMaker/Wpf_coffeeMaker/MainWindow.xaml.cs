using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Intel.RealSense;

namespace Wpf_coffeeMaker
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        //Vars for button dragging
        private Point _dragStartPoint;
        private bool _dragging = false;

        //Current position of buttons for measuring
        private System.Drawing.PointF _startPixel;
        private System.Drawing.PointF _endPixel;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //Setup measuring GUI
                SetStartPos(DragBtn_start);
                SetEndPos(DragBtn_end);

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
            // Setup filter / alignment settings
            SetupFilters(out Colorizer colorizer, out DecimationFilter decimate, out SpatialFilter spatial, out TemporalFilter temp, out HoleFillingFilter holeFill, out Align align_to);

            // Setup config settings
            var cfg = new Config();
            cfg.EnableStream(Stream.Depth, 1280, 720, Format.Z16, 30);
            cfg.EnableStream(Stream.Color, 1280, 720, Format.Rgb8, 30);

            // Pipeline start
            Pipeline pipeline = new Pipeline();
            PipelineProfile pp = pipeline.Start(cfg);

            // Allocate bitmaps for rendring.
            // Since the sample aligns the depth frames to the color frames, both of the images will have the color resolution
            using (var p = pp.GetStream(Stream.Color) as VideoStreamProfile)
            {
                Image_show.Source = new WriteableBitmap(p.Width, p.Height, 96d, 96d, PixelFormats.Rgb24, null);
            }
            Action<VideoFrame> updateColor = UpdateImage(Image_show);

            //Currently not possible in C#
            // The hardware can perform hole-filling much better and much more power efficient then our software
            // auto range = sensor.get_option_range(RS2_OPTION_VISUAL_PRESET);
            // for (auto i = range.min; i < range.max; i += range.step)
            //   if (std::string(sensor.get_option_value_description(RS2_OPTION_VISUAL_PRESET, i)) == "High Density")
            //      sensor.set_option(RS2_OPTION_VISUAL_PRESET, i);

            //var adv = AdvancedDevice.FromDevice(pp.Device);
            //string path = System.IO.Directory.GetCurrentDirectory();
            //string full = System.IO.Path.Combine(path, "HighDensityPreset.json");
            //adv.JsonConfiguration = System.IO.File.ReadAllText(full);

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
            // Setup / start frame processing
            CustomProcessingBlock processingBlock = new CustomProcessingBlock((f, src) =>
            {
                // We create a FrameReleaser object that would track
                // all newly allocated .NET frames, and ensure deterministic finalization
                // at the end of scope. 
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
            return processingBlock;
        }

        private void StartProcessingBlock(CustomProcessingBlock processingBlock, PipelineProfile pp, Action<VideoFrame> updateColor, Pipeline pipeline)
        {
            // Register to results of processing via a callback:
            processingBlock.Start(f =>
            {
                using (var frames = FrameSet.FromFrame(f))
                {
                    var color_frame = frames.ColorFrame.DisposeWith(frames);
                    var depthintr = (pp.GetStream(Stream.Depth) as VideoStreamProfile).GetIntrinsics();
                    var depth_frame = frames.DepthFrame.DisposeWith(frames);

                    PointConverter pointConverter = new PointConverter();

                    //float k1 = depth_frame.GetDistance_3d(_fromPixel, _toPixel, depthintr);
                    float distance = HelperClass.GetDistance_3d(depth_frame, _startPixel, _endPixel, depthintr);

                    Dispatcher.Invoke(DispatcherPriority.Render, updateColor, color_frame);
                    Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => DistanceTB.Text = "Distance: " + Math.Round(distance * 100, 2) + " cm"));
                }
            });

            var token = _tokenSource.Token;

            var t = Task.Factory.StartNew(() =>
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

        private void SetStartPos(Button StartButton)
        {
            //Calculate button center as start point
            Point startPoint = StartButton.TranslatePoint(new Point(0, 0), this);
            startPoint.X = startPoint.X + (StartButton.ActualWidth / 2);
            startPoint.Y = startPoint.Y + (StartButton.ActualHeight / 2);

            _startPixel = new System.Drawing.PointF((float)startPoint.X, (float)startPoint.Y);

            //Move line object
            ConnectingLine.X1 = startPoint.X;
            ConnectingLine.Y1 = startPoint.Y;
        }

        private void SetEndPos(Button EndButton)
        {
            //Calculate button center as end point
            Point endPoint = EndButton.TranslatePoint(new Point(0, 0), this);
            endPoint.X = endPoint.X + (EndButton.ActualWidth / 2);
            endPoint.Y = endPoint.Y + (EndButton.ActualHeight / 2);

            _endPixel = new System.Drawing.PointF((float)endPoint.X, (float)endPoint.Y);

            //Move line object
            ConnectingLine.X2 = endPoint.X;
            ConnectingLine.Y2 = endPoint.Y;
        }
    }//class
}//namespace
