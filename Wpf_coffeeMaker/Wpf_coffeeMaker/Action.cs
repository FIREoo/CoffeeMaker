using myObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UrRobot.Coordinates;

namespace myActionBase
{
    public class ActionLine
    {
        public myAction Action;
        public Objects target;
        public Objects destination;

        private List<string> cmdTxt = new List<string>();

        public ActionLine(myAction act, Objects tar, Objects des)
        {
            Action = act;
            target = tar;
            destination = des;
        }

        public List<string> getCmdText()//這裡就要回傳一長段動作了 因為pick不是單單一個指令
        {
            return Action.cmdText(target, destination);
        }
    }
    public class myActionAdder
    {//使用這個 目的是index的編號
        public static myAction Pick()
        {
            myAction rtn = new myAction(1, ActionName.Pick);
            return rtn;
        }
        public static myAction Place()
        {
            myAction rtn = new myAction(2, ActionName.Place);
            return rtn;
        }
        public static myAction Pour()
        {
            myAction rtn = new myAction(3, "Pour");
            //pick cmd txt
            return rtn;
        }
        public static myAction Scoop()
        {
            myAction rtn = new myAction(4, ActionName.Scoop);
            return rtn;
        }
        public static myAction AddIn()
        {
            myAction rtn = new myAction(5, ActionName.AddaSpoon);
            return rtn;
        }
        public static myAction Stir()
        {
            myAction rtn = new myAction(6, ActionName.Stir);
            return rtn;
        }
    }
    public static class ActionName
    {
      public static string getName(int index)
        {
            switch (index)
            {
                case 1:
                    return Pick;
                case 2:
                    return Place;
                case 3:
                    return Pour;
                case 4:
                    return Scoop;
                case 5:
                    return AddaSpoon;
                case 6:
                    return Stir;
                default:
                    return "";
            }

        }
        public static string Pick = "Pick up";
        public static string Place = "Place";
        public static string Pour = "Pour";
        public static string Scoop = "Scoop up";
        public static string AddaSpoon = "Add into";
        public static string Stir = "Stir";
    }

    public class myAction
    {
        public myAction(int i, string name)
        {
            index = i;
            Name = name;
        }
        public string Name { get; } = "";

        public int index { get; } = 0;

