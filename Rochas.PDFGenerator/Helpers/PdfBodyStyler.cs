using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Rochas.ImageGenerator.Enumerators;
using Rochas.ImageGenerator.Filters;
using Rochas.PDFGenerator.Enumerators;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Rochas.PDFGenerator.Helpers
{
    internal class PdfBodyStyler
    {
        private readonly PdfPageConfiguration _pageConfig;
        private readonly Regex _tokenRegex = new Regex(@"\{\{.*?\}\}", RegexOptions.Compiled);

        public PdfBodyStyler(PdfPageConfiguration pageConfig)
        {
            _pageConfig = pageConfig;
        }

        public List<PdfTokenPart> Tokenize(string template)
        {
            var result = new List<PdfTokenPart>();
            int lastIndex = 0;

            foreach (Match m in _tokenRegex.Matches(template))
            {
                if (m.Index > lastIndex)
                    result.Add(new PdfTokenPart { IsPlaceholder = false, Text = template.Substring(lastIndex, m.Index - lastIndex) });

                result.Add(new PdfTokenPart { IsPlaceholder = true, Token = m.Value });
                lastIndex = m.Index + m.Length;
            }

            if (lastIndex < template.Length)
                result.Add(new PdfTokenPart { IsPlaceholder = false, Text = template.Substring(lastIndex) });

            return result;
        }

        public void ApplyWatermark(PageDescriptor page)
        {
            if (_pageConfig.WatermarkBytes == null)
                return;

            using var imageFilter = new ImageFilter(ImageFormatEnum.Png);

            byte[] wmBytes = imageFilter.AjustOpacity(
                _pageConfig.WatermarkBytes,
                _pageConfig.WatermarkOpacity
            );

            page.Background()
                .AlignCenter()
                .AlignMiddle()
                .Image(wmBytes)
                .FitArea();
        }

        public void ComposeHeader(IContainer h)
        {
            var header = _pageConfig.HeaderComposition;
            bool hasLogo = header.LogoBytes != null && header.LogoBytes.Length > 0;
            bool hasTitle = !string.IsNullOrWhiteSpace(header.Title);

            if (!hasLogo && !hasTitle)
                return;

            h.Row(row =>
            {
                if (!hasTitle && hasLogo)
                {
                    row.RelativeItem().AlignCenter().Image(header.LogoBytes).FitArea();
                    return;
                }

                if (!hasLogo && hasTitle)
                {
                    var textDesc = row.RelativeItem().AlignCenter()
                        .Text(header.Title)
                        .FontSize(header.TitleStyle.FontSizePx ?? 20);

                    if (header.TitleStyle.Bold)
                        textDesc.Bold();
                    else
                        textDesc.NormalWeight();

                    return;
                }

                if (header.LogoAlign == PdfLogoAlignment.Left)
                {
                    row.RelativeItem(2).AlignLeft().Image(header.LogoBytes).FitArea();

                    var textDesc = row.RelativeItem(9).AlignRight()
                                      .Text(header.Title)
                                      .FontSize(header.TitleStyle.FontSizePx ?? 20);

                    if (header.TitleStyle.Bold)
                        textDesc.Bold();
                    else
                        textDesc.NormalWeight();
                }
                else
                {
                    var textDesc = row.RelativeItem(9).AlignLeft()
                                      .Text(header.Title)
                                      .FontSize(header.TitleStyle.FontSizePx ?? 20);

                    if (header.TitleStyle.Bold)
                        textDesc.Bold();
                    else
                        textDesc.NormalWeight();

                    row.RelativeItem(2).AlignRight().Image(header.LogoBytes).FitArea();
                }
            });
        }

        public string ResolveColorHex(PdfPlaceHolderStyle style)
        {
            if (style.TextColor == PdfTextColor.CustomHex &&
                !string.IsNullOrWhiteSpace(style.CustomTextColor))
                return style.CustomTextColor;

            return style.TextColor switch
            {
                PdfTextColor.White => Colors.White,
                PdfTextColor.Gray => Colors.Grey.Medium,
                PdfTextColor.LightGray => Colors.Grey.Lighten3,
                PdfTextColor.DarkGray => Colors.Grey.Darken4,
                PdfTextColor.Blue => Colors.Blue.Medium,
                PdfTextColor.DarkBlue => Colors.Blue.Darken4,
                PdfTextColor.Green => Colors.Green.Medium,
                PdfTextColor.DarkGreen => Colors.Green.Darken4,
                PdfTextColor.Red => Colors.Red.Medium,
                PdfTextColor.DarkRed => Colors.Red.Darken4,
                PdfTextColor.Yellow => Colors.Yellow.Medium,
                PdfTextColor.Orange => Colors.Orange.Medium,
                PdfTextColor.Brown => Colors.Brown.Medium,
                PdfTextColor.Cyan => Colors.Cyan.Medium,

                _ => Colors.Black,
            };
        }

        public void ApplyStyleToSpan(TextSpanDescriptor span, PdfPlaceHolderStyle style, PdfPageConfiguration cfg)
        {
            span.FontSize(style.FontSizePx ?? 12);

            if (style.Bold) span.Bold();
            if (style.Italic) span.Italic();
            if (style.Underline) span.Underline();

            var hex = ResolveColorHex(style);
            span.FontColor(hex);

            if (!string.IsNullOrWhiteSpace(style.CustomFontFamily))
            {
                try { span.FontFamily(style.CustomFontFamily); }
                catch { }
            }
        }
    }
}
