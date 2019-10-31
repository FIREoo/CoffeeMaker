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
        }
        private List<string> ActionList()
        {
            List<string> list = new List<string>();
            list.Add(SubAct.Name.Pick);
            list.Add(SubAct.Name.Place);
            list.Add(SubAct.Name.Pour);
            list.Add(SubAct.Name.AddaSpoon);
            list.Add(SubAct.Name.Scoop);
            return list;
        }
        private List<string> TargetList()
        {
            List<string> list = new List<string>();
            foreach(var obj in MainWindow.theObjects)
            {
                list.Add(obj.Name);
            }
            return list;
        }
        private List<string> DestinationList()
        {
            List<string> list = new List<string>();
            list.Add("pos");
            foreach (var obj in MainWindow.theObjects)
            {
                list.Add(obj.Name);
            }
            return list;
        }

        private void Btn_addSimAction_Click(object sender, RoutedEventArgs e)
        {
            mw.addActionBase(cb_action.Text, cb_target.Text, cb_destination.Text);
        }


    }

 

}
