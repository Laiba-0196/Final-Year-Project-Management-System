
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
    public partial class ProjectGRidPnl : UserControl
    {
        int id;
        public ProjectGRidPnl()
        {
            InitializeComponent();
            this.Padding = new Padding(5); // Padding to make room for rounded corners
            ViewProjects();
            projectPnl1.EvaluationChanged += Evaluationpnl_EvaluationChanged;
        }
        private void Evaluationpnl_EvaluationChanged(object sender, EventArgs e)
        {
            // Handle the EvaluationChanged event
            // For example, you can refresh the grid data here
            ViewProjects();

        }

        private void ProjectGRidPnl_Load(object sender, EventArgs e)
        {
            // Set the region to create rounded corners
            SetRoundRegion();
            ViewProjects();
            projectPnl1.EvaluationChanged += Evaluationpnl_EvaluationChanged;
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

        public void ViewProjects()
        {
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("Select p.Title,p.Description from Project p where p.Title NOT LIKE '!%'", con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            projectGrd.DataSource = dt;
        }

        private void SearchByNameBtn_Click_1(object sender, EventArgs e)
        {
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("SELECT p.Title,p.Description FROM Project p WHERE p.Title LIKE @Title and p.Title NOT LIKE '!%'s", con);
            cmd.Parameters.AddWithValue("@Title", "%" + SearchTxt.Text + "%");

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                projectGrd.DataSource = dt;
                refreashBtn.Visible = true;
                gunaLabel1.Visible = false;
                SearchTxt.Text = string.Empty;
            }
            else
            {
                MessageBox.Show(" project not found.");
                ViewProjects();
                SearchTxt.Text = string.Empty;
            }

            
        }

        private void Editbtn_Click_1(object sender, EventArgs e)
        {

            string title, description;
            DataGridViewRow row = projectGrd.CurrentRow;
            if (row != null)
            {
                title = row.Cells["Title"].Value.ToString();
                description = row.Cells["Description"].Value.ToString();
                projectPnl1.Title = title;
                projectPnl1.Description = description;
                projectPnl1.changetext();
                projectPnl1.TakeIDFromDataBase(title);
                projectPnl1.Visible = true; // Show the form as a dialog (modal)
            }
        }

        public void TakeIDFromDataBase(string Title)
        {
            using (var con = new SqlConnection(Configuration.getInstance().ConnectionStr))
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT Id from Project WHERE Title = @Title", con))
                {
                    cmd.Parameters.AddWithValue("@Title", Title);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            this.id = reader.GetInt32(0);
                        }
                    }
                }
            }

        }

        private void nextBtn_Click_1(object sender, EventArgs e)
        {
            projectPnl1.Visible = true;
        }

        private void projectPnl1_Load(object sender, EventArgs e)
        {

        }

        private void refreashBtn_Click(object sender, EventArgs e)
        {
            ViewProjects();
            refreashBtn.Visible = false;
            gunaLabel1.Visible = true;
        }

        private void Delbtn_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = projectGrd.CurrentRow;
            if (row != null)
            {
                string title = row.Cells["Title"].Value.ToString();
                TakeIDFromDataBase(title);
                var con = Configuration.getInstance().getConnection();
                SqlCommand cmd = new SqlCommand("UPDATE Project SET Title = '! ' + Title, Description=Description WHERE Id=@Id;", con);
                cmd.Parameters.AddWithValue("@Id", id);
                int rowsAffected = cmd.ExecuteNonQuery();
                MessageBox.Show("Successfully deleted");
                ViewProjects();
            }
        }
    }
}
