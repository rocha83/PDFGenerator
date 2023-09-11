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
        private IList<PDFTextLine>? _textList;

        #endregion

        #region Constructors

        public PDFComposer(string author = "", string title = "")
        {
            _author = author;
            _title = title;
        }

        #endregion

        #region Public Methods

        public void AddTextLine(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (_textList == null)
                    _textList = new List<PDFTextLine>();

                _textList.Add(new PDFTextLine(text));
            }
        }

        public void AddTextLine(string text, FontConfig fontConfig)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                if (_textList == null)
                    _textList = new List<PDFTextLine>();

                _textList.Add(new PDFTextLine(text, fontConfig));
            }
        }

        public void RemoveTextLine(int lineNumber)
        {
            if ((_textList != null) && (_textList.Count >= lineNumber))
                _textList.RemoveAt(lineNumber);
        }

        public byte[] Generate()
        {
            if ((_textList != null) && _textList.Count > 0)
            {
                using var sourceMemStream = new MemoryStream();
                using (var pdfDocument = new Document())
                {
                    using var writer = PdfWriter.GetInstance(pdfDocument, sourceMemStream);
                    pdfDocument.Open();
                    pdfDocument.AddCreationDate();
                    pdfDocument.AddProducer();
                    if (!string.IsNullOrWhiteSpace(_author))
                        pdfDocument.AddAuthor(_author);
                    if (!string.IsNullOrWhiteSpace(_title))
                        pdfDocument.AddTitle(_title);

                    foreach (var textLine in _textList)
                    {
                        Paragraph paragraph = null;
                        if (textLine.FontConfig == null)
                            paragraph = new Paragraph(textLine.Text);
                        else
                            paragraph = new Paragraph(textLine.Text, textLine.FontConfig.GetFontConfiguration());
                        
                        pdfDocument.Add(paragraph);
                    }

                    pdfDocument.Close();
                    writer.Close();
                }

                return sourceMemStream.ToArray();
            }
            else
                throw new Exception("No text line content informed.");
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
