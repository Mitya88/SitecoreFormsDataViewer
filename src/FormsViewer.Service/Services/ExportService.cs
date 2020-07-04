namespace FormsViewer.Service.Services
{
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;
    using FormsViewer.Service.Models;
    using Sitecore.ExperienceForms.Data.Entities;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Linq;

    /// <summary>
    /// Export Service
    /// </summary>
    public class ExportService : IExportService
    {
        /// <summary>
        /// The file download pattern
        /// </summary>
        private const string FileDownloadPattern = "/sitecore/api/ssc/forms/formdata/formdata/DownloadFile?fileId={0}";

        /// <summary>
        /// Exports to XML.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="entries">The entries.</param>
        /// <returns>XML in string</returns>
        public string ExportToXml(FormViewExportRequest request, List<List<string>> entries)
        {
            var xDocument = new XDocument();
            var root = new XElement("root");

            foreach (var row in entries)
            {
                var entry = new XElement("entry");

                for (int i = 0; i < request.Fields.Count; i++)
                {
                    entry.Add(new XElement(request.Fields[i].Trim().Replace(" ", "").ToLower(), row[i]));
                }

                root.Add(entry);
            }

            xDocument.Add(root);

            return xDocument.ToString();
        }

        /// <summary>
        /// Creates the excel.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="entries">The entries.</param>
        /// <returns></returns>
        public Stream CreateExcel(FormViewExportRequest request, List<FormEntry> entries)
        {
            int rowNumber = 1;
            MemoryStream excelStream = new MemoryStream();
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(excelStream, SpreadsheetDocumentType.Workbook))
            {
                var worksheetPart = this.CreateWorkSheet(spreadsheetDocument, "FormExport");
                SheetData sheetData = new SheetData();

                this.AddRow(sheetData, rowNumber++, request.Fields.ToArray(), new Dictionary<int, int>());

                List<object> columnValues = new List<object>();
                for (int k = 0; k < entries.Count; k++)
                {
                    columnValues.Clear();

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

                        columnValues.Add(value);
                    }

                    this.AddRow(sheetData, rowNumber++, columnValues.ToArray(), new Dictionary<int, int>());
                }

                worksheetPart.Worksheet.Append(sheetData);

                return excelStream;
            }
        }

        /// <summary>
        /// Exports to CSV.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="entries">The entries.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Adds the row.
        /// </summary>
        /// <param name="sheetData">The worksheet.</param>
        /// <param name="rowNumber">The row number.</param>
        /// <param name="values">The values.</param>
        /// <param name="specialStyleIndexes">Different style index col index - style index map. Can be used to overwrite the default style for some cells.</param>
        private void AddRow(SheetData sheetData, int rowNumber, object[] values, Dictionary<int, int> specialStyleIndexes)
        {
            Row row = new Row() { RowIndex = (uint)rowNumber };

            for (int i = 0; i < values.Length; i++)
            {
                object val = values[i];
                this.AddCell(row, rowNumber, i + 1, val);
            }

            sheetData.Append(row);
        }

        /// <summary>
        /// Add the cell
        /// </summary>
        /// <param name="row">The worksheet.</param>
        /// <param name="rowNumber">The row number</param>
        /// <param name="colNumber">The column number</param>
        /// <param name="value">The value</param>
        private void AddCell(Row row, int rowNumber, int colNumber, object value)
        {
            if (value == null)
            {
                value = "-";
            }

            var cell = new Cell();

            double doubleValue;

            if (value is DateTime)
            {
                DateTime date = ((DateTime)value).ToLocalTime();
                cell.DataType = CellValues.Number;
                cell.CellValue = new CellValue(date.ToOADate().ToString(CultureInfo.InvariantCulture));
            }
            else if (double.TryParse(value.ToString(), out doubleValue))
            {
                cell.DataType = CellValues.Number;
                cell.CellValue = new CellValue(doubleValue.ToString());
            }
            else
            {
                cell.DataType = CellValues.String;
                cell.CellValue = new CellValue(value.ToString());
            }

            row.Append(cell);
        }

        /// <summary>
        /// Create work sheet
        /// </summary>
        /// <param name="spreadsheetDocument">The document</param>
        /// <param name="sheetName">The sheet name</param>
        /// <param name="sheetId">The sheet ID</param>
        /// <returns>The worksheet</returns>
        private WorksheetPart CreateWorkSheet(SpreadsheetDocument spreadsheetDocument, string sheetName, int sheetId = 1)
        {
            // Add a WorkbookPart to the document.
            WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();

            // Add a WorksheetPart to the WorkbookPart.
            WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet();

            // Add Sheets to the Workbook.
            Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild<Sheets>(new Sheets());

            // Append a new worksheet and associate it with the workbook.
            Sheet sheet = new Sheet()
            {
                Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                SheetId = (uint)sheetId,
                Name = sheetName
            };

            sheets.Append(sheet);
            return worksheetPart;
        }
    }
}