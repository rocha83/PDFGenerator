using System;
using System.Collections.Generic;
using System.Text;

namespace Rochas.PDFGenerator.Helpers
{
    public class PdfBodyPlaceHolder
    {
        public string Key { get; set; } = "";
        public PdfPlaceHolderStyle Style { get; set; } = new PdfPlaceHolderStyle();
    }
}
