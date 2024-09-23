using DMS_v2.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Tesseract;

namespace DOH_WEBSITE.Controllers
{
    public class DocumentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Career
        public ActionResult Index()
        {
            var documents = db.Documents.Include(c => c.UploadedByRegistration);
            

            return View(documents.ToList());
        }


        // GET: Career/Details/5
        public ActionResult GetFile(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Document document = db.Documents.Find(id);
            if (document == null)
            {
                return HttpNotFound();
            }

            var filePath = Server.MapPath("~/Content/Public/Document/" + document.FileName + document.Extension);
            var fileType = document.FileType;

            return File(filePath, fileType);
        }

        // GET: Career/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Document document = db.Documents.Find(id);
            if (document == null)
            {
                return HttpNotFound();
            }

            //var filePath = Server.MapPath("~/Content/Public/Document/" + document.FileName + document.Extension);
            //var fileType = document.FileType;
            var ocrResult = GetOcrResultText(document); // Implement this method if needed

            ViewBag.OcrResult = ocrResult; // Pass the OCR result to the view

            return View(document);
        }

        // GET: Career/Create
        //[Authorize]
        public ActionResult Create()
        {
            ViewBag.DocumentCategoryId = new SelectList(db.DocumentCategories, "Id", "DocumentCategories");

            return View();
        }

        // POST: Career/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,FileName,Extension,FileType,DateUploaded,UploadedByRegistrationId,DocumentCategoryId")] Document document, HttpPostedFileBase File)
        {
            if (File != null)
            {
                if (ModelState.IsValid)
                {

                    if (File == null || File.ContentLength == 0)
                    {
                        ModelState.AddModelError("File", "File is empty or not selected.");
                        return View(); // Return with error message
                    }

                    var allowedContentTypes = new List<string> { "application/pdf", "image/jpeg", "image/jpg", "image/png" };

                    if (!allowedContentTypes.Contains(File.ContentType))
                    {
                        ModelState.AddModelError("File", "Unsupported file type.");
                        return View(); // Return with error message
                    }

                    string filename = File.FileName;
                    string fname = System.IO.Path.GetFileNameWithoutExtension(filename);
                    string extension = System.IO.Path.GetExtension(filename);

                    document.FileName = fname.Replace("_", "-").Replace("%", "").Replace(".", "");
                    document.Extension = extension;
                    document.FileType = File.ContentType;
                    document.DateUploaded = DateTime.Now;
                    document.UploadedByRegistrationId = (int)Custom.Controllers.dbEmployee.RegistrationNow().Id;

                    db.Documents.Add(document);
                    db.SaveChanges();

                    var renameFile = document.FileName + extension;
                    var path = Path.Combine(Server.MapPath("~/Content/Public/Document/"), renameFile);
                    File.SaveAs(path);
                    
                    return RedirectToAction("Index", "Document", new { id = document.Id });
                }
            }
            ViewBag.DocumentCategoryId = new SelectList(db.DocumentCategories, "Id", "DocumentCategories", document.DocumentCategoryId);
            return View(document);
        }

        public ActionResult GetOcrResult(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Document document = db.Documents.Find(id);
            if (document == null)
            {
                return HttpNotFound();
            }

            // Get the file path of the uploaded document
            var filePath = Server.MapPath("~/Content/Public/Document/" + document.FileName + document.Extension);

            if (!System.IO.File.Exists(filePath))
            {
                return HttpNotFound("File not found.");
            }

            string ocrResult = string.Empty;

            try
            { // Initialize Tesseract engine
                string tessDataPath = Server.MapPath("~/tessdata"); // Ensure you have the tessdata folder
                using (var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromFile(filePath))
                    {
                        using (var page = engine.Process(img))
                        {
                            ocrResult = page.GetText();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ocrResult = $"Error during OCR processing: {ex.Message}";
            }

            ViewBag.OcrResult = ocrResult; // Store OCR result in ViewBag
            return View(ocrResult); // Create a new view to display the result
        }

        private string GetOcrResultText(Document document)
        {
            // Initialize OCR result as an empty string
            string ocrResult = string.Empty;

            try
            {
                // Get the file path of the uploaded document
                var filePath = Server.MapPath("~/Content/Public/Document/" + document.FileName + document.Extension);

                // Check if the file exists
                if (!System.IO.File.Exists(filePath))
                {
                    return "File not found.";
                }

                if (document.FileType == "application/pdf")
                {
                    List<string> imageFiles= ConvertPdfToImages(filePath);

                    ocrResult = PerformOcrOnImage(imageFiles);

                    foreach (var imagePath in imageFiles)
                    {
                        // Clean up temporary image files=
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                }
                else
                {
                    //Initialize Tesseract engine
                    string tessDataPath = Server.MapPath("~/tessdata"); // Ensure you have the tessdata folder
                    using (var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default))
                    {
                        using (var img = Pix.LoadFromFile(filePath))
                        {
                            using (var page = engine.Process(img))
                            {
                                ocrResult = page.GetText();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ocrResult = $"Error during OCR processing: {ex.Message}";
            }

            return ocrResult;
        }

        private List<string> ConvertPdfToImages(string filePath)
        {
            string ghostscriptPath = Server.MapPath("~/gs10.03.1/bin/gswin64c.exe");

            // Output directory
            string imagePath = Server.MapPath("~/Content/Public/TempFiles");
            if (!Directory.Exists(imagePath))
            {
                Directory.CreateDirectory(imagePath);
            }

            if (!System.IO.File.Exists(filePath))
            {
                throw new FileNotFoundException($"Input PDF file not found: {filePath}");
            }

            if (Path.GetExtension(filePath).ToLower() != ".pdf")
            {
                throw new ArgumentException($"The file {filePath} is not a valid PDF.");
            }

            var imageFiles = new List<string>();

            try
            {
                // Ghostscript command arguments
                string args = $"-dNOPAUSE -dBATCH -dSAFER -sDEVICE=jpeg -r300x300 -sOutputFile=\"{Path.Combine(imagePath, "page_%03d.jpg")}\" \"{filePath}\"";

                Console.WriteLine($"Ghostscript Path: {ghostscriptPath}");
                Console.WriteLine($"Arguments: {args}");

                // Start Ghostscript process
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = ghostscriptPath,
                    Arguments = args,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = processStartInfo })
                {
                    process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
                    process.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        string error = process.StandardError.ReadToEnd();
                        throw new Exception($"Ghostscript failed with error: {error}");
                    }

                    // Check if the files are created and accessible
                    for (int i = 1; i <= process.ExitCode; i++)
                    {
                        string imageFilePath = Path.Combine(imagePath, $"page_{i:D3}.jpg");

                        // Wait until the file is accessible
                        int retries = 10;
                        while (!System.IO.File.Exists(imageFilePath) && retries > 0)
                        {
                            System.Threading.Thread.Sleep(500); // Wait for 0.5 seconds
                            retries--;
                        }

                        if (System.IO.File.Exists(imageFilePath))
                        {
                            imageFiles.Add(imageFilePath);
                        }
                        else
                        {
                            throw new Exception($"Failed to generate image for page {i}");
                        }
                    }

                    imageFiles = Directory.GetFiles(imagePath, "*.jpg").ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting PDF to images: {ex.Message}");
            }

            return imageFiles;
        }

        private string PerformOcrOnImage(List<string> imagePath)
        {
            string tessDataPath = Server.MapPath("~/tessdata");  // Path to Tesseract data
            StringBuilder text = new StringBuilder();

            using (var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default))
            {
                foreach (var image in imagePath)
                {
                    using (var img = Pix.LoadFromFile(image))
                    {
                        using (var page = engine.Process(img))
                        {
                            text.AppendLine($"Page {image + 1}:\n{page.GetText()}\n\n");
                        }
                    }
                }
            }

            return text.ToString();
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
