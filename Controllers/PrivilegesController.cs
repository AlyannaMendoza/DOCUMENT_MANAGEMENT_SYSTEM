using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DMS_v2.Models;

namespace DMS_v2.Controllers
{
    public class PrivilegesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Privileges
        public ActionResult Index()
        {
            var privileges = db.Privileges.Include(p => p.Project);
            return View(privileges.ToList());
        }

        // GET: Privileges/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Privilege privilege = db.Privileges.Find(id);
            if (privilege == null)
            {
                return HttpNotFound();
            }
            return View(privilege);
        }

        // GET: Privileges/Create
        public ActionResult Create()
        {
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "ProjectName");
            return View();
        }

        // POST: Privileges/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,PrivilegeName,ProjectId,Status")] Privilege privilege)
        {
            if (ModelState.IsValid)
            {
                db.Privileges.Add(privilege);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "ProjectName", privilege.ProjectId);
            return View(privilege);
        }

        // GET: Privileges/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Privilege privilege = db.Privileges.Find(id);
            if (privilege == null)
            {
                return HttpNotFound();
            }
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "ProjectName", privilege.ProjectId);
            return View(privilege);
        }

        // POST: Privileges/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,PrivilegeName,ProjectId,Status")] Privilege privilege)
        {
            if (ModelState.IsValid)
            {
                db.Entry(privilege).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "ProjectName", privilege.ProjectId);
            return View(privilege);
        }

        // GET: Privileges/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Privilege privilege = db.Privileges.Find(id);
            if (privilege == null)
            {
                return HttpNotFound();
            }
            return View(privilege);
        }

        // POST: Privileges/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Privilege privilege = db.Privileges.Find(id);
            db.Privileges.Remove(privilege);
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
