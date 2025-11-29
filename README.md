# Rochas.PDFGenerator

[![NuGet](https://img.shields.io/nuget/v/Rochas.PDFGenerator.svg)](https://www.nuget.org/packages/Rochas.PDFGenerator)

Biblioteca .NET para geração de PDFs a partir de **templates**, **modelos (T)** ou **DataTables**, com suporte completo a **cabeçalhos com logotipo**, **paginação no rodapé**, **estilos e cores de fontes**, **marca-d’água** e placeholders altamente customizáveis.  
Baseada em *QuestPDF* e compatível com **.NET Standard 2.1+**.

---

## 📦 Instalação

Via CLI do .NET:

```bash
dotnet add package Rochas.PDFGenerator
```

Ou via Package Manager Console:

```powershell
Install-Package Rochas.PDFGenerator
```

---

Namespace principal:

```csharp
using Rochas.PDFGenerator;
```

## 🚀 Visão Geral

A classe principal é:

```csharp
PDFComposer
```

Ela oferece 3 modos de geração de PDF:

Template + Placeholders — substituição de chaves ({{Nome}}, {{Data}}) com estilos individuais.

Model Genérico (T) — o objeto é mapeado automaticamente para placeholders correspondentes aos nomes das propriedades e estilo padrão.

DataTable — gera PDF tabular com cabeçalhos/linhas automaticamente dentro do estilo padrão informado.

Todos os modos podem usar cabeçalho, rodapé com paginação, fontes personalizadas, logo, marca-d’água, margens customizadas, estilos individuais, etc.

## ⚙️ Configuração da Página

A classe PdfPageConfiguration centraliza as configurações:

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

## 🎨 Estilos dos Placeholders

Cada placeholder no corpo pode ter estilo próprio via PdfPlaceHolderStyle:

```csharp
new PdfPlaceHolderStyle {
    Bold = true,
    Italic = true,
    Underline = true,
    FontSizePx = 16,
    TextColor = Color.DarkBlue
}
```

As chaves são representadas por:

```csharp
PdfBodyPlaceHolder { Key = "{{Nome}}", Style = ... }
```

## 📄 Modo 1 — Template + Placeholders (uso mais flexível)
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

## 📦 Modo 2 — Model Genérico (T)

Exemplo de classe:

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

Uso:

```csharp
var cliente = new Cliente {
    Nome = "ACME Ltda.",
    Documento = "00.000.000/0001-00"
};

byte[] pdf = composer.GeneratePdf(templateString, cliente, pageConfig);
```

## 📊 Modo 3 — DataTable

```csharp
DataTable table = new DataTable();
table.Columns.Add("Produto");
table.Columns.Add("Quantidade");

table.Rows.Add("Caderno", 10);
table.Rows.Add("Lápis", 20);

byte[] pdf = composer.GeneratePdf(table, pageConfig);
```

## 🧪 Exemplo Completo

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

## 🛠 Integração via ASP.NET Core

Exemplo de retorno em API:

```csharp
return File(pdfBytes, "application/pdf", "relatorio.pdf");
```

## 📄 Licença

MIT — livre para uso comercial e pessoal.