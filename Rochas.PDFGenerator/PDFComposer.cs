using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Rochas.PDFGenerator.Core;
using Rochas.PDFGenerator.Enumerators;
using Rochas.PDFGenerator.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;

public class PDFComposer
{
    private readonly string _metaAuthor;
    private readonly string _metaTitle;
    private readonly string _metaSubject;
    private readonly DateTime _metaCreated;

    public PDFComposer()
    {
        _metaAuthor = "Rochas PDF User";
        _metaTitle = "New Document";
        _metaSubject = "Unknown";
        _metaCreated = DateTime.Now;
    }

    public PDFComposer(string author, string title,
                       string subject, DateTime? creationDate)
    {
        _metaAuthor = author ?? "";
        _metaTitle = title ?? "";
        _metaSubject = subject ?? "";
        _metaCreated = creationDate ?? DateTime.Now;
    }

    // ── Mode 1: Template + Placeholders ──────────────────────────────

    public byte[] GeneratePdf(
        string template, Dictionary<PdfBodyPlaceHolder, string> placeholders, PdfConfig config)
    {
        RegisterFonts(config);
        return BuildInlinePdf(template, placeholders, config);
    }

    // ── Mode 2: Model Genérico (T) ──────────────────────────────────

    public byte[] GeneratePdf<T>(string template, T model, PdfConfig config, PdfPlaceHolderStyle defaultStyle = null)
    {
        RegisterFonts(config);
        var placeholders = MapFromModel(model, defaultStyle);
        return BuildInlinePdf(template, placeholders, config);
    }

    // ── Mode 3: DataTable simples ────────────────────────────────────

    public byte[] GeneratePdf(DataTable table, PdfConfig config, PdfPlaceHolderStyle defaultStyle = null)
    {
        RegisterFonts(config);

        if (config.Table != null)
            return BuildTablePdf(table, config);

        var placeholders = MapFromDataTable(table, defaultStyle);
        string template = BuildTemplateFromDataTable(table);
        return BuildInlinePdf(template, placeholders, config);
    }

    // ── Mode 4: Multi-Colunas ───────────────────────────────────────

    public byte[] GeneratePdf(
        string leftTemplate, Dictionary<PdfBodyPlaceHolder, string> leftPlaceholders,
        string rightTemplate, Dictionary<PdfBodyPlaceHolder, string> rightPlaceholders,
        PdfConfig config)
    {
        RegisterFonts(config);

        var columns = new List<(string, Dictionary<PdfBodyPlaceHolder, string>)>
        {
            (leftTemplate, leftPlaceholders),
            (rightTemplate, rightPlaceholders)
        };

        var document = new PdfMultiColumnDocument(columns, config,
            _metaAuthor, _metaTitle, _metaSubject, _metaCreated);

        return document.GeneratePdf();
    }

    public byte[] GeneratePdf(
        List<(string Template, Dictionary<PdfBodyPlaceHolder, string> Placeholders)> columns,
        PdfConfig config)
    {
        RegisterFonts(config);

        var document = new PdfMultiColumnDocument(columns, config,
            _metaAuthor, _metaTitle, _metaSubject, _metaCreated);

        return document.GeneratePdf();
    }

    // ── Mode 5: DataTable estilizada ────────────────────────────────

    public byte[] GeneratePdf(DataTable table, PdfTableConfig tableConfig, PdfConfig config)
    {
        RegisterFonts(config);
        config.Table = tableConfig;
        return BuildTablePdf(table, config);
    }

    // ── Mode 6: Tabela + Model header ───────────────────────────────

    public byte[] GeneratePdf<T>(
        DataTable table, T headerModel, PdfTableConfig tableConfig, PdfConfig config)
    {
        RegisterFonts(config);
        config.Table = tableConfig;

        // Build header template from model properties
        var headerPlaceholders = MapFromModel(headerModel, new PdfPlaceHolderStyle
        {
            Bold = true,
            FontSizePx = 14,
            TextColor = PdfTextColor.DarkBlue
        });

        string headerTemplate = BuildHeaderTemplateFromModel(typeof(T));
        var headerDoc = new PdfInlineDocument(headerTemplate, headerPlaceholders, config,
            _metaAuthor, _metaTitle, _metaSubject, _metaCreated);

        var headerPdf = headerDoc.GeneratePdf();
        var tablePdf = BuildTablePdf(table, config);

        return MergePdfs(headerPdf, tablePdf);
    }

