using DMS_v2.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using static DMS_v2.Models.RegistrationCreateViewModel;

namespace DMS_v2.Controllers
{
   // [Authorize]
    public class RegistrationController : Controller
    {
        public RegistrationController()
            : this(new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
        }

        public RegistrationController(UserManager<ApplicationUser> userManager)
        {
            UserManager = userManager;
        }

        public UserManager<ApplicationUser> UserManager { get; private set; }


        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: /Registration/
        //[Authorize]
        //[AuthorizeUser(AccessLevel = new string[] { "Administrator - Dengvaxia Profile" }, accessType = "administrator")]
        public ActionResult Index(RegistrationIndexViewModel model)
        {
            var reg = db.Registrations.ToList();
            model.Registrations = reg;
            return View(model);
        }

        // GET: /Registration/Details/5
        //[Authorize]
        public ActionResult Details(RegistrationDetailsViewModel model, int? id, string status)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Registration registration = db.Registrations.Find(id);
            if (registration == null)
            {
                return HttpNotFound();
            }

            if (status == null || status == "")
            {
                status = "PERSONALINFORMATION";
            }


            model.Registration = registration;
            model.UserPrivileges = db.UserPrivileges.Where(i => i.RegistrationId == registration.Id).ToList();
            ViewBag.Status = status;

            return View(model);
        }

        // GET: /Registration/Create
        //[Authorize]
        //[AuthorizeUser(AccessLevel = new string[] { "Administrator - Dengvaxia Profile" }, accessType = "administrator")]
        public ActionResult Create(int id)
        {
            RegistrationCreateViewModel model = new RegistrationCreateViewModel();
            var regreq = db.RegistrationRequests.Find(id);
            model.RegistrationRequest = regreq;

            return View(model);
        }

        // POST: /Registration/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RegistrationCreateViewModel model, int id)
        {

            if (ModelState.IsValid)
            {
                var regreq = db.RegistrationRequests.Find(id);
                model.RegistrationRequest = regreq;

                Registration registration = new Registration
                {
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    RegistrationDate = DateTime.Now,
                    AccountType = model.AccountType,
                    MiddleName = model.MiddleName,
                    RegistrationRequestId = regreq.Id,
                    IsActive = true,

                };

                db.Registrations.Add(registration);

                db.SaveChanges();


                string getcode = registration.RegistrationDate.Value.ToString("yyMMddHHmm");
                string code = String.Format("{0:X}", Convert.ToInt64(getcode));


                var last = registration.LastName.Replace("Ñ", "N");
                var username = model.Email;
                var pass = code;

                registration.UserName = username;
                registration.Code = code;
                db.Entry(registration).State = EntityState.Modified;
                db.SaveChanges();

                var user = new ApplicationUser() { UserName = username, Email = model.Email };
                var result = UserManager.Create(user, pass);

                var officerNew = db.Registrations.Find(registration.Id);
                officerNew.UserName = username;
                db.SaveChanges();

                //var RoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
                //if (!RoleManager.RoleExists(officerNew.AccountType))
                //{
                //    var role = new IdentityRole(officerNew.AccountType);
                //    RoleManager.Create(role);
                //}

                //var temp = db.Users.Single(i => i.UserName == user.UserName);
                //UserManager.AddToRole(temp.Id, officerNew.AccountType);

                db.Entry(registration).State = EntityState.Modified;
                db.SaveChanges();

                UserPrivilege userprivilege = new UserPrivilege();
                userprivilege.PrivilegeId = 2;
                userprivilege.RegistrationId = registration.Id;
                db.UserPrivileges.Add(userprivilege);
                db.SaveChanges();

                var enc = Custom.Controllers.EncryptionHelper.Encrypt(registration.Id.ToString());

                //NCRO_DTR_v4.Source.Mail mail = new Source.Mail();

                //string subject = "DOH - MMCHD COVID19 CLAIMS MONITORING SYSTEM";
                //string mailmessage = System.IO.File.ReadAllText(Server.MapPath("~/Content/Email/RequestAccount.html"));
                //mailmessage = mailmessage.Replace("#FirstName#", registration.FirstName);
                //mailmessage = mailmessage.Replace("#LastName#", registration.LastName);
                //mailmessage = mailmessage.Replace("#Password#", pass);
                //mailmessage = mailmessage.Replace("#Email#", registration.UserName);
                //mailmessage = mailmessage.Replace("#Url#", "http://Account/Login");
                //string recipient = registration.Email;

                //mail.SendMail(mailmessage, subject, recipient, "");

                regreq.AccountInformation = true;
                db.Entry(regreq).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Success", "Registration", new { @rid = enc });
            }

            return View();
        }
        public ActionResult Success(string rid)
        {
            var dec = Custom.Controllers.EncryptionHelper.Decrypt(rid);

            int? id = Int32.Parse(dec);

            var sup = db.Registrations.Single(i => i.Id == id).Id;

            var reg = db.Registrations.Find(sup);

            RegistrationCreateViewModel model = new RegistrationCreateViewModel();
            model.Registration = reg;



            return View(model);
        }