        private List<string> cmdTxt = new List<string>();
        public List<string> cmdText(Objects target, Objects destination)
        {
            cmdTxt.Clear();
            if (Name == ActionName.Pick)
                Pick(target);
            else if (Name == ActionName.Place)
                Place(target, destination);
            else if (Name == ActionName.Pour)
                Pour(target, destination);
            else if (Name == ActionName.Scoop)
                Scoop(target, destination);
            else if (Name == ActionName.AddaSpoon)
                AddIn(target, destination);
            else if (Name == ActionName.Stir)
                Stir(target, destination);
            return cmdTxt;

        }
        private void Pick(Objects target)
        {
            URCoordinates Pos = target.nowPos;
            if (target.Name.IndexOf("cup") >= 0)
            {
                foreach (string str in File.ReadAllLines("Path\\j_top.path"))//防止下個點失敗手臂 
                    cmdTxt.Add(str);

                Pos.Z = 0.2.M();
                Pos.Rx = 3.14.rad();
                Pos.Ry = 0.rad();
                Pos.Rz = 0.rad();
                cmdTxt.Add($"movep2({Pos.ToString("p[]")})");

                cmdTxt.Add($"function(cup)");
            }
            else if (target.Name.IndexOf("Spoon") >= 0)
            {
                foreach (string str in File.ReadAllLines("j_top.path"))//防止下個點失敗手臂 
                    cmdTxt.Add(str);

                Pos.Z = 0.2.M();
                Pos.Rx = 3.14.rad();
                Pos.Ry = 0.rad();
                Pos.Rz = 0.rad();
                cmdTxt.Add($"movep2({Pos.ToString("p[]")})");

                cmdTxt.Add($"function(spoon)");
            }
            if (target.Name.IndexOf("pill box") >= 0)
            {
                cmdTxt.Add("movej([1.0322,-2.00041,1.71881,-1.29203,-1.56557,2.60226])");//防止下個點失敗手臂 
                Pos.Z = 0.2.M();
                Pos.Rx = 3.14.rad();
                Pos.Ry = 0.rad();
                Pos.Rz = 0.rad();
                cmdTxt.Add($"movep2({Pos.ToString("p[]")})");

                cmdTxt.Add($"function(pillBox)");
            }
        }
        private void Place(Objects target, Objects destination)
        {//需要target   關係到怎麼放

            //還沒做 更新nowPos!!!!!!!!!!!!!!!!!!
            URCoordinates Pos = destination.nowPos;
            if (target.Name.IndexOf("cup") >= 0)
            {
                //cmdTxt.Add("movej([1.93528,-1.62399,1.80224,-1.23603,-2.53249,2.20658])");//防止下個點失敗手臂 
                //Pos.Z = 0.2.M();
                //Pos.Rx = 2.5.rad();
                //Pos.Ry = 2.5.rad();
                //Pos.Rz = -1.5.rad();
                //cmdTxt.Add($"movep({Pos.ToString("p[]")})");
               // cmdTxt.Add("movej([1.0322,-2.00041,1.71881,-1.29203,-1.56557,2.60226])");//防止下個點失敗手臂 
                Pos.Z = 0.2.M();
                Pos.Rx = 3.14.rad();
                Pos.Ry = 0.rad();
                Pos.Rz = 0.rad();
                URCoordinates goPos = PickPosition(Pos.X, Pos.Y, Pos.Z, "cup");
                cmdTxt.Add($"movep2({goPos.ToString("p[]")})");

                URCoordinates down = new URCoordinates();
                down.Z = -130.mm();
                cmdTxt.Add($"Rmovep({down.ToString("p[]")})");

                cmdTxt.Add("rq_move(0)");

                URCoordinates up = new URCoordinates();
                up.Z = 130.mm();
                cmdTxt.Add($"Rmovep({up.ToString("p[]")})");
            }
            else if (target.Name.IndexOf("Spoon") >= 0)
            {
                cmdTxt.Add("movej([1.93528,-1.62399,1.80224,-1.23603,-2.53249,2.20658])");//防止下個點失敗手臂 
                Pos.Z = 0.2.M();
                Pos.Rx = 2.5.rad();
                Pos.Ry = 2.5.rad();
                Pos.Rz = -1.5.rad();
                cmdTxt.Add($"movep({Pos.ToString("p[]")})");

                URCoordinates down = new URCoordinates();
                down.Z = -175.mm();
                cmdTxt.Add($"Rmovep({down.ToString("p[]")})");

                cmdTxt.Add("rq_move(0)");

                URCoordinates up = new URCoordinates();
                up.Z = 175.mm();
                cmdTxt.Add($"Rmovep({up.ToString("p[]")})");
            }
        }
        private void Pour(Objects target, Objects destination)
        {//destination 可能是 pos
            URCoordinates desPos = destination.nowPos;
            if (target.Name.IndexOf("cup") >= 0)
            {

                URCoordinates goPos = PickPosition(desPos.X, desPos.Y, 0.2.M(), "cup");
                PourPos(goPos.X, goPos.Y, out goPos.X, out goPos.Y);
                cmdTxt.Add($"movep({goPos.ToString("p[]")})");

                URCoordinates pour = new URCoordinates();
                pour.Rz = -50.deg();//注意!!這裡是j6軸
                cmdTxt.Add($"Rmovej({pour.ToString("[]")})");

                URCoordinates down = new URCoordinates();
                down.Z = -40.mm();
                cmdTxt.Add($"Rmovep({down.ToString("[]")})");

                pour.Rz = -50.deg();//注意!!這裡是j6軸
                cmdTxt.Add($"Rmovej({pour.ToString("[]")})");

                URCoordinates up = new URCoordinates();
                up.Z = 40.mm();
                cmdTxt.Add($"Rmovep({up.ToString("[]")})");

                URCoordinates back = new URCoordinates();
                back.Rz = 100.deg();//注意!!這裡是j6軸
                cmdTxt.Add($"Rmovej({back.ToString("[]")})");
            }
        }
        private void Scoop(Objects target, Objects destination)
        {//target  一定是湯匙
            foreach (string str in File.ReadAllLines("act_scoop.path"))
                cmdTxt.Add(str);         
        }
        private void AddIn(Objects target, Objects destination)
        {//target  一定是湯匙

        }
        private void Stir(Objects target, Objects destination)
        {//target  一定是湯匙

        }


        float URyLimitMin = -500;
        float URyLimitMax = -180;
        public Angle PickAngle(Unit Tx, Unit Ty)
        {
            if (Ty.mm.IsBetween((int)URyLimitMin, (int)URyLimitMax))
            {
                int angle = (int)((Ty.mm - (URyLimitMax)) * ((45f - 0f) / ((URyLimitMin) - (URyLimitMax))));
                return angle.deg();
            }
            else
            {
                return new Angle();
            }
        }
        public URCoordinates PickPosition(Unit Tx, Unit Ty, Unit Tz, string obj)
        {

            Angle angle = PickAngle(Tx, Ty);
            URCoordinates.Vector3 rpy = new URCoordinates.Vector3();// = new URCoordinates.Vector3(-90.deg(), 180.deg(), (-90 - angle).deg());
            if (obj == "cup")
            {
                rpy = new URCoordinates.Vector3(-45.deg(), 180.deg(), (-90 - angle.deg).deg());
            }
            else
            {
            }

            URCoordinates.Vector3 rotation = URCoordinates.ToRotVector(rpy);
            return new URCoordinates(Tx, Ty, Tz, rotation.X.rad(), rotation.Y.rad(), rotation.Z.rad());
        }
        public void PourPos(Unit Cx, Unit Cy, out Unit PourX, out Unit PourY)
        {
            Angle angle = -PickAngle(Cx, Cy);//坐標軸 X向上 Y向左 所以
            Unit dx = 30.mm();
            Unit dy = -30.mm();
            double Px = Math.Cos(angle.rad) * dx.mm - Math.Sin(angle.rad) * dy.mm;
            double Py = Math.Sin(angle.rad) * dx.mm + Math.Cos(angle.rad) * dy.mm;

            PourX = (Cx.mm + Px).mm();
            PourY = (Cy.mm + Py).mm();
        }
    }


}
