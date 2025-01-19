using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.IO;
using System.Windows.Forms;
using BITCollege_SY.Data;
using System.Reflection;
using BITCollegeWindows.CollegeService;
using BITCollege_SY.Models;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.ServiceModel.Configuration;

namespace BITCollegeWindows
{
    /// <summary>
    /// Batch:  This class provides functionality that will validate
    /// and process incoming xml files.
    /// </summary>
    public class Batch
    {
        private String inputFileName;
        private String logFileName;
        private String logData;

        BITCollege_SYContext db = new BITCollege_SYContext();

        CollegeRegistrationClient client = new CollegeRegistrationClient();

        /// <summary>
        /// Filters erroneous records and records the details of that record
        /// </summary>
        /// <param name="beforeQuery"></param>
        /// <param name="afterQuery"></param>
        /// <param name="message"></param>
        private void ProcessErrors(IEnumerable<XElement> beforeQuery, IEnumerable<XElement> afterQuery, String message)
        {
            // Retrieve all the records that failed validation by comparing before and after queries
            IEnumerable<XElement> errors = beforeQuery.Except(afterQuery);

            foreach (XElement error in errors)
            {
                // Extract relevant elements from the error record
                XElement programElement = error.Element("program");
                XElement studentNoElement = error.Element("student_no");
                XElement courseNoElement = error.Element("course_no");
                XElement registrationNoElement = error.Element("registration_no");
                XElement typeElement = error.Element("type");
                XElement gradeElement = error.Element("grade");
                XElement notesElement = error.Element("notes");

                // Append error information to logData, Do not use .Value
                logData += "\r\n--------ERROR--------";
                logData += $"\r\nFile: {inputFileName}";
                logData += $"\r\nProgram: {programElement}";
                logData += $"\r\nStudent Number: {studentNoElement}";
                logData += $"\r\nCourse Number: {courseNoElement}";
                logData += $"\r\nRegistration Number: {registrationNoElement}";
                logData += $"\r\nType: {typeElement}";
                logData += $"\r\nGrade: {gradeElement}";
                logData += $"\r\nNotes: {notesElement}";
                logData += $"\r\nNumber of Nodes: {error.Nodes().Count()}";
                logData += $"\r\nMessage: {message}";
                logData += $"\r\n---------------------";
            }
        }

        /// <summary>
        /// Verify the attributes of the xml file's root element.
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void ProcessHeader()
        {
            // Load the XML file into an XDocument object
            XDocument xmlDocument = XDocument.Load(inputFileName);

            // Get the root element of the XML file 
            XElement root = xmlDocument.Root;

            // Validate the number of attributes
            if (root.Attributes().Count() != 3)
            {
                throw new Exception("Invalid XML: Root element must contain exactly 3 attributes.");
            }

            // Validate the date attribute
            string dateAttribute = root.Attribute("date").Value;

            if (string.IsNullOrEmpty(dateAttribute) ||  DateTime.Parse(dateAttribute) != DateTime.Today)
            {
                throw new Exception($"Invalid XML: The 'date' attribute ({dateAttribute}) must be todays's date ({DateTime.Today:yyyy-MM-dd})");
            }

            // Validate the program attribute
            string programAttribute = root.Attribute("program").Value;

            if (!db.AcademicPrograms.Any(x => x.ProgramAcronym == programAttribute))
            {
                throw new Exception($"Invalid XML: The 'program' attribute ({programAttribute}) must match any program acronym in the database.");
            }

            // Validate the checksum attribute
            string checksumAttribute = root.Attribute("checksum").Value;

            if (string.IsNullOrEmpty(checksumAttribute))
            {
                throw new Exception("Invalid XML: The 'checksum' attribute is missing or invalid.");
            }

            int checksum;
            if (!int.TryParse(checksumAttribute, out checksum))
            {
                throw new Exception("Invalid XML: The 'checksum' attribute must be a numeric value.");
            }

            // Calculate the actual checksum by summing all student_no values
            IEnumerable<XElement> transactions = root.Elements("transaction");

            // Parse and sum all student_no values 
            int calculatedChecksum = transactions.Sum(transaction =>
            {
                string studentNoValue = transaction.Element("student_no")?.Value ?? "0";
                return int.Parse(studentNoValue);
            });

            if (checksum != calculatedChecksum)
            {
                throw new Exception($"Invalid XML: The 'checksum' attribute value ({checksum}) does not match the calculated checksum ({calculatedChecksum}).");
            }

        }

