using myObjects;
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
    public class ActionListView
    {
        //-1 initial
        //0 保留 none
        // 100 position
        public int ActionInedx = -1;
        public int TargetIndex = -1;
        public int DestinationIndex = -1;

        public string ActionName = "";
        public string TargetName = "";
        public string DestinationName = "";

    }


    public class ActionCmd
    {
        private static StreamWriter txt;
        private static string fileName;
        public ActionCmd(string file)
        {
            //UR = _UR;
            fileName = file;

            //need file
            //--Path//outDripTray.path

        }
        public void saveFile()
        {
            txt.Flush();
            txt.Close();
        }

        public bool add(subactInfo subact)
        {
            try
            {
                for (int i = 0; i < subact.Count(); i++)
                    txt.WriteLine(subact.infotxt[i]);
                return true;
            }
            catch //應該是檔案沒被開啟會錯誤(通常是在已經saveFile了
            {
                return false;
            }
        }
        public void start(Objects[] cups)
        {
            txt = new StreamWriter($"Path//{fileName}", false);
            foreach (Objects cup in cups)
            {
                cup.savePositionToNowPos();
            }
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

        //public static subactInfo Pick(Objects cup)
        //{
        //    subactInfo rtn = new subactInfo();
        //    URCoordinates grip = new URCoordinates(cup.gripPos());
        //    URCoordinates up = new URCoordinates(grip);
        //    //up.Y -= 0.1f;
        //    //URCoordinates debug = new URCoordinates(up);
        //    //debug.Y -= 0.02f;//在高一點以Rxyz有問題

        //    //rtn.infotxt.Add("position");
        //    //rtn.infotxt.Add(debug.ToPos());
        //    //rtn.infotxt.Add(up.ToPos());

        //    //rtn.infotxt.Add("gripper");
        //    //rtn.infotxt.Add("0");

        //    //rtn.infotxt.Add("sleep");
        //    //rtn.infotxt.Add("1000");

        //    //rtn.infotxt.Add("position");
        //    //rtn.infotxt.Add(grip.ToPos());

        //    //rtn.infotxt.Add("gripper");
        //    //rtn.infotxt.Add("31");

        //    //rtn.infotxt.Add("sleep");
        //    //rtn.infotxt.Add("1000");

        //    //rtn.infotxt.Add("position");
        //    //rtn.infotxt.Add(up.ToPos());
        //    return rtn;
        //}
        //public static subactInfo Pick(subactInfo.place ThePlace)
        //{
        //    subactInfo rtn = new subactInfo();
        //    rtn.infotxt.Add("gripper");
        //    rtn.infotxt.Add("0");
        //    rtn.infotxt.Add("sleep");
        //    rtn.infotxt.Add("1000");
        //    if (ThePlace == subactInfo.place.DripTray)
        //    {
        //        rtn.AddFile($"Path//outDripTray.path");
        //    }

        //    return rtn;
        //}
        //public static subactInfo Pick(subactInfo.thing TheObject)
        //{
        //    subactInfo rtn = new subactInfo();
        //    rtn.infotxt.Add("gripper");
        //    rtn.infotxt.Add("0");
        //    rtn.infotxt.Add("sleep");
        //    rtn.infotxt.Add("1000");
        //    if (TheObject == subactInfo.thing.Capsule)
        //    {
        //        rtn.AddFile($"Path//moveCapsule.path");
        //    }
        //    if (TheObject == subactInfo.thing.Case)
        //    {
        //        // rtn.AddFile($"Path//moveCase.path");
        //    }
        //    return rtn;
        //}
        //public static subactInfo Place(Objects cup, URCoordinates Wpoint)
        //{
        //    cup.setNowPos(Wpoint);

        //    subactInfo rtn = new subactInfo();
        //    URCoordinates grip = new URCoordinates(cup.gripPos());
        //    URCoordinates up = new URCoordinates(grip);
        //    //up.Y -= 0.1f;
        //    //URCoordinates debug = new URCoordinates(up);
        //    //debug.Y -= 0.02f;//在高一點以Rxyz有問題

        //    //rtn.infotxt.Add("position");
        //    //rtn.infotxt.Add(debug.ToPos());
        //    //rtn.infotxt.Add(up.ToPos());
        //    //rtn.infotxt.Add(grip.ToPos());
        //    //rtn.infotxt.Add("gripper");
        //    //rtn.infotxt.Add("0");

        //    //rtn.infotxt.Add("sleep");
        //    //rtn.infotxt.Add("1000");

        //    //rtn.infotxt.Add("position");
        //    //rtn.infotxt.Add(up.ToPos());
        //    return rtn;
        //}
        //public static subactInfo Place(subactInfo.place ThePlace)
        //{
        //    subactInfo rtn = new subactInfo();
        //    if (ThePlace == subactInfo.place.DripTray)
        //    {
        //        string[] file = System.IO.File.ReadAllLines($"Path//toDripTray.path");
        //        foreach (string line in file)
        //            rtn.infotxt.Add(line);
        //    }
        //    rtn.infotxt.Add("gripper");
        //    rtn.infotxt.Add("0");
        //    rtn.infotxt.Add("sleep");
        //    rtn.infotxt.Add("1000");
        //    return rtn;
        //}
        //public static subactInfo Pour(Objects toCup)
        //{
        //    subactInfo rtn = new subactInfo();
        //    URCoordinates grip = new URCoordinates(toCup.gripPos());
        //    URCoordinates up = new URCoordinates(grip);
        //    //up.Y -= 0.1f;
        //    //URCoordinates debug = new URCoordinates(up);
        //    //debug.Y -= 0.02f;//在高一點以Rxyz有問題

        //    //URCoordinates now = new URCoordinates(up);

        //    //rtn.infotxt.Add("position");
        //    //rtn.infotxt.Add(debug.ToPos());
        //    //rtn.infotxt.Add(up.ToPos());

        //    //now.X -= 0.08f;
        //    //rtn.infotxt.Add(now.ToPos());

        //    //rtn.infotxt.Add("Rmovej");
        //    //rtn.infotxt.Add("[0,0,0,0,0,-2]");

        //    //rtn.infotxt.Add("position");
        //    //rtn.infotxt.Add(debug.ToPos());
        //    return rtn;
        //}
        //public static subactInfo Trigger()
        //{
        //    subactInfo rtn = new subactInfo();
        //    rtn.AddFile("Path//trigger.path");
        //    return rtn;
        //}
        //public static subactInfo PutBoxIn()
        //{
        //    subactInfo rtn = new subactInfo();
        //    rtn.AddFile("Path//putBoxIn.path");
        //    return rtn;
        //}
        //public static subactInfo Scoop()
        //{
        //    subactInfo rtn = new subactInfo();
        //    rtn.AddFile("Path//Scoop.path");
        //    return rtn;
        //}
        //public static subactInfo Stir(Objects toCup)
        //{
        //    subactInfo rtn = new subactInfo();
        //    URCoordinates up = new URCoordinates(toCup.gripPos());
        //    //up.X -= 0.015f;
        //    //up.Z += 0.015f;
        //    //up.Y -= 0.06f;//上升
        //    //URCoordinates now = new URCoordinates(up);
        //    //rtn.infotxt.Add("pmovej");
        //    //rtn.infotxt.Add(up.ToPos());
        //    //now.Y += 0.05f;//下去

        //    //now.X -= 0.01f;
        //    //rtn.infotxt.Add(now.ToPos());
        //    //now.X += 0.01f;
        //    //now.Z += 0.01f;
        //    //rtn.infotxt.Add(now.ToPos());
        //    //now.X += 0.01f;
        //    //now.Z -= 0.01f;
        //    //rtn.infotxt.Add(now.ToPos());
        //    //now.X -= 0.01f;
        //    //now.Z -= 0.01f;
        //    //rtn.infotxt.Add(now.ToPos());
        //    //now.X -= 0.01f;
        //    //now.Z += 0.01f;
        //    //rtn.infotxt.Add(now.ToPos());
        //    //rtn.infotxt.Add(up.ToPos());
        //    return rtn;
        //}
        //public static subactInfo AddaSpoon(Objects toCup)
        //{
        //    subactInfo rtn = new subactInfo();
        //    URCoordinates up = new URCoordinates(toCup.gripPos());
        //    //up.Rx = 2.2f;
        //    //up.Ry = -2.2f;
        //    //up.Rz = 0;
        //    //up.X -= 0.09f;
        //    //up.Z += 0.02f;
        //    //up.Y += 0.03f;//下降

        //    //URCoordinates upper = new URCoordinates(up);
        //    //upper.Y -= 0.07f;//上
        //    //rtn.infotxt.Add("pmovej");
        //    //rtn.infotxt.Add(upper.ToPos());


        //    //URCoordinates add = new URCoordinates(up);
        //    //add.Rx = 3.14f;
        //    //add.Ry = 0f;
        //    //add.Rz = 0f;
        //    //add.Y -= 0.09f;//上升
        //    //add.X += 0.05f;
        //    //rtn.infotxt.Add("pmovej");
        //    //rtn.infotxt.Add(up.ToPos());
        //    //rtn.infotxt.Add("pmovej");
        //    //rtn.infotxt.Add(add.ToPos());
        //    //rtn.infotxt.Add("pmovej");
        //    //add.Y -= 0.03f;//上升
        //    //rtn.infotxt.Add(add.ToPos());
        //    return rtn;
        //}
    }

    public class subactInfo
    {
        public List<string> infotxt = new List<string>();
        public void AddFile(string filename)
        {
            string[] file = System.IO.File.ReadAllLines(filename);
            foreach (string line in file)
                infotxt.Add(line);
        }
        public int Count()
        {
            return infotxt.Count();
        }
        public enum place
        {
            DripTray = 0
        }
        public enum thing
        {
            Capsule = 0,
            Case = 1
        }
    }

    public static class ActionName
    {
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
            URCoordinates Pos = target.getPosition();
            cmdTxt.Add($"movep({Pos.ToString("p[]")})");
            cmdTxt.Add("rq_move(255)");
        }
        private void Place(Objects target, Objects destination)
        {//需要target   關係到怎麼放
            URCoordinates Pos = destination.getPosition();
            cmdTxt.Add($"movep({Pos.ToString("p[]")})");
            cmdTxt.Add("rq_move(0)");
        }
        private void Pour(Objects target, Objects destination)
        {//destination 可能是 pos
            URCoordinates Pos = destination.getPosition();
            cmdTxt.Add($"movep({Pos.ToString("p[]")})");
            cmdTxt.Add("rq_move(0)");
        }
        private void Scoop(Objects target, Objects destination)
        {//target  一定是湯匙
            URCoordinates Pos = destination.getPosition();
            cmdTxt.Add($"movep({Pos.ToString("p[]")})");
            cmdTxt.Add("rq_move(0)");
        }
        private void AddIn(Objects target, Objects destination)
        {//target  一定是湯匙
            URCoordinates Pos = destination.getPosition();
            cmdTxt.Add($"movep({Pos.ToString("p[]")})");
            cmdTxt.Add("rq_move(0)");
        }
        private void Stir(Objects target, Objects destination)
        {//target  一定是湯匙
            URCoordinates Pos = destination.getPosition();
            cmdTxt.Add($"movep({Pos.ToString("p[]")})");
            cmdTxt.Add("rq_move(0)");
        }
    }


}
