using BITCollege_SY.Data;
using BITCollege_SY.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    
    internal class Program
    {
        private static BITCollege_SYContext db = new BITCollege_SYContext();

        static void Main(string[] args)
        {
            //*******************************************************************
            //This unit test will test the first
            //scenario for a Suspended State Student. This test can be 
            //copied and modified for the remaining 9 tests.

            //Note that additional registration records will need to be inserted
            //when testing Probation and Honours States TuitionRateAdjustment.

            //Feel free to add extra records (Students, etc.) 
            //for testing to the database. 
            //*******************************************************************

            //Set up test Student
            Student student = db.Students.Find(1); //First student in the database
            student.GradePointAverage = 1.15;
            // Add 5 registration records for the student (with non-null grades)
            for (int i = 1; i <= 5; i++)
            {
                db.Registrations.Add(new Registration
                {
                    StudentId = student.StudentId,
                    Grade = 80 // Assign a non-null grade
                });
            }
            // Commit registration records to the database
            student.GradePointStateId = 1; //Assuming SuspendedState has an Id of 1 (check the database)
            db.SaveChanges();

            //Get an instance of the student's state
            GradePointState state = db.GradePointStates.Find(student.GradePointStateId);

            //call the tuition rate adjustment
            double tuitionRate = 1000 * state.TuitionRateAdjustment(student);

            //Output the expected and actual values
            Console.WriteLine("Expected: 1035");
            Console.WriteLine("Actual: " + tuitionRate);
        }
    }
}
