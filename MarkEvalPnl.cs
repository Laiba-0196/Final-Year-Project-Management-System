
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
    public partial class MarkEvalPnl : UserControl
    {
        int evaluationId, groupId;

        public event EventHandler MarkEvaluationChanged;
        // Expose the ComboBoxes as properties
        public ComboBox GroupComboBox
        {
            get { return GrpBx; }
        }

        public ComboBox EvaluationComboBox
        {
            get { return Evalbx; }
        }

        // Getter and Setter for evaluationId
        public string TotalMarks
        {
            get { return TotalMarksTxt.Text; }
            set { TotalMarksTxt.Text = value; }
        }

        // Getter and Setter for groupId
        public string ObtainMarks
        {
            get { return ObtMarkstxt.Text; }
            set { ObtMarkstxt.Text = value; }
        }


        public MarkEvalPnl()
        {
            InitializeComponent();
            this.Padding = new Padding(5);
            GroupToComboBox();
            EvaluationToComboBox();
            GrpBx.SelectedIndexChanged += ComboBox_DropDownClosed;
            Evalbx.SelectedIndexChanged += ComboBox_DropDownClosed;
        }

        protected virtual void OnEvaluationChanged(EventArgs e)
        {
            MarkEvaluationChanged?.Invoke(this, e);
        }

        private void MarkEvalPnl_Load(object sender, EventArgs e)
        {
            // Set the region to create rounded corners
            SetRoundRegion();

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


        private void GroupToComboBox()
        {
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("SELECT CONCAT('G-',G.Id) AS GroupId FROM [Group] AS G LEFT JOIN GroupProject AS GP ON G.Id=GP.GroupId LEFT JOIN GroupStudent AS GS ON GS.GroupId=G.Id LEFT JOIN Project AS P ON GP.ProjectId=P.Id WHERE GS.Status=3 GROUP BY G.Id,P.Id,P.Title,G.Created_On", con);
            SqlDataAdapter d = new SqlDataAdapter(cmd);
            DataSet dt = new DataSet();
            d.Fill(dt);
            GrpBx.DataSource = dt.Tables[0];
            GrpBx.DisplayMember = "GroupId";
        }

        private void EvaluationToComboBox()
        {
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("SELECT E.Name FROM Evaluation E EXCEPT SELECT E.Name FROM Evaluation E JOIN GroupEvaluation GE ON GE.EvaluationId = E.Id JOIN [Group] G ON G.Id = GE.GroupId WHERE  E.Name not like '!%' and G.Id = '" + groupId + "'", con);
            SqlDataAdapter d = new SqlDataAdapter(cmd);
            DataSet dt = new DataSet();
            d.Fill(dt);
            Evalbx.DataSource = dt.Tables[0];
            Evalbx.DisplayMember = "Name";
        }
        private void GetSelectedEvaluationId(string name)
        {
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("SELECT Id,TotalMarks FROM Evaluation WHERE Name=@Name and  Name not like '!%'", con);
            cmd.Parameters.AddWithValue("@Name", name);
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                evaluationId = int.Parse(reader["Id"].ToString());
                TotalMarksTxt.Text = reader["TotalMarks"].ToString();
            }
            reader.Close();
        }


        private void ComboBox_DropDownClosed(object sender, EventArgs e)
        {
            if (sender.Equals(Evalbx))
            {
                GetSelectedEvaluationId(Evalbx.Text);
            }
            else
            {
                if (GrpBx.Text != string.Empty)
                {
                    string[] GId = GrpBx.Text.Split('-');
                    this.groupId = int.Parse(GId[1]);
                    EvaluationToComboBox();
                }
                else
                {
                    groupId = -1; // Set groupId to indicate no group selected
                }
            }
        }

        private bool ObtainedMarksValidations(string marks)
        {
            string numbers = "0123456789";
            bool isValid = true;
            foreach (char n in marks)
            {
                if (!numbers.Contains(n.ToString()))
                {
                    return false;
                }
            }
            return isValid;
        }

        private void Addbtn_Click(object sender, EventArgs e)
        {
            if (GrpBx.Text == string.Empty)
            {
                MessageBox.Show("Please Select A Group to evaluate", "Error");
            }
            else if (Evalbx.Text == string.Empty)
            {
                MessageBox.Show("Please Select Evaluation Title to evaluate", "Error");
            }
            else if (ObtMarkstxt.Text == string.Empty)
            {
                MessageBox.Show("A Group cannot be assigned null marks in any evaluation", "Error");
            }
            else if (int.Parse(ObtMarkstxt.Text) > int.Parse(TotalMarksTxt.Text))
            {
                MessageBox.Show("Obtained Marks cannot be greater than total marks", "Error");
            }
            else if (ObtainedMarksValidations(ObtMarkstxt.Text))
            {
                if (Addbtn.Text.ToString() == "Add")
                {
                    try
                    {
                        var con = Configuration.getInstance().getConnection();
                        SqlCommand cmd = new SqlCommand("INSERT INTO GroupEvaluation VALUES (@GroupId,@EvaluationId,@ObtainedMarks, @Date)", con);
                        cmd.Parameters.AddWithValue("@GroupId", groupId);
                        cmd.Parameters.AddWithValue("@EvaluationId", evaluationId);
                        cmd.Parameters.AddWithValue("@ObtainedMarks", int.Parse(ObtMarkstxt.Text));
                        cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                        cmd.ExecuteNonQuery();
                        OnEvaluationChanged(EventArgs.Empty);
                        MessageBox.Show("Successfully saved");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error");
                        return;
                    }
                }
                else
                {
                    try
                    {
                        var con = Configuration.getInstance().getConnection();
                        SqlCommand cmd = new SqlCommand("UPDATE GroupEvaluation SET ObtainedMarks = @ObtainedMarks, EvaluationDate=@Date WHERE GroupId=@GroupId AND EvaluationId=@EvaluationId", con);
                        cmd.Parameters.AddWithValue("@GroupId", groupId);
                        cmd.Parameters.AddWithValue("@EvaluationId", evaluationId);
                        cmd.Parameters.AddWithValue("@ObtainedMarks", int.Parse(ObtMarkstxt.Text));
                        cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                        cmd.ExecuteNonQuery();
                        OnEvaluationChanged(EventArgs.Empty);
                        MessageBox.Show("Record Updated");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error");
                        return;
                    }
                }
            }
            else
            {
                MessageBox.Show("Obtained marks can only be int", "Error");
            }
        }


        public void updateIDs(int evlIDint,int GrpID)
        {
            this.evaluationId = evlIDint;
            this.groupId = GrpID;
        }

        public void changeText()
        {
            Addbtn.Text = "Update";
        }

        private void BackBtn_Click_1(object sender, EventArgs e)
        {
            this.Visible = false;
            Addbtn.Text = "Add";
            EvaluationComboBox.Enabled = true; // Disabling the ComboBox
            GroupComboBox.Enabled = true;

        }
    }
}