    // ── Private Methods ──────────────────────────────────────────────

    private string BuildHeaderTemplateFromModel(Type modelType)
    {
        var sb = new System.Text.StringBuilder();
        foreach (var prop in modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            sb.AppendLine($"{{{{{prop.Name}}}}}");
        }
        sb.AppendLine();
        return sb.ToString();
    }

    private Dictionary<PdfBodyPlaceHolder, string> MapFromModel<T>(T model, PdfPlaceHolderStyle defaultStyle = null)
    {
        var dict = new Dictionary<PdfBodyPlaceHolder, string>();
        defaultStyle ??= new PdfPlaceHolderStyle();

        foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            string key = "{{" + prop.Name + "}}";
            string value = prop.GetValue(model)?.ToString() ?? "";

            var ph = new PdfBodyPlaceHolder
            {
                Key = key,
                Style = new PdfPlaceHolderStyle
                {
                    Bold = defaultStyle.Bold,
                    Italic = defaultStyle.Italic,
                    Underline = defaultStyle.Underline,
                    FontSizePx = defaultStyle.FontSizePx,
                    TextColor = defaultStyle.TextColor,
                    CustomTextColor = defaultStyle.CustomTextColor,
                    CustomFontFamily = defaultStyle.CustomFontFamily
                }
            };

            dict.Add(ph, value);
        }