        /// <summary>
        /// Processes and validates the child elements of transmission.
        /// </summary>
        private void ProcessDetails()
        {
            // Load the XML file into an XDocument object
            XDocument xDocument = XDocument.Load(inputFileName);

            // Retrieve all transaction elements 
            IEnumerable<XElement> totalTransactions = xDocument.Descendants().Elements("transaction");

            // Validate each transaction has 7 child elements
            IEnumerable<XElement> validChildElements = totalTransactions.Where(x => 
            x.Elements().Nodes().Count() == 7);

            ProcessErrors(totalTransactions, validChildElements, "Invalid transaction: Must be 7 child elements.");

            // Validate the program element match the program attribute of root element
            string programAttribute = xDocument.Root.Attribute("program").Value;
            
            IEnumerable<XElement> validProgram = validChildElements.Where(x => 
            x.Element("program").Value == programAttribute);
            
            ProcessErrors(validChildElements, validProgram, "Invalid transaction: Program element does not match root program attribute");

            // Validate the type element is numeric
            IEnumerable<XElement> validTypeNumeric = validProgram.Where(x => 
            Utility.Numeric.IsNumeric(x.Element("type").Value, NumberStyles.Number));
            
            ProcessErrors(validProgram, validTypeNumeric, "Invalid transaction: Type element is not numeric");

            // Validate the grade element is numeric or '*'
            IEnumerable<XElement> validGrade = validTypeNumeric.Where(x =>
            Utility.Numeric.IsNumeric(x.Element("grade").Value, NumberStyles.Number) || x.Element("grade").Value =="*");
            
            ProcessErrors(validTypeNumeric, validGrade, "Invalid transaction: Grade element is not numeric or '*'.");

            // Validate the type element is either 1 or 2
            IEnumerable<XElement> validType = validGrade.Where(x =>
            x.Element("type").Value == "1" || x.Element("type").Value == "2");

            ProcessErrors(validGrade, validType, "Invalid transaction: Type element is not 1 or 2.");

            // Validate the grade for type 1 (course registration) must be '*'
            // and for type 2 (grading) must be between 0 and 100
            IEnumerable<XElement> validGradeForType = validType.Where(x =>
            (x.Element("type").Value == "1" && x.Element("grade").Value == "*") ||
            (x.Element("type").Value == "2"
            && Utility.Numeric.IsNumeric(x.Element("grade").Value, NumberStyles.Float)
            && double.Parse(x.Element("grade").Value) >= 0
            && double.Parse(x.Element("grade").Value) <= 100));

            ProcessErrors(validType, validGradeForType, "Invalid transaction: Invalid grade value for the specified type.");

            // Validate the student_no exists in the database
            List<long> validStudentNumbers = db.Students.Select(x => x.StudentNumber).ToList();
            
            IEnumerable<XElement> validStudents = validGradeForType.Where(x => 
            validStudentNumbers.Contains(long.Parse(x.Element("student_no").Value)));

            ProcessErrors(validGradeForType, validStudents, "Invalid transaction: Student number does not exist in the database.");

            // Validate the course_no exists in the database for type 1 or is '*' for type 2
            List<string> validCourseNumbers = db.Courses.Select(x => x.CourseNumber).ToList();
            
            IEnumerable<XElement> validCourses = validStudents.Where(x =>
            (x.Element("course_no").Value == "*" && x.Element("type").Value == "2") ||

            validCourseNumbers.Contains((x.Element("course_no").Value)));

            ProcessErrors(validStudents, validCourses, "Invalid transaction: Course number does not exist or is invalid.");

            // Validate the registration_no exists in the database for type 2 or is '*' for type 1.
            List<long> validRegistrationNumbers = db.Registrations.Select(x => x.RegistrationNumber).ToList();

            IEnumerable<XElement> validRegistrations = validCourses.Where(x =>
            (x.Element("type").Value == "1" && x.Element("registration_no").Value == "*") ||
            (validRegistrationNumbers.Contains(long.Parse(x.Element("registration_no").Value))));
      
            ProcessErrors(validCourses, validRegistrations, "Invalid transaction: Registration number does not exist or is invalid.");

            // Process the remaining valid transactions
            ProcessTransactions(validRegistrations);
        }

