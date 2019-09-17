using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Intel.RealSense;
using Alturos.Yolo;
using Alturos.Yolo.Model;
using System.Runtime.InteropServices;
using myEmguLibrary;
using myObjects;
//using System.Windows;
using Size = System.Drawing.Size;
using System.Windows.Threading;
using System.Threading;

namespace CS_coffeeMakerV3
{
    public partial class Form1 : Form
    {
        Mat matrix;
        bool loop = false;
        static Mat mat_color;
        static Mat mat_color_map;
        static Mat mat_depth;
        static Mat mat_show;
        Size mappingColor = new Size(950, 500);

        static YoloWrapper yoloWrapper;
        static IEnumerable<YoloItem> items;

        public static int timeTick = 0;
        static Mat mat_Bcup = new Mat(100, 300, DepthType.Cv8U, 3);
        static Mat mat_Pcup = new Mat(100, 300, DepthType.Cv8U, 3);

        public static Objects handing = new Objects();

        public Form1()
        {
            InitializeComponent();

            PointF[] src = new[] {
                    new PointF(0,0),
                    new PointF(0,720),
                    new PointF(1280,720),
                    new PointF(1280,0) };

            int offsetX = -23;
            int offsetY = 0;

            int shiftX = (1280 - mappingColor.Width) / 2;
            int shiftY = (720 - mappingColor.Height) / 2;
            PointF[] dst = new[] {
                    new PointF(shiftX+offsetX,shiftY+offsetY),
                    new PointF(shiftX+offsetX,shiftY+mappingColor.Height+offsetY),
                    new PointF(shiftX+mappingColor.Width+offsetX,shiftY+mappingColor.Height+offsetY),
                    new PointF(shiftX+mappingColor.Width+offsetX,shiftY+offsetY) };

            matrix = CvInvoke.GetPerspectiveTransform(src, dst);

            creatObject();
        }
        public static Objects[] cups = new Objects[2];
        public static Objects machine = new Objects();
        private void creatObject()
        {
            machine.Name = "machine";

            cups[0] = new Objects();
            cups[0].Name = "blue cup";
            cups[0].color = Color.FromArgb(153, 217, 234);

            cups[1] = new Objects();
            cups[1].Name = "pink cup";
            cups[1].color = Color.FromArgb(255, 180, 200);
        }

