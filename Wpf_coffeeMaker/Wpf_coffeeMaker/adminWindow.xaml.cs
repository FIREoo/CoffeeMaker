using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using myActionBase;
using UrRobot.Coordinates;


namespace Wpf_coffeeMaker
{

    public partial class adminWindow : Window
    {
        MainWindow mw = (MainWindow)Application.Current.MainWindow;
        public adminWindow()
        {
            InitializeComponent();

            List<string> itemNames = ActionList();//準備資料
            cb_action.ItemsSource = itemNames;  //資料繫結

            List<string> itemNames2 = TargetList();//準備資料
            cb_target.ItemsSource = itemNames2;  //資料繫結

            List<string> itemNames3 = DestinationList();//準備資料
            cb_destination.ItemsSource = itemNames3;  //資料繫結

            List<string> itemNames4 = TemplateList();//準備資料
            cb_simTemplate.ItemsSource = itemNames4;  //資料繫結
        }
        private List<string> ActionList()
        {
            List<string> list = new List<string>();
            foreach (var act in MainWindow.actLv)
                list.Add(act.Name);
            return list;
        }
        private List<string> TargetList()
        {
            List<string> list = new List<string>();
            foreach (var obj in MainWindow.objects)
                list.Add(obj.Name);
            return list;
        }
        private List<string> DestinationList()
        {
            List<string> list = new List<string>();

            foreach (var obj in MainWindow.objects)
                list.Add(obj.Name);

            list.Add("pos");
            return list;
        }
        private List<string> TemplateList()
        {
            List<string> list = new List<string>();
            list.Add("pick");
            list.Add("place");
            return list;
        }

        private void Btn_addSimAction_Click(object sender, RoutedEventArgs e)
        {
            ActionLine actLine;
            if (cb_destination.SelectedIndex >= MainWindow.objects.Count)//代表選到pos
            {
                myObjects.Objects pos = new myObjects.Objects(100, "pos");
                pos.nowPos = new URCoordinates(tb_pos_x.Text.toFloat(), tb_pos_y.Text.toFloat(), tb_pos_z.Text.toFloat(), 0, 0, 0);
                actLine = new ActionLine(MainWindow.actLv[cb_action.SelectedIndex], MainWindow.objects[cb_target.SelectedIndex], pos);
            }
            else
            {
                //actLine = new ActionLine(MainWindow.actions[cb_action.SelectedIndex], MainWindow.objects[cb_target.SelectedIndex], MainWindow.objects[cb_destination.SelectedIndex]);
                actLine = new ActionLine(MainWindow.actLv[cb_action.SelectedIndex], MainWindow.objects[cb_target.SelectedIndex], MainWindow.objects[cb_destination.SelectedIndex]);
            }

            MainWindow.mainAction.Add(actLine);
            mw.ActionLine2ListView(actLine);
        }

        private void Cb_simTemplate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_simTemplate.SelectedIndex == 0)
            {
                cb_action.SelectedIndex = 1;
                cb_target.SelectedIndex = 1;
                cb_destination.SelectedIndex = 0;
            }
            else if(cb_simTemplate.SelectedIndex == 1)
            {
                {
                    cb_action.SelectedIndex = 2;
                    cb_target.SelectedIndex = 1;
                    cb_destination.SelectedIndex = 9;//pos
                }
            }
        }
    }



}
