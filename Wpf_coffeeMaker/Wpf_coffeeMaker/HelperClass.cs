using System;
using System.Diagnostics;
using System.Drawing;
using Intel.RealSense;

namespace Wpf_coffeeMaker
{
    static class HelperClass
    {
        public static float GetDistance_3d(this DepthFrame frame, PointF from, PointF to, Intrinsics intr)
        {
            // Query the frame for distance
            // Note: this can be optimized
            // It is not recommended to issue an API call for each pixel
            // (since the compiler can't inline these)
            // However, in this example it is not one of the bottlenecks

            float udist = frame.GetDistance((int)@from.X, (int)@from.Y); //From
            float vdist = frame.GetDistance((int)to.X, (int)to.Y); //To

            // Deproject from pixel to point in 3D
            var upoint = DeprojectPixelToPoint(intr, from, udist);
            var vpoint = DeprojectPixelToPoint(intr, to, vdist);

            // Calculate euclidean distance between the two points
            return (float)Math.Sqrt(Math.Pow(upoint[0] - vpoint[0], 2) +
                                     Math.Pow(upoint[1] - vpoint[1], 2) +
                                     Math.Pow(upoint[2] - vpoint[2], 2));
        }

        public static float[] DeprojectPixelToPoint(Intrinsics intrin, PointF pixel, float depth)
        {
            Debug.Assert(intrin.model != Distortion.ModifiedBrownConrady); // Cannot deproject from a forward-distorted image
            Debug.Assert(intrin.model != Distortion.Ftheta); // Cannot deproject to an ftheta image

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

        static float[] ProjectPointToPixel(Intrinsics intrin, float[] point)
        {
            float[] pixel = new float[2];
            //assert(intrin->model != RS2_DISTORTION_INVERSE_BROWN_CONRADY); // Cannot project to an inverse-distorted image

            float x = point[0] / point[2], y = point[1] / point[2];

            if (intrin.model == Distortion.ModifiedBrownConrady)
            {
                float r2 = x * x + y * y;
                float f = 1 + intrin.coeffs[0] * r2 + intrin.coeffs[1] * r2 * r2 + intrin.coeffs[4] * r2 * r2 * r2;
                x *= f;
                y *= f;
                float dx = x + 2 * intrin.coeffs[2] * x * y + intrin.coeffs[3] * (r2 + 2 * x * x);
                float dy = y + 2 * intrin.coeffs[3] * x * y + intrin.coeffs[2] * (r2 + 2 * y * y);
                x = dx;
                y = dy;
            }
            if (intrin.model == Distortion.Ftheta)
            {
                float r = (float)Math.Sqrt(x * x + y * y);
                float rd = (float)(1.0f / intrin.coeffs[0] * Math.Atan((2 * r * Math.Tan(intrin.coeffs[0] / 2.0f))));
                x *= rd / r;
                y *= rd / r;
            }

            pixel[0] = x * intrin.fx + intrin.ppx;
            pixel[1] = y * intrin.fy + intrin.ppy;
            return pixel;
        }
    }
}
