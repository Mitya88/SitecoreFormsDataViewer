namespace FormsViewer.Service.Models
{
    using System.Collections.Generic;

    public class FormsViewerResponse
    {
        public List<string> Headers { get; set; }

        public List<List<string>> Entries { get; set; }
    }
}