using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBMIDPRO
{
    public partial class mainfrm : Form
    {
        public mainfrm()
        {
            InitializeComponent();
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }


        private void minimizebtn_Click(object sender, EventArgs e)
        {
            WindowState=FormWindowState.Minimized;
        }

        private void maxbtn_Click(object sender, EventArgs e)
        {
            if(WindowState==FormWindowState.Normal)
            {
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                WindowState = FormWindowState.Normal;
            }
          
        }

        private void ExitBtn_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void stdbtn_Click(object sender, EventArgs e)
        {
            HideOtherPnl();
            headlbl.Text = "Student Management";
            studGridPnl1.Visible = true;

        }

        private void advBtn_Click(object sender, EventArgs e)
        {
             HideOtherPnl();
             headlbl.Text = "Advisor Add Board";
            advisoryGrdPnnl1.Visible = true;
        }

        private void grpBtn_Click(object sender, EventArgs e)
        {
            HideOtherPnl();
            headlbl.Text = "Group Management";
            groupGrdPnl1.Visible = true;

        }

        private void proBtn_Click(object sender, EventArgs e)
        {
            HideOtherPnl();
            headlbl.Text = "Projects Management";
            projectGRidPnl1.Visible = true;

        }

        private void assignadvBtn_Click(object sender, EventArgs e)
        {
            HideOtherPnl();
            headlbl.Text = "Advisory Management";
            advisorDataPnl1.Visible = true;
        }

        private void EvlBtn_Click(object sender, EventArgs e)
        {
            HideOtherPnl();
            headlbl.Text = "Evaluation Board";
            evalutaionGrid1.Visible = true;
        }

        private void MarkavlBtn_Click(object sender, EventArgs e)
        {
            HideOtherPnl();
            headlbl.Text = "Evaluation Marks";
            evlMrksGrdPnl1.Visible = true;
        }

        private void homebtn_Click(object sender, EventArgs e)
        {
            HideOtherPnl();
            headlbl.Text = "FYP Management";
            this.Visible = true;
        }

        private void HideOtherPnl()
        {
            projectGRidPnl1.Visible = false;
           evlMrksGrdPnl1.Visible = false;
            evalutaionGrid1.Visible = false;
            advisorDataPnl1.Visible = false;
            groupGrdPnl1.Visible = false;
            studGridPnl1.Visible = false;
            advisoryGrdPnnl1.Visible = false;
            reports1.Visible = false;
        }


        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void studGridPnl1_Load(object sender, EventArgs e)
        {
/*            Studentpnl stu = new Studentpnl();
            addPanels(stu)*/;
        }

        private void mainfrm_Load(object sender, EventArgs e)
        {

        }

        private void grppnl_Paint(object sender, PaintEventArgs e)
        {

        }

        private void rprtbtn_Click(object sender, EventArgs e)
        {
            HideOtherPnl();
            headlbl.Text = "Reports Board";
            reports1.Visible = true;
        }

        private void reports1_Load(object sender, EventArgs e)
        {

        }
    }


}

