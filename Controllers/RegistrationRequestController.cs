using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using DMS_v2.Models;
using static DMS_v2.Models.RegistrationCreateViewModel;

namespace BED_TRACKER_SAMPLE.Controllers
{

    public class RegistrationRequestController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Index()
        {
            RegistrationRequestIndexViewModel model = new RegistrationRequestIndexViewModel();
            model.RegistrationRequests = db.RegistrationRequests.ToList();

            return View(model);
        }
        // GET: /Registration/Create
        [AllowAnonymous]
        public ActionResult Create()
        {
            RegistrationRequestCreateViewModel model = new RegistrationRequestCreateViewModel();

            return View(model);
        }

        // POST: /Registration/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RegistrationRequestCreateViewModel model)
        {
            var email = model.Email;
            var emp = db.Registrations.ToList();
            if (emp.Where(i => i.Email == email).Count() > 0)
            {
                ViewBag.email = "Email Already Taken!";
                return View();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    RegistrationRequest registration = new RegistrationRequest();
                    registration.Email = model.Email;
                    registration.FirstName = model.FirstName;

                    registration.LastName = model.LastName;

                    registration.MiddleName = model.MiddleName;
                    registration.RequestDate = DateTime.Now;

                    db.RegistrationRequests.Add(registration);
                    db.SaveChanges();

                    var enc = Custom.Controllers.EncryptionHelper.Encrypt(registration.Id.ToString());

                    return RedirectToAction("Success", "RegistrationRequest", new { @rid = enc });
                }
            }

            return View();
        }

        [AllowAnonymous]
        public ActionResult Success(string rid)
        {
            var dec = Custom.Controllers.EncryptionHelper.Decrypt(rid);

            int? id = Int32.Parse(dec);

            var sup = db.RegistrationRequests.Single(i => i.Id == id).Id;

            var reg = db.RegistrationRequests.Find(sup);

            RegistrationVerifyAccountModel model = new RegistrationVerifyAccountModel();
            model.RegistrationRequest = reg;
            return View(model);
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