        /// <summary>
        /// Processes all valid transaction records.
        /// </summary>
        /// <param name="transactionRecords"></param>
        private void ProcessTransactions(IEnumerable<XElement> transactionRecords)
        {
            // Iterate through transactions
            foreach (XElement transaction in transactionRecords)
            {
                // Retrieve necessary value from the transaction
                string type = transaction.Element("type").Value;
                string studentNo = transaction.Element("student_no").Value;
                string courseNo = transaction.Element("course_no").Value;
                string grade = transaction.Element("grade").Value;
                string registrationNo = transaction.Element("registration_no").Value;
                string notes = transaction.Element("notes").Value;

                if (type == "1") // registration
                {
                    long studentNoParsed = long.Parse(studentNo);

                    Student student = db.Students.FirstOrDefault(x => x.StudentNumber == studentNoParsed  );
                    int studentId = student.StudentId;

                    Course course = db.Courses.FirstOrDefault(x => x.CourseNumber == courseNo);
                    int courseId = course.CourseId;

                    int result = client.RegisterCourse(studentId, courseId, notes);

                    if (result == 0)
                    {
                        logData += $"Student: {studentNoParsed} has successfully registered for Course: {courseNo}.\n";
                    }
                    else
                    {
                        logData += Utility.BusinessRules.RegisterError(result);
                    }
                }
                else if (type == "2") //grading
                {
                    long registrationNoParsed = long.Parse(registrationNo);

                    Registration registration = db.Registrations.FirstOrDefault(x => x.RegistrationNumber == registrationNoParsed);
                    int registrationId = registration.RegistrationId;

                    double normalizedGrade = double.Parse(grade) / 100;
                    double? result = client.UpdateGrade(normalizedGrade, registrationId, notes);

                    if (registration.Grade != normalizedGrade)
                    {
                        logData += $"\r\nA grade of: {grade} has been successfully applied to registration: {registrationNo}\n";
                    }
                    else
                    {
                        logData += $"\r\nGRADE UPDATE ERROR\n";
                    }
                }
            }
        }

        /// <summary>
        /// Write logData when a file is processed.
        /// </summary>
        /// <returns>capturedLogData</returns>
        public String WriteLogData()
        {
            //StreamWriter srLog = new StreamWriter("OutData.txt");
            StreamWriter srLog = new StreamWriter(logFileName, true);
            srLog.WriteLine(logData);
            srLog.Close();

            // Capture the logData in a local variable
            string capturedLogData = logData;

            // Set logData to an empty String 
            logData = string.Empty;

            // Set logFileName to an empty String
            logFileName = string.Empty;

            return capturedLogData;
        }

        /// <summary>
        /// Initiates the batch process by determining the appropriate filename and
        /// then proceeding with the header and detail processing.
        /// </summary>
        /// <param name="programAcronym">The acronym of the program.</param>
        public void ProcessTransmission(String programAcronym)
        {
            // Generate inputFileName
            DateTime currentDate = DateTime.Now;

            inputFileName = $"{currentDate.Year}-{currentDate.DayOfYear}-{programAcronym}.xml";

            // Generate logFileName
            logFileName = $"LOG {currentDate.Year}-{currentDate.DayOfYear:D3}-{programAcronym}.txt";

            try
            {
                // Check if the input file exists
                if (File.Exists(inputFileName))
                {

                    try
                    {
                        ProcessHeader();
                        ProcessDetails();
                    }
                    catch (Exception ex)
                    {
                        logData += "\r\nError: " + ex.Message;
                    }                  
                }
                else
                {
                    string errorMessage = $"File {inputFileName} does not exist.";
                    logData += "\r\nError: " + errorMessage;
                    MessageBox.Show(errorMessage, "Invalid File", MessageBoxButtons.OK);
                }
            }
            catch (Exception ex)
            {
                logData += $"Exception occurred:{ex.Message}\n";
            }
        }
    }
}
