using OCR_TRIAL.Data;
using OCR_TRIAL.Models;
using OCR_TRIAL.Service;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace OCR_TRIAL.Controllers
{
    public class FilesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IOcrService _ocrService;
        private readonly IFileService _fileService;

        public FilesController(AppDbContext context, IOcrService ocrService, IFileService fileService)
        {
            _context = context;
            _ocrService = ocrService;
            _fileService = fileService;
        }

        [HttpGet]
        public ActionResult Index(string searchQuery)

        {
            var filesQuery = _context.FileMetadatas.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                filesQuery = filesQuery
                    .Include(f => f.OcrResult)
                    .Where(f => f.FileName.Contains(searchQuery) ||
                               (f.OcrResult != null && f.OcrResult.ExtractedText.Contains(searchQuery)));
            }

            var files = filesQuery.ToList();

            ViewData["CurrentFilter"] = searchQuery;

            return View(files);
        }

        // GET: Files
        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength == 0)
            {
                ModelState.AddModelError("File", "File is empty or not selected.");
                return View(); // Return with error message
            }

            var allowedContentTypes = new List<string> { "application/pdf", "image/jpeg", "image/jpg" };

            if (!allowedContentTypes.Contains(file.ContentType))
            {
                ModelState.AddModelError("File", "Unsupported file type.");
                return View(); // Return with error message
            }

            var uploadDate = DateTime.Now;
            var monthFolder = $"{uploadDate.ToString("MMMM")}_{uploadDate.Year.ToString()}";
            var dayFolder = $"{uploadDate.Month}-{uploadDate.Day}-{uploadDate.Year.ToString()}";

            var folderPath = Path.Combine(
               Server.MapPath("~/Documents Uploaded"),
               uploadDate.Year.ToString(),
               monthFolder,
               dayFolder
            );

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            var uniqueFileName = $"{timestamp}_{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(folderPath, uniqueFileName);

            Task.Delay(500);

            // Save the file to disk
            file.SaveAs(filePath);

            byte[] fileData;
            string ocrResult = string.Empty;
            string fileName = file.FileName;

            Task.Delay(500);

            using (var memoryStream = new MemoryStream())
            {
                Task.Delay(500);
                file.InputStream.CopyTo(memoryStream);

                if (file.ContentType == "application/pdf")
                {
                    Task.Delay(500);
                    ocrResult = _ocrService.PdfPigTextExtraction(memoryStream);
                    //fileData = _fileService.GhostscriptPdfCompression(filePath);
                }
                else
                {
                    var performTesseract = _ocrService.OcrAndSearchablePdf(memoryStream, file.FileName);
                    ocrResult = performTesseract.OcrText;
                    fileName = performTesseract.PdfFileName;
                    fileData = performTesseract.PdfData;

                    //fileData = _fileService.ImageSharpCompression(memoryStream, 600, 800, 50); // Synchronous call
                }

                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var fsMemoryStream = new MemoryStream())
                    {
                        fileStream.CopyToAsync(fsMemoryStream);
                        fileData = fsMemoryStream.ToArray();
                    }
                }

                //using (var stream = new FileStream(filePath, FileMode.Create))
                //{
                //    stream.Write(fileData, 0, fileData.Length);
                //}

                // Save OCR result
                var ocrResultRecord = new OcrResult
                {
                    ExtractedText = ocrResult
                };

                // Save metadata and file data
                var uploadedFileMetadata = new FileMetadata
                {
                    FileName = fileName,
                    ContentType = file.ContentType,
                    Size = new FileInfo(filePath).Length,
                    FilePath = filePath,
                    UploadDate = uploadDate,
                    OcrResult = ocrResultRecord
                };

                var uploadedFileData = new FileData
                {
                    Data = fileData,
                    FileMetadata = uploadedFileMetadata
                };

                // Add OCR result, file metadata, and file data to the database
                _context.OcrResults.Add(ocrResultRecord);
                _context.FileMetadatas.Add(uploadedFileMetadata);
                _context.FileDatas.Add(uploadedFileData);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }


        [HttpGet]
        public ActionResult Details(int id)
        {
            var file = _context.FileMetadatas
                .Include(f => f.OcrResult)
                .FirstOrDefault(f => f.Id == id);

            if (file == null)
            {
                return HttpNotFound();
            }

            return View(file);
        }

        [HttpGet]
        public ActionResult GetFile(int id)
        {
            var fileData = _context.FileDatas
                .Include(fd => fd.FileMetadata)
                .FirstOrDefault(fd => fd.FileMetadataId == id);

            if (fileData == null)
            {
                return HttpNotFound();
            }

            var contentType = fileData.FileMetadata.ContentType;
            var fileName = fileData.FileMetadata.FileName;

            Response.Headers.Add("Content-Disposition", $"inline; filename=\"{Uri.EscapeDataString(fileName)}\"");

            return File(fileData.Data, contentType);

            //if (contentType.StartsWith("image/"))
            //{
            //    // Create searchable PDF from image data
            //    //using (var memoryStream = new MemoryStream(fileData.Data))
            //    //{
            //    //    //var pdfBytes = await _ocrService.ImageToSearchablePdfAsync(memoryStream, fileName);

            //    //    // Set the response to download the PDF
            //    //    Response.Headers.Append("Content-Disposition", $"attachment; filename={Path.GetFileNameWithoutExtension(fileName)}.pdf");
            //    //    return File(pdfBytes, "application/pdf");
            //    //}
        }
    }
}