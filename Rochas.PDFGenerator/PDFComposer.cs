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

    public PDFComposer(string author, string title,
                       string subject, DateTime creationDate)
    {
        _metaAuthor = author ?? "";
        _metaTitle = title ?? "";
        _metaSubject = subject ?? "";
        _metaCreated = creationDate;
    }

    public byte[] GeneratePdf(
        string template,
        Dictionary<PdfBodyPlaceHolder, string> placeholders,
        PdfPageConfig config)
    {
        RegisterFonts(config);
        
        return BuildPdf(template, placeholders, config);
    }

    public byte[] GeneratePdf<T>(string template, T model, PdfPageConfig config, PdfPlaceHolderStyle defaultStyle = null)
    {
        RegisterFonts(config);

        // map model -> placeholders using defaultStyle
        var placeholders = MapFromModel(model, defaultStyle);
        
        return BuildPdf(template, placeholders, config);
    }

    public byte[] GeneratePdf(DataTable table, PdfPageConfig config, PdfPlaceHolderStyle defaultStyle = null)
    {
        RegisterFonts(config);

        var placeholders = MapFromDataTable(table, defaultStyle);

        // Template será construído dinamicamente
        string template = BuildTemplateFromDataTable(table);

        return BuildPdf(template, placeholders, config);
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

        // Header
        foreach (DataColumn col in table.Columns)
        {
            sb.Append(col.ColumnName).Append(" | ");
        }
        sb.AppendLine();
        sb.AppendLine(new string('-', table.Columns.Count * 10));

        // Detail Lines
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

    // Private Methods

    private byte[] BuildPdf(string template, Dictionary<PdfBodyPlaceHolder, string> placeholders, PdfPageConfig config)
    {
        var document = new PdfInlineDocument(
            template,
            placeholders,
            config,
            _metaAuthor,
            _metaTitle,
            _metaSubject,
            _metaCreated
        );

        return document.GeneratePdf();
    }

    private void RegisterFonts(PdfPageConfig config)
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
