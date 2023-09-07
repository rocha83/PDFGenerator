using System;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;
using System.Data;
using System.Text;

namespace Rochas.PDFGenerator
{
    public static class PDFConverter
    {
        #region Public Static Methods

        public static byte[] GenerateFromHtml(string htmlContent)
        {
            return RenderPDF(htmlContent);
        }

        public static void SaveFromHtml(string htmlContent, string destinationFile)
        {
            var pdfContent = RenderPDF(htmlContent);

            File.WriteAllBytes(destinationFile, pdfContent);
        }

        public static byte[] GenerateFromDataTable(DataTable dataTable)
        {
            var htmlTable = ConvertDataTableToHTML(dataTable);

            return RenderPDF(htmlTable);
        }

        public static void SaveFromDataTable(DataTable dataTable, string destinationFile)
        {
            var pdfContent = GenerateFromDataTable(dataTable);

            File.WriteAllBytes(destinationFile, pdfContent);
        }



        #endregion

        #region Helper Methods

        private static byte[] RenderPDF(string htmlContent)
        {
            using var sourceMemStream = new MemoryStream();
            using (var pdfDocument = new iTextSharp.text.Document())
            {
                using var writer = PdfWriter.GetInstance(pdfDocument, sourceMemStream);
                pdfDocument.Open();
                pdfDocument.AddCreationDate();

                using (var htmlWorker = new HtmlWorker(pdfDocument))
                {
                    using var stringReader = new StringReader(htmlContent);
                    htmlWorker.Parse(stringReader);
                }

                pdfDocument.Close();
            }

            return sourceMemStream.ToArray();
        }

        private static string ConvertDataTableToHTML(DataTable dataTable, bool useHeader = true)
        {
            var strBuilder = new StringBuilder();
            strBuilder.AppendLine("<table>");

            if (useHeader)
            {
                strBuilder.AppendLine("<tr>");
                for (int colCounter = 0; colCounter < dataTable.Columns.Count; colCounter++)
                    strBuilder.Append(string.Concat("<th>", dataTable.Columns[colCounter].ColumnName, "</th>"));

                strBuilder.AppendLine();
                strBuilder.AppendLine("</tr>");
            }

            for (int rowCounter = 0; rowCounter < dataTable.Rows.Count; rowCounter++)
            {
                strBuilder.AppendLine("<tr>");
                for (int colCounter = 0; colCounter < dataTable.Columns.Count; colCounter++)
                {
                    var cellValue = dataTable.Rows[rowCounter][colCounter].ToString();
                    strBuilder.Append(string.Concat("<td>", cellValue, "</td>"));
                }
                strBuilder.AppendLine();
                strBuilder.AppendLine("</tr>");
            }

            strBuilder.AppendLine("</table>");

            return strBuilder.ToString();
        }

        #endregion
    }
}
