using BITCollege_SY.Data;
using BITCollege_SY.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.Text;
using Utility;

namespace BITCollegeService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "CollegeRegistration" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select CollegeRegistration.svc or CollegeRegistration.svc.cs at the Solution Explorer and start debugging.
    public class CollegeRegistration : ICollegeRegistration
    {
        private BITCollege_SYContext db = new BITCollege_SYContext();

        public void DoWork()
        {
        }

        /// <summary>
        /// Drop a course based on the given registration ID.
        /// </summary>
        /// <param name="registrationId">The ID of the course registration to be dropped</param>
        /// <returns>Return true if it is successfully dropped, otherwise false.</returns>
        public bool DropCourse(int registrationId)
        {
            try
            {
                Registration registrationRecord =
                    db.Registrations.SingleOrDefault(x => x.RegistrationId == registrationId);           

                if(registrationRecord == null)
                {
                    return false;
                }

                db.Registrations.Remove(registrationRecord);
                db.SaveChanges();

                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Registers a student for a course.
        /// </summary>
        /// <param name="studentId">The ID of student.</param>
        /// <param name="courseId">The ID of the course the student is registering for.</param>
        /// <param name="notes">Notes about the registration</param>
        /// <returns>The registration ID as an integer.</returns>
        public int RegisterCourse(int studentId, int courseId, string notes)
        {
            IQueryable<Registration> allRecord = 
                db.Registrations.Where(x => x.StudentId == studentId && x.CourseId == courseId);

            Course course = db.Courses.Find(courseId);

            Student student = db.Students.Find(studentId);

            IEnumerable<Registration> nullRecords = 
                allRecord.Where(x => x.Grade == null);

            int returnCode = 0;

            if (nullRecords.Count() > 0)
            {
                returnCode = -100;
            }

            if (BusinessRules.CourseTypeLookup(course.CourseType) == CourseType.MASTERY)
            {
                MasteryCourse masteryCourse = (MasteryCourse)course;

                IEnumerable<Registration> notNullRecords = allRecord.Where(x => x.Grade != null);

                if (notNullRecords.Count() >= masteryCourse.MaximumAttempts)
                {
                    returnCode = -200;
                }

            }

            if (returnCode == 0)
            {
                try
                {
                    Registration registration = new Registration();
                    registration.StudentId = studentId;
                    registration.CourseId = courseId;
                    registration.Notes = notes;
                    registration.Grade = null;
                    registration.RegistrationDate = DateTime.Now;
                    registration.SetNextRegistrationNumber();

                    db.Registrations.Add(registration);
                    db.SaveChanges();

                    student.OutstandingFees +=
                        course.TuitionAmount * student.GradePointState.TuitionRateAdjustment(student);
                   
                    return 0;
                }
                catch (Exception)
                {

                    return -300;
                }
            }

            return returnCode;
        }

        /// <summary>
        /// Updates the grade of a course registration.
        /// </summary>
        /// <param name="grade">The new grade to be assgined.</param>
        /// <param name="registrationId">The ID of the course the student is registering for.</param>
        /// <param name="notes">Notes about the registration</param>
        /// <returns>The updated grade as a nullable if the operation is sucessful.</returns>
        public double? UpdateGrade(double grade, int registrationId, string notes)
        {
            try
            {
                Registration RetrievedRecord =
                        db.Registrations.SingleOrDefault(x => x.RegistrationId == registrationId);

                if(RetrievedRecord == null)
                {
                    return null;
                }

                RetrievedRecord.Grade = grade;
                RetrievedRecord.Notes = notes;

                db.SaveChanges();

                double? updatedResult = CalculateGradePointAverage(RetrievedRecord.StudentId);

                return updatedResult;
            }
            catch (Exception)
            {

                return null;
            }
        }

        /// <summary>
        /// Calculate grade point average given a student ID
        /// </summary>
        /// <param name="studentId">The ID of the student</param>
        /// <returns>Calculated grade point average.</returns>
        private double? CalculateGradePointAverage(int studentId)
        {
            double totalCreditHours = 0;
            double totalGradePointValue = 0;
            double? calculatedGradePointAverage = null;

            IQueryable<Registration> allRegistration = 
                db.Registrations.Where(x => x.Grade != null && x.StudentId == studentId);


            foreach (Registration registration in allRegistration.ToList())
            {
                double grade = registration.Grade.Value;
                CourseType courseType = BusinessRules.CourseTypeLookup(registration.Course.CourseType);


                if (courseType == CourseType.AUDIT)
                {
                    continue;
                }

                double gradePoint = BusinessRules.GradeLookup(grade, courseType);
                double gradePointValue = gradePoint * registration.Course.CreditHours;

                totalGradePointValue += gradePointValue;
                totalCreditHours += registration.Course.CreditHours;
            }

            if(totalCreditHours == 0)
            {
                calculatedGradePointAverage = null;
            }
            else
            {
                calculatedGradePointAverage = totalGradePointValue / totalCreditHours;

                Student student = db.Students.SingleOrDefault(x => x.StudentId == studentId);

                if (student != null)
                {
                    student.GradePointAverage = calculatedGradePointAverage;
                    student.GradePointState.StateChangeCheck(student);
                    db.SaveChanges();
                }
   
            }
            return calculatedGradePointAverage;
        }
    }
}
