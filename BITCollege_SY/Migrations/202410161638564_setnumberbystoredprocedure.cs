﻿namespace BITCollege_SY.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class setnumberbystoredprocedure : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Courses", "CourseNumber", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Courses", "CourseNumber", c => c.String(nullable: false));
        }
    }
}
