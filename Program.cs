using System;

public class Class1
{
	public Class1()
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
        student.GradePointAverage = .44;
        student.GradePointStateId = 1; //Assuming SuspendedState has an Id of 1 (check the database)
        db.SaveChanges();

        //Get an instance of the student's state
        GradePointState state = db.GradePointStates.Find(student.GradePointStateId);

        //call the tuition rate adjustment
        double tuitionRate = 1000 * state.TuitionRateAdjustment(student);

        //Output the expected and actual values
        Console.WriteLine("Expected: 1150");
        Console.WriteLine("Actual: " + tuitionRate);
    }
}
