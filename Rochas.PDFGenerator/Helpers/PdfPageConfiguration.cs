using System;
using Rochas.PDFGenerator.Enumerators;

namespace Rochas.PDFGenerator.Helpers
{
    public class PdfPageConfiguration
    {
        public float MarginLeft { get; set; } = 30;
        public float MarginRight { get; set; } = 30;
        public float MarginTop { get; set; } = 30;
        public float MarginBottom { get; set; } = 30;

        public byte[]? WatermarkBytes { get; set; }
        public int WatermarkOpacity { get; set; } = 30;

        public PdfFontFamily FontFamily { get; set; } = PdfFontFamily.LiberationSans;
        public byte[]? CustomFontBytes { get; set; }

        public bool FooterPagination { get; set; } = true;

        public PdfHeaderComposition HeaderComposition { get; set; } = new PdfHeaderComposition();
    }
}