        public ActionResult Deactivate(int id = 0)
        {
            Registration reg = db.Registrations.Find(id);
            if (reg == null)
            {
                return HttpNotFound();
            }
            Deactivation deactivate = new Deactivation();
            deactivate.RegistrationId = id;
            deactivate.Registration = db.Registrations.Single(i => i.Id == id);
            if (this.TempData["customErrorMessage"] != null)
                ViewBag.CustomErrorMessage = this.TempData["customErrorMessage"];

            if (this.TempData["customSuccessMessage"] != null)
                ViewBag.CustomSuccessMessage = this.TempData["customSuccessMessage"];
            var emplist = db.Registrations.ToList();
            deactivate.Registrations = emplist;

            return View(deactivate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deactivate(Deactivation deactivate, int id)
        {

            Registration reg = db.Registrations.Find(id);
            deactivate.RegistrationId = id;
            var emplist = db.Registrations.ToList();
            deactivate.Registrations = emplist;

            reg.IsActive = false;
            reg.DeactivatedByRegistrationId = (int)db.Registrations.Single(i => i.UserName == User.Identity.Name).Id;
            reg.DeactivatedRemarks = deactivate.Remark;
            db.Entry(reg).State = EntityState.Modified;
            db.SaveChanges();

            this.TempData["customSuccessMessage"] = "You have successfully deactivated this account.";
            return RedirectToAction("Details", "Registration", new { id = reg.Id });


        }

        public ActionResult Activate(int id = 0)
        {
            Registration reg = db.Registrations.Find(id);
            if (reg == null)
            {
                return HttpNotFound();
            }
            Deactivation deactivate = new Deactivation();
            deactivate.RegistrationId = id;
            deactivate.Registration = db.Registrations.Single(i => i.Id == id);
            if (this.TempData["customErrorMessage"] != null)
                ViewBag.CustomErrorMessage = this.TempData["customErrorMessage"];

            if (this.TempData["customSuccessMessage"] != null)
                ViewBag.CustomSuccessMessage = this.TempData["customSuccessMessage"];
            var emplist = db.Registrations.ToList();
            deactivate.Registrations = emplist;

            return View(deactivate);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Activate(Deactivation deactivate, int id)
        {

            Registration reg = db.Registrations.Find(id);
            deactivate.RegistrationId = id;
            var emplist = db.Registrations.ToList();
            deactivate.Registrations = emplist;

            reg.IsActive = true;
            reg.DeactivatedByRegistrationId = null;
            reg.DeactivatedRemarks = null;
            db.Entry(reg).State = EntityState.Modified;
            db.SaveChanges();

            this.TempData["customSuccessMessage"] = "You have successfully deactivated this account.";
            return RedirectToAction("Details", "Registration", new { id = reg.Id });


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
