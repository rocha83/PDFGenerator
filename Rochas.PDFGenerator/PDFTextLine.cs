using System;
using System.Collections.Generic;
using System.Text;

namespace Rochas.PDFGenerator
{
    internal class PDFTextLine
    {
        public PDFTextLine(string text, LineConfig fontConfig = null)
        {
            Text = text;
            LineConfig = fontConfig;
        }

        public string Text { get; set; }
        public LineConfig? LineConfig { get; set; }
    }
}
