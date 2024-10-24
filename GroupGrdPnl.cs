
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

namespace DBMIDPRO
{
    public partial class GroupGrdPnl : UserControl
    {

        public GroupGrdPnl()
        {
            InitializeComponent();
            DisplayGroups();
            grpMngPnl2.EvaluationChanged += Evaluationpnl_EvaluationChanged;
        }

        private void Evaluationpnl_EvaluationChanged(object sender, EventArgs e)
        {
            // Handle the EvaluationChanged event
            // For example, you can refresh the grid data here
            DisplayGroups();

        }

        private void GroupGrdPnl_Load(object sender, EventArgs e)
        {
            // Set the region to create rounded corners
            SetRoundRegion();
            DisplayGroups();
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



        private void CreateBtn_Click(object sender, EventArgs e)
        {
            try
            {
                int id = 0;
                var con = Configuration.getInstance().getConnection();
                SqlCommand cmd = new SqlCommand("INSERT INTO [Group](Created_On) VALUES(@date); SELECT Id FROM [Group] ORDER BY Id Desc", con);
                cmd.Parameters.AddWithValue("@date", DateTime.Today);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    id = int.Parse(reader["Id"].ToString());
                }
                reader.Close();
                MessageBox.Show("Group G-" + id + " Created Successfully");
                grpMngPnl2.groupId = id;
                grpMngPnl2.CreatedGroupText = "G-" + id;
                grpMngPnl2.CallOtherFunction();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                return;
            }
            DisplayGroups();
            grpMngPnl2.Visible = true;
        }


        public void DisplayGroups()
        {
            try
            {
                var con = Configuration.getInstance().getConnection();
                SqlCommand cmd = new SqlCommand("SELECT CONCAT('G-', G.Id) AS GroupId, P.Id AS ProjectId, P.Title, COUNT(GS.StudentId) AS GStudent, (SELECT FORMAT(G.Created_On, 'dd-MM-yyyy')) AS Created_On FROM [Group] AS G LEFT JOIN GroupProject AS GP ON G.Id = GP.GroupId LEFT JOIN GroupStudent AS GS ON GS.GroupId = G.Id LEFT JOIN Project AS P ON GP.ProjectId = P.Id WHERE GS.Status = 3 AND P.Title NOT LIKE '%!%' GROUP BY G.Id, P.Id, P.Title, G.Created_On", con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                GrpGrd.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DetailBtn_Click(object sender, EventArgs e)
        {
            if (GrpGrd.CurrentRow != null)
            {
                DataGridViewRow row = GrpGrd.CurrentRow;
                string GroupId = row.Cells["GroupId"].Value.ToString();
                string projectId = row.Cells["ProjectId"].Value.ToString();
                string[] GId = GroupId.Split('-');
                int groupId = int.Parse(GId[1]);
                grpMngPnl2.groupId = groupId;
                try
                {
                    grpMngPnl2.projectId = int.Parse(projectId);
                    grpMngPnl2. GetCurrentProjectAssigned();
                }
                catch
                {
                    grpMngPnl2.TitleName = "Not Assigned";
                }
                grpMngPnl2.CreatedGroupText = "G-" + groupId;
                grpMngPnl2. GetAllStudentsOfGroup();
                grpMngPnl2.HideOtherThing();
                grpMngPnl2.Visible = true;
            }

        }


        private void SearchByNameBtn_Click(object sender, EventArgs e)
        {
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("SELECT CONCAT('G-',G.Id) AS GroupId,P.Id AS ProjectId,P.Title,COUNT(GS.StudentId) AS GStudent,(SELECT FORMAT(G.Created_On, 'dd-MM-yyyy')) AS Created_On FROM [Group] AS G LEFT JOIN GroupProject AS GP ON G.Id=GP.GroupId LEFT JOIN GroupStudent AS GS ON GS.GroupId=G.Id LEFT JOIN Project AS P ON GP.ProjectId=P.Id WHERE GS.Status=3 and P.Title like @Title GROUP BY G.Id,P.Id,P.Title,G.Created_On", con);
            cmd.Parameters.AddWithValue("@Title", "%" + SearchTxt.Text + "%");
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);
            if (dt.Rows.Count > 0)
            {
                GrpGrd.DataSource = dt;
                refreashBtn.Visible = true;
                gunaLabel1.Visible = false;
                SearchTxt.Text = string.Empty;
            }
            else
            {
                MessageBox.Show(" student of this Registration number  not found.");
                DisplayGroups();
                SearchTxt.Text = string.Empty;
            }
            


        }

        private void refreashBtn_Click(object sender, EventArgs e)
        {
            DisplayGroups();
            refreashBtn.Visible = false;
            gunaLabel1.Visible = true;
        }

        private void Editbtn_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = GrpGrd.CurrentRow;
            if (row != null)
            {
                if (row.Cells["GroupId"].Value != null && row.Cells["ProjectId"].Value != null)
                {
                    string GroupId = row.Cells["GroupId"].Value.ToString();
                    string projectId = row.Cells["ProjectId"].Value.ToString();
                    string[] GId = GroupId.Split('-');
                    int groupId = int.Parse(GId[1]);
                    grpMngPnl2.groupId = groupId;
                    try
                    {
                        grpMngPnl2.projectId = int.Parse(projectId);
                        grpMngPnl2.GetCurrentProjectAssigned();
                        grpMngPnl2.callotherfunctionForEdit();
                        grpMngPnl2.Visible = true;
                    }
                    catch
                    {
                        grpMngPnl2.TitleName = "Not Assigned";
                    }

                }
            }
        }

        private void grpMngPnl2_Load(object sender, EventArgs e)
        {

        }
    }
}
