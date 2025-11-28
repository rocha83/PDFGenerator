using System;
using System.Collections.Generic;
using System.Text;

namespace Rochas.PDFGenerator.Helpers
{
    internal class PdfTokenPart
    {
        public bool IsPlaceholder { get; set; }
        public string Token { get; set; } = "";
        public string Text { get; set; } = "";
    }
}
