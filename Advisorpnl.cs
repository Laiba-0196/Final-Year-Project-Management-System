
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
    public partial class Advisorpnl : UserControl
    {
        int id;
        int ids;

        public event EventHandler EvaluationChanged;
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

        public string Gender
        {
            get { return GenderBx.Text; }
            set { GenderBx.Text = value; }
        }

        public string salary
        {
            get { return salrytxt.Text; }
            set { salrytxt.Text = value; }
        }

        public string designation
        {
            get { return DesiCmboBx.Text; }
            set { DesiCmboBx.Text = value; }
        }

        public Advisorpnl()
        {
            InitializeComponent();
            this.Padding = new Padding(5); // Padding to make room for rounded corners
            ValueforComboBxOfGender();
            DesignationToComboBox();
        }

        private void Advisorpnl_Load(object sender, EventArgs e)
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




        public void ValueforComboBxOfGender()
        {
            try
            {
                var con = Configuration.getInstance().getConnection();
                SqlCommand cmd = new SqlCommand("SELECT Value FROM Lookup WHERE Category='GENDER'", con);
                SqlDataAdapter d = new SqlDataAdapter(cmd);
                DataSet dt = new DataSet();
                d.Fill(dt);
                // Assuming GenderBx is the name of your ComboBox
                GenderBx.DataSource = dt.Tables[0]; // Set the data source
                GenderBx.DisplayMember = "Value"; // Specify the display member
            }
            catch (Exception ex)
            {
                // Handle any exceptions here
                MessageBox.Show("Error: " + ex.Message);
            }
        }


        public void DesignationToComboBox()
        {
            try
            {
                var con = Configuration.getInstance().getConnection();
                SqlCommand cmd = new SqlCommand("Select Value from Lookup WHERE Category=\'DESIGNATION\'", con);
                SqlDataAdapter d = new SqlDataAdapter(cmd);
                DataSet dt = new DataSet();
                d.Fill(dt);
                DesiCmboBx.DataSource = dt.Tables[0]; // Set the data source
                DesiCmboBx.DisplayMember = "Value"; // Specify the display member
            }
            catch (Exception ex)
            {
                // Handle any exceptions here
                MessageBox.Show("Error: " + ex.Message);
            }


        }



        public void changetext()
        {
            AddBtn.Text = "Update";
        }

        public void UpdateSelectedRowInDatabase( int ids)
        {
            this.ids = ids;
        }

        public void TakeIDFromDataBase(string firstName, string LastName, string contect, string email, string DESIGNATION)

        {
            int desig = giveDesignation(DESIGNATION);
            using (var con = new SqlConnection(Configuration.getInstance().ConnectionStr))
            {
                //con.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT P.Id FROM Person P JOIN Advisor A ON A.Id=P.Id JOIN Lookup L ON L.Id=P.Gender JOIN Lookup L1 ON L1.Id=A.Designation where p.FirstName=@FirstName and p.LastName=@LastName and p.Contact= @Contact and p.Email=@Email and A.Designation=@Designation", con))
                {
                    cmd.Parameters.AddWithValue("@FirstName", firstName);
                    cmd.Parameters.AddWithValue("@LastName", LastName);
                    cmd.Parameters.AddWithValue("@Contact", contect);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Designation", desig);

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


        public int giveGender(string gen)
        {
            int g = -1;
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("SELECT Id FROM Lookup WHERE Category=\'GENDER\' AND Value=@gender", con);
            cmd.Parameters.AddWithValue("@gender", gen);
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                g = int.Parse(reader["Id"].ToString());
            }
            reader.Close();
            return g;
        }
        public int giveDesignation(string desig)
        {
            int d = -1;
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("SELECT Id FROM Lookup WHERE Category=\'DESIGNATION\' AND Value=@DESIGNATION", con);
            cmd.Parameters.AddWithValue("@DESIGNATION", desig);
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                d = int.Parse(reader["Id"].ToString());
            }
            reader.Close();
            return d;
        }


        private void BackBtn_Click(object sender, EventArgs e)
        {
            this.Visible = false;
            AddBtn.Text = "ADD";
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {

            int gender = giveGender(GenderBx.Text);
            int desig = giveDesignation(DesiCmboBx.Text);

            if (Validation())
            {
                if (DesiCmboBx.Text != "")
                {
                    int salary = 0;
                    if (!string.IsNullOrEmpty(salrytxt.Text) && decimal.TryParse(salrytxt.Text, out decimal sal) && sal > 0)
                    {
                        salary = int.Parse(salrytxt.Text);
                    }
                    if (AddBtn.Text == "ADD")
                    {
                        try
                        {
                            var con = Configuration.getInstance().getConnection();
                            SqlCommand cmd = new SqlCommand("INSERT INTO Person(FirstName,LastName,Contact,Email,DateOfBirth,Gender) VALUES (@FirstName,@LastName, @Contact,@Email,@DateOfBirth, @Gender); INSERT INTO Advisor(Id,Designation,Salary) VALUES ((SELECT Id FROM Person WHERE FirstName = @FirstName AND LastName=@LastName AND Contact=@Contact AND Email=@Email AND DateOfBirth=@DateOfBirth AND Gender=@Gender) ,@Designation, @Salary);", con);
                            cmd.Parameters.AddWithValue("@FirstName", FirstNtxt.Text);
                            cmd.Parameters.AddWithValue("@LastName", LastTxt.Text);
                            cmd.Parameters.AddWithValue("@Contact", ContectTxt.Text);
                            cmd.Parameters.AddWithValue("@Email", Emailtxt.Text);
                            cmd.Parameters.AddWithValue("@DateOfBirth", DOB.Value.ToString("yyyy-MM-dd"));
                            cmd.Parameters.AddWithValue("@Gender", gender);
                            cmd.Parameters.AddWithValue("@Designation", desig);
                            cmd.Parameters.AddWithValue("@Salary", salary);
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
                    else if(AddBtn.Text == "Update")
                    {
                        try
                        {
                            var con = Configuration.getInstance().getConnection();
                            SqlCommand cmd = new SqlCommand("UPDATE Person SET FirstName = @FirstName, LastName=@LastName, Contact=@Contact, Email=@Email, DateOfBirth=@DateOfBirth, Gender=@Gender WHERE Id=@Ids; UPDATE Advisor SET Designation=@Designation, Salary=@Salary WHERE Id=@Ids;", con);
                            cmd.Parameters.AddWithValue("@FirstName", FirstNtxt.Text);
                            cmd.Parameters.AddWithValue("@LastName", LastTxt.Text);
                            cmd.Parameters.AddWithValue("@Contact", ContectTxt.Text);
                            cmd.Parameters.AddWithValue("@Email", Emailtxt.Text);
                            cmd.Parameters.AddWithValue("@DateOfBirth", DOB.Value.ToString("yyyy-MM-dd"));
                            cmd.Parameters.AddWithValue("@Gender", gender);
                            cmd.Parameters.AddWithValue("@Designation", desig);
                            cmd.Parameters.AddWithValue("@Salary", salary);
                            cmd.Parameters.AddWithValue("@Ids", ids);
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
        }

        private bool Validation()
        {
            bool isValid = true;
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
            if (!Validations.SalaryValidations(salrytxt.Text))
            {
                return false;
            }
            if (!Validations.DoBValidations(DOB.Text, 1970, 2000))
            {
                return false;
            }

            return isValid;
        }

        protected virtual void OnEvaluationChanged(EventArgs e)
        {
            EvaluationChanged?.Invoke(this, e);
        }
    }
}
