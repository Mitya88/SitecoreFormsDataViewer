using System;
using System.Collections.Generic;

namespace FormsViewer.Service.Models
{
    public class FormViewExportRequest
    {
        public Guid FormId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public List<string> Fields { get; set; }

        public string ExportOption { get; set; }
    }
}