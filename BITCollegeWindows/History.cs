using BITCollege_SY.Data;
using BITCollege_SY.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BITCollegeWindows
{
    public partial class History : Form
    {

        ///given:  student and registration data will passed throughout 
        ///application. This object will be used to store the current
        ///student and selected registration
        ConstructorData constructorData;

        //Instantiate an object of BITCollege_SYContext class
        BITCollege_SYContext db = new BITCollege_SYContext();

        /// <summary>
        /// given:  This constructor will be used when called from the
        /// Student form.  This constructor will receive 
        /// specific information about the student and registration
        /// further code required:  
        /// </summary>
        /// <param name="constructorData">constructorData object containing
        /// specific student and registration data.</param>
        public History(ConstructorData constructorData)
        {
            InitializeComponent();

            // Populate the constuctorData 
            this.constructorData = constructorData;

            // Populate the upper controls
            studentNumberMaskedTextBox.Text = constructorData.Student.StudentNumber.ToString();

            fullNameLabel1.Text = constructorData.Student.FullName.ToString();

            descriptionLabel1.Text = constructorData.Student.AcademicProgram.Description.ToString();

            registrationDataGridView.DataSource = constructorData.Registration.ToString();
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
        /// given:  Open this form in top right corner of the frame.
        /// further code required:
        /// </summary>
        private void History_Load(object sender, EventArgs e)
        {
            this.Location = new Point(0, 0);

            try
            {
                // Define a LINQ-to-SQL Server query selecting data from the Registrations and Courses
                // tables whose StudentId corresponds to Student passed to this form                       
                var innerJoinQuery =
                    from course in db.Courses
                    join registration in db.Registrations on course.CourseId equals registration.CourseId
                    where registration.StudentId == constructorData.Student.StudentId
                    select new
                    {
                        registration.RegistrationNumber,
                        registration.RegistrationDate,
                        Course = registration.Course.Title,
                        registration.Grade,
                        registration.Notes
                    };

                // Set the DataSource property of the BindingSource object representing the
                // DataGridView control to the result set of this query
                registrationDataGridView.DataSource = innerJoinQuery.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK);
            }
        }
    }
}
