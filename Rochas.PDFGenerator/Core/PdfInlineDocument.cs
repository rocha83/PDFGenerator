using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Rochas.PDFGenerator.Helpers;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Rochas.PDFGenerator.Core
{
    internal class PdfInlineDocument : IDocument
    {
        private readonly string _template;
        private readonly Dictionary<PdfBodyPlaceHolder, string> _placeholders;
        private readonly PdfPageConfiguration _config;

        private readonly string _metaAuthor;
        private readonly string _metaTitle;
        private readonly string _metaSubject;
        private readonly DateTime _created;

        private readonly PdfBodyStyler _styler;

        public PdfInlineDocument(
            string template, Dictionary<PdfBodyPlaceHolder, string> placeholders, PdfPageConfiguration pageConfig,
            string author, string title, string subject, DateTime created)
        {
            _styler = new PdfBodyStyler(pageConfig);

            _template = template ?? "";
            _placeholders = placeholders ?? new Dictionary<PdfBodyPlaceHolder, string>();
            _config = pageConfig;

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

                page.Header().Element(h => _styler.ComposeHeader(h));

                page.Footer().Element(f =>
                {
                    if (_config.FooterPagination)
                    {
                        f.AlignCenter().Text(txt =>
                        {
                            txt.DefaultTextStyle(s => s.FontSize(10));

                            txt.Span("Página ");
                            txt.CurrentPageNumber();

                            txt.Element(e =>
                            {
                                e.ShowIf(ctx => ctx.TotalPages > 1)
                                .PaddingVertical(-2).Text(t =>
                                 {
                                     t.Span(" de ");
                                     t.TotalPages();
                                 });
                            });
                        });
                    }
                });

                if (_config.WatermarkBytes != null)
                    _styler.ApplyWatermark(page);

                page.Content().Padding(10).Element(c =>
                {
                    c.Text(text =>
                    {
                        var parts = _styler.Tokenize(_template);

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
                                _styler.ApplyStyleToSpan(span, style, _config);
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
    }
}
