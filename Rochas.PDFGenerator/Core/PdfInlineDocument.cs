using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Rochas.ImageGenerator.Enumerators;
using Rochas.ImageGenerator.Filters;
using Rochas.PDFGenerator.Enumerators;
using Rochas.PDFGenerator.Helpers;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Rochas.PDFGenerator.Core
{
    internal class PdfInlineDocument : IDocument
    {
        private readonly string _template;
        private readonly Dictionary<PdfBodyPlaceHolder, string> _placeholders;
        private readonly PdfPageConfig _config;

        private readonly string _metaAuthor;
        private readonly string _metaTitle;
        private readonly string _metaSubject;
        private readonly DateTime _created;

        private readonly Regex _tokenRegex = new Regex(@"\{\{.*?\}\}", RegexOptions.Compiled);

        public PdfInlineDocument(
            string template, Dictionary<PdfBodyPlaceHolder, string> placeholders, PdfPageConfig config,
            string author, string title, string subject, DateTime created)
        {
            _template = template ?? "";
            _placeholders = placeholders ?? new Dictionary<PdfBodyPlaceHolder, string>();
            _config = config;

            _metaAuthor = author ?? "";
            _metaTitle = title ?? "";
            _metaSubject = subject ?? "";
            _created = created;
        }

        public DocumentMetadata GetMetadata()
        {
            var signature = ComputeHashSignature();

            return new DocumentMetadata
            {
                Title = _metaTitle,
                Author = _metaAuthor,
                Subject = _metaSubject,
                CreationDate = _created,
                Keywords = $"RochasHash={signature}"
            };
        }

        private string ComputeHashSignature()
        {
            using var sha = SHA256.Create();
            var raw = $"{_metaAuthor}|{_metaTitle}|{_created:O}|{Guid.NewGuid()}";
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        public DocumentSettings GetSettings() => new DocumentSettings();

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.MarginLeft(_config.MarginLeft);
                page.MarginRight(_config.MarginRight);
                page.MarginTop(_config.MarginTop);
                page.MarginBottom(_config.MarginBottom);

                page.Header().Element(h => ComposeHeader(h));

                if (_config.WatermarkBytes != null)
                    ApplyWatermark(page);

                page.Content().Padding(10).Element(c =>
                {
                    c.Text(text =>
                    {
                        var parts = Tokenize(_template);

                        foreach (var part in parts)
                        {
                            if (part.IsPlaceholder)
                            {
                                var ph = _placeholders.Keys.FirstOrDefault(
                                    k => string.Equals(k.Key, part.Token, StringComparison.OrdinalIgnoreCase));

                                string value;
                                PdfPlaceHolderStyle style;

                                if (ph != null)
                                {
                                    value = _placeholders[ph] ?? "";
                                    style = ph.Style ?? new PdfPlaceHolderStyle();
                                }
                                else
                                {
                                    value = part.Token;
                                    style = new PdfPlaceHolderStyle();
                                }

                                var span = text.Span(value);
                                ApplyStyleToSpan(span, style, _config);
                            }
                            else
                            {
                                text.Span(part.Text);
                            }
                        }
                    });
                });
            });
        }

        private List<PdfTokenPart> Tokenize(string template)
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

        private void ApplyWatermark(PageDescriptor page)
        {
            if (_config.WatermarkBytes == null)
                return;

            using var imageFilter = new ImageFilter(ImageFormatEnum.Jpg);

            byte[] wmBytes = imageFilter.AjustOpacity(
                _config.WatermarkBytes,
                _config.WatermarkOpacity
            );

            page.Background()
                .AlignCenter()
                .AlignMiddle()
                .Image(wmBytes)
                .FitArea();
        }

        private void ComposeHeader(IContainer h)
        {
            var header = _config.Header;
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
                    row.RelativeItem(3).AlignLeft().Image(header.LogoBytes).FitArea();

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

                    row.RelativeItem(3).AlignRight().Image(header.LogoBytes).FitArea();
                }
            });

            h.PaddingVertical(10).LineHorizontal(1).LineColor("black");
        }

        private string ResolveColorHex(PdfPlaceHolderStyle style)
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

        private void ApplyStyleToSpan(TextSpanDescriptor span, PdfPlaceHolderStyle style, PdfPageConfig cfg)
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
