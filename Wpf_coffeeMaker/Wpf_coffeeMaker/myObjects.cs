using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrRobot.Coordinates;

namespace myObjects
{
    public class Objects
    {
        public enum states
        {
            stop = 0,
            move = 1
        }
        public string Name = "";
        public System.Windows.Media.Color color;
        private int fileterSize { get; set; } = 6;
        private List<float> x = new List<float>();//unit M
        private List<float> y = new List<float>();//unit M
        private List<float> z = new List<float>();//unit M
        private float moveD { get; set; } = 0.05f;
        private float lastx = 0;
        private float lasty = 0;
        private float lastz = 0;
        public float gripOffset_M_x = 0;
        public float gripOffset_M_y = 0;
        public float gripOffset_M_z = 0;
        private URCoordinates nowPos = new URCoordinates(0, 0, 0, 0, 0, 0);//再action中 可能因為中間移動過 所以必須記錄移動後的位置 以免之後抓空

        public void saveNowPos()
        {
            nowPos = new URCoordinates(getX_m(), getY_m(), getZ_m(), 0, 0, 0);
        }
        public URCoordinates getNowPos()
        {
            return new URCoordinates(getX_m(), getY_m(), getZ_m(), 0, 0, 0);
        }
        public void setNowPos(URCoordinates urc)
        {
            nowPos = new URCoordinates(urc);
        }

        public void setPos_mm(float x_mm, float y_mm, float z_mm)
        {
            setX_mm(x_mm);
            setY_mm(y_mm);
            setZ_mm(z_mm);
        }
        public void setPos_m(float x_mm, float y_mm, float z_mm)
        {
            setX_mm(x_mm * 1000f);
            setY_mm(y_mm * 1000f);
            setZ_mm(z_mm * 1000f);
        }
        private void setX_mm(float x_mm)
        {
            x.Add(x_mm / 1000f);
            if (x.Count > fileterSize)
                x.RemoveAt(0);
        }
        private void setY_mm(float y_mm)
        {
            y.Add(y_mm / 1000f);
            if (y.Count > fileterSize)
                y.RemoveAt(0);
        }
        private void setZ_mm(float z_mm)
        {
            z.Add(z_mm / 1000f);
            if (z.Count > fileterSize)
                z.RemoveAt(0);
        }
        public float getX_m()
        {
            return avg(x, 0, fileterSize);
        }
        public float getY_m()
        {
            return avg(y, 0, fileterSize);
        }
        public float getZ_m()
        {
            return avg(z, 0, fileterSize);
        }

        public URCoordinates gripPos(int deg = 0)
        {
            if (deg == 0)
                //return new URCoordinates(nowPos.X + gripOffset_M_x, nowPos.Y + gripOffset_M_y, nowPos.Z + gripOffset_M_z, (float)(Math.PI), 0, 0);
                return new URCoordinates(nowPos.X.M + gripOffset_M_x, 0.297f, nowPos.Z.M + gripOffset_M_z, (float)(Math.PI), 0, 0);//evil

            throw new System.ArgumentException("未完成", "現在只能水平");
        }

        private float avg(List<float> v, int startIndex, int count)
        {
            if (v.Count < (count + startIndex))//值不能少於count
                return 0;

            float sum = 0;
            for (int i = 0; i < count; i++)
                sum += v[i + startIndex];
            return sum / (float)count;
        }

        public states State()
        {
            if (x.Count < fileterSize)
                return states.stop;


            //前半 跟 後半做比較
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
            lastx = avg(x, 0, fileterSize);
            lasty = avg(y, 0, fileterSize);
            lastz = avg(z, 0, fileterSize);
            float avgx = avg(x, 0, fileterSize);
            float avgy = avg(y, 0, fileterSize);
            float avgz = avg(z, 0, fileterSize);
            double dx = (lastx - avgx) * (lastx - avgx);
            double dy = (lasty - avgy) * (lasty - avgy);
            double dz = (lastz - avgz) * (lastz - avgz);
            return (float)Math.Pow((dx + dy + dz), 0.5d);
        }
        public float Distanse(URCoordinates pos)
        {
            float avgx = avg(x, 0, fileterSize);
            float avgy = avg(y, 0, fileterSize);
            float avgz = avg(z, 0, fileterSize);
            double dx = (pos.X.M - avgx) * (pos.X.M - avgx);
            double dy = (pos.Y.M - avgy) * (pos.Y.M - avgy);
            double dz = (pos.Z.M - avgz) * (pos.Z.M - avgz);
            return (float)Math.Pow((dx + dy + dz), 0.5d);

        }
        public states S = states.stop;

        /// <summary>
        /// a.Name == b.Name
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(Objects a, Objects b)
        {
            return a.Name == b.Name;
        }
        /// <summary>
        /// a.Name != b.Name
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(Objects a, Objects b)
        {
            return a.Name != b.Name;
        }
    }
}
