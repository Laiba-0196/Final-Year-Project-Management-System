
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using System.Xml.Linq;

namespace DBMIDPRO
{
    public partial class EvlMrksGrdPnl : UserControl
    {
        public EvlMrksGrdPnl()
        {
            InitializeComponent();
            DisplayMarkedEValuations();
            markEvalPnl1.MarkEvaluationChanged += Evaluationpnl_EvaluationChanged;
        }

        private void EvlMrksPnl_Load(object sender, EventArgs e)
        {
            // Set the region to create rounded corners
            SetRoundRegion();
        }

        private void Evaluationpnl_EvaluationChanged(object sender, EventArgs e)
        {
            // Handle the EvaluationChanged event
            // For example, you can refresh the grid data here
            DisplayMarkedEValuations();

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

        private void SearchByNameBtn_Click(object sender, EventArgs e)
        {
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("Select CONCAT('G-',GroupId) AS GroupId,Name,EvaluationId,TotalMarks,ObtainedMarks,(SELECT FORMAT(EvaluationDate, 'dd-MM-yyyy')) AS [EvaluationDate] FROM GroupEvaluation EV JOIN Evaluation E ON EV.EvaluationId=E.Id where EV.GroupId like @GroupId and  E.Name not like '!%'", con);
            cmd.Parameters.AddWithValue("@GroupId", "%" + EvalTxt.Text + "%");
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                MrkEvlGrd.DataSource = dt;
                refreashBtn.Visible = true;
                gunaLabel1.Visible = false;
                EvalTxt.Text = string.Empty;

            }
            else
            {
                MessageBox.Show(" student of this Registration number  not found.");
                DisplayMarkedEValuations();
                EvalTxt.Text = string.Empty;
            }
            

        }

        public void DisplayMarkedEValuations()
        {
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("Select CONCAT('G-',GroupId) AS GroupId,Name,EvaluationId,TotalMarks,ObtainedMarks,(SELECT FORMAT(EvaluationDate, 'dd-MM-yyyy')) AS [EvaluationDate] FROM GroupEvaluation EV JOIN Evaluation E ON EV.EvaluationId=E.Id and  E.Name not like '!%'", con);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            MrkEvlGrd.DataSource = dt;
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            string GId;
            int EId, OM, TM;
            DataGridViewRow row = MrkEvlGrd.CurrentRow;
            if (row != null)
            {
                EId = Int32.Parse(row.Cells["EvaluationId"].Value.ToString());
                GId = row.Cells["GroupId"].Value.ToString();
                TM = int.Parse(row.Cells["TotalMarks"].Value.ToString());
                OM = int.Parse(row.Cells["ObtainedMarks"].Value.ToString());

                markEvalPnl1.changeText(); // Assuming this method exists in your UserControl

                string[] GID = GId.Split('-');
                int id = int.Parse(GID[1]);

                // Assigning values to UserControl properties
                markEvalPnl1.GroupComboBox.SelectedIndex = markEvalPnl1.GroupComboBox.FindStringExact(GId);
                markEvalPnl1.EvaluationComboBox.SelectedIndex = markEvalPnl1.EvaluationComboBox.FindStringExact(EId.ToString());
                markEvalPnl1.ObtainMarks = OM.ToString();
                markEvalPnl1.TotalMarks = TM.ToString();
                markEvalPnl1.EvaluationComboBox.Enabled = false; // Disabling the ComboBox
                markEvalPnl1.GroupComboBox.Enabled = false;

                // Call the update method
                markEvalPnl1.updateIDs(EId, id);

                // Make UserControl visible
                markEvalPnl1.Visible = true;
            }
        }


        private void NextBtn_Click_1(object sender, EventArgs e)
        {
            markEvalPnl1.Visible = true;
        }

        private void markEvalPnl1_Load(object sender, EventArgs e)
        {

        }

        private void refreashBtn_Click(object sender, EventArgs e)
        {
            DisplayMarkedEValuations();
            refreashBtn.Visible = false;
            gunaLabel1.Visible = true;
        }
    }
}
