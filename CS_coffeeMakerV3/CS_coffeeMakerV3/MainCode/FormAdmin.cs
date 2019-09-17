using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using myObjects;

namespace CS_coffeeMakerV3
{
    public partial class FormAdmin : Form
    {
        Form1 mainForm;
        public FormAdmin(Form1 parentForm)
        {
            InitializeComponent();
            mainForm = parentForm;
        }

        private void Button_act_toggle_Click(object sender, EventArgs e)
        {
            //foreach (Objects cup in Form1.cups)
            //    if (Form1.handing != cup)
            mainForm.ActionBaseAdd("Place", Form1.handing.Name, color1: Color.FromArgb(240,230,176), color2: Form1.handing.color);
            Form1.handing = Form1.machine;
            mainForm.ActionBaseAdd("Toggle", "", color1: Color.FromArgb(200,190,231), color2: new Color());

        }

        private void Button_act_pour_Click(object sender, EventArgs e)
        {
            //這裡偷吃步 用handing到給 非handing  //多個杯子會有問題          
            if (Form1.handing == Form1.cups[0])
            {
                mainForm.ActionBaseAdd("Pour", Form1.cups[0].Name, color1: Color.FromArgb(180,230,30), color2: Form1.cups[0].color);
                mainForm.ActionBaseAdd("to", Form1.cups[1].Name, color1: Color.FromArgb(180, 230, 30), color2: Form1.cups[1].color);
            }
            else if (Form1.handing == Form1.cups[1])
            {
                mainForm.ActionBaseAdd("Pour", Form1.cups[1].Name, color1: Color.FromArgb(180, 230, 30), color2: Form1.cups[1].color);
                mainForm.ActionBaseAdd("to", Form1.cups[0].Name, color1: Color.FromArgb(180, 230, 30), color2: Form1.cups[0].color);
            }
            else
                mainForm.ActionBaseAdd("Error", "", new Color());
        }
    }
}
