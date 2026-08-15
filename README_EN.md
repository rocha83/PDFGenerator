# Rochas.PDFGenerator

[![NuGet](https://img.shields.io/nuget/v/Rochas.PDFGenerator.svg)](https://www.nuget.org/packages/Rochas.PDFGenerator)

.NET library for generating PDFs from **templates**, **models (T)**, or **DataTables**, with full support for **headers with logos**, **footer pagination**, **font styles and colors**, **watermarks**, **styled tables**, **multi-column layouts**, and highly customizable placeholders.
Based on *QuestPDF* and compatible with **.NET Standard 2.1+**.

---

## 📦 Setup

```bash
dotnet add package Rochas.PDFGenerator
```

---

## 🚀 Overview

The main class is `PDFComposer`, offering **7 PDF generation modes**:

| Mode | Description |
|------|-------------|
| **Template + Placeholders** | Replace keys `{{Name}}` with individual styles |
| **Generic Model (T)** | Object automatically mapped to placeholders |
| **DataTable** | Tabular PDF with headers/rows |
| **Multi-Column** | Two templates side by side with configurable ratios |
| **Multi-Column List** | N columns via template list |
| **Styled Table** | DataTable with borders, alternating rows, custom header |
| **Table + Model Header** | Object header + item table |

---

## ⚙️ Configuration (PdfConfig)

The `PdfConfig` class centralizes all settings:

```csharp
var config = new PdfConfig
{
    MarginLeft = 40,
    MarginRight = 40,
    MarginTop = 50,
    MarginBottom = 50,

    FontFamily = PdfFontFamily.Montserrat,
    CustomFontBytes = File.ReadAllBytes("fonts/Montserrat-Regular.ttf"),

    Header = new PdfHeaderConfig
    {
        LogoBytes = File.ReadAllBytes("images/logo.png"),
        LogoAlign = PdfLogoAlignment.Left,
        Title = "Report Title"
    },

    WatermarkBytes = File.ReadAllBytes("images/watermark.png"),
    WatermarkOpacity = 30,
    FooterPagination = true,

    // Table mode (optional)
    Table = new PdfTableConfig
    {
        Style = PdfTableStyle.Bordered,
        HeaderColor = "#1E3A5F",
        AlternatingRowColor = "#F0F4F8"
    },

    // Column mode (optional)
    Columns = new PdfColumnConfig
    {
        Count = 2,
        Ratios = new[] { 60f, 40f },
        Gap = 10
    }
};
```

> **Backward compatible:** `PdfPageConfiguration` and `PdfHeaderComposition` still work as aliases.

---

## 📄 Mode 1 — Template + Placeholders

```csharp
var template = "Client: {{Name}}\nDate: {{Date}}";

var placeholders = new Dictionary<PdfBodyPlaceHolder, string>
{
    { new PdfBodyPlaceHolder { Key = "{{Name}}", Style = new PdfPlaceHolderStyle { Bold = true, FontSizePx = 16 } }, "ACME Ltd" },
    { new PdfBodyPlaceHolder { Key = "{{Date}}", Style = new PdfPlaceHolderStyle { Italic = true } }, DateTime.Now.ToString("dd/MM/yyyy") }
};

byte[] pdf = composer.GeneratePdf(template, placeholders, config);
```

## 📦 Mode 2 — Generic Model (T)

```csharp
var client = new { Name = "ACME Ltd", Document = "00.000.000/0001-00" };

byte[] pdf = composer.GeneratePdf("Client: {{Name}}\nDoc: {{Document}}", client, config);
```

## 📊 Mode 3 — DataTable

```csharp
DataTable table = new DataTable();
table.Columns.Add("Product");
table.Columns.Add("Quantity");
table.Rows.Add("Notebook", 10);

byte[] pdf = composer.GeneratePdf(table, config);
```

## 📐 Mode 4 — Multi-Column (2 columns)

```csharp
config.Columns = new PdfColumnConfig { Count = 2, Ratios = new[] { 60f, 40f }, Gap = 10 };

var leftPlaceholders = new Dictionary<PdfBodyPlaceHolder, string>
{
    { new PdfBodyPlaceHolder { Key = "{{Name}}" }, "ACME Ltd" },
    { new PdfBodyPlaceHolder { Key = "{{Doc}}" }, "00.000.000/0001-00" }
};

var rightPlaceholders = new Dictionary<PdfBodyPlaceHolder, string>
{
    { new PdfBodyPlaceHolder { Key = "{{Date}}" }, "08/07/2026" },
    { new PdfBodyPlaceHolder { Key = "{{Total}}" }, "$1,500.00" }
};

byte[] pdf = composer.GeneratePdf(
    "Name: {{Name}}\nDoc: {{Doc}}", leftPlaceholders,
    "Date: {{Date}}\nTotal: {{Total}}", rightPlaceholders,
    config);
```

## 📐 Mode 5 — Multi-Column List (N columns)

```csharp
var columns = new List<(string Template, Dictionary<PdfBodyPlaceHolder, string> Placeholders)>
{
    ("Name: {{Name}}\nDoc: {{Doc}}", leftPlaceholders),
    ("Date: {{Date}}\nOrder: {{Order}}", centerPlaceholders),
    ("Total: {{Total}}\nStatus: {{Status}}", rightPlaceholders)
};

byte[] pdf = composer.GeneratePdf(columns, config);
```

## 📋 Mode 6 — Styled Table

```csharp
config.Table = new PdfTableConfig
{
    Style = PdfTableStyle.Bordered,
    HeaderColor = "#1E3A5F",
    HeaderTextBold = true,
    AlternatingRowColor = "#F0F4F8",
    FontSize = 10,
    RowPadding = 5
};

DataTable table = new DataTable();
table.Columns.Add("Product");
table.Columns.Add("Qty");
table.Columns.Add("Value");
table.Rows.Add("Notebook", 2, "$12,000.00");

byte[] pdf = composer.GeneratePdf(table, config);
```

## 📋 Mode 7 — Table + Model Header

```csharp
var headerModel = new { Client = "ACME Ltd", CNPJ = "00.000.000/0001-00", Date = "08/07/2026" };

var tableConfig = new PdfTableConfig
{
    Style = PdfTableStyle.Bordered,
    HeaderColor = "#1E3A5F"
};

byte[] pdf = composer.GeneratePdf(table, headerModel, tableConfig, config);
```

---

## 📋 PdfTableConfig — Table Styles

```csharp
new PdfTableConfig
{
    Style = PdfTableStyle.Bordered,    // Bordered | Striped | Minimal | Grid
    HeaderColor = "#1E3A5F",
    HeaderTextBold = true,
    HeaderFontSize = 10,
    AlternatingRowColor = "#F0F4F8",
    FontSize = 10,
    RowPadding = 5,
    ShowBorders = true,
    BorderColor = "#CCCCCC"
};
```

## 📐 PdfColumnConfig — Columns

```csharp
new PdfColumnConfig
{
    Count = 2,                    // Number of columns
    Ratios = new[] { 60f, 40f }, // Proportional ratios (only used in Proportional mode; null = equal split)
    Gap = 10,                     // Space between columns
    DividerStyle = PdfColumnDividerStyle.None,
    FitMode = PdfColumnFitMode.Proportional // Proportional (default) | AutoFit
};
```

**FitMode behavior:**
- `Proportional` (default) — columns fill the available width proportionally (`Ratios`, or equal split when null). Text wraps inside each column.
- `AutoFit` — columns are sized to their content (true content auto-fit). If the combined content is wider than the page, the document automatically falls back to proportional sizing instead of throwing.

---

## 🧪 Complete Sample

```csharp
var composer = new PDFComposer(
    author: "XYZ System",
    title: "Client Report",
    subject: "Active Clients",
    creationDate: DateTime.Now
);

var config = new PdfConfig
{
    FontFamily = PdfFontFamily.Montserrat,
    Header = new PdfHeaderConfig
    {
        LogoBytes = File.ReadAllBytes("logo.png"),
        LogoAlign = PdfLogoAlignment.Left,
        Title = "Client Report"
    },
    WatermarkBytes = File.ReadAllBytes("watermark.png"),
    WatermarkOpacity = 20,
    FooterPagination = true,
    Table = new PdfTableConfig
    {
        Style = PdfTableStyle.Bordered,
        HeaderColor = "#1E3A5F"
    }
};

byte[] pdf = composer.GeneratePdf(table, config);
File.WriteAllBytes("Clients.pdf", pdf);
```

## 🛠 ASP.NET Core Integration

```csharp
return File(pdfBytes, "application/pdf", "report.pdf");
```

## 📄 License

GPL v2 — free for commercial and personal use.
