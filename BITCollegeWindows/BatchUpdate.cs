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
    public partial class BatchUpdate : Form
    {
        BITCollege_SYContext db = new BITCollege_SYContext();
        Batch batch = new Batch();

        public BatchUpdate()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Batch processing
        /// Further code to be added.
        /// </summary>
        private void lnkProcess_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Get the selected acronym from ComboBox
            string acronym = descriptionComboBox.SelectedValue.ToString();

            // Check if the acronym is valid (non-null)
            if (string.IsNullOrEmpty(acronym))
            {
                MessageBox.Show("Please select a program acronym.", "Invalid Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Get the list of acronyms from the database
            List<string> listOfAcronyms = db.AcademicPrograms.Select(x => x.ProgramAcronym).ToList();

            // Check which radio button is selected
            if (radSelect.Checked)
            {
                // Process the transmission for the selected acronym
                batch.ProcessTransmission(acronym);

                // Get and append the log data to the RichTextBox
                rtxtLog.AppendText(batch.WriteLogData());
            }
            else if(radAll.Checked)
            {
                // Process each acronym for the "all transmissions" scenario
                foreach (string programAcronym in listOfAcronyms)
                {
                    batch.ProcessTransmission(programAcronym);

                    rtxtLog.AppendText(batch.WriteLogData());
                }
            }
        }

        /// <summary>
        /// given:  Always open this form in top right of frame.
        /// Further code to be added.
        /// </summary>
        private void BatchUpdate_Load(object sender, EventArgs e)
        {
            this.Location = new Point(0, 0);

            academicProgramBindingSource.DataSource = db.AcademicPrograms.ToList();
        }

        /// <summary>
        /// Handle the radAll checkedChanged event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void radAll_CheckedChanged(object sender, EventArgs e)
        {
            if(radSelect.Checked)
            {
                descriptionComboBox.Enabled = true;
            }
            else
            {
                descriptionComboBox.Enabled = false;
            }
        }

        /// <summary>
        /// Test
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void testButton_Click(object sender, EventArgs e)
        {
            Batch batch;
            batch = new Batch();
            batch.ProcessTransmission("VT");
            rtxtLog.Text = batch.WriteLogData();
        }
    }
}