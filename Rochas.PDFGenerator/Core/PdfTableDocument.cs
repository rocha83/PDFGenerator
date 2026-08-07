using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Rochas.PDFGenerator.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Rochas.PDFGenerator.Core
{
    internal class PdfTableDocument : IDocument
    {
        private readonly DataTable _dataTable;
        private readonly PdfConfig _config;
        private readonly string _metaAuthor;
        private readonly string _metaTitle;
        private readonly string _metaSubject;
        private readonly DateTime _created;

        private readonly PdfBodyStyler _styler;

        public PdfTableDocument(
            DataTable dataTable,
            PdfConfig config,
            string author, string title, string subject, DateTime created)
        {
            _styler = new PdfBodyStyler(config);
            _dataTable = dataTable;
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
            var tableConfig = _config.Table ?? new PdfTableConfig();

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
                    content.Table(table =>
                    {
                        int colCount = _dataTable.Columns.Count;

                        table.ColumnsDefinition(columns =>
                        {
                            for (int i = 0; i < colCount; i++)
                                columns.RelativeColumn(1);
                        });

                        // Header row
                        table.Header(header =>
                        {
                            for (int i = 0; i < colCount; i++)
                            {
                                var colName = _dataTable.Columns[i].ColumnName;

                                header.Cell().Element(cell =>
                                {
                                    cell
                                        .Background(_styler.ResolveRawColorHex(tableConfig.HeaderColor ?? "#333333"))
                                        .PaddingVertical(tableConfig.RowPadding)
                                        .PaddingHorizontal(tableConfig.RowPadding)
                                        .Text(colName)
                                        .FontSize(tableConfig.HeaderFontSize)
                                        .FontColor(Colors.White);
                                });
                            }
                        });

                        // Data rows
                        for (int r = 0; r < _dataTable.Rows.Count; r++)
                        {
                            var row = _dataTable.Rows[r];
                            bool isAlternate = tableConfig.AlternatingRowColor != null && r % 2 == 1;

                            for (int c = 0; c < colCount; c++)
                            {
                                var cellValue = row[c]?.ToString() ?? "";

                                table.Cell().Element(cell =>
                                {
                                    IContainer container = cell;

                                    if (isAlternate)
                                        container = container.Background(_styler.ResolveRawColorHex(tableConfig.AlternatingRowColor));

                                    if (tableConfig.ShowBorders && tableConfig.Style == PdfTableStyle.Bordered)
                                    {
                                        container = container.BorderBottom(0.5f);
                                        if (tableConfig.BorderColor != null)
                                            container = container.BorderColor(_styler.ResolveRawColorHex(tableConfig.BorderColor));
                                        else
                                            container = container.BorderColor(Colors.Grey.Lighten2);
                                    }

                                    container
                                        .PaddingVertical(tableConfig.RowPadding)
                                        .PaddingHorizontal(tableConfig.RowPadding)
                                        .Text(cellValue)
                                        .FontSize(tableConfig.FontSize);
                                });
                            }
                        }
                    });
                });
            });
        }
    }
}
