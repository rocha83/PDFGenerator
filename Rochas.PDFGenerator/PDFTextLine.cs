using System;
using System.Collections.Generic;
using System.Text;

namespace Rochas.PDFGenerator
{
    internal class PDFTextLine
    {
        public PDFTextLine(string text, FontConfig fontConfig = null)
        {
            Text = text;
            FontConfig = fontConfig;
        }

        public string Text { get; set; }
        public FontConfig? FontConfig { get; set; }
    }
}
