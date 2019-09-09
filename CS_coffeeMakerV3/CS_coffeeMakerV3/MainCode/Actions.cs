using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UrRobot;
using myObjects;

namespace CS_coffeeMakerV3
{
    class Actions
    {
        private static robotControl UR;
        private static StreamWriter txt;
        private static string fileName;
        public Actions(robotControl _UR, string file)
        {
            UR = _UR;
            fileName = file;
            txt = new StreamWriter($"Path//{file}", false);
        }
        public void end()
        {
            txt.Flush();
            txt.Close();
        }
        public bool execute()
        {
            try
            {
                txt.Flush();
                txt.Close();
            }
            catch { }
            if (UR.serverRunning == false)
            {
                return false;
            }
            UR.goFilePath(fileName);
            return true;
        }
        public void add(subactInfo subact)
        {
            for (int i = 0; i < subact.Count(); i++)
                txt.WriteLine(subact.infotxt[i]);
        }
        public void start(Objects[] cups)
        {
            foreach (Objects cup in cups)
            {
                cup.saveNowPos();
            }
        }
    }
    class Subact
    {
        public static subactInfo Pick(Objects cup)
        {
            subactInfo rtn = new subactInfo();
            URCoordinates up = new URCoordinates(cup.gripPos());
            URCoordinates debug = new URCoordinates(up);
            URCoordinates down = new URCoordinates(up);

            debug.Y -= 0.02f;//上升
            rtn.infotxt.Add("position");
            rtn.infotxt.Add(debug.ToPos());
            rtn.infotxt.Add(up.ToPos());

            rtn.infotxt.Add("gripper");
            rtn.infotxt.Add("0");

            rtn.infotxt.Add("sleep");
            rtn.infotxt.Add("1000");

            down.Y += 0.07f;//下降
            rtn.infotxt.Add("position");
            rtn.infotxt.Add(down.ToPos());

            rtn.infotxt.Add("gripper");
            rtn.infotxt.Add("35");

            rtn.infotxt.Add("sleep");
            rtn.infotxt.Add("1000");

            up.Y -= 0.02f;//上升
            rtn.infotxt.Add("position");
            rtn.infotxt.Add(up.ToPos());
            return rtn;
        }
        public static subactInfo Pick(subactInfo.place ThePlace)
        {
            subactInfo rtn = new subactInfo();
            rtn.infotxt.Add("gripper");
            rtn.infotxt.Add("0");
            rtn.infotxt.Add("sleep");
            rtn.infotxt.Add("1000");
            if (ThePlace == subactInfo.place.DripTray)
            {
                string[] file = System.IO.File.ReadAllLines($"Path//outDripTray.path");
                foreach (string line in file)
                    rtn.infotxt.Add(line);
            }

            return rtn;
        }
        public static subactInfo Place(Objects cup, URCoordinates Wpoint)
        {
            cup.setNowPos(Wpoint);

            subactInfo rtn = new subactInfo();
            URCoordinates up = new URCoordinates(cup.gripPos());
            URCoordinates debug = new URCoordinates(up);
            URCoordinates down = new URCoordinates(up);
            down.Y += 0.07f;//下降

            debug.Y -= 0.02f;//上升
            rtn.infotxt.Add("position");
            rtn.infotxt.Add(debug.ToPos());
            rtn.infotxt.Add(up.ToPos());
            rtn.infotxt.Add(down.ToPos());
            rtn.infotxt.Add("gripper");
            rtn.infotxt.Add("0");

            rtn.infotxt.Add("sleep");
            rtn.infotxt.Add("1000");

            rtn.infotxt.Add("position");
            rtn.infotxt.Add(up.ToPos());
            return rtn;
        }
        public static subactInfo Place(subactInfo.place ThePlace)
        {
            subactInfo rtn = new subactInfo();
            if (ThePlace == subactInfo.place.DripTray)
            {
                string[] file = System.IO.File.ReadAllLines($"Path//toDripTray.path");
                foreach (string line in file)
                    rtn.infotxt.Add(line);
            }
            rtn.infotxt.Add("gripper");
            rtn.infotxt.Add("0");
            rtn.infotxt.Add("sleep");
            rtn.infotxt.Add("1000");
            return rtn;
        }
        public static subactInfo Pour(Objects toCup)
        {
            subactInfo rtn = new subactInfo();
            URCoordinates up = new URCoordinates(toCup.gripPos());
            up.Y -= 0.03f;//上升
            URCoordinates debug = new URCoordinates(up);
            debug.Y -= 0.01f;//上升
            URCoordinates now = new URCoordinates(up);
            rtn.infotxt.Add("position");
            rtn.infotxt.Add(debug.ToPos());
            rtn.infotxt.Add(up.ToPos());
            now.X -= 0.09f;
            rtn.infotxt.Add(now.ToPos());
            now.Rx -= 1.57f;
            now.Ry += 1.57f;
            rtn.infotxt.Add(now.ToPos());
            now.Rx -= 1.13f;
            now.Ry += 1.13f;
            now.X += 0.06f;
            rtn.infotxt.Add(now.ToPos());
            up.Y -= 0.02f;//上升
            rtn.infotxt.Add(up.ToPos());
            rtn.infotxt.Add(debug.ToPos());
            return rtn;
        }
        public static subactInfo Trigger()
        {
            subactInfo rtn = new subactInfo();
            rtn.AddFile("Path//trigger.path");
            return rtn;
        }
        public static subactInfo Scoop()
        {
            subactInfo rtn = new subactInfo();
            rtn.AddFile("Path//Scoop.path");
            return rtn;
        }
        public static subactInfo Stir(Objects toCup)
        {
            subactInfo rtn = new subactInfo();
            URCoordinates up = new URCoordinates(toCup.gripPos());
            up.X -= 0.015f;
            up.Z += 0.015f;
            up.Y -= 0.06f;//上升
            URCoordinates now = new URCoordinates(up);
            rtn.infotxt.Add("pmovej");
            rtn.infotxt.Add(up.ToPos());
            now.Y += 0.05f;//下去

            now.X -= 0.01f;
            rtn.infotxt.Add(now.ToPos());
            now.X += 0.01f;
            now.Z += 0.01f;
            rtn.infotxt.Add(now.ToPos());
            now.X += 0.01f;
            now.Z -= 0.01f;
            rtn.infotxt.Add(now.ToPos());
            now.X -= 0.01f;
            now.Z -= 0.01f;
            rtn.infotxt.Add(now.ToPos());
            now.X -= 0.01f;
            now.Z += 0.01f;
            rtn.infotxt.Add(now.ToPos());
            rtn.infotxt.Add(up.ToPos());
            return rtn;
        }
        public static subactInfo AddaSpoon(Objects toCup)
        {
            subactInfo rtn = new subactInfo();
            URCoordinates up = new URCoordinates(toCup.gripPos());
            up.Rx = 2.2f;
            up.Ry = -2.2f;
            up.Rz = 0;
            up.X -= 0.09f;
            up.Z += 0.02f;
            up.Y += 0.03f;//下降

            URCoordinates upper = new URCoordinates(up);
            upper.Y -= 0.07f;//上
            rtn.infotxt.Add("pmovej");
            rtn.infotxt.Add(upper.ToPos());


            URCoordinates add = new URCoordinates(up);
            add.Rx = 3.14f;
            add.Ry = 0f;
            add.Rz = 0f;
            add.Y -= 0.09f;//上升
            add.X += 0.05f;
            rtn.infotxt.Add("pmovej");
            rtn.infotxt.Add(up.ToPos());
            rtn.infotxt.Add("pmovej");
            rtn.infotxt.Add(add.ToPos());
            rtn.infotxt.Add("pmovej");
            add.Y -= 0.03f;//上升
            rtn.infotxt.Add(add.ToPos());
            return rtn;
        }
        public static class Name
        {
            public static string Pick = "Pick";
            public static string Place = "Place";
            public static string Pour = "Pour";
            public static string Trigger = "Touggle";
            public static string Scoop = "Ladle";
            public static string AddaSpoon = "Add";
            public static string Stir = "Stir";
        }

    }

    class subactInfo
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
    }
}
