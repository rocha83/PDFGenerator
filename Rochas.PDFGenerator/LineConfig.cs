using System;
using System.Drawing;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Rochas.PDFGenerator.Constants;
using Rochas.PDFGenerator.Enumerators;

namespace Rochas.PDFGenerator
{
    public class LineConfig
    {
        public string FontName { get; set; }
        public Color FontColor { get; set; }
        public float FontSize { get; set; }

        public bool IsItalic { get; set; }

        public bool IsUnderlined { get; set; }

        public TextAlignEnum TextAlignment { get; set; }

        internal Font GetFontConfiguration()
        {
            var fontStyle = 0; //Normal
            if (IsItalic)
                fontStyle = Font.ITALIC;
            else if (IsUnderlined)
                fontStyle = Font.UNDERLINE;

            BaseFont baseFont = BaseFont.CreateFont(FontName, BaseFont.CP1252, false);
            return new Font(baseFont, FontSize, fontStyle, new BaseColor(FontColor));
        }
    }
}
