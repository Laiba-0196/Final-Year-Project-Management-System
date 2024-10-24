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

namespace DBMIDPRO
{
    public partial class AdvisorDataPnl : UserControl
    {
        public AdvisorDataPnl()
        {
            InitializeComponent();
            this.Padding = new Padding(5); // Padding to make room for rounded corners
            ViewAdvisoryBoard();
            advisorBrdPnl2.EvaluationChanged += Evaluationpnl_EvaluationChanged;
        }


        private void Evaluationpnl_EvaluationChanged(object sender, EventArgs e)
        {
            // Handle the EvaluationChanged event
            // For example, you can refresh the grid data here
            ViewAdvisoryBoard();

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

        private void nextBtn_Click(object sender, EventArgs e)
        {
            advisorBrdPnl1.Visible = true;
        }

        public void ViewAdvisoryBoard()
        {
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("SELECT PA.ProjectId, MAX(P.Title) Title, MAX(CASE WHEN PA.AdvisorRole = 11 THEN CONCAT(Person.FirstName,' ',Person.LastName) END) AS MainAdvisor, MAX(CASE WHEN PA.AdvisorRole = 12 THEN CONCAT(Person.FirstName,' ',Person.LastName) END) AS CoAdvisor, MAX(CASE WHEN PA.AdvisorRole = 14 THEN CONCAT(Person.FirstName,' ',Person.LastName) END) AS IndustryAdvisor FROM  ProjectAdvisor PA INNER JOIN Advisor A ON PA.AdvisorId = A.Id JOIN Project P ON P.Id=PA.ProjectId JOIN Person ON Person.Id=A.Id where Person.FirstName not like '%!%' and P.Title not like '%!%' GROUP BY PA.ProjectId", con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            proadvisorGrd.DataSource = dt.DefaultView;
        }


        private void AdvisorDataPnl_Load_1(object sender, EventArgs e)
        {
            SetRoundRegion();
            ViewAdvisoryBoard();
        }

        private void backbtn_Click(object sender, EventArgs e)
        {
            advisorBrdPnl2.Visible = true;
        }

        private void EditButton1_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = proadvisorGrd.CurrentRow;
            if (row != null)
            {
                try
                {
                    int id = Convert.ToInt32(row.Cells["ProjectId"].Value);
                    string adv1 = row.Cells["MainAdvisor"].Value.ToString();
                    string adv2 = row.Cells["CoAdvisor"].Value.ToString();
                    string adv3 = row.Cells["IndustryAdvisor"].Value.ToString();

                    advisorBrdPnl2.projectId = id;
                    advisorBrdPnl2.MainAdvisorName = adv1;
                    advisorBrdPnl2.CoAdvisor = adv2;
                    advisorBrdPnl2.IndustryAdvisor = adv3;

                    advisorBrdPnl2.callotherFunction();
                    advisorBrdPnl2.Visible = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("No row selected.");
            }
        }

        private void advisorBrdPnl2_Load(object sender, EventArgs e)
        {

        }

        private void SearchByNameBtn_Click(object sender, EventArgs e)
        {
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("SELECT PA.ProjectId, MAX(P.Title) Title, MAX(CASE WHEN PA.AdvisorRole = 11 THEN CONCAT(Person.FirstName,' ',Person.LastName) END) AS MainAdvisor, MAX(CASE WHEN PA.AdvisorRole = 12 THEN CONCAT(Person.FirstName,' ',Person.LastName) END) AS CoAdvisor, MAX(CASE WHEN PA.AdvisorRole = 14 THEN CONCAT(Person.FirstName,' ',Person.LastName) END) AS IndustryAdvisor FROM  ProjectAdvisor PA INNER JOIN Advisor A ON PA.AdvisorId = A.Id JOIN Project P ON P.Id=PA.ProjectId JOIN Person ON Person.Id=A.Id  where P.Title like @Title where Person.FirstName not like '%!%' and P.Title not like '%!%' GROUP BY PA.ProjectId", con);
            cmd.Parameters.AddWithValue("@Title", "%" + SearchTxt.Text + "%");
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                proadvisorGrd.DataSource = dt.DefaultView;
                refreashBtn.Visible = true;
                gunaLabel1.Visible = false;
                SearchTxt.Text = string.Empty;
            }
            else
            {
                MessageBox.Show(" Advisor  not found.");
                ViewAdvisoryBoard(); 
                SearchTxt.Text = string.Empty;
            }
            

        }

        private void refreashBtn_Click(object sender, EventArgs e)
        {
            ViewAdvisoryBoard();
            refreashBtn.Visible = false;
            gunaLabel1.Visible = true;
        }
    }
}
