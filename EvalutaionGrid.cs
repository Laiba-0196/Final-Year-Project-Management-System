
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
    public partial class EvalutaionGrid : UserControl
    {
        int id;
        public EvalutaionGrid()
        {
            InitializeComponent();
            ViewEvaluation();
            evaluationpnl1.EvaluationChanged += Evaluationpnl_EvaluationChanged;
        }

        private void EvalutaionGrid_Load(object sender, EventArgs e)
        {
            // Set the region to create rounded corners
            SetRoundRegion();
            ViewEvaluation();
        }


        private void Evaluationpnl_EvaluationChanged(object sender, EventArgs e)
        {
            // Handle the EvaluationChanged event
            // For example, you can refresh the grid data here
            ViewEvaluation();

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

        public void ViewEvaluation()
        {
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("Select Name,TotalMarks,TotalWeightage from Evaluation E where E.Name not like '%!%'", con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            EvalGrd.DataSource = dt;
        }

        private void nextBtn_Click(object sender, EventArgs e)
        {
            evaluationpnl1.Visible = true;
        }

        private void Editbtn_Click(object sender, EventArgs e)
        {

            string Name, TotalMarks,TotalWeightage;
            DataGridViewRow row = EvalGrd.CurrentRow;
            if (row != null)
            {
                Name = row.Cells["Name"].Value.ToString();
                TotalMarks = row.Cells["TotalMarks"].Value.ToString();
                TotalWeightage = row.Cells["TotalWeightage"].Value.ToString();
                evaluationpnl1.Name = Name;
                evaluationpnl1.Totalmarks = TotalMarks;
                evaluationpnl1.Weightage = TotalWeightage;
                evaluationpnl1.ChangeText();
                evaluationpnl1.IDfromDataBase(Name);
                evaluationpnl1.Visible = true; // Show the form as a dialog (modal)
            }
        }

        private void SearchByNameBtn_Click(object sender, EventArgs e)
        {
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("Select Name,TotalMarks,TotalWeightage from Evaluation E where Name like @Name  and  E.Name not like '!%'", con);
            cmd.Parameters.AddWithValue("@Name", "%" + SearchTxt.Text + "%");

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (dt.Rows.Count > 0)
            {
                EvalGrd.DataSource = dt;
                refreashBtn.Visible = true;
                gunaLabel1.Visible = false;
                SearchTxt.Text = string.Empty;
            }
            else
            {
                MessageBox.Show(" Evaluation not found.");
                ViewEvaluation();
                SearchTxt.Text = string.Empty;
            }

        
        }

        private void evaluationpnl1_Load(object sender, EventArgs e)
        {

        }

        private void refreashBtn_Click(object sender, EventArgs e)
        {
            ViewEvaluation();
            refreashBtn.Visible = false;
            gunaLabel1.Visible = true;
        }

        public void IDfromDataBase(string Name)

        {
            using (var con = new SqlConnection(Configuration.getInstance().ConnectionStr))
            {

                 con.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT E.Id from Evaluation E where E.Name=@Name", con))
                {
                    cmd.Parameters.AddWithValue("@Name", Name);
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

        private void Delbtn_Click(object sender, EventArgs e)
        {

            DataGridViewRow row = EvalGrd.CurrentRow;
            if (row != null)
            {
                string EName = row.Cells["Name"].Value.ToString();
                IDfromDataBase(EName);
                var con = Configuration.getInstance().getConnection();
                SqlCommand cmd = new SqlCommand("UPDATE Evaluation SET Name = CONCAT('!', Name), TotalMarks=@TotalMarks, TotalWeightage=@TotalWeightage WHERE Id=@Id;", con);
                cmd.Parameters.AddWithValue("@Id", id);
                MessageBox.Show("Deleted Successfully");
                ViewEvaluation();
            }
        }
    }
}
