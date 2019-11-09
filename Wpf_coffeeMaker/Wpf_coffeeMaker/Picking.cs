using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UrRobot.Coordinates;

namespace Picking
{
   public static class PickingPos
    {
       static  float URyLimitMin = -500;
        static float URyLimitMax = -180;
        /// <summary>確認有沒有超出工作區，不然算出來也到不了/// </summary>
        public static bool PickCheck(Unit Tx, Unit Ty)
        {
            if (Ty.mm.IsBetween((int)URyLimitMin, (int)URyLimitMax))
                return true;
            return false;
        }
        /// <summary>夾取的斜度/// </summary>
        public static Angle PickAngle(Unit Tx, Unit Ty)
        {
            if (PickCheck(Tx,Ty))
            {
                int angle = (int)((Ty.mm - (URyLimitMax)) * ((45f - 0f) / ((URyLimitMin) - (URyLimitMax))));
                return angle.deg();
            }
            else
            {
                Console.WriteLine("超出工作區");
                return new Angle();
            }
        }
        /// <summary>夾取的6D座標/// </summary>
        public static URCoordinates PickPose(Unit Tx, Unit Ty, Unit Tz, string obj)
        {
            Angle angle = PickAngle(Tx, Ty);
            URCoordinates.Vector3 rpy = new URCoordinates.Vector3();// = new URCoordinates.Vector3(-90.deg(), 180.deg(), (-90 - angle).deg());
            if(obj == "level"|| obj == "H" )
            {
                rpy = new URCoordinates.Vector3(-90.deg(), 180.deg(), (-90 - angle.deg).deg());
            }
            else if (obj == "V"||obj == "pill" || obj == "pill box" || obj == "box"|| obj == "pillBox")
            {
                rpy = new URCoordinates.Vector3(-180.deg(), 0.deg(), 0.deg());
            }
            else if (obj == "cup")
            {
                rpy = new URCoordinates.Vector3(-45.deg(), 180.deg(), (-90 - angle.deg).deg());
            }
            else if (obj == "add")
            {
                rpy = new URCoordinates.Vector3(-90.deg(), -90.deg(), (-90 - angle.deg).deg());
            }

            else
            {
                Console.WriteLine("沒有這個Pos耶...");
            }

            URCoordinates.Vector3 rotation = URCoordinates.ToRotVector(rpy);
            return new URCoordinates(Tx, Ty, Tz, rotation.X.rad(), rotation.Y.rad(), rotation.Z.rad());
        }
        /// <summary>倒水時 位移後的(x, y)/// </summary>
        public static void PourPos(Unit Cx, Unit Cy, out Unit PourX, out Unit PourY)
        {
            Angle angle = -PickAngle(Cx, Cy);//坐標軸 X向上 Y向左 所以
            Unit dx = 30.mm();
            Unit dy = -30.mm();
            double Px = Math.Cos(angle.rad) * dx.mm - Math.Sin(angle.rad) * dy.mm;
            double Py = Math.Sin(angle.rad) * dx.mm + Math.Cos(angle.rad) * dy.mm;

            PourX = (Cx.mm + Px).mm();
            PourY = (Cy.mm + Py).mm();
        }
        /// <summary>加入粉時 位移後的(x, y)/// </summary>
        public static void AddInPos(Unit Cx, Unit Cy, out Unit AddX, out Unit AddY)
        {
            Angle angle = -PickAngle(Cx, Cy);//坐標軸 X向上 Y向左 所以
            Unit dx = 0.mm();
            Unit dy = -90.mm();
            double Px = Math.Cos(angle.rad) * dx.mm - Math.Sin(angle.rad) * dy.mm;
            double Py = Math.Sin(angle.rad) * dx.mm + Math.Cos(angle.rad) * dy.mm;

            AddX = (Cx.mm + Px).mm();
            AddY = (Cy.mm + Py).mm();
        }
    }
}
