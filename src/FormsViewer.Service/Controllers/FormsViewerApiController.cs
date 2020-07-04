namespace FormsViewer.Service.Controllers
{
    using FormsViewer.Service.Models;
    using FormsViewer.Service.Services;
    using Sitecore.ContentSearch;
    using Sitecore.ContentSearch.SearchTypes;
    using Sitecore.ExperienceForms.Analytics.Reporting;
    using Sitecore.ExperienceForms.Configuration;
    using Sitecore.ExperienceForms.Data;
    using Sitecore.ExperienceForms.Data.Entities;
    using Sitecore.ExperienceForms.Data.SqlServer;
    using Sitecore.ExperienceForms.Reporting;
    using Sitecore.ExperienceForms.Reporting.Models;
    using Sitecore.Services.Infrastructure.Web.Http;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web.Http;
    using System.Web.Http.Cors;

    [Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class FormsViewerApiController : ServicesApiController
    {
        private const string FileDownloadPattern = "/sitecore/api/ssc/forms/formdata/formdata/DownloadFile?fileId={0}";

        private const string IndexName = "sitecore_master_index";

        private readonly IFormDataProvider dataProvider;

        private readonly IExportService exportService;

        private readonly IFormStatisticsProvider statisticsProvider;

        public FormsViewerApiController(IFormDataProvider dataProvider, IExportService exportService):this(dataProvider, exportService, null)
        {
            this.dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            this.exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        }

        public FormsViewerApiController(IFormDataProvider dataProvider, IExportService exportService, IFormStatisticsProvider statisticsProvider)
        {
            this.dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            this.exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
            this.statisticsProvider = statisticsProvider;
        }

        [HttpGet]
        public List<Form> Forms()
        {
            var index = ContentSearchManager.GetIndex(IndexName);
            var response = new List<Form>();
            using (var searchContext = index.CreateSearchContext())
            {
                var result = searchContext.GetQueryable<SearchResultItem>().Where(t => t.TemplateName == "Form" && t.Name != "__Standard Values").ToList();

                foreach (var form in result)
                {
                    response.Add(new Form
                    {
                        Name = form.Name,
                        Id = form.ItemId.ToString()
                    });
                }
            }

            return response;
        }

        [HttpPost]
        public FormsViewerResponse Detail([FromBody]FormViewRequest request)
        {
            var result = this.dataProvider.GetEntries(request.FormId, request.StartDate, request.EndDate);

            var response = new FormsViewerResponse
            {
                Headers = result.SelectMany(t => t.Fields).Select(t => t.FieldName).Distinct().ToList(),
                Entries = new List<List<string>>()
            };

            foreach (var entry in result)
            {
                List<string> rowData = new List<string>();

                // Add to the first place the created date.
                rowData.Add(entry.Created.ToString());

                foreach (var header in response.Headers)
                {
                    var field = entry.Fields.FirstOrDefault(t => t.FieldName == header);
                    string value = string.Empty;
                    if (field == null)
                    {
                        value = "-";
                    }
                    else if (field.ValueType.Equals("System.Collections.Generic.List`1[Sitecore.ExperienceForms.Data.Entities.StoredFileInfo]", StringComparison.OrdinalIgnoreCase))
                    {
                        if (field.Value.Contains(','))
                        {
                            foreach (var file in field.Value.Split(',').Where(t => !string.IsNullOrEmpty(t)))
                            {
                                value += string.Format("<a target=\"_blank\" href=\"{0}\">Download</a><br>", Sitecore.Web.WebUtil.GetFullUrl(string.Format(FileDownloadPattern, file)));
                            }
                        }
                        else
                        {
                            value = string.Format("<a target=\"_blank\" href=\"{0}\">Download</a>", Sitecore.Web.WebUtil.GetFullUrl(string.Format(FileDownloadPattern, field.Value)));
                        }
                    }
                    else
                    {
                        value = field.Value;
                    }

                    rowData.Add(value);
                }

                response.Entries.Add(rowData);
            }

            // Insert created header
            response.Headers.Insert(0, "Created");

            return response;
        }

        [HttpGet]
        public SettingsResponse Settings()
        {
            var response = new SettingsResponse
            {
                XDbEnabled = Sitecore.Configuration.Settings.GetBoolSetting("Xdb.Enabled", false)
            };

            return response;
        }

        [HttpPost]
        public FormStatistics Statistics([FromBody]FormViewRequest request)
        {
            return this.statisticsProvider.GetFormStatistics(request.FormId,
                request.StartDate.HasValue ? request.StartDate.Value : DateTime.MinValue,
                request.EndDate.HasValue ? request.EndDate.Value : DateTime.MaxValue);
        }

        [HttpPost]
        public HttpResponseMessage ExportFormData([FromBody]FormViewExportRequest request)
        {
            var entries = this.dataProvider.GetEntries(request.FormId, request.StartDate, request.EndDate);
            string formName = "sample";
            string fileName = string.Format("Export_{0}_{1}.csv", formName, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Stream stream = null;
            if (request.ExportOption.Equals("excel", StringComparison.OrdinalIgnoreCase))
            {
                stream = this.exportService.CreateExcel(request, entries.ToList());
                stream.Seek(0, SeekOrigin.Begin);
            }
            else
            {
                stream = new MemoryStream();
                StreamWriter writer = new StreamWriter(stream);
                writer.Write(CreateReport(request, entries.ToList()));
                writer.Flush();
                stream.Position = 0;
            }

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = fileName };
            return result;
        }

        private string CreateReport(FormViewExportRequest request, List<FormEntry> entries)
        {
            string result = string.Empty;

            var response = new FormsViewerResponse();

            response.Entries = new List<List<string>>();
            foreach (var entry in entries)
            {
                List<string> rowData = new List<string>();
                foreach (var header in request.Fields)
                {
                    var field = entry.Fields.FirstOrDefault(t => t.FieldName == header);
                    string value = string.Empty;
                    if (field == null)
                    {
                        if (header.Equals("Created"))
                        {
                            value = entry.Created.ToString();
                        }
                        else
                        {
                            value = "-";
                        }
                    }
                    else if (field.ValueType.Equals("System.Collections.Generic.List`1[Sitecore.ExperienceForms.Data.Entities.StoredFileInfo]", StringComparison.OrdinalIgnoreCase))
                    {
                        if (field.Value.Contains(','))
                        {
                            foreach (var file in field.Value.Split(',').Where(t => !string.IsNullOrEmpty(t)))
                            {
                                value += string.Format("<a target=\"_blank\" href=\"{0}\">Download</a><br>", Sitecore.Web.WebUtil.GetFullUrl(string.Format(FileDownloadPattern, file)));
                            }
                        }
                        else
                        {
                            value = string.Format("<a target=\"_blank\" href=\"{0}\">Download</a>", Sitecore.Web.WebUtil.GetFullUrl(string.Format(FileDownloadPattern, field.Value)));
                        }
                    }
                    else
                    {
                        value = field.Value;
                    }

                    rowData.Add(value);
                }

                response.Entries.Add(rowData);
            }

            if (request.ExportOption.Equals("xml", StringComparison.OrdinalIgnoreCase))
            {
                return exportService.ExportToXml(request, response.Entries.ToList());
            }
            else if (request.ExportOption.Equals("csv", StringComparison.OrdinalIgnoreCase))
            {
                return exportService.ExportToCsv(request, entries.ToList());
            }

            return result;
        }
    }
}