using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Rochas.PDFGenerator.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Rochas.PDFGenerator.Core
{
    internal class PdfMultiColumnDocument : IDocument
    {
        private readonly List<(string Template, Dictionary<PdfBodyPlaceHolder, string> Placeholders)> _columns;
        private readonly PdfConfig _config;
        private readonly string _metaAuthor;
        private readonly string _metaTitle;
        private readonly string _metaSubject;
        private readonly DateTime _created;

        private readonly PdfBodyStyler _styler;

        public PdfMultiColumnDocument(
            List<(string Template, Dictionary<PdfBodyPlaceHolder, string> Placeholders)> columns,
            PdfConfig config,
            string author, string title, string subject, DateTime created)
        {
            _styler = new PdfBodyStyler(config);
            _columns = columns;
            _config = config;
            _metaAuthor = author ?? "";
            _metaTitle = title ?? "";
            _metaSubject = subject ?? "";
            _created = created;
        }

        public DocumentMetadata GetMetadata()
        {
            using var sha = SHA256.Create();
            var raw = $"{_metaAuthor}|{_metaTitle}|{_created:O}|{Guid.NewGuid()}";
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            var signature = BitConverter.ToString(bytes).Replace("-", "");

            return new DocumentMetadata
            {
                Title = _metaTitle,
                Author = _metaAuthor,
                Subject = _metaSubject,
                CreationDate = _created,
                Keywords = $"RochasHash={signature}"
            };
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

                page.Content().Padding(10).Element(content =>
                {
                    content.Row(row =>
                    {
                        var colConfig = _config.Columns;
                        float[] ratios = colConfig?.Ratios ?? GenerateDefaultRatios(_columns.Count);
                        float gap = colConfig?.Gap ?? 10;

                        for (int i = 0; i < _columns.Count; i++)
                        {
                            float ratio = i < ratios.Length ? ratios[i] : 100f / _columns.Count;

                            if (i > 0)
                                row.ConstantItem(gap).Text("");

                            var colData = _columns[i];
                            row.RelativeItem((int)ratio).Column(col =>
                            {
                                RenderColumnContent(col, colData.Template, colData.Placeholders);
                            });
                        }
                    });
                });
            });
        }

        private float[] GenerateDefaultRatios(int columnCount)
        {
            var ratios = new float[columnCount];
            float each = 100f / columnCount;
            for (int i = 0; i < columnCount; i++)
                ratios[i] = each;
            return ratios;
        }

        private void RenderColumnContent(ColumnDescriptor col, string template, Dictionary<PdfBodyPlaceHolder, string> placeholders)
        {
            var lines = template.Split('\n');

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    col.Item().PaddingBottom(5).Text("");
                    continue;
                }

                col.Item().Text(text =>
                {
                    var parts = _styler.Tokenize(line);

                    foreach (var part in parts)
                    {
                        if (part.IsPlaceholder)
                        {
                            var ph = placeholders.Keys.FirstOrDefault(
                                k => string.Equals(k.Key, part.Token, StringComparison.OrdinalIgnoreCase));

                            string value;
                            PdfPlaceHolderStyle style;

                            if (ph != null)
                            {
                                value = placeholders[ph] ?? "";
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
            }
        }
    }
}
