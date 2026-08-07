using System;
using Rochas.PDFGenerator.Enumerators;

#nullable enable

namespace Rochas.PDFGenerator.Helpers
{
    public class PdfConfig
    {
        public float MarginLeft { get; set; } = 30;
        public float MarginRight { get; set; } = 30;
        public float MarginTop { get; set; } = 30;
        public float MarginBottom { get; set; } = 30;

        public byte[]? WatermarkBytes { get; set; }
        public int WatermarkOpacity { get; set; } = 30;

        public PdfFontFamily FontFamily { get; set; } = PdfFontFamily.LiberationSans;
        public byte[]? CustomFontBytes { get; set; }

        public bool FooterPagination { get; set; } = true;

        public PdfHeaderConfig Header { get; set; } = new PdfHeaderConfig();

        public PdfTableConfig? Table { get; set; }
        public PdfColumnConfig? Columns { get; set; }
        public PdfChartConfig? Chart { get; set; }
    }

    public class PdfHeaderConfig
    {
        public byte[]? LogoBytes { get; set; }
        public PdfLogoAlignment LogoAlign { get; set; } = PdfLogoAlignment.Left;
        public string? Title { get; set; }
        public PdfPlaceHolderStyle TitleStyle { get; set; } = new PdfPlaceHolderStyle { Bold = true, FontSizePx = 20 };
    }

    public class PdfTableConfig
    {
        public PdfTableStyle Style { get; set; } = PdfTableStyle.Bordered;
        public string? HeaderColor { get; set; }
        public bool HeaderTextBold { get; set; } = true;
        public float HeaderFontSize { get; set; } = 10;
        public string? AlternatingRowColor { get; set; }
        public float FontSize { get; set; } = 10;
        public float RowPadding { get; set; } = 5;
        public float ColumnGap { get; set; } = 5;
        public bool ShowBorders { get; set; } = true;
        public string? BorderColor { get; set; }
    }

    public class PdfColumnConfig
    {
        public int Count { get; set; } = 2;
        public float[]? Ratios { get; set; }
        public float Gap { get; set; } = 10;
        public PdfColumnDividerStyle DividerStyle { get; set; } = PdfColumnDividerStyle.None;
        public string? DividerColor { get; set; }
    }

    public class PdfChartConfig
    {
        public PdfChartType Type { get; set; } = PdfChartType.VerticalBar;
        public float Width { get; set; } = 500;
        public float Height { get; set; } = 300;
        public string? Title { get; set; }
        public string[]? Labels { get; set; }
        public decimal[]? Values { get; set; }
        public string[]? Colors { get; set; }
        public bool ShowValues { get; set; } = true;
        public bool ShowGrid { get; set; } = true;
    }

    public enum PdfTableStyle
    {
        Bordered,
        Striped,
        Minimal,
        Grid
    }

    public enum PdfColumnDividerStyle
    {
        None,
        Solid,
        Dashed,
        Dotted
    }

    public enum PdfChartType
    {
        VerticalBar,
        HorizontalBar,
        Pizza
    }

    // Backward compatibility aliases
    public class PdfPageConfiguration : PdfConfig { }
    public class PdfHeaderComposition : PdfHeaderConfig { }
}
