using System;

namespace FormsViewer.Service.Models
{
    public class FormViewRequest
    {
        public Guid FormId { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}