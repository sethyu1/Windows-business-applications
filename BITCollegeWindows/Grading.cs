using BITCollegeWindows.CollegeService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utility;

namespace BITCollegeWindows
{
    public partial class Grading : Form
    {

        ///given:  student and registration data will passed throughout 
        ///application. This object will be used to store the current
        ///student and selected registration
        ConstructorData constructorData;

        /// <summary>
        /// given:  This constructor will be used when called from the
        /// Student form.  This constructor will receive 
        /// specific information about the student and registration
        /// further code required:  
        /// </summary>
        /// <param name="constructorData">constructorData object containing
        /// specific student and registration data.</param>
        public Grading(ConstructorData constructor)
        {
            InitializeComponent();

            // Populate the constructorData
            this.constructorData = constructor;

            // Populate the upper controls
            studentNumberMaskedTextBox.Text = constructor.Student.StudentNumber.ToString();

            fullNameLabel1.Text = constructor.Student.FullName.ToString();

            descriptionLabel1.Text = constructor.Student.AcademicProgram.Description.ToString();

            // Populate the lower controls
            courseNumberLabel1.Text = constructor.Registration.Course.CourseNumber.ToString();

            titleLabel1.Text = constructor.Registration.Course.Title.ToString();

            courseTypeLabel1.Text = constructor.Registration.Course.CourseType.ToString();

            // Grade formating
            if (constructor.Registration.Grade.HasValue)
            {
                gradeTextBox.Text = (constructor.Registration.Grade.Value * 100).ToString("F2") + "%";
            }
            else
            {
                gradeTextBox.Text = ""; // Handle null case
            }
        }

        /// <summary>
        /// given: This code will navigate back to the Student form with
        /// the specific student and registration data that launched
        /// this form.
        /// </summary>
        private void lnkReturn_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //return to student with the data selected for this form
            StudentData student = new StudentData(constructorData);
            student.MdiParent = this.MdiParent;
            student.Show();
            this.Close();
        }

        /// <summary>
        /// given:  Always open in this form in the top right corner of the frame.
        /// further code required:
        /// </summary>
        private void Grading_Load(object sender, EventArgs e)
        {
            this.Location = new Point(0, 0);

            //courseNumberLabel1 = Utility.BusinessRules.CourseFormat(ConstructorData.Registraion.Course.CourseType);
            
            //usage:
            //String mask = BusinessRules.CourseFormat("Mastery");
            //result:  mask = ">L-00-0-00"

            if (constructorData.Registration.Grade != null)
            {
                gradeTextBox.Enabled = false;
                lnkUpdate.Enabled = false;
                lblExisting.Enabled = true;
            }
            else
            {
                gradeTextBox.Enabled = true;
                lnkUpdate.Enabled = true;
                lblExisting.Enabled = false;
            }
        }

        /// <summary>
        /// Handles the logic for updating a student grade
        /// </summary>
        private void lnkUpdate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string textBoxValue = gradeTextBox.Text;

            //usage:
            //String formatted = "123-456-789";
            //formatted = ClearFormatting(formatted);
            //**result: formatted = 123456789

            // 75.00% - 0.75
            string formattedGrade = Utility.Numeric.ClearFormatting(textBoxValue, "%");

            if (Utility.Numeric.IsNumeric(formattedGrade, NumberStyles.Number))
            {
                // Convert the cleaned string value to a double
                double numericGrade = Double.Parse(textBoxValue);

                numericGrade /= 100;

                // Check if the numeric grade is within the range 0 -1
                if (numericGrade < 0 || numericGrade > 1)
                {
                    MessageBox.Show("\"The grade must be entered as a decimal value between 0 and 1 (e.g., 0.74).");
                }
                else
                {
                    // Use WCF Web Service to update grade.
                    CollegeRegistrationClient client = new CollegeRegistrationClient();

                    client.UpdateGrade(numericGrade * 100, 
                        constructorData.Registration.RegistrationId, 
                        constructorData.Registration.Notes);

                    gradeTextBox.Enabled = false;
                }
            }
            else
            {
                // Handle the case where the value is not numeric
                MessageBox.Show("Please enter a valid numeric grade.");
            }
        }
    }
}
