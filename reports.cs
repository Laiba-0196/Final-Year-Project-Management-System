using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Font;
/*using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;*/
using iText.Layout.Properties;
using System;
using System.Data.SqlClient;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Font = iTextSharp.text.Font;
using System.Drawing.Imaging;
using System.Collections.Generic;



namespace DBMIDPRO
{
    public partial class reports : UserControl
    {
        Font boldFont = FontFactory.GetFont(FontFactory.TIMES_BOLD, 16);
        Font textFont = FontFactory.GetFont(FontFactory.TIMES_ROMAN, 12);

        public reports()
        {
            InitializeComponent();
            SetRoundRegion();
        }

        private void SetRoundRegion()
        {
            System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath();
            int radius = 20; // Adjust the radius to change the roundness of corners
            int diameter = radius * 2;

            // Top left corner
            path.AddArc(new System.Drawing.Rectangle(0, 0, diameter, diameter), 180, 90);
            // Top right corner
            path.AddArc(new System.Drawing.Rectangle(Width - diameter - 1, 0, diameter, diameter), 270, 90);
            // Bottom right corner
            path.AddArc(new System.Drawing.Rectangle(Width - diameter - 1, Height - diameter - 1, diameter, diameter), 0, 90);
            // Bottom left corner
            path.AddArc(new System.Drawing.Rectangle(0, Height - diameter - 1, diameter, diameter), 90, 90);

            path.CloseFigure();
            Region = new Region(path);
        }


        private Document TitlePage(ref Document document)
        {
            // Add content to the document
            Paragraph title = new Paragraph("FYP Management System", boldFont);
            title.Font.Size = 28;
            title.Alignment = Element.ALIGN_CENTER;
            document.Add(title);

            //Add images
            System.Drawing.Image image = Properties.Resources.uet_logo;
            iTextSharp.text.Image image1 = iTextSharp.text.Image.GetInstance(image, System.Drawing.Imaging.ImageFormat.Png);
            image1.Alignment = Element.ALIGN_CENTER;
            image1.ScaleAbsolute(150f, 150f);
            document.Add(image1);
            // Add session info
            Paragraph session = new Paragraph("Session: 2022 - 2026", textFont);
            session.Alignment = Element.ALIGN_CENTER;
            document.Add(session);

            // Add submitted by
            Paragraph submittedBy = new Paragraph("Submitted By:", boldFont);
            submittedBy.Font.Size = 16;
            submittedBy.Alignment = Element.ALIGN_CENTER;
            document.Add(submittedBy);
            Paragraph submittedByDetails = new Paragraph("Iqra Tariq        2022-CS-29", textFont);
            submittedByDetails.Font.Size = 14;
            submittedByDetails.Alignment = Element.ALIGN_CENTER;
            document.Add(submittedByDetails);

            // Add submitted to
            Paragraph submittedTo = new Paragraph("Submitted To:", boldFont);
            submittedTo.Font.Size = 16;
            submittedTo.Alignment = Element.ALIGN_CENTER;
            document.Add(submittedTo);
            Paragraph submittedToDetails = new Paragraph("Sir Nazeef UL Haq", textFont);
            submittedToDetails.Font.Size = 14;
            submittedToDetails.Alignment = Element.ALIGN_CENTER;
            document.Add(submittedToDetails);

            // Add department and university info
            Paragraph department = new Paragraph("Department of Computer Science", textFont);
            department.Font.Size = 18;
            department.Alignment = Element.ALIGN_CENTER;
            document.Add(department);
            Paragraph university = new Paragraph("University of Engineering And Technology, Lahore", boldFont);
            university.Font.Size = 24;
            university.Alignment = Element.ALIGN_CENTER;
            document.Add(university);
            return document;
        }


