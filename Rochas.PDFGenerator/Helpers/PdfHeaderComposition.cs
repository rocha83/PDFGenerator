using Rochas.PDFGenerator.Enumerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rochas.PDFGenerator.Helpers
{
    public class PdfHeaderComposition
    {
        public byte[]? LogoBytes { get; set; }
        public PdfLogoAlignment LogoAlign { get; set; } = PdfLogoAlignment.Left;
        public string? Title { get; set; }
        public PdfPlaceHolderStyle TitleStyle { get; set; } = new PdfPlaceHolderStyle { Bold = true, FontSizePx = 20 };
    }
}