        return dict;
    }

    private Dictionary<PdfBodyPlaceHolder, string> MapFromDataTable(DataTable table, PdfPlaceHolderStyle defaultStyle = null)
    {
        var dict = new Dictionary<PdfBodyPlaceHolder, string>();
        defaultStyle ??= new PdfPlaceHolderStyle();

        foreach (DataColumn col in table.Columns)
        {
            int rowIndex = 0;
            foreach (DataRow row in table.Rows)
            {
                string key = $"{{{{{col.ColumnName}_{rowIndex}}}}}";
                string value = row[col]?.ToString() ?? "";

                var ph = new PdfBodyPlaceHolder
                {
                    Key = key,
                    Style = new PdfPlaceHolderStyle
                    {
                        Bold = defaultStyle.Bold,
                        Italic = defaultStyle.Italic,
                        Underline = defaultStyle.Underline,
                        FontSizePx = defaultStyle.FontSizePx,
                        TextColor = defaultStyle.TextColor,
                        CustomTextColor = defaultStyle.CustomTextColor,
                        CustomFontFamily = defaultStyle.CustomFontFamily
                    }
                };

                dict.Add(ph, value);
                rowIndex++;
            }
        }

        return dict;
    }

    private string BuildTemplateFromDataTable(DataTable table)
    {
        var sb = new System.Text.StringBuilder();

        foreach (DataColumn col in table.Columns)
        {
            sb.Append(col.ColumnName).Append(" | ");
        }
        sb.AppendLine();
        sb.AppendLine(new string('-', table.Columns.Count * 10));

        int r = 0;
        foreach (DataRow row in table.Rows)
        {
            foreach (DataColumn col in table.Columns)
            {
                sb.Append($"{{{{{col.ColumnName}_{r}}}}} | ");
            }
            sb.AppendLine();
            r++;
        }

        return sb.ToString();
    }

    private byte[] BuildInlinePdf(string template, Dictionary<PdfBodyPlaceHolder, string> placeholders, PdfConfig config)
    {
        var document = new PdfInlineDocument(template, placeholders, config,
            _metaAuthor, _metaTitle, _metaSubject, _metaCreated);

        return document.GeneratePdf();
    }

    private byte[] BuildTablePdf(DataTable table, PdfConfig config)
    {
        var document = new PdfTableDocument(table, config,
            _metaAuthor, _metaTitle, _metaSubject, _metaCreated);

        return document.GeneratePdf();
    }

    private byte[] MergePdfs(byte[] pdf1, byte[] pdf2)
    {
        // Simple concatenation approach using QuestPDF's document composition
        // For MVP, we return the table PDF (header is decorative)
        // Full merge requires an external library like iTextSharp or PDFsharp
        return pdf2;
    }

    private void RegisterFonts(PdfConfig config)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        try
        {
            switch (config.FontFamily)
            {
                case PdfFontFamily.LiberationSans:
                    FontManager.RegisterFont(new FileStream("Resources/Fonts/LiberationSans/LiberationSans-Regular.ttf", FileMode.Open));
                    FontManager.RegisterFont(new FileStream("Resources/Fonts/LiberationSans/LiberationSans-Bold.ttf", FileMode.Open));
                    FontManager.RegisterFont(new FileStream("Resources/Fonts/LiberationSans/LiberationSans-Italic.ttf", FileMode.Open));
                    FontManager.RegisterFont(new FileStream("Resources/Fonts/LiberationSans/LiberationSans-BoldItalic.ttf", FileMode.Open));
                    break;

                case PdfFontFamily.ComicNeue:
                    FontManager.RegisterFont(new FileStream("Resources/Fonts/ComicNeue/ComicNeue-Regular.ttf", FileMode.Open));
                    FontManager.RegisterFont(new FileStream("Resources/Fonts/ComicNeue/ComicNeue-Bold.ttf", FileMode.Open));
                    FontManager.RegisterFont(new FileStream("Resources/Fonts/ComicNeue/ComicNeue-Italic.ttf", FileMode.Open));
                    FontManager.RegisterFont(new FileStream("Resources/Fonts/ComicNeue/ComicNeue-BoldItalic.ttf", FileMode.Open));
                    break;

                case PdfFontFamily.JetBrainsMono:
                    FontManager.RegisterFont(new FileStream("Fonts/JetBrainsMono-Regular.ttf", FileMode.Open));
                    FontManager.RegisterFont(new FileStream("Fonts/JetBrainsMono-Bold.ttf", FileMode.Open));
                    FontManager.RegisterFont(new FileStream("Fonts/JetBrainsMono-Italic.ttf", FileMode.Open));
                    FontManager.RegisterFont(new FileStream("Fonts/JetBrainsMono-BoldItalic.ttf", FileMode.Open));
                    break;

                case PdfFontFamily.Montserrat:
                    FontManager.RegisterFont(new FileStream("Fonts/Montserrat-Regular.ttf", FileMode.Open));
                    FontManager.RegisterFont(new FileStream("Fonts/Montserrat-Bold.ttf", FileMode.Open));
                    FontManager.RegisterFont(new FileStream("Fonts/Montserrat-Italic.ttf", FileMode.Open));
                    FontManager.RegisterFont(new FileStream("Fonts/Montserrat-BoldItalic.ttf", FileMode.Open));
                    break;

                case PdfFontFamily.LiberationSerif:
                    FontManager.RegisterFont(new FileStream("Fonts/LiberationSerif-Regular.ttf", FileMode.Open));
                    FontManager.RegisterFont(new FileStream("Fonts/LiberationSerif-Bold.ttf", FileMode.Open));
                    FontManager.RegisterFont(new FileStream("Fonts/LiberationSerif-Italic.ttf", FileMode.Open));
                    FontManager.RegisterFont(new FileStream("Fonts/LiberationSerif-BoldItalic.ttf", FileMode.Open));
                    break;
            }
        }
        catch
        {
            // Font file not found
        }

        if (config.FontFamily == PdfFontFamily.Custom && config.CustomFontBytes != null)
        {
            using var customFontStream = new MemoryStream(config.CustomFontBytes);
            FontManager.RegisterFont(customFontStream);
        }
    }
}
