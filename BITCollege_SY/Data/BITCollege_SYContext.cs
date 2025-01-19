using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace BITCollege_SY.Data
{
    public class BITCollege_SYContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public BITCollege_SYContext() : base("name=BITCollege_SYContext")
        {
        }

        public System.Data.Entity.DbSet<BITCollege_SY.Models.Student> Students { get; set; }

        public System.Data.Entity.DbSet<BITCollege_SY.Models.AcademicProgram> AcademicPrograms { get; set; }

        public System.Data.Entity.DbSet<BITCollege_SY.Models.GradePointState> GradePointStates { get; set; }

        public System.Data.Entity.DbSet<BITCollege_SY.Models.Course> Courses { get; set; }

        public System.Data.Entity.DbSet<BITCollege_SY.Models.Registration> Registrations { get; set; }

        public System.Data.Entity.DbSet<BITCollege_SY.Models.AuditCourse> AuditCourses { get; set; }

        public System.Data.Entity.DbSet<BITCollege_SY.Models.GradeCourse> GradeCourses { get; set; }

        public System.Data.Entity.DbSet<BITCollege_SY.Models.HonoursState> HonnoursStates { get; set; }

        public System.Data.Entity.DbSet<BITCollege_SY.Models.MasteryCourse> MasteryCourses { get; set; }

        public System.Data.Entity.DbSet<BITCollege_SY.Models.ProbationState> ProbationStates { get; set; }

        public System.Data.Entity.DbSet<BITCollege_SY.Models.RegularState> RegularStates { get; set; }

        public System.Data.Entity.DbSet<BITCollege_SY.Models.SuspendedState> SuspendedStates { get; set; }

        public System.Data.Entity.DbSet<BITCollege_SY.Models.NextUniqueNumber> NextUniqueNumbers { get; set; }

        public System.Data.Entity.DbSet<BITCollege_SY.Models.NextAuditCourse> NextAuditCourses { get; set; }

        public System.Data.Entity.DbSet<BITCollege_SY.Models.NextGradeCourse> NextGradeCourses { get; set; }

        public System.Data.Entity.DbSet<BITCollege_SY.Models.NextMasteryCourse> NextMasteryCourses { get; set; }

        public System.Data.Entity.DbSet<BITCollege_SY.Models.NextRegistration> NextRegistrations { get; set; }

        public System.Data.Entity.DbSet<BITCollege_SY.Models.NextStudent> NextStudents { get; set; }
    }
}