        public void ActionBaseAdd(string act, string detial, Color color1 = new Color(), Color color2 = new Color())
        {
            ListViewItem item1 = new ListViewItem("");
            item1.UseItemStyleForSubItems = false;
            item1.SubItems.Add(act);
            item1.SubItems[1].BackColor = color1;
            item1.SubItems.Add(detial);
            item1.SubItems[2].BackColor = color2;
            //  item1.SubItems[2].ForeColor = color;
            this.Invoke((MethodInvoker)(() => listView_action.Items.Add(item1)));
        }
        //Real sense
        #region Realsense
        int avgNum = 10;//平均幾張
        int avgPivot = 0;//現在張數
        Mat RS_depth(DepthFrame DF)
        {
            Size RS_depthSize = new Size(1280, 720);
            int SizeOfDepth = 1280 * 720;
            Mat rtn = new Mat(RS_depthSize, DepthType.Cv8U, 1);
            ushort[,] udepth = new ushort[SizeOfDepth, avgNum];
            ushort[] ubuffer = new ushort[SizeOfDepth];
            byte[] bdepth = new byte[SizeOfDepth];
            //copy to buffer
            DF.CopyTo(ubuffer);
            for (int i = 0; i < SizeOfDepth; i++)
                udepth[i, avgPivot] = ubuffer[i];

            float max = 1000;
            for (int i = 0; i < SizeOfDepth; i++)
            {
                ushort sum = 0;
                int C = 0;
                //---加總 所有這個pixel的buffer值
                if (avgPivot < avgNum)//如果 現有張數不夠，則先直接平均
                {
                    for (int n = 0; n <= avgPivot; n++)//pivot 是 index 所以要<=
                        if (udepth[i, n] > 0)//有值 才算 沒值跳過
                        {
                            sum += udepth[i, n];
                            C++;
                        }
                }
                else
                {
                    for (int n = 0; n < avgNum; n++)//總共avgNum張
                        if (udepth[i, n] > 0)//有值 才算 沒值跳過
                        {
                            sum += udepth[i, n];
                            C++;
                        }
                }

                //---計算平均
                if (C == 0)//全部都沒值 (不然除法會算錯)
                    ubuffer[i] = 0;
                else//如果有值 就平均
                    ubuffer[i] = (ushort)(sum / C);

                //---將值map成byte
                if (ubuffer[i] > max)
                    bdepth[i] = 0;
                else
                    bdepth[i] = (byte)(ubuffer[i] * (255.0f / max));
            }//each pixel

            avgPivot++;
            if (avgPivot == avgNum)
                avgPivot = 0;

            Marshal.Copy(bdepth, 0, rtn.DataPointer, SizeOfDepth);

            return rtn;
        }
        Mat RS_color(VideoFrame CF, Mat matrix = null)
        {
            Size RS_depthSize = new Size(1280, 720);
            int SizeOfDepth = 1280 * 720;
            Mat rtn = new Mat(RS_depthSize, DepthType.Cv8U, 3);

            byte[] bcolor = new byte[1280 * 720 * 3];
            Marshal.Copy(CF.Data, bcolor, 0, 720 * 1280 * 3);
            Marshal.Copy(bcolor, 0, rtn.DataPointer, 720 * 1280 * 3);
            if (matrix != null)
                CvInvoke.WarpPerspective(rtn, rtn, matrix, new Size(1280, 720));
            return rtn;
        }
        #endregion Realsense

        private void ColorToolStripMenuItem_Click(object sender, EventArgs e)
        {//右鍵image box

        }

        private void MixToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Pipeline pipe = new Pipeline();
            var cfg = new Config();
            cfg.EnableStream(Stream.Depth, 1280, 720, Format.Z16);
            cfg.EnableStream(Stream.Color, 1280, 720, Format.Bgr8);

            var pp = pipe.Start(cfg);

