using FormsViewer.Service.Helpers;
using FormsViewer.Service.Models;
using Sitecore.ExperienceForms.Data.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace FormsViewer.Service.Services
{
    public class ExportService
    {
        private const string FileDownloadPattern = "/sitecore/api/ssc/forms/formdata/formdata/DownloadFile?fileId={0}";

        public string ExportToExcel(FormViewExportRequest request, List<FormEntry> entries)
        {
            StringBuilder strExcelXml = new StringBuilder();
            //First Write the Excel Header
            strExcelXml.Append(ExcelHelper.ExcelHeader());
            // Worksheet options Required only one time 
            strExcelXml.Append(ExcelHelper.ExcelWorkSheetOptions());

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
                        if (request.Fields[j].Equals("Created"))
                        {
                            value = entries.ToList()[k].Created.ToString();
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
                                value += Sitecore.Web.WebUtil.GetFullUrl(string.Format(FileDownloadPattern, file));
                            }
                        }
                        else
                        {
                            value = Sitecore.Web.WebUtil.GetFullUrl(string.Format(FileDownloadPattern, field.Value));
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

            return ExcelHelper.ConvertHTMLToExcelXML(strExcelXml.ToString());
        }

        public string ExportToXml(FormViewExportRequest request, List<List<string>> entries)
        {
            XmlSerializer x = new XmlSerializer(entries.GetType());
            var xml = "";

            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    x.Serialize(writer, entries);
                    xml = sww.ToString();
                }
            }

            return xml;
        }

        public string ExportToCsv(FormViewExportRequest request, List<FormEntry> entries)
        {
            StringBuilder sb = new StringBuilder();

            string headerRow = string.Empty;
            headerRow = string.Join(",", request.Fields.ToArray());
            sb.AppendLine(headerRow);
            for (int k = 0; k < entries.Count; k++)
            {
                // Row Tag
                StringBuilder row = new StringBuilder();
                for (int j = 0; j < request.Fields.Count; j++)
                {
                    // Cell Tags
                    
                    var field = entries.ToList()[k].Fields.FirstOrDefault(t => t.FieldName == request.Fields[j]);
                    string value = string.Empty;
                    if (field == null)
                    {
                        if (request.Fields[j].Equals("Created"))
                        {
                            value = entries.ToList()[k].Created.ToString();
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
                                value += Sitecore.Web.WebUtil.GetFullUrl(string.Format(FileDownloadPattern, file));
                            }
                        }
                        else
                        {
                            value = Sitecore.Web.WebUtil.GetFullUrl(string.Format(FileDownloadPattern, field.Value));
                        }
                    }
                    else
                    {
                        value = field.Value;
                    }
                    row.Append(value);
                    row.Append(",");
                }
                sb.AppendLine(row.ToString());
            }

            return sb.ToString();
        }
    }
}