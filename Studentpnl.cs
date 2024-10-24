
using System;
using System.Data.SqlClient;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Globalization;


namespace DBMIDPRO
{

    public partial class Studentpnl : UserControl
    {
        public event EventHandler EvaluationChanged;
        int gender = 1;
        int id;
        int ids;

        private StudGridPnl studGridPnlInstance;
        public string FirstName
        {
            get { return FirstNtxt.Text; }
            set { FirstNtxt.Text = value; }
        }

        public string LastName
        {
            get { return LastTxt.Text; }
            set { LastTxt.Text = value; }
        }
        public string Contact
        {
            get { return ContectTxt.Text; }
            set { ContectTxt.Text = value; }
        }

        public string Email
        {
            get { return Emailtxt.Text; }
            set { Emailtxt.Text = value; }
        }

        public string RegistrationNumber
        {
            get { return RegNumtxt.Text; }
            set { RegNumtxt.Text = value; }
        }

        public string Gender
        {
            get { return GenderBx.SelectedItem.ToString(); }
            set { GenderBx.SelectedItem = value; }
        }
        public Studentpnl()
        {
            InitializeComponent();
            this.Padding = new Padding(5);
            ValueforComboBxOfGender();
        }

        private void Studentpnl_Load(object sender, EventArgs e)
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

