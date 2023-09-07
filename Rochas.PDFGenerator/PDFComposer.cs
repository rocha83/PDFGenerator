using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rochas.PDFGenerator
{
    public class PDFComposer : IDisposable
    {
        #region Declarations

        private string? _author;
        private string? _title;
        private IList<string>? _textList;

        #endregion

        #region Constructors

        public PDFComposer(string author = "", string title = "")
        {
            _author = author;
            _title = title;
        }

        #endregion

        #region Public Methods

        public void AddText(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (_textList == null)
                    _textList = new List<string>();

                _textList.Add(text);
            }
        }

        public void RemoveText()
        {
            if ((_textList != null) && (_textList.Count > 0))
                _textList.RemoveAt(_textList.Count - 1);
        }

        public byte[] Generate()
        {
            if ((_textList != null) && _textList.Count > 0)
            {
                using var sourceMemStream = new MemoryStream();
                using (var pdfDocument = new iTextSharp.text.Document())
                {
                    pdfDocument.AddCreationDate();
                    pdfDocument.AddProducer();
                    if (!string.IsNullOrWhiteSpace(_author))
                        pdfDocument.AddAuthor(_author);
                    if (!string.IsNullOrWhiteSpace(_title))
                        pdfDocument.AddTitle(_title);

                    using var writer = PdfWriter.GetInstance(pdfDocument, sourceMemStream);
                    pdfDocument.Open();
                    foreach (var text in _textList)
                    {
                        var paragraph = new Paragraph(text);
                        pdfDocument.Add(paragraph);
                    }

                    pdfDocument.Close();
                    writer.Close();
                }

                return sourceMemStream.ToArray();
            }
            else
                throw new Exception("No text content informed.");
        }

        public void Save(string destinationFile)
        {
            var pdfContent = Generate();
            File.WriteAllBytes(destinationFile, pdfContent);
        }

        public void Dispose()
        {
            GC.ReRegisterForFinalize(this);
        }

        #endregion
    }
}
