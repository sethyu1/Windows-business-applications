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
    public partial class StudentData : Form
    {
        ///Given: Student and Registration data will be retrieved
        ///in this form and passed throughout application
        ///These variables will be used to store the current
        ///Student and selected Registration
        ConstructorData constructorData = new ConstructorData();

        BITCollege_SYContext db = new BITCollege_SYContext();

        /// <summary>
        /// This constructor will be used when this form is opened from
        /// the MDI Frame.
        /// </summary>
        public StudentData()
        {
            InitializeComponent();
        }

        /// <summary>
        /// given:  This constructor will be used when returning to StudentData
        /// from another form.  This constructor will pass back
        /// specific information about the student and registration
        /// based on activites taking place in another form.
        /// </summary>
        /// <param name="constructorData">constructorData object containing
        /// specific student and registration data.</param>
        public StudentData(ConstructorData constructor)
        {
            InitializeComponent();
            //Further code to be added.

            // Set the constructorData instance variable to the value of the corresponding argument
            this.constructorData = constructor;

            // Pass back studentnumber to the studentNumber TextBox.
            studentNumberMaskedTextBox.Text = constructorData.Student.StudentNumber.ToString();

            // Call the MaskedTextBox_Leave event passing null for each of the event arguments
            studentNumberMaskedTextBox_Leave(null, null);
        }

        /// <summary>
        /// given: Open grading form passing constructor data.
        /// </summary>
        private void lnkUpdateGrade_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PopulateConstructorData();
            Grading grading = new Grading(constructorData);
            grading.MdiParent = this.MdiParent;
            grading.Show();
            this.Close();
        }

        /// <summary>
        /// given: Open history form passing constructor data.
        /// </summary>
        private void lnkViewDetails_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PopulateConstructorData();
            History history = new History(constructorData);
            history.MdiParent = this.MdiParent;
            history.Show();
            this.Close();
        }

        /// <summary>
        /// Populate constructorData with the current Student record & current Registration record
        /// </summary>
        private void PopulateConstructorData()
        {
            this.constructorData.Student = (Student)studentBindingSource.Current;
            this.constructorData.Registration = (Registration)registrationBindingSource.Current;
        }

        /// <summary>
        /// given:  Opens the form in top right corner of the frame.
        /// </summary>
        private void StudentData_Load(object sender, EventArgs e)
        {
            //keeps location of form static when opened and closed
            this.Location = new Point(0, 0);
        }

        /// <summary>
        /// Handles the Leave event for studentNumberMaskedTextBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void studentNumberMaskedTextBox_Leave(object sender, EventArgs e)
        {
            BITCollege_SYContext db = new BITCollege_SYContext();

            // Gets the string input of studentNumberMaskedTextBox and removes the "-"
            // string studentNumberInput = studentNumberMaskedTextBox.Text.Replace("-", "");

            string studentNumberInput = Utility.Numeric.ClearFormatting(studentNumberMaskedTextBox.Text, "-");

            //usage:
            //String formatted = "123-456-789";
            //formatted = ClearFormatting(formatted);
            //**result: formatted = 123456789

            // Converts input to long
            long studentNumberMasked = long.Parse(studentNumberInput);

            // Checks if user has completed the requirements for the Mask.
            if (studentNumberMaskedTextBox.MaskCompleted)
            {
                // Define a LINQ-to-SQL query to select student from database
                Student RetrievedStudent = db.Students.FirstOrDefault(x => x.StudentNumber == studentNumberMasked);

                // Checks if theres any records received
                if (RetrievedStudent == null)
                {
                    // Disable link labels
                    lnkUpdateGrade.Enabled = false;
                    lnkViewDetails.Enabled = false;

                    // Set focus on MaskedTextBox
                    studentNumberMaskedTextBox.Focus();

                    // Clear BindingSource object
                    studentBindingSource.DataSource = typeof(Student);
                    registrationBindingSource.DataSource = typeof(Registration);

                    // Display error MessageBox
                    MessageBox.Show("Student " + studentNumberInput + " does not exist.", "Invalid Student Number", MessageBoxButtons.OK);
                }
                else
                {
                    // Set DataSource property of BindingSource representing Student controls to RetrievedStudent
                    studentBindingSource.DataSource = RetrievedStudent;

                    // Define LINQ-to-SQL queery to selecct all Registration record from database
                    IQueryable<Registration> retrievedRegistration = db.Registrations.Where(x => x.StudentId == RetrievedStudent.StudentId);

                    // Create a list of all registration
                    List<Registration> allRegistration = retrievedRegistration.ToList();

                    if (allRegistration == null || allRegistration.Count == 0)
                    {
                        // Disable link labels
                        lnkUpdateGrade.Enabled = false;
                        lnkViewDetails.Enabled = false;

                        registrationBindingSource.DataSource = typeof(Registration);
                    }
                    else
                    {
                        // Set DataSource property of BindingSource representing Registrations controls to allRegistration
                        registrationBindingSource.DataSource = allRegistration;

                        // Enable registration number combo box
                        registrationNumberComboBox.Enabled = true;

                        // Enable link labels
                        lnkUpdateGrade.Enabled = true;
                        lnkViewDetails.Enabled = true;

                        // Use the if statement to avoid nullReferenceException.
                        if (constructorData.Registration != null)
                        {
                            // Populate the registartionNumberComboBox 
                            registrationNumberComboBox.Text = constructorData.Registration.RegistrationNumber.ToString();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please enter a completed student number");
            }
        }
    }
}