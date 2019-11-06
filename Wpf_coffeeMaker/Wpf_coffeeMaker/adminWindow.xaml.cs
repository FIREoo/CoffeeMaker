using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
            list.Add("pour");
            list.Add("組1");
            list.Add("PPP");
            return list;
        }
        private void Cb_simTemplate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_simTemplate.SelectedIndex == 0)
            {
                cb_action.SelectedIndex = 1;
                cb_target.SelectedIndex = 1;
                cb_destination.SelectedIndex = 0;
            }
            else if (cb_simTemplate.SelectedIndex == 1)
            {
                {
                    cb_action.SelectedIndex = 2;
                    cb_target.SelectedIndex = 1;
                    cb_destination.SelectedIndex = 9;//pos
                }
            }
            else if (cb_simTemplate.SelectedIndex == 2)
            {
                {
                    cb_action.SelectedIndex = 3;
                    cb_target.SelectedIndex = 1;//blue cup
                    cb_destination.SelectedIndex = 2;//pink
                }
            }
            else if (cb_simTemplate.SelectedIndex == 3)
            {
                {
                    cb_action.SelectedIndex = 1;//pick
                    cb_target.SelectedIndex = 1;//blue cup
                    cb_destination.SelectedIndex = 0;
                    Btn_addSimAction_Click(null, null);


                    cb_action.SelectedIndex = 3;//pour
                    cb_target.SelectedIndex = 1;//blue cup
                    cb_destination.SelectedIndex = 2;//pink
                    Btn_addSimAction_Click(null, null);
     

                    cb_action.SelectedIndex = 2;//place
                    cb_target.SelectedIndex = 1;//blue cup
                    cb_destination.SelectedIndex = 9;//pos
                    tb_pos_x.Text = "-0.1"; tb_pos_y.Text = "-0.35";
                    Btn_addSimAction_Click(null, null);
   

                    cb_action.SelectedIndex = 1;//pick
                    cb_target.SelectedIndex = 2;//pink cup
                    cb_destination.SelectedIndex = 0;
                    Btn_addSimAction_Click(null, null);


                    cb_action.SelectedIndex = 2;//place
                    cb_target.SelectedIndex = 2;//pink cup
                    cb_destination.SelectedIndex = 9;//pos
                    tb_pos_x.Text = "-0.1"; tb_pos_y.Text = "-0.22";
                    Btn_addSimAction_Click(null, null);

                }
            }
            else if (cb_simTemplate.SelectedIndex == 4)
            {
                cb_action.SelectedIndex = 1;//pick
                cb_target.SelectedIndex = 1;//blue cup
                cb_destination.SelectedIndex = 0;
                Btn_addSimAction_Click(null, null);

                cb_action.SelectedIndex = 3;
                cb_target.SelectedIndex = 1;//blue cup
                cb_destination.SelectedIndex = 2;//pink
                Btn_addSimAction_Click(null, null);

                cb_action.SelectedIndex = 2;
                cb_target.SelectedIndex = 1;
                cb_destination.SelectedIndex = 9;//pos
                Btn_addSimAction_Click(null, null);
            }
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
                try
                {
                    actLine = new ActionLine(MainWindow.actLv[cb_action.SelectedIndex], MainWindow.objects[cb_target.SelectedIndex], MainWindow.objects[cb_destination.SelectedIndex]);
                }
                catch
                {
                    MessageBox.Show("沒選??");
                    return;
                }
            }

            MainWindow.mainAction.Add(actLine);
            mw.ActionLine2ListView(actLine);
        }



        private void CheckBox_thres_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.show_thres = (bool)((CheckBox)sender).IsChecked;
        }
        private void CheckBox_level_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.show_level = (bool)((CheckBox)sender).IsChecked;
        }
        

        private void Button_fake_blueCup_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.objects[1].nowPos = new URCoordinates(0.mm(), -220.mm(), 200.mm());
        }
        private void Button_fake_spoon_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.objects[4].nowPos = new URCoordinates(150.mm(), -220.mm(), 200.mm());
        }
        private void Button_fake_pinkCup_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.objects[2].nowPos = new URCoordinates(80.mm(), -350.mm(), 200.mm());
        }
        private void Button_fake_redPillBox_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.objects[6].nowPos = new URCoordinates(-100.mm(), -350.mm(), 200.mm());
        }

        private void Button_set_gripOffset(object sender, RoutedEventArgs e)
        {
            MainWindow.val_gripOffset_cup.X = val_gripOffset_cup_x.Text.toInt();
            MainWindow.val_gripOffset_cup.Y = val_gripOffset_cup_y.Text.toInt();
        }
    }



}
