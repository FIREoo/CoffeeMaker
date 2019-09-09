using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrRobot;

namespace myObjects
{
    class Objects
    {
        public enum states
        {
            stop = 0,
            move = 1
        }
        public string Name = "";
        private const int fileterSize = 10;
        private List<float> x = new List<float>();//unit M
        private List<float> y = new List<float>();//unit M
        private List<float> z = new List<float>();//unit M
        private float moveD = 0.02f;
        private float lastx = 0;
        private float lasty = 0;
        private float lastz = 0;
        public float gripOffset_M_x = 0;
        public float gripOffset_M_y = 0;
        public float gripOffset_M_z = 0;
        private URCoordinates nowPos = new URCoordinates();//再action中 可能因為中間移動過 所以必須記錄移動後的位置 以免之後抓空

        public void saveNowPos()
        {
            nowPos = new URCoordinates(getX_m(), getY_m(), getZ_m(), 0, 0, 0);
        }
        public void setNowPos(URCoordinates urc)
        {
            nowPos = new URCoordinates(urc);
        }


        private int[] filterIndex = new[] { 0, 0, 0 };
        public void setX_mm(float x_mm)
        {
            x.Add(x_mm / 1000f);
            if (x.Count > fileterSize)
                x.RemoveAt(0);

        }
        public void setY_mm(float y_mm)
        {
            y.Add(y_mm / 1000f);
            if (y.Count > fileterSize)
                y.RemoveAt(0);
        }
        public void setZ_mm(float z_mm)
        {
            z.Add(z_mm / 1000f);
            if (z.Count > fileterSize)
                z.RemoveAt(0);

        }
        public float getX_m()
        {
            return avg(x, fileterSize / 2, fileterSize / 2);
        }
        public float getY_m()
        {
            return avg(y, fileterSize / 2, fileterSize / 2);
        }
        public float getZ_m()
        {
            return avg(z, fileterSize / 2, fileterSize / 2);
        }

        public URCoordinates gripPos(int deg = 0)
        {
            if (deg == 0)
                return new URCoordinates(nowPos.X + gripOffset_M_x, nowPos.Y + gripOffset_M_y, nowPos.Z + gripOffset_M_z, (float)(Math.PI), 0, 0);
            throw new System.ArgumentException("未完成", "現在只能水平");
        }

        private float avg(List<float> v, int startIndex, int count)
        {
            if (v.Count < 10)
                return 0;
            float sum = 0;
            for (int i = 0; i < count; i++)
                sum += v[i + startIndex];
            return sum / (float)count;
        }

        public states State()
        {
            lastx = avg(x, 0, fileterSize / 2);
            lasty = avg(y, 0, fileterSize / 2);
            lastz = avg(z, 0, fileterSize / 2);
            float avgx = avg(x, fileterSize / 2, fileterSize / 2);
            float avgy = avg(y, fileterSize / 2, fileterSize / 2);
            float avgz = avg(z, fileterSize / 2, fileterSize / 2);
            float dx = (lastx - avgx) * (lastx - avgx);
            float dy = (lasty - avgy) * (lasty - avgy);
            float dz = (lastz - avgz) * (lastz - avgz);
            if ((dx + dy + dz) > moveD * moveD)
                return states.move;
            else
                return states.stop;
        }
        public float moveDistanse()
        {
            lastx = avg(x, 0, fileterSize / 2);
            lasty = avg(y, 0, fileterSize / 2);
            lastz = avg(z, 0, fileterSize / 2);
            float avgx = avg(x, fileterSize / 2, fileterSize / 2);
            float avgy = avg(y, fileterSize / 2, fileterSize / 2);
            float avgz = avg(z, fileterSize / 2, fileterSize / 2);
            double dx = (lastx - avgx) * (lastx - avgx);
            double dy = (lasty - avgy) * (lasty - avgy);
            double dz = (lastz - avgz) * (lastz - avgz);
            return (float)Math.Pow((dx + dy + dz), 0.5d);
        }
        public states S = states.stop;
    }
}
