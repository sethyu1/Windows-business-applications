using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BITCollege_SY.Data;
using BITCollege_SY.Models;

namespace BITCollege_SY.Controllers
{
    public class GradeCoursesController : Controller
    {
        private BITCollege_SYContext db = new BITCollege_SYContext();

        // GET: GradeCourses
        public ActionResult Index()
        {
            var gradeCourse = db.GradeCourses.Include(g => g.AcademicProgram);
            return View(gradeCourse.ToList());
        }

        // GET: GradeCourses/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GradeCourse gradeCourse = db.GradeCourses.Find(id);
            if (gradeCourse == null)
            {
                return HttpNotFound();
            }
            return View(gradeCourse);
        }

        // GET: GradeCourses/Create
        public ActionResult Create()
        {
            ViewBag.AcademicProgramId = new SelectList(db.AcademicPrograms, "AcademicProgramId", "ProgramAcronym");
            return View();
        }

        // POST: GradeCourses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CourseId,AcademicProgramId,CourseNumber,Title,CreditHours,TuitionAmount,Notes,AssignmentWeight,ExamWeight")] GradeCourse gradeCourse)
        {
            if (ModelState.IsValid)
            {
                gradeCourse.SetNextCourseNumber();
                db.Courses.Add(gradeCourse);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AcademicProgramId = new SelectList(db.AcademicPrograms, "AcademicProgramId", "ProgramAcronym", gradeCourse.AcademicProgramId);
            return View(gradeCourse);
        }

        // GET: GradeCourses/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GradeCourse gradeCourse = db.GradeCourses.Find(id);
            if (gradeCourse == null)
            {
                return HttpNotFound();
            }
            ViewBag.AcademicProgramId = new SelectList(db.AcademicPrograms, "AcademicProgramId", "ProgramAcronym", gradeCourse.AcademicProgramId);
            return View(gradeCourse);
        }

        // POST: GradeCourses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CourseId,AcademicProgramId,CourseNumber,Title,CreditHours,TuitionAmount,Notes,AssignmentWeight,ExamWeight")] GradeCourse gradeCourse)
        {
            if (ModelState.IsValid)
            {
                db.Entry(gradeCourse).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AcademicProgramId = new SelectList(db.AcademicPrograms, "AcademicProgramId", "ProgramAcronym", gradeCourse.AcademicProgramId);
            return View(gradeCourse);
        }

        // GET: GradeCourses/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            GradeCourse gradeCourse = db.GradeCourses.Find(id);
            if (gradeCourse == null)
            {
                return HttpNotFound();
            }
            return View(gradeCourse);
        }

        // POST: GradeCourses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            GradeCourse gradeCourse = db.GradeCourses.Find(id);
            db.Courses.Remove(gradeCourse);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
