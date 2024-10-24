
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
    public partial class ProjectPnl : UserControl
    {
        int id;
        public event EventHandler EvaluationChanged;
        public ProjectPnl()
        {
            InitializeComponent();
            this.Padding = new Padding(5); // Padding to make room for rounded corners
        }


        public string Title
        {
            get { return titletxt.Text; }
            set { titletxt.Text = value; }
        }
        public string Description
        {
            get { return DescrTxt.Text; }
            set { DescrTxt.Text = value; }
        }

        private void ProjectPnl_Load(object sender, EventArgs e)
        {
            // Set the region to create rounded corners
            SetRoundRegion();

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

            OnEvaluationChanged(EventArgs.Empty);
        }

        public void changetext()
        {
            Addbtn.Text = "Update";
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
            Addbtn.Text = "Add";
        }

        private bool Validation()
        {
            string title = titletxt.Text;
            bool titleValid = false;
            string Alphabets = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz.";
            foreach (char n in title)
            {
                if (Alphabets.Contains(n.ToString()))
                {
                    titleValid = true;
                }
            }
            if (titleValid)
            {
                titleValid = !Validations.ValidationInDatabase("SELECT Title FROM Project WHERE Title='" + title + "' AND Id<>" + id);
                if (!titleValid)
                {
                    MessageBox.Show("There already exists one Project With the same name", "Error");
                }
            }
            else
            {
                MessageBox.Show("Project Title must Contain Alphabets", "Error");
            }
            return titleValid;
        }
        private void Addbtn_Click(object sender, EventArgs e)
        {

            if (DescrTxt.Text == "")
            {
                MessageBox.Show("Please Add Title of Project", "Error");
            }
            else if (DescrTxt.Text == "")
            {
                MessageBox.Show("Please Add Title of Description first", "Error");
            }
            else if (Validation())
            {
                if (Addbtn.Text == "Add")
                {
                    try
                    {
                        var con = Configuration.getInstance().getConnection();
                        SqlCommand cmd = new SqlCommand("INSERT INTO Project(Title,Description) VALUES (@Title,@Description)", con);
                        cmd.Parameters.AddWithValue("@Title", titletxt.Text);
                        cmd.Parameters.AddWithValue("@Description", DescrTxt.Text);
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
                        SqlCommand cmd = new SqlCommand("UPDATE Project SET Title = @Title, Description=@Description WHERE Id=@Id;", con);
                        cmd.Parameters.AddWithValue("@Title", titletxt.Text);
                        cmd.Parameters.AddWithValue("@Description", DescrTxt.Text);
                        cmd.Parameters.AddWithValue("@Id", id);
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
        }
        protected virtual void OnEvaluationChanged(EventArgs e)
        {
            EvaluationChanged?.Invoke(this, e);
        }

        private void gunaPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
