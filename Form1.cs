using Guna.UI.WinForms;
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
    public partial class Loginfm : Form
    {

        public Loginfm()
        {
            InitializeComponent();
            PBar.Value = 0;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
   
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void ClearDataFromForm()
        {
 
        }

        private void SignInbtn_Click(object sender, EventArgs e)
        {

            ClearDataFromForm();

        }
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {


            if (PBar.Value< 100)
            {
                PBar.Value += 1;
                PBar.Text = "FYP Data..." + PBar.Value.ToString() + "%";
            }
            else
            {
                timer1.Stop();
          
                Form form2 = new mainfrm();
                form2.Show();
                this.Hide();
            }
        }

        private void PBar_Click(object sender, EventArgs e)
        {

        }
    }
}
