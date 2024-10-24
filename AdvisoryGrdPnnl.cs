
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;

namespace DBMIDPRO
{
    public partial class AdvisoryGrdPnnl : UserControl
    {
        public AdvisoryGrdPnnl()
        {
            InitializeComponent();
            this.Padding = new Padding(5);
            advisorpnl1.EvaluationChanged += Evaluationpnl_EvaluationChanged;

        }

        private void AdvisoryGrdPnnl_Load(object sender, EventArgs e)
        {
            // Set the region to create rounded corners
            SetRoundRegion();
            ViewAdvisor();
        }
        private void Evaluationpnl_EvaluationChanged(object sender, EventArgs e)
        {
            // Handle the EvaluationChanged event
            // For example, you can refresh the grid data here
            ViewAdvisor();

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



        public void ViewAdvisor()
        {
            using (var con = new SqlConnection(Configuration.getInstance().ConnectionStr))
            {
                con.Open();

                using (SqlCommand cmd = new SqlCommand("Select L1.Value AS Designation,A.Salary, (FirstName + ' ' + LastName) AS Name,L.Value AS Gender,(SELECT FORMAT(DateOfBirth, 'dd-MM-yyyy')) AS [DoB],Contact,Email from Person P JOIN Advisor A ON A.Id=P.Id JOIN Lookup L ON L.Id=P.Gender JOIN Lookup L1 ON L1.Id=A.Designation where P.FirstName NOT LIKE '!%'", con))
                {
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    AdvisorGrd.DataSource = dt;
                }
            }
        }


        private void nextBtn_Click(object sender, EventArgs e)
        {

            advisorpnl1.Visible = true;
        }



        private void AdvisorGrd_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void SearchByNameBtn_Click(object sender, EventArgs e)
        {
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("SELECT  L1.Value AS Designation, A.Salary, (FirstName + ' ' + LastName) AS Name, L.Value AS Gender, (SELECT FORMAT(DateOfBirth, 'dd-MM-yyyy')) AS [DoB], Contact, Email FROM Person P JOIN Advisor A ON A.Id = P.Id JOIN Lookup L ON L.Id = P.Gender JOIN Lookup L1 ON L1.Id = A.Designation WHERE FirstName LIKE @FirstName  and P.FirstName NOT LIKE '!%' ORDER BY CASE WHEN FirstName = @FirstName THEN 0 ELSE 1 END", con);
            cmd.Parameters.AddWithValue("@FirstName", "%" + SearchTxt.Text + "%");

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                AdvisorGrd.DataSource = dt;
                refreashBtn.Visible = true;
                gunaLabel1.Visible = false;
                SearchTxt.Text = string.Empty;
            }
            else
            {
                MessageBox.Show(" Advisor  not found.");
                ViewAdvisor();
                SearchTxt.Text = string.Empty;
            }
          


        }
        private void Editbtn_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = AdvisorGrd.CurrentRow;
            if (row != null)
            {
                string name = row.Cells["Name"].Value.ToString();
                string[] names = name.Split(' ');
                string FName = names[0];
                string LName = names.Length > 1 ? names[1] : ""; // Handling cases where last name might be missing
                string contact = row.Cells["Contact"].Value.ToString();
                string email = row.Cells["Email"].Value.ToString();
                string designation = row.Cells["Designation"].Value.ToString();
                string salary = row.Cells["Salary"].Value.ToString();
                string dob = row.Cells["DoB"].Value.ToString();
                string gender = row.Cells["Gender"].Value.ToString();
      
                advisorpnl1.changetext();
                // Update values in studentpnl1 UserControl
                advisorpnl1.FirstName = FName;
                advisorpnl1.LastName = LName;
                advisorpnl1.Contact = contact;
                advisorpnl1.Email = email;
                advisorpnl1.salary = salary;
                advisorpnl1.Gender = gender;
                advisorpnl1.designation = designation;
                // Call the update method
                advisorpnl1.TakeIDFromDataBase(FName,LName,contact,email,designation);
                advisorpnl1.Visible = true;
                advisorpnl1.ValueforComboBxOfGender();
               advisorpnl1. DesignationToComboBox();

            }
}

        private void Delbtn_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = AdvisorGrd.CurrentRow;
            if (row != null)
            {
                string name = row.Cells["Name"].Value.ToString();
                string[] names = name.Split(' ');
                string FName = names[0];
                string LName = names.Length > 1 ? names[1] : ""; // Handling cases where last name might be missing
                string email = row.Cells["Email"].Value.ToString();

                using (var con = new SqlConnection(Configuration.getInstance().ConnectionStr))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand("UPDATE Person SET FirstName = '! ' + FirstName WHERE Id IN (SELECT Id FROM Advisor WHERE Email = @Email and FirstName = @FirstName and LastName=@LastName)", con))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@FirstName", FName);
                        cmd.Parameters.AddWithValue("@LastName", LName);
                        int rowsAffected = cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Successfully deleted");
                ViewAdvisor();
            }
        }

        private void advisorpnl1_Load(object sender, EventArgs e)
        {

        }

        private void refreashBtn_Click(object sender, EventArgs e)
        {
            ViewAdvisor();
            refreashBtn.Visible = false;
            gunaLabel1.Visible = true;
        }
    }
}
