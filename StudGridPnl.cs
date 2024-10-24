
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBMIDPRO
{
    public partial class StudGridPnl : UserControl
    {

        DataGridViewRow selectedRow;
        bool isAdvisor = false;
        public StudGridPnl()
        {

            InitializeComponent();
            this.Padding = new Padding(5); // Padding to make room for rounded corners
            ViewStudent();
            studentpnl1.EvaluationChanged += Evaluationpnl_EvaluationChanged;
            hideElement();
            CrossBtn.Visible = false;

        }
        public void hideElement()
        {
            Delbtn.Visible = false;
            gunaButton1.Visible = false;
            StudentGrd.Visible = false;
            panel2.Visible = false;
            nextBtn.Visible = false;
        }

        public void viewelement()
        {
            Delbtn.Visible = true;
            
            StudentGrd.Visible = true;
            panel2.Visible = true;
            nextBtn.Visible = true;
        }

        private void StudGridPnl_Load(object sender, EventArgs e)
        {
            // Set the region to create rounded corners
            SetRoundRegion();
            ViewStudent();
            hideElement();
        }
        private void Evaluationpnl_EvaluationChanged(object sender, EventArgs e)
        {
            // Handle the EvaluationChanged event
            // For example, you can refresh the grid data here
            ViewStudent();

        }

        private void SetRoundRegion()
        {
            GraphicsPath path = new GraphicsPath();
            int radius = 20; // Adjust the radius to change the roundness of corners
            int diameter = radius * 2;

            // Top left corner
            path.AddArc(new Rectangle(0, 0, diameter, diameter), 180, 90);
            // Top right corner
            path.AddArc(new Rectangle(this.Width - diameter - 1, 0, diameter, diameter), 270, 90);
            // Bottom right corner
            path.AddArc(new Rectangle(this.Width - diameter - 1, this.Height - diameter - 1, diameter, diameter), 0, 90);
            // Bottom left corner
            path.AddArc(new Rectangle(0, this.Height - diameter - 1, diameter, diameter), 90, 90);

            path.CloseFigure();
            this.Region = new Region(path);
        }



        public void ViewStudent()
        {
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("SELECT S.RegistrationNo AS [Reg], (P.FirstName + ' ' + P.LastName) AS Name, L.Value AS Gender, (SELECT FORMAT(P.DateOfBirth, 'dd-MM-yyyy')) AS [DoB], P.Contact, P.Email FROM Person P JOIN Student S ON S.Id = P.Id JOIN Lookup L ON L.Id = P.Gender  where P.FirstName NOT LIKE '!%'", con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            StudentGrd.DataSource = dt;
        }

        private void nextBtn_Click(object sender, EventArgs e)
        {
            studentpnl1.Visible = true;
        }

        private void SearchByNameBtn_Click(object sender, EventArgs e)
        {

           
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("SELECT S.RegistrationNo AS [Reg], (P.FirstName + ' ' + P.LastName) AS Name, L.Value AS Gender, CONVERT(varchar, P.DateOfBirth, 105) AS [DoB], P.Contact, P.Email FROM Person P JOIN Student S ON S.Id = P.Id JOIN Lookup L ON L.Id = P.Gender WHERE S.RegistrationNo LIKE @FirstName  and P.FirstName NOT LIKE '!%' ORDER BY CASE WHEN S.RegistrationNo = @FirstName THEN 0 ELSE 1 END", con);
            cmd.Parameters.AddWithValue("@FirstName", "%" + RegTxt.Text + "%");

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            gunaButton1.Visible = true;
            if (dt.Rows.Count > 0)
            {
                StudentGrd.DataSource = dt;
                StudentGrd.Visible = true;
                if (isAdvisor==true)
                { refreashBtn.Visible = true;
                    gunaLabel1.Visible = false;
                }
                if(isAdvisor==false)
                { gunaLabel1.Visible = true;
                    refreashBtn.Visible = false;
                }
               
                RegTxt.Text = string.Empty;
            }
            else
            {
                MessageBox.Show( " student of this Registration number  not found.");
                ViewStudent();
                RegTxt.Text = string.Empty;
            }
        }




        private void gunaButton1_Click(object sender, EventArgs e)
        {

            DataGridViewRow row = StudentGrd.CurrentRow;
            if (row != null)
            {
                selectedRow = row; // Store the selected row
                string name = row.Cells["Name"].Value.ToString();
                string[] names = name.Split(' ');
                string FName = names[0];
                string LName = names.Length > 1 ? names[1] : "";
                string contact = row.Cells["Contact"].Value.ToString();
                string email = row.Cells["Email"].Value.ToString();
                string regno = row.Cells["Reg"].Value.ToString();
                string gender = row.Cells["Gender"].Value.ToString();

                studentpnl1.changetext();
                // Update values in studentpnl1 UserControl
                studentpnl1.FirstName = FName;
                studentpnl1.LastName = LName;
                studentpnl1.Contact = contact;
                studentpnl1.Email = email;
                studentpnl1.RegistrationNumber = regno;
                studentpnl1.Gender = gender;

                studentpnl1.registraionEnable();
                // Call the update method
                studentpnl1.TakeRegForUpdation(regno);

                // Make studentpnl1 visible
                studentpnl1.Visible = true;
            }
        }

        private void Delbtn_Click(object sender, EventArgs e)
        {


            DataGridViewRow row = StudentGrd.CurrentRow;
            if (row != null)
            {
                selectedRow = row; // Store the selected row
                string regno = row.Cells["Reg"].Value.ToString();
                var con = Configuration.getInstance().getConnection();
                SqlCommand cmd = new SqlCommand("UPDATE Person SET FirstName = '! ' + FirstName WHERE Id IN (SELECT Id FROM Student WHERE RegistrationNo = @RegNo)", con);
                cmd.Parameters.AddWithValue("@RegNo", regno);
                int rowsAffected = cmd.ExecuteNonQuery();
                MessageBox.Show("Successfully deleted");
                ViewStudent();
            }
        }

        private void studentpnl1_Load(object sender, EventArgs e)
        {

        }

        private void refreashBtn_Click(object sender, EventArgs e)
        {
            ViewStudent();
            refreashBtn.Visible = false;
            gunaLabel1.Visible = true;
        }
        public void hideBtn()
        {
            AdvisorBtn.Visible = false;
            Student.Visible = false;


        }

        private void AdvisorBtn_Click(object sender, EventArgs e)
        {
            hideBtn();
            viewelement();
            isAdvisor = true;
            CrossBtn.Visible = true;
            gunaButton1.Visible = true;

        }

        public void showBtn()
        {
            AdvisorBtn.Visible = true;
            Student.Visible = true;
        }

        private void Student_Click(object sender, EventArgs e)
        {
            hideBtn();
            hideElement();
            CrossBtn.Visible = true;
            
            panel2.Visible = true;
            MessageBox.Show("Enter Your Registration number to find your detail");
    
        }

        private void CrossBtn_Click(object sender, EventArgs e)
        {
            hideElement();
            showBtn();
            CrossBtn.Visible = false;
        }
    }
}
