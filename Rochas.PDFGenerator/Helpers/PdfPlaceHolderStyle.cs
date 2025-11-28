using System;
using Rochas.PDFGenerator.Enumerators;

namespace Rochas.PDFGenerator.Helpers
{
    public class PdfPlaceHolderStyle
    {
        public bool Bold { get; set; } = false;
        public bool Italic { get; set; } = false;
        public bool Underline { get; set; } = false;
        public float? FontSizePx { get; set; } = null;
        public PdfTextColor TextColor { get; set; } = PdfTextColor.Black;
        public string? CustomTextColor { get; set; } = null;
        public string? CustomFontFamily { get; set; } = null;
    }
}