            Task.Run(() =>
            {
                mat_color = new Mat(720, 1280, DepthType.Cv8U, 3);
                mat_color_map = new Mat(mappingColor, DepthType.Cv8U, 3);
                mat_depth = new Mat(720, 1280, DepthType.Cv8U, 1);
                mat_show = new Mat(720, 1280, DepthType.Cv8U, 3);

                yoloWrapper = new YoloWrapper("modle\\yolov3-tiny-3obj.cfg", "modle\\yolov3-tiny-3obj_3cup.weights", "modle\\obj.names");
                string detectionSystemDetail = string.Empty;
                if (!string.IsNullOrEmpty(yoloWrapper.EnvironmentReport.GraphicDeviceName))
                    detectionSystemDetail = $"({yoloWrapper.EnvironmentReport.GraphicDeviceName})";
                Console.WriteLine($"Detection System:{yoloWrapper.DetectionSystem}{detectionSystemDetail}");

                loop = true;
                while (loop)
                {
                    using (var frames = pipe.WaitForFrames())
                    {
                        mat_color = RS_color(frames.ColorFrame.DisposeWith(frames), matrix);
                        mat_depth = RS_depth(frames.DepthFrame);
                        Mat mat_depth_c3 = new Mat(720, 1280, DepthType.Cv8U, 3);
                        CvInvoke.CvtColor(mat_depth, mat_depth_c3, ColorConversion.Gray2Bgr);
                        CvInvoke.AddWeighted(mat_color, 0.5, mat_depth_c3, 0.5, 0.0, mat_show);

                        CvInvoke.Imwrite("yolo1.png", mat_color);
                        try { items = yoloWrapper.Detect(@"yolo1.png"); }
                        catch { break; }

                        foreach (YoloItem item in items)
                        {
                            string name = item.Type;
                            int x = item.X;
                            int y = item.Y;
                            int H = item.Height;
                            int W = item.Width;
                            Point center = item.Center();

                            CvInvoke.PutText(mat_show, name, new Point(x, y), FontFace.HersheySimplex, 1, new MCvScalar(50, 230, 230));
                            CvInvoke.PutText(mat_show, item.Confidence.ToString("0.0"), new Point(x, y - 20), FontFace.HersheySimplex, 0.5, new MCvScalar(50, 230, 230));
                            CvInvoke.Rectangle(mat_show, new Rectangle(x, y, W, H), new MCvScalar(50, 230, 230), 3);
                            byte depth = MyInvoke.GetValue<byte>(mat_depth, center.Y, center.X);
                            if (name == "blue cup")//index 0
                            {
                                process(cups[0], mat_Bcup, label_Bcup_msg, label_Bcup_state);
                            }
                            else if (name == "pink cup")//index 1
                            {
                                process(cups[1], mat_Pcup, label_Pcup_msg, label_Pcup_state);
                            }

                            void process(Objects detectObject, Mat mat, Label label_msg, Label label_s)
                            {
                                detectObject.setPos_mm(0,0,(float)depth.Unit_mm());
                                Objects.states s = detectObject.State();
                                this.Invoke((MethodInvoker)(() => label_s.Text = s.ToString()));
                                if (s == Objects.states.stop)
                                {
                                    CvInvoke.Line(mat, new Point(timeTick, 30), new Point(timeTick, 70), new MCvScalar(50, 50, 50));
                                }
                                else if (s == Objects.states.move)
                                {
                                    CvInvoke.Line(mat, new Point(timeTick, 30), new Point(timeTick, 70), new MCvScalar(50, 150, 150));
                                    if (handing == detectObject)//如果拿著的東西跟移動的東西一樣，代表繼續移動
                                    { }
                                    else//如果拿著的東西跟移動的東西"不"一樣，代表要新增東西
                                    {
                                        //if (handing == new Objects())//代表第一次
                                        if (cups.All(cup => cup != handing))//代表前面不是杯子 這樣才會有place
                                        { }
                                        else//代表前面有東西
                                        {
                                            ActionBaseAdd("Place", handing.Name, color1: Color.FromArgb(240, 230, 176), color2: handing.color);
                                        }

                                        ActionBaseAdd("Pick up", detectObject.Name, color1: Color.FromArgb(255, 200, 14), color2: detectObject.color);
                                    }
                                    handing = detectObject;
                                }
                                this.Invoke((MethodInvoker)(() => label_msg.Text = (detectObject.getZ_m() * 1000).ToString("0.0") + "mm"));
                            }

                            //CvInvoke.PutText(mat_show, (depth.Unit_mm()).ToString("0.00") + "mm", center, FontFace.HersheySimplex, 1, new MCvScalar(50, 230, 230));
                        }//foreach cups 
                        timeTick++;
                    }//using depth image

                    imageBox_Pcup.Image = mat_Pcup;
                    imageBox_Bcup.Image = mat_Bcup;
                    imageBox_RS.Image = mat_show;
                }
            });
        }

        private void Button_startRecordState_Click(object sender, EventArgs e)
        {
            handing = new Objects();
            listView_action.Items.Clear();
            mat_Pcup.SetTo(new MCvScalar(0, 0, 0));
            mat_Bcup.SetTo(new MCvScalar(0, 0, 0));
            timeTick = 0;
        }

        private void Button_showAdmin_Click(object sender, EventArgs e)
        {
            FormAdmin frm = new FormAdmin(this);
            frm.Show();
        }

        CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private void PCMixToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if TEST
            var cfg = new Config();
            cfg.EnableStream(Stream.Depth, 1280, 720, Format.Z16, 30);
            cfg.EnableStream(Stream.Color, 1280, 720, Format.Rgb8, 30);
            Pipeline pipeline = new Pipeline();
            PipelineProfile pp = pipeline.Start(cfg);

