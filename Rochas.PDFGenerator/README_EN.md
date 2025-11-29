# Rochas.PDFGenerator

[![NuGet](https://img.shields.io/nuget/v/Rochas.PDFGenerator.svg)](https://www.nuget.org/packages/Rochas.PDFGenerator)

.NET library for generating PDFs from **templates**, **models (T)**, or **DataTables**, with full support for **headers with logos**, **footer pagination**, **font styles and colors**, **watermarks**, and highly customizable placeholders.
Based on *QuestPDF* and compatible with **.NET Standard 2.1+**.

---

## 📦 Setup

Via .NET CLI:

```bash
dotnet add package Rochas.PDFGenerator
```

Or via Package Manager Console:

```powershell
Install-Package Rochas.PDFGenerator
```

---

Main namespace:

```csharp
using Rochas.PDFGenerator;
```

## 🚀 Visão Geral

The main class is:

```csharp
PDFComposer
```

It offers 3 PDF generation modes:

Template + Placeholders — replaces keys ({{Name}}, {{Date}}) with individual styles.
Generic Model (T) — the object is automatically mapped to placeholders corresponding to the property names and default style.
DataTable — generates tabular PDF with headers/rows automatically within the specified default style.
All modes can use headers, footers with pagination, custom fonts, logos, watermarks, custom margins, individual styles, etc.

## ⚙️ Page Configuration

The PdfPageConfiguration class centralizes the settings:

```csharp
var pageConfig = new PdfPageConfiguration {
    MarginLeft = 40,
    MarginRight = 40,
    MarginTop = 50,
    MarginBottom = 50,

    FontFamily = PdfFontFamily.Montserrat,
    // Opcional: fonte TTF customizada
    CustomFontBytes = File.ReadAllBytes("origem/sua-fonte.ttf"),

    HeaderComposition = new PdfHeaderComposition {
        LogoBytes = File.ReadAllBytes("images/logo.png"),
        LogoAlign = PdfLogoAlignment.Left,
        Title = "Relatório XYZ"
    },

    WatermarkBytes = File.ReadAllBytes("images/watermark.png"),
    WatermarkOpacity = 30,

    FooterPagination = true
};
```

## 🎨 Placeholder Styles

Each placeholder in the body can have its own style via PdfPlaceHolderStyle:

```csharp
new PdfPlaceHolderStyle {
    Bold = true,
    Italic = true,
    Underline = true,
    FontSizePx = 16,
    TextColor = Color.DarkBlue
}
```

The keys are represented by:

```csharp
PdfBodyPlaceHolder { Key = "{{Nome}}", Style = ... }
```

## 📄 Mode 1 — Template + Placeholders (more flexible use)
Template (string):

```text
Cliente: {{NomeCliente}}
Data do Relatório: {{Data}}
```

```csharp
var placeholders = new Dictionary<PdfBodyPlaceHolder, string>() {
    { new PdfBodyPlaceHolder { Key = "{{NomeCliente}}", Style = new PdfPlaceHolderStyle { Bold = true, FontSizePx = 16 } }, "ACME Ltda"  },
    { new PdfBodyPlaceHolder { Key = "{{Data}}", Style = new PdfPlaceHolderStyle { Italic = true } }, DateTime.Now.ToString("dd/MM/yyyy") }
};

byte[] pdf = composer.GeneratePdf(templateString, placeholders, pageConfig);
```

## 📦 Mode 2 — Generic Model (T)

Sample class:

```csharp
public class Cliente {
    public string Nome { get; set; }
    public string Documento { get; set; }
}
```

Template:

```text
Cliente: {{Nome}}
Documento: {{Documento}}
```

Use:

```csharp
var cliente = new Cliente {
    Nome = "ACME Ltda.",
    Documento = "00.000.000/0001-00"
};

byte[] pdf = composer.GeneratePdf(templateString, cliente, pageConfig);
```

## 📊 Mode 3 — DataTable

```csharp
DataTable table = new DataTable();
table.Columns.Add("Produto");
table.Columns.Add("Quantidade");

table.Rows.Add("Caderno", 10);
table.Rows.Add("Lápis", 20);

byte[] pdf = composer.GeneratePdf(table, pageConfig);
```

## 🧪 Complete Sample

```csharp
var composer = new PDFComposer(
    author: "Sistema XYZ",
    title: "Relatório de Clientes",
    subject: "Clientes Ativos",
    creationDate: DateTime.Now
);

var pageConfig = new PdfPageConfiguration {
    FontFamily = PdfFontFamily.Montserrat,
    CustomFontBytes = File.ReadAllBytes("fonts/Montserrat-Regular.ttf"),

    HeaderComposition = new PdfHeaderComposition {
        LogoBytes = File.ReadAllBytes("logo.png"),
        LogoAlign = PdfLogoAlignment.Left,
        Title = "Relatório de Clientes"
    },

    WatermarkBytes = File.ReadAllBytes("watermark.png"),
    WatermarkOpacity = 20,
    FooterPagination = true
};

var template = "Cliente: {{Nome}}\nDocumento: {{Documento}}";

var cliente = new Cliente { Nome = "ACME Ltda.", Documento = "00.000.000/0001-00" };

byte[] pdf = composer.GeneratePdf(template, cliente, pageConfig);

File.WriteAllBytes("Clientes.pdf", pdf);
```

## 🛠 ASP.NET Core Integration

Response sample from an API:

```csharp
return File(pdfBytes, "application/pdf", "relatorio.pdf");
```

## 📄 License

MIT — free for commercial and personal use.