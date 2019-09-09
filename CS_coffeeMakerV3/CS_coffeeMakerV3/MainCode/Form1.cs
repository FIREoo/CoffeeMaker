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
        }

        int avgNum = 10;//平均幾張
        int avgPivot = 0;//現在張數
        Mat RS_depth(DepthFrame DF)
        {
            Size RS_depthSize = new Size(1280,720);
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
        private void ColorToolStripMenuItem_Click(object sender, EventArgs e)
        {//右鍵image box
         
        }

        private void MixToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var pipe = new Pipeline();
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

                yoloWrapper = new YoloWrapper("modle\\yolov3.cfg", "modle\\yolov3.weights", "modle\\coco.names");
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

                            if (name == "cup")
                            {
                                CvInvoke.PutText(mat_show, name, new Point(x, y), FontFace.HersheySimplex, 1, new MCvScalar(50, 230, 230));
                                CvInvoke.PutText(mat_show, item.Confidence.ToString("0.0"), new Point(x, y - 20), FontFace.HersheySimplex, 0.5, new MCvScalar(50, 230, 230));
                                CvInvoke.Rectangle(mat_show, new Rectangle(x, y, W, H), new MCvScalar(50, 230, 230), 3);
                                byte depth = MyInvoke.GetValue<byte>(mat_depth, center.Y, center.X);
                                CvInvoke.PutText(mat_show, depth.ToString(), center, FontFace.HersheySimplex, 1, new MCvScalar(50, 230, 230));
                            }
                        }

                    }

                    imageBox_RS.Image = mat_show;
                }
            });
        }
    }//class form
}//namespace