            var processingBlock = StartProcessingBlock(pipeline, pp);


            mat_color = new Mat(720, 1280, DepthType.Cv8U, 3);
            mat_color_map = new Mat(mappingColor, DepthType.Cv8U, 3);
            mat_depth = new Mat(720, 1280, DepthType.Cv8U, 1);
            mat_show = new Mat(720, 1280, DepthType.Cv8U, 3);


            var token = _tokenSource.Token;

            var t = Task.Factory.StartNew(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    using (var frames = pipeline.WaitForFrames())
                    {
                        // Invoke custom processing block
                        processingBlock.ProcessFrames(frames);

                        //mat_color = RS_color(frames.ColorFrame.DisposeWith(frames), matrix);
                        mat_depth = RS_depth(frames.DepthFrame);
                       // Mat mat_depth_c3 = new Mat(720, 1280, DepthType.Cv8U, 3);
                        //CvInvoke.CvtColor(mat_depth, mat_depth_c3, ColorConversion.Gray2Bgr);
                       // CvInvoke.AddWeighted(mat_color, 0.5, mat_depth_c3, 0.5, 0.0, mat_show);
                        imageBox_RS.Image = mat_depth;
                    }
                }
            }, token);
#endif
        }
#if TEST
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
        private CustomProcessingBlock StartProcessingBlock(Pipeline pipe, PipelineProfile pp)
        {
            SetupFilters(out Colorizer colorizer, out DecimationFilter decimate, out SpatialFilter spatial, out TemporalFilter temp, out HoleFillingFilter holeFill, out Align align_to);

            CustomProcessingBlock processingBlock = SetupProcessingBlock(pipe, colorizer, decimate, spatial, temp, holeFill, align_to);
            processingBlock.Start(f =>
            {
                using (var frames2 = FrameSet.FromFrame(f))
                {
                    var depthintr = (pp.GetStream(Stream.Depth) as VideoStreamProfile).GetIntrinsics();
                    var depth_frame = frames2.DepthFrame.DisposeWith(frames2);
                    float udist = depth_frame.GetDistance(ePoint.X, ePoint.Y); //From
                    var point = DeprojectPixelToPoint(depthintr, new PointF(ePoint.X, ePoint.Y), udist);
                    Console.WriteLine($"({point[0]},{point[1]},{point[2]})");
                }
            });
            return processingBlock;
        }
       
        float[] DeprojectPixelToPoint(Intrinsics intrin, PointF pixel, float depth)
        {
            var ret = new float[3];
            float x = (pixel.X - intrin.ppx) / intrin.fx;
            float y = (pixel.Y - intrin.ppy) / intrin.fy;
            if (intrin.model == Distortion.InverseBrownConrady)
            {
                float r2 = x * x + y * y;
                float f = 1 + intrin.coeffs[0] * r2 + intrin.coeffs[1] * r2 * r2 + intrin.coeffs[4] * r2 * r2 * r2;
                float ux = x * f + 2 * intrin.coeffs[2] * x * y + intrin.coeffs[3] * (r2 + 2 * x * x);
                float uy = y * f + 2 * intrin.coeffs[3] * x * y + intrin.coeffs[2] * (r2 + 2 * y * y);
                x = ux;
                y = uy;
            }
            ret[0] = depth * x;
            ret[1] = depth * y;
            ret[2] = depth;
            return ret;
        }
#endif
        Point ePoint = new Point();
        private void ImageBox_RS_MouseDown(object sender, MouseEventArgs e)
        {
            // if (loop == true)
            // {
            ePoint = new Point(e.X, e.Y);
            //  }

        }

    }//class form
    static public class ex
    {
        static public double Unit_mm(this byte RS)
        {
            return (double)(RS + 5) * 100d / 26d;
        }
    }

}//namespace
