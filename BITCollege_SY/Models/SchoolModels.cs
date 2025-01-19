using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Utility;
using BITCollege_SY.Data;
using System.Web.Mvc;
using System.Security.Policy;
using static BITCollege_SY.Models.NextRegistration;
using System.Data.SqlClient;
using System.Data;
using BITCollege_SY.Migrations;

namespace BITCollege_SY.Models
{
    /// <summary>
    /// Student Model. Represents the Students table in the database.
    /// </summary>
    public class Student
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int StudentId { get; set; }

        [Required]
        [ForeignKey("GradePointState")]
        public int GradePointStateId { get; set; }

        [ForeignKey("AcademicProgram")]
        public int? AcademicProgramId { get; set; }  // nullable

        [Display(Name = "Student\nNumber")]
        public long StudentNumber { get; set; }

        [Required]
        [StringLength(35, MinimumLength = 1)]
        [Display(Name = "First\nName")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(35, MinimumLength = 1)]
        [Display(Name = "Last\nName")]
        public string LastName { get; set; }

        [Required]
        [StringLength(300)]
        public string Address { get; set; }

        [Required]
        [StringLength(300)]
        public string City { get; set; }

        [Required]
        [StringLength(2)]
        [RegularExpression("^(N[BLSTU]|[AMN]B|[BQ]C|ON|PE|SK|YT)",
            ErrorMessage = "Invalid province code. Please enter a valid Canadian province code.")]
        public string Province { get; set; }

        [Required]
        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")] // Ensures only the date part is displayed
        public DateTime DateCreated { get; set; }

        [Display(Name = "Grade Point\nAverage")]
        [DisplayFormat(DataFormatString = "{0:f2}")]
        [Range(0, 4.5)]
        public double? GradePointAverage { get; set; }   // nullable

        [Required]
        [Display(Name = "Fees")]
        [DisplayFormat(DataFormatString = "{0:C2}")]
        public double OutstandingFees { get; set; }

        [Display(Name = "Notes")]
        public string Notes { get; set; }   // no additional requirements

        [Display(Name = "Name")]
        public string FullName
        { 
            get
            {
                return String.Format("{0} {1}", FirstName, LastName);
            }
        }

        [Display(Name = "Address")]
        public string FullAddress 
        { 
            get
            {
                return String.Format("{0} {1}, {2}", Address, City, Province);
            }
        }

        // navagational property 
        // represent a 1 or a 0-1 cardinality
        public virtual GradePointState GradePointState { get; set; }

        // navagational property
        // represent a 1 or a 0-1 cardinality
        public virtual AcademicProgram AcademicProgram { get; set; }

        // navagational property
        // represent 0..* cardinality
        public virtual ICollection<Registration> Registration { get; set; }

        /// <summary>
        /// change state to the correct state associated with student
        /// </summary>
        public void ChangeState()
        {
            // step 1 : get and instance of the context object
            BITCollege_SYContext db = new BITCollege_SYContext();

            // step 2 :get an instance of GradePointState
            GradePointState currentState = db.GradePointStates.Find(GradePointStateId);

            // step 3: set up value for tracking
            int previousID = 0;

            // step 4: the loop (while)
            while (previousID != currentState.GradePointStateId)
            {
                currentState.StateChangeCheck(this);
                previousID = currentState.GradePointStateId;
                currentState = db.GradePointStates.Find(GradePointStateId);
            }
        }

