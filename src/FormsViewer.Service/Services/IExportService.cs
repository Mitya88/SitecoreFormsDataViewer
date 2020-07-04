namespace FormsViewer.Service.Services
{
    using FormsViewer.Service.Models;
    using Sitecore.ExperienceForms.Data.Entities;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Export Service
    /// </summary>
    public interface IExportService
    {
        /// <summary>
        /// Exports to XML.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="entries">The entries.</param>
        /// <returns>XML in string</returns>
        string ExportToXml(FormViewExportRequest request, List<List<string>> entries);

        /// <summary>
        /// Creates the excel.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="entries">The entries.</param>
        /// <returns></returns>
        Stream CreateExcel(FormViewExportRequest request, List<FormEntry> entries);

        /// <summary>
        /// Exports to CSV.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="entries">The entries.</param>
        /// <returns></returns>
        string ExportToCsv(FormViewExportRequest request, List<FormEntry> entries);
    }
}