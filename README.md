# Rochas.PDFGenerator

[![NuGet](https://img.shields.io/nuget/v/Rochas.PDFGenerator.svg)](https://www.nuget.org/packages/Rochas.PDFGenerator)

Biblioteca .NET para geração de PDFs a partir de **templates**, **modelos (T)** ou **DataTables**, com suporte completo a **cabeçalhos com logotipo**, **paginação no rodapé**, **estilos e cores de fontes**, **marca-d'água**, **tabelas estilizadas**, **layout multi-colunas** e placeholders altamente customizáveis.  
Baseada em *QuestPDF* e compatível com **.NET Standard 2.1+**.

---

## 📦 Instalação

```bash
dotnet add package Rochas.PDFGenerator
```

---

## 🚀 Visão Geral

A classe principal é `PDFComposer`, oferecendo **7 modos** de geração de PDF:

| Modo | Descrição |
|------|-----------|
| **Template + Placeholders** | Substituição de chaves `{{Nome}}` com estilos individuais |
| **Model Genérico (T)** | Objeto mapeado automaticamente para placeholders |
| **DataTable** | PDF tabular com cabeçalhos/linhas |
| **Multi-Colunas** | Dois templates lado a lado com proporções configuráveis |
| **Lista Multi-Colunas** | N colunas via lista de templates |
| **Tabela Estilizada** | DataTable com bordas, cores alternadas, header customizado |
| **Tabela + Model Header** | Header com dados do objeto + tabela de itens |

---

## ⚙️ Configuração (PdfConfig)

A classe `PdfConfig` centraliza todas as configurações:

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
        Title = "Relatório XYZ"
    },

    WatermarkBytes = File.ReadAllBytes("images/watermark.png"),
    WatermarkOpacity = 30,
    FooterPagination = true,

    // Modo Tabela (opcional)
    Table = new PdfTableConfig
    {
        Style = PdfTableStyle.Bordered,
        HeaderColor = "#1E3A5F",
        AlternatingRowColor = "#F0F4F8"
    },

    // Modo Colunas (opcional)
    Columns = new PdfColumnConfig
    {
        Count = 2,
        Ratios = new[] { 60f, 40f },
        Gap = 10
    }
};
```

> **Compatibilidade:** `PdfPageConfiguration` e `PdfHeaderComposition` continuam funcionando como aliases.

---

## 📄 Modo 1 — Template + Placeholders

```csharp
var template = "Cliente: {{Nome}}\nData: {{Data}}";

var placeholders = new Dictionary<PdfBodyPlaceHolder, string>
{
    { new PdfBodyPlaceHolder { Key = "{{Nome}}", Style = new PdfPlaceHolderStyle { Bold = true, FontSizePx = 16 } }, "ACME Ltda" },
    { new PdfBodyPlaceHolder { Key = "{{Data}}", Style = new PdfPlaceHolderStyle { Italic = true } }, DateTime.Now.ToString("dd/MM/yyyy") }
};

byte[] pdf = composer.GeneratePdf(template, placeholders, config);
```

## 📦 Modo 2 — Model Genérico (T)

```csharp
var cliente = new { Nome = "ACME Ltda.", Documento = "00.000.000/0001-00" };

byte[] pdf = composer.GeneratePdf("Cliente: {{Nome}}\nDoc: {{Documento}}", cliente, config);
```

## 📊 Modo 3 — DataTable

```csharp
DataTable table = new DataTable();
table.Columns.Add("Produto");
table.Columns.Add("Quantidade");
table.Rows.Add("Caderno", 10);

byte[] pdf = composer.GeneratePdf(table, config);
```

## 📐 Modo 4 — Multi-Colunas (2 colunas)

```csharp
config.Columns = new PdfColumnConfig { Count = 2, Ratios = new[] { 60f, 40f }, Gap = 10 };

var leftPlaceholders = new Dictionary<PdfBodyPlaceHolder, string>
{
    { new PdfBodyPlaceHolder { Key = "{{Nome}}" }, "ACME Ltda." },
    { new PdfBodyPlaceHolder { Key = "{{Doc}}" }, "00.000.000/0001-00" }
};

var rightPlaceholders = new Dictionary<PdfBodyPlaceHolder, string>
{
    { new PdfBodyPlaceHolder { Key = "{{Data}}" }, "07/08/2026" },
    { new PdfBodyPlaceHolder { Key = "{{Total}}" }, "R$ 1.500,00" }
};

byte[] pdf = composer.GeneratePdf(
    "Nome: {{Nome}}\nDoc: {{Doc}}", leftPlaceholders,
    "Data: {{Data}}\nTotal: {{Total}}", rightPlaceholders,
    config);
```

## 📐 Modo 5 — Lista Multi-Colunas (N colunas)

```csharp
var columns = new List<(string Template, Dictionary<PdfBodyPlaceHolder, string> Placeholders)>
{
    ("Nome: {{Nome}}\nDoc: {{Doc}}", leftPlaceholders),
    ("Data: {{Data}}\nPedido: {{Pedido}}", centerPlaceholders),
    ("Total: {{Total}}\nStatus: {{Status}}", rightPlaceholders)
};

byte[] pdf = composer.GeneratePdf(columns, config);
```

## 📋 Modo 6 — Tabela Estilizada

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
table.Columns.Add("Produto");
table.Columns.Add("Qtd");
table.Columns.Add("Valor");
table.Rows.Add("Notebook", 2, "R$ 12.000,00");

byte[] pdf = composer.GeneratePdf(table, config);
```

## 📋 Modo 7 — Tabela + Model Header

```csharp
var headerModel = new { Cliente = "ACME Ltda.", CNPJ = "00.000.000/0001-00", Data = "07/08/2026" };

var tableConfig = new PdfTableConfig
{
    Style = PdfTableStyle.Bordered,
    HeaderColor = "#1E3A5F"
};

byte[] pdf = composer.GeneratePdf(table, headerModel, tableConfig, config);
```

---

## 📋 PdfTableConfig — Estilos de Tabela

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

## 📐 PdfColumnConfig — Colunas

```csharp
new PdfColumnConfig
{
    Count = 2,                    // Número de colunas
    Ratios = new[] { 60f, 40f }, // Proporções (null = auto-fit igual)
    Gap = 10,                     // Espaço entre colunas
    DividerStyle = PdfColumnDividerStyle.None
};
```

---

## 🧪 Exemplo Completo

```csharp
var composer = new PDFComposer(
    author: "Sistema XYZ",
    title: "Relatório de Clientes",
    subject: "Clientes Ativos",
    creationDate: DateTime.Now
);

var config = new PdfConfig
{
    FontFamily = PdfFontFamily.Montserrat,
    Header = new PdfHeaderConfig
    {
        LogoBytes = File.ReadAllBytes("logo.png"),
        LogoAlign = PdfLogoAlignment.Left,
        Title = "Relatório de Clientes"
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
File.WriteAllBytes("Clientes.pdf", pdf);
```

## 🛠 Integração via ASP.NET Core

```csharp
return File(pdfBytes, "application/pdf", "relatorio.pdf");
```

## 📄 Licença

GPL v2 — livre para uso comercial e pessoal.
