using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace BITCollegeService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ICollegeRegistration" in both code and config file together.
    [ServiceContract]
    public interface ICollegeRegistration
    {
        [OperationContract]
        void DoWork();

        /// <summary>
        /// Drop a course based on the given registration ID.
        /// </summary>
        /// <param name="registrationId">The ID of the course registration to be dropped</param>
        /// <returns>Return true if it is successfully dropped, otherwise false.</returns>
        [OperationContract]
        bool DropCourse(int registrationId);

        /// <summary>
        /// Registers a sstudent for a course.
        /// </summary>
        /// <param name="studentId">The ID of student.</param>
        /// <param name="courseId">The ID of the course the student is registering for.</param>
        /// <param name="notes">Notes about the registration</param>
        /// <returns>The registration ID as an integer.</returns>
        [OperationContract]
        int RegisterCourse(int studentId, int courseId, string notes);

        /// <summary>
        /// Updates the grade of a course registration.
        /// </summary>
        /// <param name="grade">The new grade to be assgined.</param>
        /// <param name="registrationId">The ID of the course the student is registering for.</param>
        /// <param name="notes">Notes about the registration</param>
        /// <returns>The updated grade as a nullable if the operation is sucessful.</returns>
        [OperationContract]
        double? UpdateGrade(double grade, int registrationId, string notes);
    }
}
