using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DBMIDPRO
{
    public partial class AdvisorBrdPnl : UserControl
    {
        public int projectId;
        public event EventHandler EvaluationChanged;
        public string MainAdvisorName
        {
            get { return MainAdvCmbBx.Text; }
            set { MainAdvCmbBx.Text = value; }
        }
        public string CoAdvisor
        {
            get { return CoAdvisorcmboBx.Text; }
            set { CoAdvisorcmboBx.Text = value; }
        }
        public string IndustryAdvisor
        {
            get { return IndustryAdvisorCmboBx.Text; }
            set { IndustryAdvisorCmboBx.Text = value; }
        }

        protected virtual void OnEvaluationChanged(EventArgs e)
        {
            EvaluationChanged?.Invoke(this, e);
        }

        public AdvisorBrdPnl()
        {
            InitializeComponent();
            this.Padding = new Padding(5);

            ProjectToComboBox();
            AdvisorToComboBox(MainAdvCmbBx, "", "");
            AdvisorToComboBox(CoAdvisorcmboBx, "", "");
            AdvisorToComboBox(IndustryAdvisorCmboBx, "", "");
            ProjctTitlecmbObx.DropDownClosed += projectComboBox_DropDownClosed;
            MainAdvCmbBx.DropDownClosed += mainAdvisorComboBox_DropDownClosed;
            CoAdvisorcmboBx.DropDownClosed += coAdvisorComboBox_DropDownClosed;
            IndustryAdvisorCmboBx.DropDownClosed += industryAdvisorComboBox_DropDownClosed;
        }

        public void callotherFunction()
        {
            AllProjectToComboBox();
            ProjctTitlecmbObx.Text = GetProjectTitle(projectId);
            ProjctTitlecmbObx.Enabled = false;
            AdvisorToComboBox(MainAdvCmbBx, CoAdvisor, IndustryAdvisor);
            AdvisorToComboBox(CoAdvisorcmboBx, MainAdvisorName, IndustryAdvisor);
            AdvisorToComboBox(IndustryAdvisorCmboBx, MainAdvisorName, CoAdvisor);
        }
        private void AdvisorBrdPnl_Load(object sender, EventArgs e)
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

        private void backbtn_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            AddStuBtn.Text = "Add";
        }



        private void AllProjectToComboBox()
        {
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("select p.Title from project P  join GroupProject Gp on P.Id= Gp.ProjectId join [Group] g on g.id=gp.GroupId join GroupStudent gs on gs.GroupId= g.id  where gs.Status=3 and P.Title not like '%!%'", con);
            SqlDataAdapter d = new SqlDataAdapter(cmd);
            DataSet dt = new DataSet();
            d.Fill(dt);
            ProjctTitlecmbObx.DataSource = dt.Tables[0];
            ProjctTitlecmbObx.DisplayMember = "Title";
        }

        private void AdvisorToComboBox(ComboBox cb, string adv1, string adv2)
        {
            try
            {
                var con = Configuration.getInstance().getConnection();
                SqlCommand cmd = new SqlCommand("SELECT (P.FirstName+' '+P.LastName) AS Name FROM Advisor A JOIN Person P ON A.Id=P.Id WHERE (P.FirstName+' '+P.LastName)<>@name1 AND (P.FirstName+' '+P.LastName)<>@name2 and P.FirstName NOT LIKE '%!%'", con);
                cmd.Parameters.AddWithValue("@name1", adv1);
                cmd.Parameters.AddWithValue("@name2", adv2);
                SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
                DataSet dataSet = new DataSet();
                dataAdapter.Fill(dataSet);
                cb.DataSource = dataSet.Tables[0];
                cb.DisplayMember = "Name";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private void ProjectToComboBox()
        {
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("Select Title from Project P LEFT JOIN ProjectAdvisor PA ON P.Id=PA.ProjectId WHERE PA.AdvisorId IS NULL", con);
            SqlDataAdapter d = new SqlDataAdapter(cmd);
            DataSet dt = new DataSet();
            d.Fill(dt);
            ProjctTitlecmbObx.DataSource = dt.Tables[0];
            ProjctTitlecmbObx.DisplayMember = "Title";
        }

        private void GetProjectId(string title)
        {
            try
            {
                var con = Configuration.getInstance().getConnection();
                SqlCommand cmd = new SqlCommand("SELECT Id FROM Project WHERE Title=@title and Title not like '%!%'", con);
                cmd.Parameters.AddWithValue("@title", title);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    projectId = int.Parse(reader["Id"].ToString());
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                return;
            }
        }

        private string GetProjectTitle(int Id)
        {
            string title = "";
            try
            {
                var con = Configuration.getInstance().getConnection();
                SqlCommand cmd = new SqlCommand("SELECT Title FROM Project WHERE Id=@Id", con);
                cmd.Parameters.AddWithValue("@Id", Id);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    title = reader["Title"].ToString();
                }
                reader.Close();
                return title;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                return "";
            }
        }

        private int AdvisorIdFromDataBase(string Name)
        {
            int id = 0;
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("SELECT P.Id FROM Person P JOIN Advisor A ON A.Id=P.Id WHERE (P.FirstName+' '+P.LastName)=@Name", con);
            cmd.Parameters.AddWithValue("@Name", Name);
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                id = int.Parse(reader["Id"].ToString());
            }
            reader.Close();
            return id;
        }

        private int GetAdvisorRole(string role)
        {
            int id = 0;
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("SELECT Id FROM Lookup WHERE Value='" + role + "'", con);
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                id = int.Parse(reader["Id"].ToString());
            }
            reader.Close();
            return id;
        }

        private void AssignAdvisor(int role, int advId)
        {
            if(AddStuBtn.Text== "Add")
            try
            {
                var con = Configuration.getInstance().getConnection();
                SqlCommand cmd = new SqlCommand("INSERT INTO ProjectAdvisor VALUES (@AdvisorId, @ProjectId, @AdvisorRole, @AssignmentDate)", con);
                cmd.Parameters.AddWithValue("@ProjectId", projectId);
                cmd.Parameters.AddWithValue("@AdvisorId", advId);
                cmd.Parameters.AddWithValue("@AdvisorRole", role);
                cmd.Parameters.AddWithValue("@AssignmentDate", DateTime.Now);
                cmd.ExecuteNonQuery();
                    OnEvaluationChanged(EventArgs.Empty);
                }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            else
            {
                try
                {
                    var con = Configuration.getInstance().getConnection();
                    SqlCommand cmd = new SqlCommand("UPDATE ProjectAdvisor SET AdvisorId = @AdvisorId, AdvisorRole = @AdvisorRole, AssignmentDate = @AssignmentDate WHERE ProjectId = @ProjectId", con);
                    cmd.Parameters.AddWithValue("@AdvisorId", advId);
                    cmd.Parameters.AddWithValue("@AdvisorRole", role);
                    cmd.Parameters.AddWithValue("@AssignmentDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@ProjectId", projectId);
                    cmd.ExecuteNonQuery();
                    OnEvaluationChanged(EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }


            }
        }

        private void RemoveAdvisorToProject()
        {
            try
            {
                var con = Configuration.getInstance().getConnection();
                SqlCommand cmd = new SqlCommand("DELETE FROM ProjectAdvisor WHERE ProjectId=@projectId", con);
                cmd.Parameters.AddWithValue("@ProjectId", projectId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void chngText()
        {
            AddStuBtn .Text= "Update";
        }

        private void AddStuBtn_Click(object sender, EventArgs e)
        {
            if (ProjctTitlecmbObx.Text == string.Empty)
            {
                MessageBox.Show("Select a Project First");
                return;
            }
            else if (MainAdvCmbBx.Text == string.Empty || CoAdvisorcmboBx.Text == string.Empty || IndustryAdvisorCmboBx.Text == string.Empty)
            {
                MessageBox.Show("Select an Advisor to be assigned");
                return;
            }
            GetProjectId(ProjctTitlecmbObx.Text);
            RemoveAdvisorToProject();
            int mainId = AdvisorIdFromDataBase(MainAdvCmbBx.Text);
            int coId = AdvisorIdFromDataBase(CoAdvisorcmboBx.Text);
            int inId = AdvisorIdFromDataBase(IndustryAdvisorCmboBx.Text);
            AssignAdvisor(GetAdvisorRole("Main Advisor"), mainId);
            AssignAdvisor(GetAdvisorRole("Co-Advisror"), coId);
            AssignAdvisor(GetAdvisorRole("Industry Advisor"), inId);
            OnEvaluationChanged(EventArgs.Empty);
            if (AddStuBtn.Text == "Add")
            {
                MessageBox.Show("Advisors Assigned Successfully");
            }
            else
            {
                MessageBox.Show("Advisors Update Successfully");
            }
        }

        private void ProjctTitlecmbObx_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void projectComboBox_DropDownClosed(object sender, EventArgs e)
        {
            GetProjectId(ProjctTitlecmbObx.Text);
        }
        private void mainAdvisorComboBox_DropDownClosed(object sender, EventArgs e)
        {
            string advisor1, advisor2;
            advisor1 = CoAdvisorcmboBx.Text;
            advisor2 = IndustryAdvisorCmboBx.Text;
            AdvisorToComboBox(CoAdvisorcmboBx, MainAdvCmbBx.Text, IndustryAdvisorCmboBx.Text);
            CoAdvisorcmboBx.Text = advisor1;
            IndustryAdvisorCmboBx.Text = advisor2;
            AdvisorToComboBox(IndustryAdvisorCmboBx, CoAdvisorcmboBx.Text, MainAdvCmbBx.Text);
            CoAdvisorcmboBx.Text = advisor1;
            IndustryAdvisorCmboBx.Text = advisor2;
        }

        private void coAdvisorComboBox_DropDownClosed(object sender, EventArgs e)
        {
            string advisor1, advisor2;
            advisor1 = MainAdvCmbBx.Text;
            advisor2 = IndustryAdvisorCmboBx.Text;
            AdvisorToComboBox(MainAdvCmbBx, CoAdvisorcmboBx.Text, IndustryAdvisorCmboBx.Text);
            MainAdvCmbBx.Text = advisor1;
            IndustryAdvisorCmboBx.Text = advisor2;
            AdvisorToComboBox(IndustryAdvisorCmboBx, CoAdvisorcmboBx.Text, MainAdvCmbBx.Text);
            MainAdvCmbBx.Text = advisor1;
            IndustryAdvisorCmboBx.Text = advisor2;
        }

        private void industryAdvisorComboBox_DropDownClosed(object sender, EventArgs e)
        {
            string advisor1, advisor2;
            advisor1 = MainAdvCmbBx.Text;
            advisor2 = CoAdvisorcmboBx.Text;
            AdvisorToComboBox(MainAdvCmbBx, CoAdvisorcmboBx.Text, IndustryAdvisorCmboBx.Text);
            MainAdvCmbBx.Text = advisor1;
            CoAdvisorcmboBx.Text = advisor2;
            AdvisorToComboBox(CoAdvisorcmboBx, MainAdvCmbBx.Text, IndustryAdvisorCmboBx.Text);
            MainAdvCmbBx.Text = advisor1;
            CoAdvisorcmboBx.Text = advisor2;

        }
    }
}
