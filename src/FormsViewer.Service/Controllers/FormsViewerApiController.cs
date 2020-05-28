namespace FormsViewer.Service.Controllers
{
    using FormsViewer.Service.Models;
    using Sitecore.Configuration;
    using Sitecore.ContentSearch;
    using Sitecore.ContentSearch.SearchTypes;
    using Sitecore.ExperienceForms.Analytics.Reporting;
    using Sitecore.ExperienceForms.Configuration;
    using Sitecore.ExperienceForms.Data.SqlServer;
    using Sitecore.ExperienceForms.Reporting.Models;
    using Sitecore.Services.Infrastructure.Web.Http;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Web.Http;
    using System.Web.Http.Cors;
    using System.Xml;
    using System.Xml.Serialization;

    //[Authorize]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class FormsViewerApiController : ServicesApiController
    {
        private const string FileDownloadPattern = "/sitecore/api/ssc/forms/formdata/formdata/DownloadFile?fileId={0}";

        private const string IndexName = "sitecore_master_index";

        public FormsViewerApiController()
        {
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
            var provider = new SqlFormDataProvider(new SqlServerApiFactory(new SqlConnectionSettings(new FormsConfigurationSettings())));
            var result = provider.GetEntries(request.FormId, request.StartDate, request.EndDate);


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

        [HttpPost]

        public HttpResponseMessage ExportFormData([FromBody]FormViewExportRequest request)
        {
            string formName = "sample";
            string fileName = string.Format("Export_{0}_{1}.csv", formName, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(CreateReport(request));
            writer.Flush();
            stream.Position = 0;

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StreamContent(stream);
            //result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = fileName };
            return result;
        }

        private string CreateReport(FormViewExportRequest request)
        {
            string result = string.Empty;

            var provider = new SqlFormDataProvider(new SqlServerApiFactory(new SqlConnectionSettings(new FormsConfigurationSettings())));

            var entries = provider.GetEntries(request.FormId, request.StartDate, request.EndDate);

            var response = new FormsViewerResponse();

            response.Entries = new List<List<string>>();
            foreach (var entry in entries)
            {
                List<string> rowData = new List<string>();

                // Add to the first place the created date.
                rowData.Add(entry.Created.ToString());
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


            // TODO: export response.entries..

            if (request.ExportOption.Equals("xml", StringComparison.OrdinalIgnoreCase))
            {
                XmlSerializer x = new XmlSerializer(response.Entries.GetType());
                var xml = "";

                using (var sww = new StringWriter())
                {
                    using (XmlWriter writer = XmlWriter.Create(sww))
                    {
                        x.Serialize(writer, response.Entries);
                        xml = sww.ToString();
                    }
                }

                return xml;
            }
            else if (request.ExportOption.Equals("excel", StringComparison.OrdinalIgnoreCase))
            {
                StringBuilder strExcelXml = new StringBuilder();
                //First Write the Excel Header
                strExcelXml.Append(ExcelHeader());
                // Worksheet options Required only one time 
                strExcelXml.Append(ExcelWorkSheetOptions());

                // Create First Worksheet tag
                strExcelXml.Append(
                    "<Worksheet ss:Name=\"WorkSheet" + 1 + "\">");
                // Then Table Tag
                strExcelXml.Append("<Table>");

                strExcelXml.Append("<tr>");
                foreach (var field in request.Fields)
                {
                    strExcelXml.Append(string.Format("<td>{0}</td>", field));
                }

                strExcelXml.Append("</tr>");
                for (int k = 0; k < entries.Count; k++)
                {
                    // Row Tag
                    strExcelXml.Append("<tr>");
                    for (int j = 0; j < request.Fields.Count; j++)
                    {
                        // Cell Tags
                        strExcelXml.Append("<td>");
                        var field = entries.ToList()[k].Fields.FirstOrDefault(t => t.FieldName == request.Fields[j]);
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
                                    value += string.Format("<a target=\"_blank\" href=\"{0}\">Download</a>", Sitecore.Web.WebUtil.GetFullUrl(string.Format(FileDownloadPattern, file)));
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
                        strExcelXml.Append(value);
                        strExcelXml.Append("</td>");
                    }
                    strExcelXml.Append("</tr>");
                }
                strExcelXml.Append("</Table>");
                strExcelXml.Append("</Worksheet>");
                // Close the Workbook tag (in Excel header 
                // you can see the Workbook tag)
                strExcelXml.Append("</Workbook>\n");

                return ConvertHTMLToExcelXML(strExcelXml.ToString());

            }
            else if (request.ExportOption.Equals("excel", StringComparison.OrdinalIgnoreCase))
            {
                //TODO
            }

            return result;
        }

        // Final Filtaration of String Code generated by above code
        public static string ConvertHTMLToExcelXML(string strHtml)
        {

            // Just to replace TR with Row
            strHtml = strHtml.Replace("<tr>", "<Row ss:AutoFitHeight=\"1\" >\n");
            strHtml = strHtml.Replace("</tr>", "</Row>\n");

            //replace the cell tags
            strHtml = strHtml.Replace("<td>", "<Cell><Data ss:Type=\"String\">");
            strHtml = strHtml.Replace("</td>", "</Data></Cell>\n");

            return strHtml;
        }

        private string ExcelWorkSheetOptions()
        {
            // This is Required Only Once ,	But this has to go after the First Worksheet's First Table		
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("\n<WorksheetOptions xmlns=\"urn:schemas-microsoft-com:office:excel\">\n<Selected/>\n </WorksheetOptions>\n");
            return sb.ToString();
        }

        private string ExcelHeader()

        {
            // Excel header
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<?xml version=\"1.0\"?>\n");
            sb.Append("<?mso-application progid=\"Excel.Sheet\"?>\n");
            sb.Append(
              "<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\" ");
            sb.Append("xmlns:o=\"urn:schemas-microsoft-com:office:office\" ");
            sb.Append("xmlns:x=\"urn:schemas-microsoft-com:office:excel\" ");
            sb.Append("xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\" ");
            sb.Append("xmlns:html=\"http://www.w3.org/TR/REC-html40\">\n");
            sb.Append(
              "<DocumentProperties xmlns=\"urn:schemas-microsoft-com:office:office\">");
            sb.Append("</DocumentProperties>");
            sb.Append(
              "<ExcelWorkbook xmlns=\"urn:schemas-microsoft-com:office:excel\">\n");
            sb.Append("<ProtectStructure>False</ProtectStructure>\n");
            sb.Append("<ProtectWindows>False</ProtectWindows>\n");
            sb.Append("</ExcelWorkbook>\n");
            return sb.ToString();
        }
    }
}