        private Document GetProjectAndAdvisoryBoard(ref Document document)
        {
            try
            {
                document.NewPage();
                Paragraph title = new Paragraph("Project Title Along Advisory Board", boldFont);
                title.SpacingBefore = 20f;
                title.SpacingAfter = 20f;
                title.Font.Size = 20;
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                var con = Configuration.getInstance().getConnection();
                SqlCommand cmd = new SqlCommand("SELECT MAX(P.Title) Title,MAX(P.Description) AS [Project Description], MAX(CASE WHEN L.Value='Main Advisor' THEN CONCAT(Person.FirstName,' ',Person.LastName) END) AS [Main Advisor], MAX(CASE WHEN L.Value='Co-Advisror' THEN CONCAT(Person.FirstName,' ',Person.LastName) END) AS [Co Advisor], MAX(CASE WHEN L.Value='Industry Advisor' THEN CONCAT(Person.FirstName,' ',Person.LastName) END) AS [Industry Advisor] FROM  ProjectAdvisor PA INNER JOIN Advisor A ON PA.AdvisorId = A.Id JOIN Project P ON P.Id=PA.ProjectId JOIN Person ON Person.Id=A.Id JOIN Lookup L ON L.Id=PA.AdvisorRole  where Person.FirstName not like '!%' and P.Title NOT LIKE '!%' GROUP BY PA.ProjectId", con);
                SqlDataReader reader = cmd.ExecuteReader();

                PdfPTable table = new PdfPTable(reader.FieldCount);
                table.WidthPercentage = 100;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(reader.GetName(i)));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.BackgroundColor = new BaseColor(128, 128, 128);
                    table.AddCell(cell);
                }

                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(reader[i].ToString()));
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        table.AddCell(cell);
                    }
                }
                reader.Close();
                document.Add(table);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return document;
        }

        private void GenerateProjectAdvisoryBoardReport()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PDF (*.pdf)|*.pdf";
            sfd.FileName = "AdvisoryBoard.pdf";
            bool errorMessage = false;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(sfd.FileName))
                {
                    try
                    {
                        File.Delete(sfd.FileName);


                    }
                    catch (Exception ex)
                    {
                        errorMessage = true;
                        MessageBox.Show("Unable to write data in disk" + ex.Message);
                    }
                }
                if (!errorMessage)
                {
                    // Create new PDF document
                    Document document = new Document();
                    PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(sfd.FileName, FileMode.Create));
                    document.Open();


                    document = TitlePage(ref document);
                    document = GetProjectAndAdvisoryBoard(ref document);

                    // Close PDF document and writer
                    document.Close();
                    writer.Close();
                }
            }
        }

        private void ProandadvBtn_Click(object sender, EventArgs e)
        {
            GenerateProjectAdvisoryBoardReport();
        }

        private void reports_Load(object sender, EventArgs e)
        {
            SetRoundRegion();
        }

        //-----------------------------------projectMembers-----------------------------------------------//
        private Document GetStudents(ref Document document)
        {
            try
            {
                document.NewPage();
                Paragraph title = new Paragraph("All Students", boldFont);
                title.SpacingBefore = 20f;
                title.SpacingAfter = 20f;
                title.Font.Size = 20;
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                var con = Configuration.getInstance().getConnection();
                SqlCommand cmd = new SqlCommand("Select S.RegistrationNo AS [Registration No], (FirstName + ' ' + LastName) AS Name,L.Value AS Gender,(SELECT FORMAT(DateOfBirth, 'dd-MM-yyyy')) AS [DoB],Contact,Email from Person P JOIN Student S ON S.Id=P.Id JOIN Lookup L ON L.Id=P.Gender", con);
                SqlDataReader reader = cmd.ExecuteReader();

                PdfPTable table = new PdfPTable(reader.FieldCount);
                table.WidthPercentage = 100;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(reader.GetName(i)));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.BackgroundColor = new BaseColor(128, 128, 128);
                    table.AddCell(cell);
                }

                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(reader[i].ToString()));
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        table.AddCell(cell);
                    }
                }
                reader.Close();
                document.Add(table);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return document;
        }

        private Document GetAdvisors(ref Document document)
        {
            try
            {
                document.NewPage();
                Paragraph title = new Paragraph("All Advisors", boldFont);
                title.SpacingBefore = 20f;
                title.SpacingAfter = 20f;
                title.Font.Size = 20;
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                var con = Configuration.getInstance().getConnection();
                SqlCommand cmd = new SqlCommand("Select (FirstName + ' ' + LastName) AS Name, L1.Value AS Designation,A.Salary,L.Value AS Gender,(SELECT FORMAT(DateOfBirth, 'dd-MM-yyyy')) AS [DoB],Contact,Email from Person P JOIN Advisor A ON A.Id=P.Id JOIN Lookup L ON L.Id=P.Gender JOIN Lookup L1 ON L1.Id=A.Designation", con);
                SqlDataReader reader = cmd.ExecuteReader();

                PdfPTable table = new PdfPTable(reader.FieldCount);
                table.WidthPercentage = 100;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(reader.GetName(i)));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.BackgroundColor = new BaseColor(128, 128, 128);
                    table.AddCell(cell);
                }

                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(reader[i].ToString()));
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        table.AddCell(cell);
                    }
                }
                reader.Close();
                document.Add(table);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return document;
        }


        private void GenerateStudentandAdvisorReport()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PDF (*.pdf)|*.pdf";
            sfd.FileName = "FYP Members.pdf";
            bool errorMessage = false;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(sfd.FileName))
                {
                    try
                    {
                        File.Delete(sfd.FileName);


                    }
                    catch (Exception ex)
                    {
                        errorMessage = true;
                        MessageBox.Show("Unable to write data in disk" + ex.Message);
                    }
                }
                if (!errorMessage)
                {
                    // Create new PDF document
                    Document document = new Document();
                    PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(sfd.FileName, FileMode.Create));
                    document.Open();


                    document = TitlePage(ref document);
                    document = GetStudents(ref document);
                    document = GetAdvisors(ref document);

                    // Close PDF document and writer
                    document.Close();
                    writer.Close();
                }
            }
        }

        private void memberBtn_Click(object sender, EventArgs e)
        {
            GenerateStudentandAdvisorReport();
        }


        //----------------------------------markssheet-------------------------------------------//


        private List<int> GetEvaluationIds()
        {
            List<int> evaluationIds = new List<int>();
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("SELECT Id FROM Evaluation ORDER BY Id ASC", con);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                evaluationIds.Add(int.Parse(reader["Id"].ToString()));
            }
            reader.Close();

            return evaluationIds;
        }

        private List<string> GetEvaluationTitle()
        {
            List<string> evaluationTitles = new List<string>();
            var con = Configuration.getInstance().getConnection();
            SqlCommand cmd = new SqlCommand("SELECT CONCAT(Name,CHAR(13),TotalMarks) AS name FROM Evaluation where Evaluation.Name not like '!%' ORDER BY Id ASC", con);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                evaluationTitles.Add(reader["name"].ToString());
            }
            reader.Close();

            return evaluationTitles;
        }

        private Document MarkSheet(ref Document document)
        {
            try
            {
                List<int> evaluationIds = GetEvaluationIds();
                List<string> evaluationTitle = GetEvaluationTitle();
                if (evaluationIds.Count > 0)
                {
                    //MAX(CASE WHEN L.Value = 'Main Advisor' THEN CONCAT(Person.FirstName, ' ', Person.LastName) END) AS[Main Advisor]
                    string query = "";
                    int idx = 0;
                    foreach (int evaluationId in evaluationIds)
                    {
                        query += ",MAX(CASE WHEN E.Id=" + evaluationId + " THEN GE.ObtainedMarks END) AS [" + evaluationTitle[idx] + "]";
                        idx++;
                    }
                    document.NewPage();

                    Paragraph title = new Paragraph("Mark Sheet", boldFont);
                    title.SpacingBefore = 20f;
                    title.SpacingAfter = 20f;
                    title.Font.Size = 20;
                    title.Alignment = Element.ALIGN_CENTER;
                    document.Add(title);

                    var con = Configuration.getInstance().getConnection();
                    SqlCommand cmd = new SqlCommand("SELECT S.RegistrationNo AS [Reg No],MAX(CONCAT(P.FirstName,' ',P.LastName)) AS [Student Name],MAX(Pr.Title) AS [Project Title] " + query + ",SUM((GE.ObtainedMarks*E.TotalWeightage)/E.TotalMarks) AS [Total Marks] FROM GroupEvaluation AS GE JOIN Evaluation AS E ON GE.EvaluationId=E.Id JOIN GroupStudent AS GS ON GS.GroupId=GE.GroupId JOIN Student AS S ON S.Id=GS.StudentId JOIN Person AS P ON P.Id=S.Id JOIN GroupProject AS GP ON GP.GroupId=GE.GroupId JOIN Project AS Pr ON Pr.Id=GP.ProjectId WHERE GS.Status IN (SELECT Id FROM Lookup WHERE Value='Active') and where P.FirstName not like '%!%' and Pr.Title NOT LIKE '%!%' and  E.Name not like '%!%' GROUP BY GE.GroupId,Pr.Id,S.Id,S.RegistrationNo ORDER BY GE.GroupId,Pr.Id,S.RegistrationNo", con);
                    SqlDataReader reader = cmd.ExecuteReader();


                    PdfPTable table = new PdfPTable(reader.FieldCount);
                    table.WidthPercentage = 100;
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(reader.GetName(i)));
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.BackgroundColor = new BaseColor(128, 128, 128);
                        table.AddCell(cell);
                    }



                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            PdfPCell cell = new PdfPCell(new Phrase(reader[i].ToString()));
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            table.AddCell(cell);
                        }
                    }
                    reader.Close();
                    document.Add(table);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return document;
        }

        private void GenerateMarkSheetReport()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PDF (*.pdf)|*.pdf";
            sfd.FileName = "Mark_Sheet.pdf";
            bool errorMessage = false;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(sfd.FileName))
                {
                    try
                    {
                        File.Delete(sfd.FileName);


                    }
                    catch (Exception ex)
                    {
                        errorMessage = true;
                        MessageBox.Show("Unable to write data in disk" + ex.Message);
                    }
                }
                if (!errorMessage)
                {
                    // Create new PDF document
                    Document document = new Document();
                    PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(sfd.FileName, FileMode.Create));
                    document.Open();


                    document = TitlePage(ref document);
                    document = MarkSheet(ref document);

                    // Close PDF document and writer
                    document.Close();
                    writer.Close();
                }
            }
        }
     
        private void ProEvalMarkBtn_Click(object sender, EventArgs e)
        {
            GenerateMarkSheetReport();
        }

        //---------------------------projectdetail--------------------------------------------\\
        private Document GetAllproject(ref Document document)
        {
            try
            {
                document.NewPage();
                Paragraph title = new Paragraph("Project Detail", boldFont);
                title.SpacingBefore = 20f;
                title.SpacingAfter = 20f;
                title.Font.Size = 20;
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                var con = Configuration.getInstance().getConnection();
                SqlCommand cmd = new SqlCommand("SELECT P.Title AS ProjectTitle, (PE.FirstName + ' ' + PE.LastName) AS ProjectAdvisor, (PEs.FirstName + ' ' + PEs.LastName) AS StudentName, GP.GroupId " +
                                                 "FROM Project P " +
                                                 "LEFT JOIN ProjectAdvisor PA ON P.Id = PA.ProjectId " +
                                                 "JOIN Advisor A ON A.Id = PA.AdvisorId " +
                                                 "JOIN Person PE ON PE.Id = A.Id " +
                                                 "JOIN GroupProject GP ON GP.ProjectId = P.Id " +
                                                 "JOIN [Group] G ON G.Id = GP.GroupId " +
                                                 "JOIN GroupStudent GS ON GS.GroupId = GP.GroupId " +
                                                 "JOIN Student S ON S.Id = GS.StudentId " +
                                                 "JOIN Person PEs ON PEs.Id = S.Id " +
                                                 "WHERE P.Title NOT LIKE '%!%' AND PE.FirstName NOT LIKE '%!%' AND PEs.FirstName NOT LIKE '%!%'", con);
                SqlDataReader reader = cmd.ExecuteReader();

                PdfPTable table = new PdfPTable(reader.FieldCount);
                table.WidthPercentage = 100;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(reader.GetName(i)));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.BackgroundColor = new BaseColor(128, 128, 128);
                    table.AddCell(cell);
                }

                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(reader[i].ToString()));
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        table.AddCell(cell);
                    }
                }
                reader.Close();
                document.Add(table);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return document;
        }



        private void GenerateProjectReport()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PDF (*.pdf)|*.pdf";
            sfd.FileName = "ProjectDetail.pdf";
            bool errorMessage = false;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(sfd.FileName))
                {
                    try
                    {
                        File.Delete(sfd.FileName);


                    }
                    catch (Exception ex)
                    {
                        errorMessage = true;
                        MessageBox.Show("Unable to write data in disk" + ex.Message);
                    }
                }
                if (!errorMessage)
                {
                    // Create new PDF document
                    Document document = new Document();
                    PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(sfd.FileName, FileMode.Create));
                    document.Open();


                    document = TitlePage(ref document);
                    document = GetAllproject(ref document);

                    // Close PDF document and writer
                    document.Close();
                    writer.Close();
                }
            }
        }

        private void ProBtn_Click(object sender, EventArgs e)
        {
            GenerateProjectReport();
        }

        //----------------------StudentDetail------------------------------------------//



        private Document GetPresentStudent(ref Document document)
        {
            try
            {
                document.NewPage();
                Paragraph title = new Paragraph("PresentStudent", boldFont);
                title.SpacingBefore = 20f;
                title.SpacingAfter = 20f;
                title.Font.Size = 20;
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                var con = Configuration.getInstance().getConnection();
                SqlCommand cmd = new SqlCommand("select (P.FirstName +' '+P.LastName) as PresentStudent,S.RegistrationNo,P.DateOfBirth as DOB,p.Contact,P.Email,P.Gender\r\nfrom Student S left join\r\nperson P on S.Id=P.Id\r\njoin GroupStudent GS on GS.StudentId=S.Id\r\njoin [Group] G on G.Id=GS.GroupId\r\nwhere P.FirstName not like '%!%';", con);
                SqlDataReader reader = cmd.ExecuteReader();

                PdfPTable table = new PdfPTable(reader.FieldCount);
                table.WidthPercentage = 100;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(reader.GetName(i)));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.BackgroundColor = new BaseColor(128, 128, 128);
                    table.AddCell(cell);
                }

                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(reader[i].ToString()));
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        table.AddCell(cell);
                    }
                }
                reader.Close();
                document.Add(table);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return document;
        }


        private Document GetArchivedStudent(ref Document document)
        {
            try
            {
                document.NewPage();
                Paragraph title = new Paragraph("ArchivedStudent", boldFont);
                title.SpacingBefore = 20f;
                title.SpacingAfter = 20f;
                title.Font.Size = 20;
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                var con = Configuration.getInstance().getConnection();
                SqlCommand cmd = new SqlCommand("select (P.FirstName +' '+P.LastName) as PresentStudent,S.RegistrationNo,P.DateOfBirth as DOB,p.Contact,P.Email,P.Gender\r\nfrom Student S left join\r\nperson P on S.Id=P.Id\r\njoin GroupStudent GS on GS.StudentId=S.Id\r\njoin [Group] G on G.Id=GS.GroupId\r\nwhere P.FirstName  like '%!%';", con);
                SqlDataReader reader = cmd.ExecuteReader();

                PdfPTable table = new PdfPTable(reader.FieldCount);
                table.WidthPercentage = 100;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(reader.GetName(i)));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.BackgroundColor = new BaseColor(128, 128, 128);
                    table.AddCell(cell);
                }

                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(reader[i].ToString()));
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        table.AddCell(cell);
                    }
                }
                reader.Close();
                document.Add(table);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return document;
        }

        private void StudentReport()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "PDF (*.pdf)|*.pdf";
            sfd.FileName = "StudentDetail.pdf";
            bool errorMessage = false;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                if (File.Exists(sfd.FileName))
                {
                    try
                    {
                        File.Delete(sfd.FileName);


                    }
                    catch (Exception ex)
                    {
                        errorMessage = true;
                        MessageBox.Show("Unable to write data in disk" + ex.Message);
                    }
                }
                if (!errorMessage)
                {
                    // Create new PDF document
                    Document document = new Document();
                    PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(sfd.FileName, FileMode.Create));
                    document.Open();


                    document = TitlePage(ref document);
                    document = GetPresentStudent(ref document);
                    document = GetArchivedStudent(ref document);

                    // Close PDF document and writer
                    document.Close();
                    writer.Close();
                }
            }
        }

        private void StuDirecReport_Click(object sender, EventArgs e)
        {
            StudentReport();
        }

        private void backbtn_Click(object sender, EventArgs e)
        {
            this.Visible = false;
        }
    }
}
