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
using System.Xml.Linq;

namespace DBMIDPRO
{
    public partial class evaluationpnl : UserControl
    {
        int id=0;
        int ids;
        public event EventHandler EvaluationChanged;
        public evaluationpnl()
        {
            InitializeComponent();
        }

        private void gunaPanel1_Paint(object sender, PaintEventArgs e)
        {
            // Set the region to create rounded corners
            SetRoundRegion();

        }

        public string Name
        {
            get { return Nametxt.Text; }
            set { Nametxt.Text = value; }
        }
        public string Weightage
        {
            get { return TotalWeightagetxt.Text; }
            set { TotalWeightagetxt.Text = value; }
        }

        public string Totalmarks
        {
            get { return TotalMarksTxt.Text; }
            set { TotalMarksTxt.Text = value; }
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

        public void ChangeText()
        {
            Addbtn.Text = "Update";
        }
        public void SelectedRowID(int ids)
        {
            this.ids = ids;
        }


        public void IDfromDataBase(string Name)

        {
            using (var con = new SqlConnection(Configuration.getInstance().ConnectionStr))
            {

             con.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT E.Id from Evaluation E where E.Name=@Name ", con))
                {
                    cmd.Parameters.AddWithValue("@Name", Name);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            this.ids = reader.GetInt32(0);
                        }
                    }

                }

            }

        }

        /*        private bool WeightageSumCalculate(int weightage)
                {
                    int totalWeightage = 0;
                    var con = Configuration.getInstance().getConnection();
                    SqlCommand cmd = new SqlCommand("SELECT SUM(TotalWeightage) AS total FROM Evaluation WHERE Id<>'" + id + "'", con);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        // Use int.TryParse to safely parse the string to an integer
                        if (!int.TryParse(reader["total"].ToString(), out totalWeightage))
                        {
                            // Handle the case where parsing fails
                            // For example, you can log the error or return false
                            reader.Close();
                            return false;
                        }
                    }
                    reader.Close();
                    if (totalWeightage + weightage > 100)
                    {
                        return false;
                    }
                    return true;
                }*/

        private void Nametxt_TextChanged(object sender, EventArgs e)
        {

        }

        private void evaluationpnl_Load(object sender, EventArgs e)
        {
            // Set the region to create rounded corners
            SetRoundRegion();
        }

        private void backbtn_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            Addbtn.Text = "Add";
        }

        private bool Validation()
        {
            string title = Nametxt.Text;
            bool titleValid = false;
            if (title == "")
            {
                return false;
            }
            string Alphabets = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz ";
            foreach (char n in title)
            {
                if (Alphabets.Contains(n.ToString()))
                {
                    titleValid = true;
                }
            }
            if (titleValid)
            {
                titleValid = !Validations.ValidationInDatabase("SELECT Name FROM Evaluation WHERE Name='" + title + "' AND Id<>" + id);
                if (!titleValid)
                {
                    MessageBox.Show("There already exists one Evaluation With the same name", "Error");
                }
            }
            else
            {
                MessageBox.Show("Evaluation Title must contain at least one Alphabet", "Error");
            }
            return titleValid;
        }





        private void Addbtn_Click(object sender, EventArgs e)
        {

            if (Nametxt.Text == "")
            {
                MessageBox.Show(" Enter Name of the Evaluation", "Error");
            }
            else if (TotalMarksTxt.Text == "")
            {
                MessageBox.Show(" Enter Total Marks of the Evaluation " + Nametxt.Text, "Error");
            }
            else if (TotalWeightagetxt.Text == "")
            {
                MessageBox.Show(" Enter Total Weightahe of the Evaluation " + Nametxt.Text, "Error");
            }
           /* else if (!WeightageSumCalculate(int.Parse(TotalWeightagetxt.Text.ToString())))
            {
                MessageBox.Show("Total Weightage of Over All Evaluations cannot be over 100", "Error");
            }*/
            else if (Validation())
            {
                if (Addbtn.Text == "Add")
                {
                    try
                    {
                        var con = Configuration.getInstance().getConnection();
                        SqlCommand cmd = new SqlCommand("INSERT INTO Evaluation(Name,TotalMarks,TotalWeightage) VALUES (@Name,@TotalMarks,@TotalWeightage)", con);
                        cmd.Parameters.AddWithValue("@Name", Nametxt.Text);
                        cmd.Parameters.AddWithValue("@TotalMarks", TotalMarksTxt.Text);
                        cmd.Parameters.AddWithValue("@TotalWeightage", TotalWeightagetxt.Text);
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
                        SqlCommand cmd = new SqlCommand("UPDATE Evaluation SET Name = @Name, TotalMarks=@TotalMarks, TotalWeightage=@TotalWeightage WHERE Id=@Id;", con);
                        cmd.Parameters.AddWithValue("@Name", Nametxt.Text);
                        cmd.Parameters.AddWithValue("@TotalMarks", TotalMarksTxt.Text);
                        cmd.Parameters.AddWithValue("@TotalWeightage", TotalWeightagetxt.Text);
                        cmd.Parameters.AddWithValue("@Id", ids);
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
    }
}
