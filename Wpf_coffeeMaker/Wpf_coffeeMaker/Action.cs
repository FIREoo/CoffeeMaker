using myObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UrRobot.Coordinates;
using Picking;
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
        private void AddFile(string fileName)
        {
            if (fileName.IndexOf(".path") < 0)
                fileName += ".path";
            if (fileName.IndexOf("Path\\") < 0)
                fileName = "Path\\" + fileName;

            foreach (string str in File.ReadAllLines(fileName))//防止下個點失敗手臂 
                cmdTxt.Add(str);
        }
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
                AddFile("Path\\j_top.path");//防止下個點失敗手臂 

                cmdTxt.Add($"function(cup)");
            }
            else if (target.Name.IndexOf("Spoon") >= 0)
            {
                AddFile("act_pickSpoon.path");
            }
            if (target.Name.IndexOf("pill box") >= 0)
            {
                AddFile("Path\\j_top.path");//防止下個點失敗手臂 
                cmdTxt.Add($"function(pillBox)");
            }
        }
        private void Place(Objects target, Objects destination)
        {//需要target   關係到怎麼放

            //還沒做 更新nowPos!!!!!!!!!!!!!!!!!!
            URCoordinates Pos = destination.nowPos;
            if (target.Name.IndexOf("cup") >= 0)
            {
                Pos.Z = 0.2.M();
                Pos.Rx = 3.14.rad();
                Pos.Ry = 0.rad();
                Pos.Rz = 0.rad();
                URCoordinates goPos = PickingPos.PickPose(Pos.X, Pos.Y, Pos.Z, "cup");
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
                AddFile("act_pickSpoon.path");
            }
        }
        private void Pour(Objects target, Objects destination)
        {//destination 可能是 pos
            URCoordinates desPos = destination.nowPos;
            if (target.Name.IndexOf("cup") >= 0)
            {

                URCoordinates goPos = PickingPos.PickPose(desPos.X, desPos.Y, 0.2.M(), "cup");
                PickingPos.PourPos(goPos.X, goPos.Y, out goPos.X, out goPos.Y);
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

            AddFile("act_scoop.path");
        }
        private void AddIn(Objects target, Objects destination)
        {//target  一定是湯匙 //destination 一定是cup
            URCoordinates desPos = destination.nowPos;
            URCoordinates goPos = PickingPos.PickPose(desPos.X, desPos.Y, 0.2.M(), "add");
            PickingPos.AddInPos(goPos.X, goPos.Y, out goPos.X, out goPos.Y);
            cmdTxt.Add($"movep2({goPos.ToString("p[]")})");

            URCoordinates down0 = new URCoordinates();
            down0.Z = -20.mm();
            cmdTxt.Add($"Rmovep({down0.ToString("[]")})");

            URCoordinates pour1 = new URCoordinates();
            pour1.Rz = -45.deg();//注意!!這裡是j6軸
            cmdTxt.Add($"Rmovej({pour1.ToString("[]")})");

            URCoordinates down1 = new URCoordinates();
            down1.X = 10.mm();
            down1.Y = 20.mm();
            cmdTxt.Add($"Rmovep({down1.ToString("[]")})");

            URCoordinates pour2 = new URCoordinates();
            pour2.Rz = -25.deg();//注意!!這裡是j6軸
            cmdTxt.Add($"Rmovej({pour2.ToString("[]")})");

            URCoordinates up = new URCoordinates();
            up.Z = 20.mm();
            cmdTxt.Add($"Rmovep({up.ToString("[]")})");

            URCoordinates back = new URCoordinates();
            back.Rz = 70.deg();//注意!!這裡是j6軸
            cmdTxt.Add($"Rmovej({back.ToString("[]")})");
        }
        private void Stir(Objects target, Objects destination)
        {//target  一定是湯匙

        }

    }


}
