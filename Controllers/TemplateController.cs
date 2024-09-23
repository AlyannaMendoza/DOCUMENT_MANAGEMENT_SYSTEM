using DMS_v2.Models;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;
using System;

namespace BED_TRACKER_SAMPLE.Controllers
{
    public class TemplateController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        //
        // GET: /Template/

        public ActionResult SessionPrivilege(int? sectionid)
        {
            int id = (int)db.Registrations.Single(i => i.UserName == User.Identity.Name).Id;
            Registration registration = db.Registrations.Find(id);
            if (ModelState.IsValid)
            {
                registration.SessionPrivilegeId = sectionid;
                db.Entry(registration).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }

            return View(registration);
        }


        public PartialViewResult _NavBar()
        {
            if (User.Identity.Name == null)
            {
                ViewBag.FailedLogin = true;
            }
            TemplateViewModel model = new TemplateViewModel();

            Registration Registration = Custom.Controllers.dbEmployee.RegistrationNow();
            int id = Registration.Id;
            var user = db.UserPrivileges.Where(i => i.RegistrationId == Registration.Id).ToList();
            model.Registration = Registration;
            model.UserPrivileges = user;

            //ViewBag.ETSApproval = 0;
            //var officerstep = db.ETS_OfficerInCharge.Where(i => i.RegistrationId == Registration.Id).ToList();
            //foreach (var it in officerstep)
            //{
            //    ViewBag.ETSApproval += db.ETS_WorkSheets.Where(i => i.ETS_StepId == it.ETS_StepId).Count();
            //}

            return PartialView(model);
        }
        public PartialViewResult _NavBarNotification()
        {
            if (User.Identity.Name == null)
            {
                ViewBag.FailedLogin = true;
            }
            TemplateViewModel model = new TemplateViewModel();


            Registration Registration = db.Registrations.Single(i => i.UserName == User.Identity.Name);
            int id = Registration.Id;
            var user = db.UserPrivileges.Where(i => i.RegistrationId == Registration.Id).ToList();
            model.Registration = Registration;
            model.UserPrivileges = user;

            return PartialView(model);
        }
        public PartialViewResult _NavBar2()
        {
            if (User.Identity.Name == null)
            {
                ViewBag.FailedLogin = true;
            }
            TemplateViewModel model = new TemplateViewModel();

            Registration employee = db.Registrations.Single(i => i.UserName == User.Identity.Name);
            int id = employee.Id;
            var user = db.UserPrivileges.Where(i => i.RegistrationId == employee.Id).ToList();

            var rd = ControllerContext.ParentActionViewContext.RouteData;
            var currentAction = rd.GetRequiredString("action");
            var currentController = rd.GetRequiredString("controller");

            model.Registration = employee;
            model.ControllerName = currentController;
            model.ActionName = currentAction;


            return PartialView(model);
        }

        [ChildActionOnly]
        public PartialViewResult ProjectOption()
        {

            TemplateViewModel model = new TemplateViewModel();
            Registration Registration = db.Registrations.Single(i => i.UserName == User.Identity.Name);
            var user = db.UserPrivileges.Where(i => i.RegistrationId == Registration.Id).ToList();
            model.Registration = Registration;
            model.UserPrivileges = user;

            var userprivi = user.Count();

            if (userprivi == 0)
            {
                return PartialView(model);
            }


            return PartialView(model);
        }


        public PartialViewResult ProjectLabel()
        {
            TemplateViewModel model = new TemplateViewModel();

            Registration Registration = db.Registrations.Single(i => i.UserName == User.Identity.Name);
            int id = Registration.Id;
            var user = db.UserPrivileges.Where(i => i.RegistrationId == Registration.Id).ToList();
            model.Registration = Registration;
            model.UserPrivileges = user;

            var userprivi = user.Where(i => i.RegistrationId == id).Count();

            return PartialView(model);
        }



        public ActionResult Header()
        {

            return View();
        }

    }
}