        private void ValueforComboBxOfGender()
        {
            try
            {
                var con = Configuration.getInstance().getConnection();
               // con.Open(); // Open the connection
                SqlCommand cmd = new SqlCommand("SELECT Value FROM Lookup WHERE Category='GENDER'", con);
                SqlDataAdapter d = new SqlDataAdapter(cmd);
                DataSet dt = new DataSet();
                d.Fill(dt);

                // Assuming GenderBx is the name of your ComboBox
                GenderBx.DataSource = dt.Tables[0]; // Set the data source
                GenderBx.DisplayMember = "Value"; // Specify the display member

                //con.Close(); // Close the connection when done
            }
            catch (Exception ex)
            {
                // Handle any exceptions here
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        public int returnSelectedGender(string gen)
        {
            int g = 0;
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("SELECT Id FROM Lookup WHERE Category='GENDER' AND Value=@gender", con);
            cmd.Parameters.AddWithValue("@gender", gen);
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                g = int.Parse(reader["Id"].ToString());
            }
            reader.Close();
            return g;
        }

        //for validation 
        private bool Validation()
        {
            bool isValid = true;
            if (AddBtn.Text == "ADD")
            {
                if (!Validations.RegistrationNoValidations(RegNumtxt.Text, id))
                {
                    return false;
                }
            }
            if (!Validations.FirstNameValidations(FirstNtxt.Text))
            {
                return false;
            }
            if (!Validations.LastNameValidations(LastTxt.Text))
            {
                return false;
            }
            if (!Validations.ContactValidations(ContectTxt.Text))
            {
                return false;
            }
            if (!Validations.EmailValidations(Emailtxt.Text))
            {
                return false;
            }
            if (!Validations.DoBValidations(DOB.Text, 1995, 2005))
            {
                return false;
            }
            if (GenderBx.Text == "")
            {
                MessageBox.Show("Select a Gender First", "Error");
                return false;
            }

            return isValid;
        }
        public void changetext()
        {
            AddBtn.Text = "Update";
        }
        public void TakeRegForUpdation(string Regno)
        {
            using (var con = new SqlConnection(Configuration.getInstance().ConnectionStr))
            {
               con.Open();
                using(SqlCommand cmd = new SqlCommand("SELECT P.Id FROM Person P JOIN Student S ON S.Id = P.Id JOIN Lookup L ON L.Id = P.Gender where S.RegistrationNo=@RegNo", con))
                    {
                    cmd.Parameters.AddWithValue("@RegNo", Regno);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            this.ids = reader.GetInt32(0);
                        }
                    }

                }
                con.Close();
            }
        }
        private void AddBtn_Click(object sender, EventArgs e)
        {

            if (Validation())
            {

                if (AddBtn.Text == "ADD")
                {
                    gender = returnSelectedGender(GenderBx.Text);
                    try
                    {

                        var con = Configuration.getInstance().getConnection();
                        SqlCommand cmd = new SqlCommand("INSERT INTO Person(FirstName,LastName,Contact,Email,DateOfBirth,Gender) VALUES (@FirstName,@LastName,@Contact,@Email,@DateOfBirth,@Gender); INSERT INTO Student(Id,RegistrationNo) VALUES ((select id from person WHERE FirstName = @FirstName AND LastName=@LastName AND Contact=@Contact AND Email=@Email AND DateOfBirth=@DateOfBirth AND Gender=@Gender), @RegNo)", con);
                        cmd.Parameters.AddWithValue("@FirstName", FirstNtxt.Text);
                        cmd.Parameters.AddWithValue("@LastName", LastTxt.Text);
                        cmd.Parameters.AddWithValue("@Contact", ContectTxt.Text);
                        cmd.Parameters.AddWithValue("@Email", Emailtxt.Text);
                        cmd.Parameters.AddWithValue("@DateOfBirth", DOB.Value.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@Gender", gender);
                        cmd.Parameters.AddWithValue("@RegNo", RegNumtxt.Text);
                        cmd.ExecuteNonQuery();
                        OnEvaluationChanged(EventArgs.Empty);
                        MessageBox.Show("Successfully student saved");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error");
                        return;
                    }
                }
                else if (AddBtn.Text == "Update")
                {

                    try
                    {
                        var con = Configuration.getInstance().getConnection();

                        SqlCommand cmd = new SqlCommand("UPDATE Person SET FirstName = @FirstName, LastName=@LastName," +
                            " Contact=@Contact, Email=@Email, DateOfBirth=@DateOfBirth, Gender=@Gender WHERE Id=@Ids;" +
                            " UPDATE Student SET RegistrationNo=@RegNo WHERE Id=@Ids;", con);

                        cmd.Parameters.AddWithValue("@FirstName", FirstNtxt.Text);
                        cmd.Parameters.AddWithValue("@LastName", LastTxt.Text);
                        cmd.Parameters.AddWithValue("@Contact", ContectTxt.Text);
                        cmd.Parameters.AddWithValue("@Email", Emailtxt.Text);
                        cmd.Parameters.AddWithValue("@DateOfBirth", DOB.Value.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@Gender", gender);
                        cmd.Parameters.AddWithValue("@RegNo", RegNumtxt.Text);
                        cmd.Parameters.AddWithValue("@Ids", ids);
                        cmd.ExecuteNonQuery();
                        OnEvaluationChanged(EventArgs.Empty);
                        MessageBox.Show("Update Successfully ");
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

        public void registraionEnable()
        {
            RegNumtxt.Enabled = false;
        }


        private void clear()
        {
            FirstNtxt.Clear();
            LastTxt.Clear();
            ContectTxt.Clear();
            Emailtxt.Clear();
            DOB.Text = string.Empty;
            RegNumtxt.Text = string.Empty;
        }



        





        private void gunaLabel4_Click(object sender, EventArgs e)
        {

        }

        private void Emailtxt_TextChanged(object sender, EventArgs e)
        {

        }

        private void gunaLabel5_Click(object sender, EventArgs e)
        {

        }

        private void gunaLabel1_Click(object sender, EventArgs e)
        {

        }
        private void gunaLabel6_Click(object sender, EventArgs e)
        {

        }

        private void gunaLabel2_Click(object sender, EventArgs e)
        {

        }

        private void gunaLabel3_Click(object sender, EventArgs e)
        {

        }

        private void RegLbl_Click(object sender, EventArgs e)
        {

        }

        private void RegNumtxt_TextChanged(object sender, EventArgs e)
        {

        }

        private void BackBtn_Click(object sender, EventArgs e)
        {
            /*studGridPnl1.ViewStudent();*/
            this.Visible = false;
            AddBtn.Text = "ADD";
            RegNumtxt.Enabled = true;


        }

        private void FirstNtxt_TextChanged(object sender, EventArgs e)
        {

        }

        private void LastTxt_TextChanged(object sender, EventArgs e)
        {

        }

        private void DOB_ValueChanged(object sender, EventArgs e)
        {

        }

        private void GenderBx_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ContectTxt_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
