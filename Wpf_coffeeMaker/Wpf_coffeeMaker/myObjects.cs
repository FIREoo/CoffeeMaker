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
        public Objects(int index, string name)
        {
            this.index = index;
            Name = name;
            nowPos = new URCoordinates();
        }
        public Objects(URCoordinates pos)
        {
            Name = "pos";
            nowPos = new URCoordinates(pos);
        }
        public enum states
        {
            stop = 0,
            move = 1
        }
        public string Name { get; } = "";
        public int index { get; } = -1;
        public URCoordinates nowPos { get; set; }

        public System.Windows.Media.Color color;
        public Unit gripOffset_x = new Unit();
        public Unit gripOffset_y = new Unit();
        public Unit gripOffset_z = new Unit();

        public int fileterSize { get; set; } = 6;
        public float moveD { get; set; } = 0.05f;


        private List<float> list_x = new List<float>();//unit M
        private List<float> list_y = new List<float>();//unit M
        private List<float> list_z = new List<float>();//unit M
        private float lastx = 0;
        private float lasty = 0;
        private float lastz = 0;

        public void pushPosition(Unit X, Unit Y, Unit Z)
        {
            setX_M(X.M);
            setY_M(Y.M);
            setZ_M(Z.M);
        }
        private void setX_M(float x_m)
        {
            list_x.Add(x_m);
            if (list_x.Count > fileterSize)
                list_x.RemoveAt(0);
        }
        private void setY_M(float y_m)
        {
            list_y.Add(y_m);
            if (list_y.Count > fileterSize)
                list_y.RemoveAt(0);
        }
        private void setZ_M(float z_m)
        {
            list_z.Add(z_m);
            if (list_z.Count > fileterSize)
                list_z.RemoveAt(0);
        }

        public void getFilterPos(out Unit X, out Unit Y, out Unit Z)
        {
            X = avg(list_x, 0, fileterSize).M();
            Y = avg(list_y, 0, fileterSize).M();
            Z = avg(list_z, 0, fileterSize).M();
        }

        public URCoordinates gripPos(int deg = 0)
        {
            if (deg == 0)
                //return new URCoordinates(nowPos.X + gripOffset_M_x, nowPos.Y + gripOffset_M_y, nowPos.Z + gripOffset_M_z, (float)(Math.PI), 0, 0);
                return new URCoordinates(nowPos.X.M + gripOffset_x.M, 0.297f, nowPos.Z.M + gripOffset_z.M, (float)(Math.PI), 0, 0);//evil

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
            if (list_x.Count < fileterSize)
                return states.stop;

            //前半 跟 後半做比較
            lastx = avg(list_x, 0, fileterSize / 2);
            lasty = avg(list_y, 0, fileterSize / 2);
            lastz = avg(list_z, 0, fileterSize / 2);
            float avgx = avg(list_x, fileterSize / 2, fileterSize / 2);
            float avgy = avg(list_y, fileterSize / 2, fileterSize / 2);
            float avgz = avg(list_z, fileterSize / 2, fileterSize / 2);
            float dx = (lastx - avgx) * (lastx - avgx);
            float dy = (lasty - avgy) * (lasty - avgy);
            float dz = (lastz - avgz) * (lastz - avgz);
            if ((dx + dy + dz) > moveD * moveD)
                return states.move;
            else
                return states.stop;
        }
        public Unit moveDistanse()
        {
            lastx = avg(list_x, 0, fileterSize);
            lasty = avg(list_y, 0, fileterSize);
            lastz = avg(list_z, 0, fileterSize);
            float avgx = avg(list_x, 0, fileterSize);
            float avgy = avg(list_y, 0, fileterSize);
            float avgz = avg(list_z, 0, fileterSize);
            double dx = (lastx - avgx) * (lastx - avgx);
            double dy = (lasty - avgy) * (lasty - avgy);
            double dz = (lastz - avgz) * (lastz - avgz);
            return ((float)Math.Pow((dx + dy + dz), 0.5d)).M();
        }
        public Unit Distanse(URCoordinates pos)
        {
            float avgx = avg(list_x, 0, fileterSize);
            float avgy = avg(list_y, 0, fileterSize);
            float avgz = avg(list_z, 0, fileterSize);
            double dx = (pos.X.M - avgx) * (pos.X.M - avgx);
            double dy = (pos.Y.M - avgy) * (pos.Y.M - avgy);
            double dz = (pos.Z.M - avgz) * (pos.Z.M - avgz);
            return ((float)Math.Pow((dx + dy + dz), 0.5d)).M();

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
