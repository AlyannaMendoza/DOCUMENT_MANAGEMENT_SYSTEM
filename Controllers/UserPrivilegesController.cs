using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DMS_v2.Models;

namespace BED_TRACKER_SAMPLE.Controllers
{
    // [AuthorizeUser(AccessLevel = new string[] { "Administrator - Dengvaxia Profile" }, accessType = "administrator")]
    public class UserPrivilegesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: UserPrivileges
        public ActionResult Index()
        {
            var userPrivileges = db.Registrations.Where(i => i.IsActive == true);
            return View(userPrivileges.ToList());
        }

        // GET: UserPrivileges/Details/5
        public ActionResult Details(int? id)
        {
            var model = new UserPrivilegeDetailsViewModel();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            model.Registration_Info = db.Registrations.Find(id);
            model.UserPrivilege_List = db.UserPrivileges.Where(i => i.RegistrationId == id).ToList();



            ViewBag.UserPrivilege = new SelectList(db.Privileges.Where(i => i.PrivilegeName != "Administrator" && i.PrivilegeName != "Admin" && i.PrivilegeName != "Standard").OrderBy(i => i.PrivilegeName), "Id", "PrivilegeName");
            return View(model);
        }

        // GET: UserPrivileges/Create
        public ActionResult Create()
        {
            ViewBag.PrivilegeId = new SelectList(db.Privileges, "Id", "PrivilegeName");
            ViewBag.RegistrationId = new SelectList(db.Registrations, "Id", "FirstName");
            return View();
        }

        // POST: UserPrivileges/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,RegistrationId,PrivilegeId")] UserPrivilege userPrivilege)
        {
            if (ModelState.IsValid)
            {
                db.UserPrivileges.Add(userPrivilege);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.PrivilegeId = new SelectList(db.Privileges, "Id", "PrivilegeName", userPrivilege.PrivilegeId);
            ViewBag.RegistrationId = new SelectList(db.Registrations, "Id", "FirstName", userPrivilege.RegistrationId);
            return View(userPrivilege);
        }

        // GET: UserPrivileges/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserPrivilege userPrivilege = db.UserPrivileges.Find(id);
            if (userPrivilege == null)
            {
                return HttpNotFound();
            }
            ViewBag.PrivilegeId = new SelectList(db.Privileges, "Id", "PrivilegeName", userPrivilege.PrivilegeId);
            ViewBag.RegistrationId = new SelectList(db.Registrations, "Id", "FirstName", userPrivilege.RegistrationId);
            return View(userPrivilege);
        }

        // POST: UserPrivileges/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,RegistrationId,PrivilegeId")] UserPrivilege userPrivilege)
        {
            if (ModelState.IsValid)
            {
                db.Entry(userPrivilege).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.PrivilegeId = new SelectList(db.Privileges, "Id", "PrivilegeName", userPrivilege.PrivilegeId);
            ViewBag.RegistrationId = new SelectList(db.Registrations, "Id", "FirstName", userPrivilege.RegistrationId);
            return View(userPrivilege);
        }

        // GET: UserPrivileges/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserPrivilege userPrivilege = db.UserPrivileges.Find(id);
            if (userPrivilege == null)
            {
                return HttpNotFound();
            }
            return View(userPrivilege);
        }

        // POST: UserPrivileges/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UserPrivilege userPrivilege = db.UserPrivileges.Find(id);
            db.UserPrivileges.Remove(userPrivilege);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        public JsonResult AddPrivilege(int regId, int PrivilegeId)
        {
            if (db.UserPrivileges.Where(i => i.RegistrationId == regId && i.PrivilegeId == PrivilegeId).Count() > 0)
            {
                var ErrorMessage = "The privilege that you are adding is already added to this user!";
                return Json(new { isItemAdd = false, ErrorMessage = ErrorMessage }, JsonRequestBehavior.AllowGet);

            }
            var userPrivilege = new UserPrivilege
            {
                PrivilegeId = PrivilegeId,
                RegistrationId = regId
            };

            db.UserPrivileges.Add(userPrivilege);
            db.SaveChanges();
            var privilege = db.Privileges.Include(i => i.Project).FirstOrDefault(i => i.Id == PrivilegeId);
            if (privilege.ProjectId == 2)
            {
                privilege.PrivilegeName = privilege.PrivilegeName + " <a class='btn btn-default btn-xs' onclick='window.location.reload()'>Add City</a>";
            }
            return Json(new { isItemAdd = true, ProjectName = privilege.Project.ProjectName, PrivilegeName = privilege.PrivilegeName, UserPrivilegeId = userPrivilege.Id }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult RemovePrivilege(int UserPrivilegeId)
        {
            var UserPrivilege = db.UserPrivileges.Find(UserPrivilegeId);
            db.UserPrivileges.Remove(UserPrivilege);
            db.SaveChanges();


            return Json(new { isItemRemoved = true }, JsonRequestBehavior.AllowGet);
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
