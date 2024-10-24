
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
    public partial class GrnPnl : UserControl
    {
        private int groupId;
        private int projectId;
        public GrnPnl()
        {
            InitializeComponent();
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

        public GrnPnl(int groupId, string projectId, string type)
        {
            InitializeComponent();
            this.groupId = groupId;
            try
            {
                this.projectId = int.Parse(projectId);
                GetCurrentProjectAssigned();
            }
            catch
            {
                TitleLbl.Text = "NotAssigned";
            }
            CreatedGrpLbl.Text = "G-" + groupId;
            GetAllStudentsOfGroup();
            Projectbx.Visible = false; // Set visibility to false
            Studentbx.Visible = false; // Set visibility to false
            GrpInfoGrd.Visible = false; // Set visibility to false
            GroupInformGrid.Visible = true; // Set visibility to true
        }
        private void Addbtn_Click(object sender, EventArgs e)
        {

            if (Projectbx.Text == string.Empty)
            {
                MessageBox.Show("You need To First Select a project to assign to this group", "Error");
                return;
            }
            GetProjectId(Projectbx.Text);
            if (TitleLbl.Text == "NotAssigned")
            {
                try
                {
                    var con = Configuration.getInstance().getConnection();
                    SqlCommand cmd = new SqlCommand("INSERT INTO GroupProject VALUES (@ProjectId, @GroupId, @Date)", con);
                    cmd.Parameters.AddWithValue("@ProjectId", projectId);
                    cmd.Parameters.AddWithValue("@GroupId", groupId);
                    cmd.Parameters.AddWithValue("@Date", DateTime.Now);

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
            else
            {
                try
                {
                    var con = Configuration.getInstance().getConnection();
                    SqlCommand cmd = new SqlCommand("UPDATE GroupProject SET ProjectId=@ProjectId,AssignmentDate=@Date WHERE GroupId=@GroupId", con);
                    cmd.Parameters.AddWithValue("@ProjectId", projectId);
                    cmd.Parameters.AddWithValue("@GroupId", groupId);
                    cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                    cmd.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
            GetCurrentProjectAssigned();
            ProjectToComboBox();
            Projectbx.Text = string.Empty;
            MessageBox.Show("Project Assigned Successfully", "Success");
        }

        private void AddStuBtn_Click(object sender, EventArgs e)
        {
            int stuId;
            if (Studentbx.Text != string.Empty)
            {
                stuId = StudentIdFromDataBase();
                try
                {
                    var con = Configuration.getInstance().getConnection();
                    SqlCommand cmd = new SqlCommand("DELETE FROM GroupStudent WHERE StudentId=@StudentId AND GroupId=@GroupId; INSERT INTO GroupStudent VALUES (@GroupId, @StudentId,@Status,@Date)", con);
                    cmd.Parameters.AddWithValue("@StudentId", stuId);
                    cmd.Parameters.AddWithValue("@GroupId", groupId);
                    cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                    cmd.Parameters.AddWithValue("@Status", 3);
                    cmd.ExecuteNonQuery();
                    Studentbx.Text = string.Empty;

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);

                }
            }
            StudentToComboBox();
            GetStudentsOfGroup(GrpInfoGrd);
        }

        private void GrpInfoGrd_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (GrpInfoGrd.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = GrpInfoGrd.SelectedRows[0];
                // Access cell values using column indexes or names
                int studentId = Convert.ToInt32(selectedRow.Cells["Id"].Value);

                try
                {
                    var con = Configuration.getInstance().getConnection();
                    SqlCommand cmd = new SqlCommand("UPDATE GroupStudent SET Status=@Status WHERE GroupId=@GroupId AND StudentId = @StudentId", con);
                    cmd.Parameters.AddWithValue("@StudentId", studentId);
                    cmd.Parameters.AddWithValue("@GroupId", groupId);
                    cmd.Parameters.AddWithValue("@Status", 4);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            // Refresh ComboBox and DataGridView
            StudentToComboBox();
            GetStudentsOfGroup(GrpInfoGrd);

     
        
        }

        private int StudentIdFromDataBase()
        {
            int id = 0;
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("SELECT Id FROM Student AS S WHERE S.RegistrationNo=@RegNo", con);
            cmd.Parameters.AddWithValue("@RegNo", Studentbx.Text);
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                id = int.Parse(reader["Id"].ToString());
            }
            reader.Close();
            return id;
        }


        private void GetProjectId(string title)
        {
            try
            {
                var con = Configuration.getInstance().getConnection();
                SqlCommand cmd = new SqlCommand("SELECT Id FROM Project WHERE Title=@title", con);
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


        private void StudentToComboBox()
        {
            try
            {
                var con = Configuration.getInstance().getConnection();
                SqlCommand cmd = new SqlCommand("SELECT s.RegistrationNo FROM Student s LEFT JOIN (SELECT * FROM GroupStudent GS1 WHERE GS1.AssignmentDate = ( SELECT MAX(GS2.AssignmentDate) FROM GroupStudent GS2 WHERE GS2.StudentId = GS1.StudentId)) recent ON s.Id = recent.StudentID WHERE recent.Status = 4 OR recent.GroupID IS NULL", con);
                SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
                DataSet dataSet = new DataSet();
                dataAdapter.Fill(dataSet);
                Studentbx.DataSource = dataSet.Tables[0];
                Studentbx.DisplayMember = "RegistrationNo";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }


        private void ProjectToComboBox()
        {
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("Select Title from Project WHERE Id<>'" + projectId + "'", con);
            SqlDataAdapter d = new SqlDataAdapter(cmd);
            DataSet dt = new DataSet();
            d.Fill(dt);
            Projectbx.DataSource = dt.Tables[0];
            Projectbx.DisplayMember = "Title";
        }

        private void GetCurrentProjectAssigned()
        {
            try
            {
                var con = Configuration.getInstance().getConnection();
                SqlCommand cmd = new SqlCommand("SELECT Title FROM Project AS P,GroupProject AS GP WHERE  P.Id=GP.ProjectId AND GP.GroupId=@GroupId", con);
                cmd.Parameters.AddWithValue("@GroupId", groupId);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    TitleLbl.Text = reader["Title"].ToString();
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                return;
            }
        }

        private void GetStudentsOfGroup(DataGridView dg)
        {
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("SELECT CONCAT(P.FirstName ,' ',P.LastName) AS Name,S.Id ,S.RegistrationNo ,L.Value AS Status  FROM GroupStudent AS GS JOIN Lookup AS L ON GS.Status = L.Id JOIN Student AS S ON S.Id = GS.StudentId JOIN Person AS P ON P.Id = S.Id WHERE GS.GroupId = @GroupId AND GS.Status=@Status", con);
            cmd.Parameters.AddWithValue("@GroupId", groupId);
            cmd.Parameters.AddWithValue("@Status", 3);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            dg.DataSource = dt.DefaultView;
        }


        private void GetAllStudentsOfGroup()
        {
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("SELECT CONCAT(P.FirstName ,' ',P.LastName) AS Name,S.Id ,S.RegistrationNo ,L.Value AS Status  FROM GroupStudent AS GS JOIN Lookup AS L ON GS.Status = L.Id JOIN Student AS S ON S.Id = GS.StudentId JOIN Person AS P ON P.Id = S.Id WHERE GS.GroupId = @GroupId", con);
            cmd.Parameters.AddWithValue("@GroupId", groupId);
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            GrpInfoGrd.DataSource = dt.DefaultView;
        }

        private void GrnPnl_Load(object sender, EventArgs e)
        {
            SetRoundRegion();

        }

        private void backbtn_Click(object sender, EventArgs e)
        {
            this.Visible = false;
        }
    }



}
