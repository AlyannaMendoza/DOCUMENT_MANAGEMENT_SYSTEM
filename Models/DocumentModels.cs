using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DMS_v2.Models
{
    public class Document
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Extension { get; set; }
        public string FileType { get; set; }
        public DateTime? DateUploaded { get; set; }
        public int? UploadedByRegistrationId { get; set; }
        public virtual Registration UploadedByRegistration { get; set; }
        public int? DocumentCategoryId { get; set; }
        public virtual DocumentCategory DocumentCategory { get; set; }
    }
}