        /// <summary>
        /// Set the next student number
        /// </summary>
        public void SetNextStudentNumber()
        {
            String discriminator = "NextStudent";

            // get the nextNumber from nextNumber method
            long? nextNumber = StoredProcedures.NextNumber(discriminator);

            // set the next course number
            if (nextNumber != null)
            {
                StudentNumber = (long)nextNumber;
            }
            else
            {
                throw new Exception("Unable to generate next student number");
            }
        }
    }

    /// <summary>
    /// GradePointState Model. Represents the GradePointStates in the database.
    /// </summary>
    public abstract class GradePointState
    {
        protected static BITCollege_SYContext db = new BITCollege_SYContext();

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int GradePointStateId { get; set; }

        [Required]
        [Display(Name = "Lower\nLimit")]
        [DisplayFormat(DataFormatString = "{0:f2}")]
        public double LowerLimit { get; set; }

        [Required]
        [Display(Name = "Upper\nLimit")]
        [DisplayFormat(DataFormatString = "{0:f2}")]
        public double UpperLimit { get; set; }

        [Required]
        [Display(Name = "Tuition\nRate\nFactor")]
        [DisplayFormat(DataFormatString = "{0:f2}")]
        public double TuitionRateFactor { get; set; }

        [Display(Name = "State")]
        public string Description 
        { 
            get
            {
                return GetType().Name.Substring(0, GetType().Name.LastIndexOf("State"));
            }
        }

        // navagational property
        // represent 0..* cardinality
        public virtual ICollection<Student> Student { get; set; }

        /// <summary>
        /// A blueprint for state change check.
        /// </summary>
        /// <param name="student">student object</param>
        public abstract void StateChangeCheck(Student student);

        /// <summary>
        /// A blueprint for tuition rate adjustment.
        /// </summary>
        /// <param name="student"></param>
        /// <returns>the adjusted tuition amount</returns>
        public abstract double TuitionRateAdjustment(Student student);

    }

    /// <summary>
    /// derived class from GradePointState
    /// </summary>
    public class SuspendedState : GradePointState 
    {
        private static SuspendedState suspendedState;

        /// <summary>
        /// set the default value in constructor.
        /// </summary>
        private SuspendedState()
        {
            LowerLimit = 0.00;
            UpperLimit = 1.00;
            TuitionRateFactor = 1.1;
        }

        /// <summary>
        /// Get instance of suspendedstate
        /// </summary>
        /// <returns>Populated suspendedState</returns>
        public static SuspendedState GetInstance()
        {
            if (suspendedState == null)
            {
                suspendedState = db.SuspendedStates.SingleOrDefault();

                if(suspendedState == null)
                {
                    suspendedState = new SuspendedState();
                    db.SuspendedStates.Add(suspendedState);
                    db.SaveChanges();
                }
            }
            
            return suspendedState;
        }

        /// <summary>
        /// evaluate student grade for change.
        /// </summary>
        /// <param name="student">student object</param>
        public override void StateChangeCheck(Student student)
        {
            if (student.GradePointAverage > 1)
            {
                student.GradePointStateId = ProbationState.GetInstance().GradePointStateId;
            }
        }

        /// <summary>
        /// Adjust the tuitioin based on GPA
        /// </summary>
        /// <param name="student">student object</param>
        /// <returns>adjusted tuition rate</returns>
        public override double TuitionRateAdjustment(Student student)
        {
            // use a local variable
            double adjustedRate = TuitionRateFactor;

            if (student.GradePointAverage <0.5)
            {
                adjustedRate += 0.05;
            }

            else if (student.GradePointAverage < 0.75)
            {
                adjustedRate += 0.02;
            }

            return adjustedRate;
        }
    }

    /// <summary>
    /// derived class from GradePointState
    /// </summary>
    public class ProbationState : GradePointState
    {
        private static ProbationState probationState;

        /// <summary>
        /// set the default value in constructor.
        /// </summary>
        private ProbationState()
        {
            LowerLimit = 1.00;
            UpperLimit = 2.00;
            TuitionRateFactor = 1.075;
        }

        /// <summary>
        /// Gets instance of probation state
        /// </summary>
        /// <returns>populated instance</returns>
        public static ProbationState GetInstance()
        {
            if(probationState == null)
            {
                probationState = db.ProbationStates.SingleOrDefault();

                if(probationState == null)
                {
                    probationState = new ProbationState();
                    db.ProbationStates.Add(probationState);
                    db.SaveChanges();
                }
            }

            return probationState;
        }

        /// <summary>
        /// Evaluates the students GPA for change
        /// </summary>
        /// <param name="student">student object</param>
        public override void StateChangeCheck(Student student)
        {
            if(student.GradePointAverage < 1)
            {
                student.GradePointStateId = SuspendedState.GetInstance().GradePointStateId;
            }

            if (student.GradePointAverage > 2)
            {
                student.GradePointStateId = RegularState.GetInstance().GradePointStateId;
            }
        }
        
        /// <summary>
        ///  Adjusts the tuition based on GPA
        /// </summary>
        /// <param name="student"> student object</param>
        /// <returns> the adjusted tuition rate</returns>
        public override double TuitionRateAdjustment(Student student)
        {
            // use a local variable
            double adjustedRate = TuitionRateFactor;

            IQueryable<Registration> registrationCount = 
                db.Registrations.Where (x => x.StudentId == student.StudentId && x.Grade != null);

            if(registrationCount.Count() >= 5 )
            {
                adjustedRate -= .04;
            }
            return adjustedRate;
        }
    }

    /// <summary>
    /// derived class from GradePointState
    /// </summary>
    public class RegularState : GradePointState
    {
        private static RegularState regularState;

        /// <summary>
        /// set the default value in constructor.
        /// </summary>
        private RegularState()
        {
            LowerLimit = 2.00;
            UpperLimit = 3.70;
            TuitionRateFactor = 1.0;
        }

        /// <summary>
        /// Get instance of regularState
        /// </summary>
        /// <returns>Populated regularState</returns>
        public static RegularState GetInstance()
        {
            if (regularState == null)
            {
                regularState = db.RegularStates.SingleOrDefault();

                if (regularState == null)
                {
                    regularState = new RegularState();
                    db.RegularStates.Add(regularState);
                    db.SaveChanges();
                }
            }

            return regularState;
        }

        /// <summary>
        /// evluate student for a change
        /// </summary>
        /// <param name="student">student object</param>
        public override void StateChangeCheck(Student student)
        {
            if (student.GradePointAverage < 2)
            {
                student.GradePointStateId = ProbationState.GetInstance().GradePointStateId;
            }

            if (student.GradePointAverage > 3.7)
            {
                student.GradePointStateId = HonoursState.GetInstance().GradePointStateId;
            }
        }

        /// <summary>
        /// return the tuition rate for regular state.
        /// </summary>
        /// <param name="student">student object</param>
        /// <returns>the unchanged tuition rate</returns>
        public override double TuitionRateAdjustment(Student student)
        {
            return TuitionRateFactor;
        }
    }

    /// <summary>
    /// derived class from GradePointState
    /// </summary>
    public class HonoursState : GradePointState
    {
        private static HonoursState honoursState;

        /// <summary>
        /// set the default value in constructor.
        /// </summary>
        private HonoursState()
        {
            LowerLimit = 3.70;
            UpperLimit = 4.50;
            TuitionRateFactor = 0.90;
        }

        /// <summary>
        /// Get instance of HonoursState
        /// </summary>
        /// <returns>populated honoursState</returns>
        public static HonoursState GetInstance()
        {
            if(honoursState == null)
            {
                honoursState = db.HonnoursStates.SingleOrDefault();

                if (honoursState == null)
                {
                    honoursState = new HonoursState();
                    db.HonnoursStates.Add(honoursState);
                    db.SaveChanges();
                }
            }

            return honoursState;
        }

        /// <summary>
        /// evalute student for a change
        /// </summary>
        /// <param name="student">student object</param>
        public override void StateChangeCheck(Student student)
        {
            if (student.GradePointAverage < 3.7)
            {
                student.GradePointStateId = RegularState.GetInstance().GradePointStateId;
            }
        }

        /// <summary>
        /// adjust the tuition rate based on GPA and completed courses
        /// </summary>
        /// <param name="student">student object</param>
        /// <returns>adjusted tuition rate</returns>
        public override double TuitionRateAdjustment(Student student)
        {
            // use a local variable
            double adjustedRate = TuitionRateFactor;

            IQueryable<Registration> registrationCount =
                db.Registrations.Where(x => x.StudentId == student.StudentId && x.Grade != null);

            if (registrationCount.Count() >= 5)
            {
                adjustedRate -= .05;
            }

            if (student.GradePointAverage > 4.25)
            {
                adjustedRate -= .02;
            }
            return adjustedRate;
        }
    }

    /// <summary>
    /// AcademicProgram Model. Represents the AcademicProgram in the database.
    /// </summary>
    public class AcademicProgram
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int AcademicProgramId { get; set; }

        [Required]
        [Display(Name = "Program")]
        public string ProgramAcronym { get; set; }

        [Required]
        [Display(Name = "Program\nName")]
        public string Description { get; set; }

        // navagational property
        // represent 0..* cardinality
        public virtual ICollection<Student> Student { get; set; }

        // navagational property
        // represnet 0..* cardinality
        public virtual ICollection<Course> Course { get; set; }
    }

    /// <summary>
    /// Course Model. Represents the Course in the database.
    /// </summary>
    public abstract class Course
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int CourseId { get; set; }

        [ForeignKey("AcademicProgram")]
        public int? AcademicProgramId { get; set; } // nullable

        [Display(Name = "Course\nNumber")]
        public string CourseNumber { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:f2}")]
        [Display(Name = "Credit\nHours")]
        public double CreditHours { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:c2}")]
        [Display(Name = "Tuition")]
        public double TuitionAmount { get; set; }

        [Display(Name = "Course\nType")]
        public string CourseType 
        { 
            get
            {
                return GetType().Name.Substring(0, GetType().Name.LastIndexOf("Course"));
            }
        }

        public string Notes { get; set; }

        /// <summary>
        /// A blueprint for setting next course number
        /// </summary>
        /// <returns>next course number</returns>
        public abstract void SetNextCourseNumber();

        // navagational properties
        // represents 0..1 cardinality
        public virtual AcademicProgram AcademicProgram { get; set; }

        // navagational properites
        // represents 0..* cardinality
        public virtual ICollection<Registration> Registration { get; set; }
    }

    /// <summary>
    /// derived class from Course
    /// </summary>
    public class GradeCourse : Course
    {
        [Required]
        [Display(Name = "Assignments")]
        [DisplayFormat(DataFormatString = "{0:P2}", ApplyFormatInEditMode = true)]
        public double AssignmentWeight { get; set; }

        [Required]
        [Display(Name = "Exams")]
        [DisplayFormat(DataFormatString = "{0:P2}", ApplyFormatInEditMode = true)]
        public double ExamWeight { get; set; }

        /// <summary>
        /// Set the next course Number.
        /// </summary>
        public override void SetNextCourseNumber()
        {
            String discriminator = "NextGradeCourse";

            // get the nextNumber from nextNumber method
            long? nextNumber = StoredProcedures.NextNumber(discriminator);

            // set the next course number
            CourseNumber = String.Format("G-{0:D6}", nextNumber);
        }
    }

    /// <summary>
    /// derived class from Course
    /// </summary>
    public class MasteryCourse : Course
    {
        [Required]
        [Display(Name = "Maximum\nAttempts")]
        public int MaximumAttempts { get; set; }

        /// <summary>
        /// Set the next courseNumber.
        /// </summary>
        public override void SetNextCourseNumber()
        {
            String discriminator = "NextMasteryCourse";

            // get the nextNumber from nextNumber method
            long? nextNumber = StoredProcedures.NextNumber(discriminator);

            // set the next course number
            CourseNumber = String.Format("M-{0:D5}", nextNumber);
        }
    }

    /// <summary>
    /// derived class from Course
    /// </summary>
    public class AuditCourse : Course
    {
        /// <summary>
        /// Set the next courseNumber.
        /// </summary>
        public override void SetNextCourseNumber()
        {
            String discriminator = "NextAuditCourse";

            // get the nextNumber from nextNumber method
            long? nextNumber = StoredProcedures.NextNumber(discriminator);

            // set the next course number
            CourseNumber = String.Format("A-{0:D4}", nextNumber);
        }
    }

    /// <summary>
    /// Registation Model. Represents the Registration in the database.
    /// </summary>
    public class Registration
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int RegistrationId { get; set; }

        [Required]
        [ForeignKey("Student")]
        public int StudentId { get; set; }

        [Required]
        [ForeignKey("Course")]
        public int CourseId { get; set; }

        [Display(Name = "Registration\nNumber")]
        public long RegistrationNumber { get; set; }

        [Required]
        [Display(Name = "Date")]
        [DisplayFormat(DataFormatString = "{0:d}")]
        public DateTime RegistrationDate { get; set; }

        [Range(0, 1)]
        [DisplayFormat(NullDisplayText = "Ungraded")]
        public double? Grade { get; set; }

        public string Notes { get; set; }

        /// <summary>
        /// Set next Registration Number
        /// </summary>
        public void SetNextRegistrationNumber()
        {
            String discriminator = "NextRegistration";

            // get the nextNumber from nextNumber method
            long? nextNumber = StoredProcedures.NextNumber(discriminator);

            // set the next Registration number
            RegistrationNumber = (long)nextNumber;
        }

        // navagational properties
        // represents 1 cardinality
        public virtual Course Course { get; set; }

        // navagational properties
        // represents 1 cardinality
        public virtual Student Student { get; set; }
    }

    /// <summary>
    /// NextUniqueNumber model, represents the next unique number
    /// </summary>
    public abstract class NextUniqueNumber
    {
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Key]
        [Required]
        public int NextUniqueNumberId { get; set; }

        [Required]
        public long NextAvailableNumber { get; set; }

        protected static BITCollege_SYContext db = new BITCollege_SYContext();
    }

    /// <summary>
    /// NextStudent model, represents the next student.
    /// </summary>
    public class NextStudent : NextUniqueNumber
    {
        private static NextStudent nextStudent;

        /// <summary>
        /// set the default value in constructor.
        /// </summary>
        private NextStudent()
        {
            NextAvailableNumber = 20000000;
        }

        /// <summary>
        /// Get instance of nextStudent
        /// </summary>
        /// <returns>populated nextStudent</returns>
        public static NextStudent GetInstance()
        {
            if (nextStudent == null)
            {
                nextStudent = db.NextStudents.SingleOrDefault();

                if (nextStudent == null)
                {
                    nextStudent = new NextStudent();
                    db.NextStudents.Add(nextStudent);
                    db.SaveChanges();
                }         
            }

            return nextStudent;
        }
    }

    /// <summary>
    /// NextRegistration model, represents the next registration
    /// </summary>
    public class NextRegistration : NextUniqueNumber
    {
        private static NextRegistration nextRegistration;

        /// <summary>
        /// set the default value in constructor.
        /// </summary>
        private NextRegistration()
        {
             NextAvailableNumber = 700;
        }

        /// <summary>
        /// Get instance of nextRegistration
        /// </summary>
        /// <returns>populated nextRegistration</returns>
        public static NextRegistration GetInstance()
        {
            if (nextRegistration == null)
            {
                nextRegistration = db.NextRegistrations.SingleOrDefault();

                if (nextRegistration == null)
                {
                    nextRegistration = new NextRegistration();
                    db.NextRegistrations.Add(nextRegistration);
                    db.SaveChanges();
                }
            }

            return nextRegistration;
        }
    }

    /// <summary>
    /// NextGradeCourse model, represents the nextGradeCourse
    /// </summary>
    public class NextGradeCourse : NextUniqueNumber
    {
        private static NextGradeCourse nextGradeCourse;

        /// <summary>
        /// set the default value in constructor.
        /// </summary>
        private NextGradeCourse()
        {
            NextAvailableNumber = 200000;
        }

        /// <summary>
        /// Get instance of nextGradeCourse
        /// </summary>
        /// <returns>populated nextRegistration</returns>
        public static NextGradeCourse GetInstance()
        {
            if (nextGradeCourse == null)
            {
                nextGradeCourse = db.NextGradeCourses.SingleOrDefault();

                if (nextGradeCourse == null)
                {
                    nextGradeCourse = new NextGradeCourse();
                    db.NextGradeCourses.Add(nextGradeCourse);
                    db.SaveChanges();
                }
            }

            return nextGradeCourse;
        }
    }

    /// <summary>
    /// NextAuditCourse model, represents nextAuditCourse
    /// </summary>
    public class NextAuditCourse : NextUniqueNumber
    {
        private static NextAuditCourse nextAuditCourse;

        /// <summary>
        /// set the default value in constructor.
        /// </summary>
        private NextAuditCourse()
        {
            NextAvailableNumber = 2000;
        }

        /// <summary>
        /// Get the instance of nextAuditCourse
        /// </summary>
        /// <returns>populated nextAuditCourse</returns>
        public static NextAuditCourse GetInstance()
        {
            if (nextAuditCourse == null)
            {
                nextAuditCourse = db.NextAuditCourses.SingleOrDefault();

                if (nextAuditCourse == null)
                {
                    nextAuditCourse = new NextAuditCourse();
                    db.NextAuditCourses.Add(nextAuditCourse);
                    db.SaveChanges();
                }
            }

            return nextAuditCourse;
        }
    }

    /// <summary>
    /// NextMasteryCourse model, represents nextMasteryCourse
    /// </summary>
    public class NextMasteryCourse : NextUniqueNumber
    {
        private static NextMasteryCourse nextMasteryCourse;

        /// <summary>
        /// Set the default value in constructor
        /// </summary>
        private NextMasteryCourse()
        {
            NextAvailableNumber = 20000;
        }

        /// <summary>
        /// Get the instance of nextMasteryCourse
        /// </summary>
        /// <returns>populated nextMasteryCourse</returns>
        public static NextMasteryCourse GetInstance()
        {
            if (nextMasteryCourse == null)
            {
                nextMasteryCourse = db.NextMasteryCourses.SingleOrDefault();

                if (nextMasteryCourse == null)
                {
                    nextMasteryCourse = new NextMasteryCourse();
                    db.NextMasteryCourses.Add(nextMasteryCourse);
                    db.SaveChanges();
                }
            }

            return nextMasteryCourse;
        }
    }

    /// <summary>
    /// StoredProcedure model, represents a stored procedure in database.
    /// </summary>
    public static class StoredProcedures
    {
        /// <summary>
        /// Get the next avaviable number 
        /// </summary>
        /// <param name="discriminator"></param>
        /// <returns>next avaviable number</returns>
        public static long? NextNumber(string discriminator)
        {
            try
            {
                long? returnValue = 0;

                // Connection to database
                using (SqlConnection connection = new SqlConnection("Data Source=SETHCANADA\\GINSENG; " +
                "Initial Catalog=BITCollege_SYContext;Integrated Security=True"))
                {
                    // Excute storedProcedure
                    SqlCommand storedProcedure = new SqlCommand("next_number", connection);
                    storedProcedure.CommandType = CommandType.StoredProcedure;

                    // Add Parameters
                    storedProcedure.Parameters.AddWithValue("@Discriminator", discriminator);

                    // Get Output
                    SqlParameter outputParameter = new SqlParameter("@NewVal", SqlDbType.BigInt)
                    {
                        Direction = ParameterDirection.Output
                    };

                    storedProcedure.Parameters.Add(outputParameter);

                    connection.Open();

                    storedProcedure.ExecuteNonQuery();

                    connection.Close();

                    returnValue = (long?)outputParameter.Value;
                }

                return returnValue;
            }

            catch(Exception)
            {
                return null;
            }
        }
    }
} 