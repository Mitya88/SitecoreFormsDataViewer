namespace FormsViewer.Service.Controllers
{
    using FormsViewer.Service.Models;
    using Sitecore.Configuration;
    using Sitecore.ContentSearch;
    using Sitecore.ContentSearch.SearchTypes;
    using Sitecore.ExperienceForms.Analytics.Reporting;
    using Sitecore.ExperienceForms.Configuration;
    using Sitecore.ExperienceForms.Data.SqlServer;
    using Sitecore.ExperienceForms.Data.SqlServer.Converters;
    using Sitecore.ExperienceForms.Data.SqlServer.Parsers;
    using Sitecore.ExperienceForms.Diagnostics;
    using Sitecore.ExperienceForms.Reporting.Models;
    using Sitecore.Services.Infrastructure.Web.Http;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Cors;

    //[Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class FormsViewerApiController : ServicesApiController
    {
        public const string FileDownloadPattern = "/sitecore/api/ssc/forms/formdata/formdata/DownloadFile?fileId={0}";

        public FormsViewerApiController()
        {
        }

        [HttpGet]
        public List<Form> Forms()
        {
            var index = ContentSearchManager.GetIndex("sitecore_master_index");
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
            //var provider = new SqlFormDataProvider
            //    (new SqlConnectionSettings(new FormsConfigurationSettings()),
            //    new Logger(),
            //    new FormDataParser(),
            //    new FormFieldDataConverter());

            var provider = new SqlFormDataProvider(new SqlServerApiFactory(new SqlConnectionSettings(new FormsConfigurationSettings())));

            var result = provider.GetEntries(request.FormId, request.StartDate, request.EndDate);

            var response = new FormsViewerResponse();
            response.Headers = result.SelectMany(t => t.Fields).Select(t => t.FieldName).Distinct().ToList();
            
            response.Entries = new List<List<string>>();
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

        [HttpPost]
        public FormStatistics Statistics([FromBody]FormViewRequest request)
        {
            if (!Settings.GetBoolSetting("Xdb.Enabled", false))
            {
                return new FormStatistics
                {
                    SuccessSubmits = -1
                };
            }

            var provider  = new FormStatisticsProvider(new ReportingQueryFactory(new ReportDataProviderFactory()));

            return provider.GetFormStatistics(request.FormId, 
                request.StartDate.HasValue ? request.StartDate.Value : DateTime.MinValue, 
                request.EndDate.HasValue ? request.EndDate.Value : DateTime.MaxValue);
        }
    }
}